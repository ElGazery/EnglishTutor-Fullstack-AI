using EnglishTutor.Domain.Entities;

namespace EnglishTutor.Application.Specifications
{
    public class ChatSessionWithMessagesSpecification : BaseSpecification<ChatSession>
    {
        public ChatSessionWithMessagesSpecification(int sessionId)
            : base(s => s.Id == sessionId)
        {
            AddInclude(s => s.Messages);
        }
    }
}
