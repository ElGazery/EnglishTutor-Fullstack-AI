using EnglishTutor.Domain.Enums;

namespace EnglishTutor.Domain.Entities
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public MessageRole Role { get; set; }
        public string Content { get; set; } = string.Empty;
        public AnswerMode AnswerMode { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Ownership via session
        public int SessionId { get; set; }
        public ChatSession Session { get; set; } = null!;
    }
}
