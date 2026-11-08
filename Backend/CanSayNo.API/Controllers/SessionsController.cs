using CanSayNo.Application.Features.Sessions.Commands.EvaluateSession;
using CanSayNo.Application.Features.Sessions.Commands.SendMessage;
using CanSayNo.Application.Features.Sessions.Commands.StartSession;
using CanSayNo.Application.Features.Sessions.Common;
using CanSayNo.Application.Features.Sessions.Queries.GetMySessions;
using CanSayNo.Application.Features.Sessions.Queries.GetSessionById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CanSayNo.API.Controllers
{
    [Authorize] // Oturumlar her zaman giriş yapmış kullanıcıya ait.
    public class SessionsController : BaseController
    {
        private readonly IMediator _mediator;

        public SessionsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Senaryo için pratik oturumu başlatır. Premium senaryo aktif abonelik ister (403
        /// premiumRequired); ücretsiz kullanıcı günlük limiti aşarsa 429 dailyLimitReached.
        /// Aynı senaryoda henüz mesaj atılmamış açık oturum varsa o döner.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(SessionDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(429)]
        public async Task<IActionResult> StartSession([FromBody] StartSessionCommand command)
        {
            var result = await _mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Kullanıcının kendi oturumunu mesajları ve (varsa) değerlendirmesiyle döndürür.
        /// </summary>
        /// <summary>
        /// Kullanıcı mesajı → karakterin cevabı. Güncellenmiş oturumu döner; son rauntta
        /// status "completed" olur. Hatalar: 409 sessionNotActive, 422 aiResponseRefused,
        /// 429 sessionTokenLimitReached, 503 aiUnavailable — bu üçünde raunt ilerlemez.
        /// </summary>
        [HttpPost("{id:guid}/messages")]
        [EnableRateLimiting("ai")]
        [ProducesResponseType(typeof(SessionDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        [ProducesResponseType(422)]
        [ProducesResponseType(429)]
        [ProducesResponseType(503)]
        public async Task<IActionResult> SendMessage(Guid id, [FromBody] SendMessageRequest body)
        {
            var result = await _mediator.Send(new SendMessageCommand
            {
                SessionId = id,
                Content = body.Content
            });
            return HandleResult(result);
        }

        /// <summary>
        /// Tamamlanmış pratiği değerlendirir (5 eksen 0-100, güçlü yönler, öneriler, koç notu)
        /// ve kullanıcının seri/XP/rozetlerini günceller. İdempotent: ikinci çağrı aynı sonucu
        /// LLM'e gitmeden döner. 409 sessionNotCompleted, 503 aiUnavailable.
        /// </summary>
        [HttpPost("{id:guid}/evaluate")]
        [EnableRateLimiting("ai")]
        [ProducesResponseType(typeof(SessionDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        [ProducesResponseType(503)]
        public async Task<IActionResult> EvaluateSession(Guid id)
        {
            var result = await _mediator.Send(new EvaluateSessionCommand(id));
            return HandleResult(result);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(SessionDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetSessionById(Guid id)
        {
            var result = await _mediator.Send(new GetSessionByIdQuery(id));
            return HandleResult(result);
        }

        /// <summary>
        /// Kullanıcının oturumlarını en yeniden eskiye sayfalı döndürür (pageSize en fazla 50).
        /// </summary>
        [HttpGet("my")]
        [ProducesResponseType(typeof(IEnumerable<SessionListItemDto>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetMySessions(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _mediator.Send(new GetMySessionsQuery
            {
                Page = page,
                PageSize = pageSize
            });

            return HandlePagedResult(result);
        }
    }
}
