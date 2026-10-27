using EnglishTutor.Application.Common.ResponseBase.Response;
using EnglishTutor.Application.Features.ChatSessions.Commands.CreateSession;
using EnglishTutor.Application.Features.ChatSessions.Commands.DeleteSession;
using EnglishTutor.Application.Features.ChatSessions.Queries.GetMySessions;
using EnglishTutor.Application.Features.ChatSessions.Queries.GetSessionById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishTutor.Api.Controllers
{
    [Route("api/sessions")]
    [ApiController]
    [Authorize]
    public class ChatSessionsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ChatSessionsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<CreateSessionResponseDto>>> CreateSession(
            [FromBody] CreateSessionCommand? command,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command ?? new CreateSessionCommand(), cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<SessionSummaryDto>>>> GetMySessions(
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetMySessionsQuery(), cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<SessionDetailDto>>> GetSessionById(
            [FromRoute] int id,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetSessionByIdQuery(id), cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteSession(
            [FromRoute] int id,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeleteSessionCommand(id), cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
    }
}
