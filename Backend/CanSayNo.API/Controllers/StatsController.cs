using CanSayNo.Application.Features.Stats.Queries.GetMyStats;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CanSayNo.API.Controllers
{
    [Authorize]
    public class StatsController : BaseController
    {
        private readonly IMediator _mediator;

        public StatsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Giriş yapmış kullanıcının seri, haftalık aktivite, XP/seviye ve rozetleri.
        /// Rozetler camelCase string (firstNo, threeDayStreak, ...).
        /// </summary>
        [HttpGet("me")]
        [ProducesResponseType(typeof(UserStatsDto), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetMyStats()
        {
            var result = await _mediator.Send(new GetMyStatsQuery());
            return HandleResult(result);
        }
    }
}
