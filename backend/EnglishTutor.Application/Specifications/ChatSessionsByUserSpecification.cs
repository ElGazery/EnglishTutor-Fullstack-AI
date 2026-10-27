using EnglishTutor.Domain.Entities;

namespace EnglishTutor.Application.Specifications
{
    public class ChatSessionsByUserSpecification : BaseSpecification<ChatSession>
    {
        public ChatSessionsByUserSpecification(string userId)
            : base(s => s.UserId == userId)
        {
            AddInclude(s => s.Messages);
            ApplyOrderByDescending(s => s.CreatedAt);
        }
    }
}
