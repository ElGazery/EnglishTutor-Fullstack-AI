namespace EnglishTutor.Domain.Entities
{
    public class ChatSession
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Ownership
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}
