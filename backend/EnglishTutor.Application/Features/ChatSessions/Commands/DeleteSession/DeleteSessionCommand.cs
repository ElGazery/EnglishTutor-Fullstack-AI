using EnglishTutor.Application.Common.ResponseBase.Response;
using EnglishTutor.Application.Interfaces.Repositories;
using EnglishTutor.Application.Interfaces.Services;
using EnglishTutor.Domain.Entities;
using EnglishTutor.Domain.Exceptions;
using MediatR;

namespace EnglishTutor.Application.Features.ChatSessions.Commands.DeleteSession
{
    public record DeleteSessionCommand(int Id) : IRequest<ApiResponse<bool>>;

    public class DeleteSessionCommandHandler : IRequestHandler<DeleteSessionCommand, ApiResponse<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public DeleteSessionCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<ApiResponse<bool>> Handle(
            DeleteSessionCommand request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedCustomException("User is not authenticated.");
            }

            var session = await _unitOfWork.Repository<ChatSession>().GetByIdAsync(request.Id);
            if (session == null)
            {
                throw new NotFoundCustomException($"Chat session with ID {request.Id} was not found.");
            }

            if (session.UserId != userId)
            {
                throw new ForbiddenCustomException("You do not have permission to delete this chat session.");
            }

            await _unitOfWork.Repository<ChatSession>().DeleteAsync(session);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApiResponse<bool>(true, "Chat session deleted successfully.", 200);
        }
    }
}
