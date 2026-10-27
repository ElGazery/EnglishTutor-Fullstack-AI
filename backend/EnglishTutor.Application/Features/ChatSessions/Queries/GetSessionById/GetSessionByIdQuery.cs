using EnglishTutor.Application.Common.ResponseBase.Response;
using EnglishTutor.Domain.Enums;
using MediatR;

namespace EnglishTutor.Application.Features.ChatSessions.Queries.GetSessionById
{
    public record GetSessionByIdQuery(int Id) : IRequest<ApiResponse<SessionDetailDto>>;

    public record SessionDetailDto(
        int Id,
        string Title,
        DateTime CreatedAt,
        List<ChatMessageDto> Messages);

    public record ChatMessageDto(
        int Id,
        MessageRole Role,
        string Content,
        AnswerMode AnswerMode,
        DateTime CreatedAt);
}
