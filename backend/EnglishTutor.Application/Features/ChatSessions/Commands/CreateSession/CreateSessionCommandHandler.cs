using EnglishTutor.Application.Common.ResponseBase.Response;
using EnglishTutor.Application.Interfaces.Repositories;
using EnglishTutor.Application.Interfaces.Services;
using EnglishTutor.Domain.Entities;
using EnglishTutor.Domain.Exceptions;
using MediatR;

namespace EnglishTutor.Application.Features.ChatSessions.Commands.CreateSession
{
    public class CreateSessionCommandHandler : IRequestHandler<CreateSessionCommand, ApiResponse<CreateSessionResponseDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public CreateSessionCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<ApiResponse<CreateSessionResponseDto>> Handle(
            CreateSessionCommand request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedCustomException("User is not authenticated.");
            }

            var session = new ChatSession
            {
                UserId = userId,
                Title = string.IsNullOrWhiteSpace(request.Title) ? "New Conversation" : request.Title.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<ChatSession>().AddAsync(session);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = new CreateSessionResponseDto(
                session.Id,
                session.Title,
                session.CreatedAt);

            return new ApiResponse<CreateSessionResponseDto>(dto, "Chat session created successfully.", 201);
        }
    }
}
