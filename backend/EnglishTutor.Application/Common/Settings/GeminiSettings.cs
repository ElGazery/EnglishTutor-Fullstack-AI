namespace EnglishTutor.Application.Common.Settings
{
    public class GeminiSettings
    {
        public const string SectionName = "GeminiSettings";

        /// <summary>
        /// The Gemini model name, e.g. "gemini-1.5-flash"
        /// </summary>
        public string Model { get; set; } = "gemini-1.5-flash";

        /// <summary>
        /// API key — read from environment variable or User Secrets; never commit to source control.
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// Base URL for the Gemini REST API.
        /// </summary>
        public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com";

        /// <summary>
        /// Maximum number of previous chat messages sent as context to the AI.
        /// </summary>
        public int MaxContextMessages { get; set; } = 6;

        /// <summary>
        /// Timeout in seconds for AI requests.
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Ordered cascade of models to try if the current model hits a quota limit (429) or transient error.
        /// </summary>
        public List<string> FallbackModels { get; set; } = new();

        /// <summary>
        /// Multiple API keys for load balancing and quota multiplication (Round-Robin & Failover).
        /// </summary>
        public List<string> ApiKeys { get; set; } = new();

        public IReadOnlyList<string> GetAllApiKeys()
        {
            var list = new List<string>();
            if (ApiKeys != null && ApiKeys.Count > 0)
            {
                list.AddRange(ApiKeys.Where(k => !string.IsNullOrWhiteSpace(k)));
            }
            if (!string.IsNullOrWhiteSpace(ApiKey) && !list.Contains(ApiKey))
            {
                list.Insert(0, ApiKey);
            }
            return list;
        }
    }
}
