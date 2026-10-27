using EnglishTutor.Application.Common.ResponseBase.Response;
using MediatR;

namespace EnglishTutor.Application.Features.ChatSessions.Queries.GetMySessions
{
    public record GetMySessionsQuery : IRequest<ApiResponse<List<SessionSummaryDto>>>;

    public record SessionSummaryDto(
        int Id,
        string Title,
        DateTime CreatedAt,
        int MessageCount);
}
