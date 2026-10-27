using EnglishTutor.Domain.Enums;

namespace EnglishTutor.Application.Interfaces.Services
{
    /// <summary>
    /// Abstraction over any AI provider. Replace the implementation in Infrastructure
    /// without touching Application or Domain.
    /// </summary>
    public interface IAiTutorService
    {
        Task<string> GetResponseAsync(
            string question,
            AnswerMode mode,
            IReadOnlyList<ChatMessageContext> context,
            CancellationToken cancellationToken = default);
    }

    /// <summary>Lightweight context record sent to the AI provider.</summary>
    public record ChatMessageContext(string Role, string Content);
}
