using EnglishTutor.Application.Common.ResponseBase.Response;
using MediatR;

namespace EnglishTutor.Application.Features.ChatSessions.Commands.CreateSession
{
    public record CreateSessionCommand(string? Title = null) : IRequest<ApiResponse<CreateSessionResponseDto>>;

    public record CreateSessionResponseDto(
        int Id,
        string Title,
        DateTime CreatedAt);
}
