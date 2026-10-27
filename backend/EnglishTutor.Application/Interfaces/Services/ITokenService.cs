using EnglishTutor.Domain.Entities;

namespace EnglishTutor.Application.Interfaces.Services
{
    public interface ITokenService
    {
        Task<string> GenerateTokenAsync(ApplicationUser user);
    }
}
