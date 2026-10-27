using EnglishTutor.Application.Common.ResponseBase.Response;
using EnglishTutor.Application.Features.ChatMessages.Commands.SendMessage;
using EnglishTutor.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishTutor.Api.Controllers
{
    [Route("api/sessions/{sessionId:int}/messages")]
    [ApiController]
    [Authorize]
    public class ChatMessagesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ChatMessagesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public record SendMessageRequest(
            string Message,
            AnswerMode AnswerMode);

        [HttpPost]
        public async Task<ActionResult<ApiResponse<SendMessageResponseDto>>> SendMessage(
            [FromRoute] int sessionId,
            [FromBody] SendMessageRequest request,
            CancellationToken cancellationToken)
        {
            var command = new SendMessageCommand(sessionId, request.Message, request.AnswerMode);
            var result = await _mediator.Send(command, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
    }
}
