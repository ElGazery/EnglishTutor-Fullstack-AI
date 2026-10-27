using EnglishTutor.Application.Common.ResponseBase.Response;
using EnglishTutor.Domain.Enums;
using MediatR;

namespace EnglishTutor.Application.Features.ChatMessages.Commands.SendMessage
{
    public record SendMessageCommand(
        int SessionId,
        string Message,
        AnswerMode AnswerMode) : IRequest<ApiResponse<SendMessageResponseDto>>;

    public record SendMessageResponseDto(
        int UserMessageId,
        int AssistantMessageId,
        string AssistantReply,
        AnswerMode AnswerMode,
        DateTime CreatedAt);
}
