using EnglishTutor.Application.Common.ResponseBase.Response;
using MediatR;

namespace EnglishTutor.Application.Features.Accounts.Commands.Login
{
    public record LoginCommand(
        string Email,
        string Password) : IRequest<ApiResponse<LoginResponseDto>>;

    public record LoginResponseDto(
        string Id,
        string FullName,
        string Email,
        string? GradeLevel,
        string Token);
}
