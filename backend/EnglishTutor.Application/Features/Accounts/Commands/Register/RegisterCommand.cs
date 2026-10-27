using EnglishTutor.Application.Common.ResponseBase.Response;
using MediatR;

namespace EnglishTutor.Application.Features.Accounts.Commands.Register
{
    public record RegisterCommand(
        string FullName,
        string Email,
        string Password,
        string? GradeLevel) : IRequest<ApiResponse<RegisterResponseDto>>;

    public record RegisterResponseDto(
        string Id,
        string FullName,
        string Email,
        string? GradeLevel,
        string Token);
}
