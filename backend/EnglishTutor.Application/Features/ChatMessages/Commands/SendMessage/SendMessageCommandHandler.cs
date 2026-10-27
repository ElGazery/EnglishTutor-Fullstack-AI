using EnglishTutor.Application.Common.ResponseBase.Response;
using EnglishTutor.Application.Interfaces.Repositories;
using EnglishTutor.Application.Interfaces.Services;
using EnglishTutor.Application.Specifications;
using EnglishTutor.Domain.Entities;
using EnglishTutor.Domain.Enums;
using EnglishTutor.Domain.Exceptions;
using MediatR;

namespace EnglishTutor.Application.Features.ChatMessages.Commands.SendMessage
{
    public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, ApiResponse<SendMessageResponseDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAiTutorService _aiTutorService;

        public SendMessageCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IAiTutorService aiTutorService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _aiTutorService = aiTutorService;
        }

        public async Task<ApiResponse<SendMessageResponseDto>> Handle(
            SendMessageCommand request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedCustomException("User is not authenticated.");
            }

            var sessionSpec = new ChatSessionWithMessagesSpecification(request.SessionId);
            var session = await _unitOfWork.Repository<ChatSession>().GetFirstWithSpecAsync(sessionSpec, cancellationToken);

            if (session == null)
            {
                throw new NotFoundCustomException($"Chat session with ID {request.SessionId} was not found.");
            }

            if (session.UserId != userId)
            {
                throw new ForbiddenCustomException("You do not have permission to send messages in this session.");
            }

            // Auto-update title from the first question if currently default
            if (session.Title == "New Conversation" || string.IsNullOrWhiteSpace(session.Title))
            {
                var cleanQuestion = request.Message.Trim();
                session.Title = cleanQuestion.Length > 60
                    ? cleanQuestion[..57] + "..."
                    : cleanQuestion;
                await _unitOfWork.Repository<ChatSession>().UpdateAsync(session);
            }

            // Prepare conversation context for the AI (last 6 messages)
            var contextMessages = session.Messages
                .OrderBy(m => m.CreatedAt)
                .TakeLast(6)
                .Select(m => new ChatMessageContext(
                    Role: m.Role == MessageRole.Student ? "user" : "model",
                    Content: m.Content))
                .ToList();

            // 1. Create and save Student's message
            var studentMessage = new ChatMessage
            {
                SessionId = session.Id,
                Role = MessageRole.Student,
                Content = request.Message.Trim(),
                AnswerMode = request.AnswerMode,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<ChatMessage>().AddAsync(studentMessage);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 2. Query Gemini AI Tutor
            var aiReply = await _aiTutorService.GetResponseAsync(
                studentMessage.Content,
                request.AnswerMode,
                contextMessages,
                cancellationToken);

            // 3. Create and save Assistant's reply
            var assistantMessage = new ChatMessage
            {
                SessionId = session.Id,
                Role = MessageRole.Assistant,
                Content = aiReply,
                AnswerMode = request.AnswerMode,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<ChatMessage>().AddAsync(assistantMessage);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var responseDto = new SendMessageResponseDto(
                studentMessage.Id,
                assistantMessage.Id,
                assistantMessage.Content,
                assistantMessage.AnswerMode,
                assistantMessage.CreatedAt);

            return new ApiResponse<SendMessageResponseDto>(responseDto, "Reply generated successfully.", 200);
        }
    }
}
