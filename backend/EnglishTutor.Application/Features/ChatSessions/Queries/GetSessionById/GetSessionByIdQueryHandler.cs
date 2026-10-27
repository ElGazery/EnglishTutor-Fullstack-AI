using EnglishTutor.Application.Common.ResponseBase.Response;
using EnglishTutor.Application.Interfaces.Repositories;
using EnglishTutor.Application.Interfaces.Services;
using EnglishTutor.Application.Specifications;
using EnglishTutor.Domain.Entities;
using EnglishTutor.Domain.Exceptions;
using MediatR;

namespace EnglishTutor.Application.Features.ChatSessions.Queries.GetSessionById
{
    public class GetSessionByIdQueryHandler : IRequestHandler<GetSessionByIdQuery, ApiResponse<SessionDetailDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetSessionByIdQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<ApiResponse<SessionDetailDto>> Handle(
            GetSessionByIdQuery request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedCustomException("User is not authenticated.");
            }

            var spec = new ChatSessionWithMessagesSpecification(request.Id);
            var session = await _unitOfWork.Repository<ChatSession>().GetFirstWithSpecAsync(spec, cancellationToken);

            if (session == null)
            {
                throw new NotFoundCustomException($"Chat session with ID {request.Id} was not found.");
            }

            if (session.UserId != userId)
            {
                throw new ForbiddenCustomException("You do not have permission to view this chat session.");
            }

            var messageDtos = session.Messages
                .OrderBy(m => m.CreatedAt)
                .Select(m => new ChatMessageDto(
                    m.Id,
                    m.Role,
                    m.Content,
                    m.AnswerMode,
                    m.CreatedAt))
                .ToList();

            var detailDto = new SessionDetailDto(
                session.Id,
                session.Title,
                session.CreatedAt,
                messageDtos);

            return new ApiResponse<SessionDetailDto>(detailDto, "Session details retrieved successfully.", 200);
        }
    }
}
