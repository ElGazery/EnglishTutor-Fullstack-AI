namespace EnglishTutor.Application.Interfaces.Services
{
    public interface ICurrentUserService
    {
        /// <summary>Returns the authenticated user's ID from JWT claims, or null if unauthenticated.</summary>
        string? UserId { get; }
    }
}
