using EnglishTutor.Application.Common.Settings;
using EnglishTutor.Application.Interfaces.Services;
using EnglishTutor.Domain.Enums;
using EnglishTutor.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EnglishTutor.Infrastructure.Services
{
    public class GeminiAiTutorService : IAiTutorService
    {
        private readonly HttpClient _httpClient;
        private readonly GeminiSettings _settings;
        private readonly ILogger<GeminiAiTutorService> _logger;

        public GeminiAiTutorService(
            HttpClient httpClient,
            IOptions<GeminiSettings> options,
            ILogger<GeminiAiTutorService> logger)
        {
            _httpClient = httpClient;
            _settings = options.Value;
            _logger = logger;

            if (_httpClient.BaseAddress == null && !string.IsNullOrWhiteSpace(_settings.BaseUrl))
            {
                _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
            }

            if (_settings.TimeoutSeconds > 0)
            {
                _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
            }
        }

        private static int _keyIndexCounter = 0;

        public async Task<string> GetResponseAsync(
            string question,
            AnswerMode mode,
            IReadOnlyList<ChatMessageContext> context,
            CancellationToken cancellationToken = default)
        {
            var apiKeys = _settings.GetAllApiKeys();
            if (apiKeys.Count == 0)
            {
                _logger.LogWarning("No Gemini API keys configured. Returning simulated tutor response.");
                return GenerateOfflineDemoResponse(question, mode);
            }

            var systemInstruction = BuildSystemInstruction(mode);
            var contents = BuildGeminiContents(question, context);

            var requestBody = new GeminiRequest
            {
                SystemInstruction = new GeminiContent
                {
                    Parts = new List<GeminiPart> { new() { Text = systemInstruction } }
                },
                Contents = contents,
                GenerationConfig = new GeminiGenerationConfig
                {
                    Temperature = mode == AnswerMode.Quick ? 0.1 : 0.4,
                    MaxOutputTokens = mode == AnswerMode.Quick ? 300 : 8192
                }
            };

            var modelsToTry = _settings.FallbackModels != null && _settings.FallbackModels.Count > 0
                ? _settings.FallbackModels
                : new List<string> { _settings.Model };

            string? lastError = null;
            int lastStatusCode = 0;

            // Distribute requests evenly across available keys using round-robin
            int startKeyIndex = (int)((uint)Interlocked.Increment(ref _keyIndexCounter) % (uint)apiKeys.Count);

            for (int m = 0; m < modelsToTry.Count; m++)
            {
                var modelName = modelsToTry[m];

                // For each model in the priority list, try all available keys
                for (int k = 0; k < apiKeys.Count; k++)
                {
                    int keyIdx = (startKeyIndex + k) % apiKeys.Count;
                    var currentKey = apiKeys[keyIdx];
                    var endpoint = $"/v1beta/models/{modelName}:generateContent?key={currentKey}";

                    try
                    {
                        var response = await _httpClient.PostAsJsonAsync(endpoint, requestBody, cancellationToken);

                        if (response.IsSuccessStatusCode)
                        {
                            var geminiResponse = await response.Content.ReadFromJsonAsync<GeminiResponse>(cancellationToken: cancellationToken);
                            var answer = geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

                            if (!string.IsNullOrWhiteSpace(answer))
                            {
                                if (m > 0 || k > 0)
                                {
                                    _logger.LogInformation(
                                        "Generated AI response using Model [{Model}] and Key #{KeyIndex} (Model {M}/{TotalM}, Key {K}/{TotalK}).",
                                        modelName, keyIdx + 1, m + 1, modelsToTry.Count, k + 1, apiKeys.Count);
                                }
                                return answer.Trim();
                            }

                            _logger.LogWarning("Model [{Model}] with Key #{KeyIndex} returned empty content.", modelName, keyIdx + 1);
                            continue;
                        }

                        var statusCode = (int)response.StatusCode;
                        var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                        lastStatusCode = statusCode;
                        lastError = errorContent;

                        _logger.LogWarning(
                            "Model [{Model}] with Key #{KeyIndex} returned status {StatusCode}. Error: {Error}. Attempting failover...",
                            modelName, keyIdx + 1, statusCode, errorContent);

                        // If quota limit or error on this key/model, loop continues to next key or next model
                        continue;
                    }
                    catch (Exception ex) when (ex is not BadRequestCustomException)
                    {
                        _logger.LogWarning(ex, "Exception with model [{Model}] using Key #{KeyIndex}.", modelName, keyIdx + 1);
                        continue;
                    }
                }
            }

            _logger.LogError("All {ModelCount} models and {KeyCount} API keys failed. Last status: {StatusCode}, Error: {Error}",
                modelsToTry.Count, apiKeys.Count, lastStatusCode, lastError);

            if (lastStatusCode == 429)
            {
                throw new BadRequestCustomException("AI tutor is currently experiencing high demand across all keys. Please try again in a few moments.");
            }

            throw new BadRequestCustomException("Unable to generate AI tutor response after trying available models and keys. Please try again.");
        }

        private static string BuildSystemInstruction(AnswerMode mode)
        {
            var basePrompt = @"You are a friendly, encouraging, and expert AI English Tutor dedicated to preparatory-school students (middle school / ages 12 to 15).
You specialize in English grammar, vocabulary, tenses, prepositions, translation between English and Arabic, sentence correction, and spelling.";

            if (mode == AnswerMode.Quick)
            {
                return basePrompt + @"

MODE: QUICK MODE.
- Return ONLY the shortest direct correct answer.
- Give just one or two words or the single corrected word/phrase whenever appropriate (e.g., 'went', 'have eaten', 'at').
- DO NOT add full sentences, greetings, filler text, or explanations. Keep it as brief and instant as possible.";
            }

            return basePrompt + @"

MODE: DETAILED MODE.
- Explain the answer clearly, gently, and step-by-step for a preparatory-school student.
- Provide simple explanations in Arabic (شرح مبسط وواضح باللغة العربية) to help them understand the rule.
- Provide clear English example sentences with simple everyday vocabulary.
- Highlight the key rule (قاعدة الدرس) and common mistakes to avoid (أخطاء شائعة تجنبها).
- Use clean Markdown with bullet points and bold text for readability.
- IMPORTANT: Always conclude and complete your full explanation. Never stop or truncate mid-sentence. Ensure all sections, examples, and practice questions are completely finished.";
        }

        private static List<GeminiContent> BuildGeminiContents(
            string currentQuestion,
            IReadOnlyList<ChatMessageContext> context)
        {
            var contents = new List<GeminiContent>();

            foreach (var item in context)
            {
                contents.Add(new GeminiContent
                {
                    Role = item.Role.Equals("Student", StringComparison.OrdinalIgnoreCase) || item.Role.Equals("user", StringComparison.OrdinalIgnoreCase)
                        ? "user"
                        : "model",
                    Parts = new List<GeminiPart> { new() { Text = item.Content } }
                });
            }

            contents.Add(new GeminiContent
            {
                Role = "user",
                Parts = new List<GeminiPart> { new() { Text = currentQuestion } }
            });

            return contents;
        }

        private static string GenerateOfflineDemoResponse(string question, AnswerMode mode)
        {
            if (mode == AnswerMode.Quick)
            {
                return $"Correct answer: {question.Trim()} (Demo mode — set your Gemini API key in appsettings.json for live AI answers).";
            }

            return $@"### 📘 شرح توضيحي (Demo Mode)

**سؤالك:** {question}

> [!NOTE]
> هذا الرد تجريبي لأن مفتاح **Gemini API** غير مفعل بعد في ملف `appsettings.json`.

#### 1. الإجابة النموذجية
للحصول على شرح تفصيلي بالذكاء الاصطناعي الحي مع أمثلة وقواعد اللغة الإنجليزية وشروحات بالعربية، يرجى وضع مفتاح Gemini الخاص بك في ملف الإعدادات `appsettings.json`.

#### 2. أمثلة توضيحية:
- **Example 1:** *She plays tennis every Friday.* (المضارع البسيط - Present Simple)
- **Example 2:** *They went to school yesterday.* (الماضي البسيط - Past Simple)";
        }

        #region Gemini API DTOs

        private class GeminiRequest
        {
            [JsonPropertyName("system_instruction")]
            public GeminiContent? SystemInstruction { get; set; }

            [JsonPropertyName("contents")]
            public List<GeminiContent> Contents { get; set; } = new();

            [JsonPropertyName("generationConfig")]
            public GeminiGenerationConfig? GenerationConfig { get; set; }
        }

        private class GeminiContent
        {
            [JsonPropertyName("role")]
            public string? Role { get; set; }

            [JsonPropertyName("parts")]
            public List<GeminiPart> Parts { get; set; } = new();
        }

        private class GeminiPart
        {
            [JsonPropertyName("text")]
            public string Text { get; set; } = string.Empty;
        }

        private class GeminiGenerationConfig
        {
            [JsonPropertyName("temperature")]
            public double Temperature { get; set; }

            [JsonPropertyName("maxOutputTokens")]
            public int MaxOutputTokens { get; set; }
        }

        private class GeminiResponse
        {
            [JsonPropertyName("candidates")]
            public List<GeminiCandidate>? Candidates { get; set; }
        }

        private class GeminiCandidate
        {
            [JsonPropertyName("content")]
            public GeminiContent? Content { get; set; }
        }

        #endregion
    }
}
