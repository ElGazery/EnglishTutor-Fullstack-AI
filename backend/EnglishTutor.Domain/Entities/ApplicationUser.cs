using Microsoft.AspNetCore.Identity;

namespace EnglishTutor.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? GradeLevel { get; set; }  // e.g. "First Prep", "Second Prep"

        public ICollection<ChatSession> ChatSessions { get; set; } = new List<ChatSession>();
    }
}
