using EnglishTutor.Application.Common.ResponseBase.Response;
using EnglishTutor.Application.Interfaces.Repositories;
using EnglishTutor.Application.Interfaces.Services;
using EnglishTutor.Application.Specifications;
using EnglishTutor.Domain.Entities;
using EnglishTutor.Domain.Exceptions;
using MediatR;

namespace EnglishTutor.Application.Features.ChatSessions.Queries.GetMySessions
{
    public class GetMySessionsQueryHandler : IRequestHandler<GetMySessionsQuery, ApiResponse<List<SessionSummaryDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetMySessionsQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<ApiResponse<List<SessionSummaryDto>>> Handle(
            GetMySessionsQuery request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedCustomException("User is not authenticated.");
            }

            var spec = new ChatSessionsByUserSpecification(userId);
            var sessions = await _unitOfWork.Repository<ChatSession>().GetAllWithSpecAsync(spec, cancellationToken);

            var sessionDtos = sessions.Select(s => new SessionSummaryDto(
                s.Id,
                s.Title,
                s.CreatedAt,
                s.Messages?.Count ?? 0
            )).ToList();

            return new ApiResponse<List<SessionSummaryDto>>(sessionDtos, "Sessions retrieved successfully.", 200);
        }
    }
}
