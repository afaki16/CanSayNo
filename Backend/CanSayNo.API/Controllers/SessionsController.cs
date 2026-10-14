using CanSayNo.Application.Features.Sessions.Commands.StartSession;
using CanSayNo.Application.Features.Sessions.Common;
using CanSayNo.Application.Features.Sessions.Queries.GetMySessions;
using CanSayNo.Application.Features.Sessions.Queries.GetSessionById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
