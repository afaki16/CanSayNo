using CanSayNo.Application.Features.Scenarios.Common;
using CanSayNo.Application.Features.Scenarios.Queries.GetScenarioById;
using CanSayNo.Application.Features.Scenarios.Queries.GetScenarios;
using CanSayNo.Application.Features.Scenarios.Queries.GetScenariosByContext;
using CanSayNo.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CanSayNo.API.Controllers
{
    [Authorize] // Tüm endpoint'ler authenticated user gerektirir (JWT).
    public class ScenariosController : BaseController
    {
        private readonly IMediator _mediator;

        public ScenariosController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Kullanıcının erişebileceği tüm aktif senaryoları döndürür.
        /// Opsiyonel olarak bağlama ve ücretsizliğe göre filtrelenir.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ScenarioListItemDto>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetScenarios(
            [FromQuery] ContextId? contextId,
            [FromQuery] bool? isFreeOnly)
        {
            var result = await _mediator.Send(new GetScenariosQuery
            {
                ContextId = contextId,
                IsFreeOnly = isFreeOnly
            });

            return HandleResult(result);
        }

        /// <summary>
        /// Tek senaryonun tam detayını döndürür. Premium içerik premium olmayan
        /// kullanıcıya da döner (istemci paywall'a yönlendirir).
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ScenarioDetailDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetScenarioById(string id)
        {
            var result = await _mediator.Send(new GetScenarioByIdQuery(id));
            return HandleResult(result);
        }

        /// <summary>
        /// Belirli bir bağlama ait aktif senaryoları döndürür.
        /// </summary>
        [HttpGet("by-context/{contextId}")]
        [ProducesResponseType(typeof(IEnumerable<ScenarioListItemDto>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetScenariosByContext(ContextId contextId)
        {
            var result = await _mediator.Send(new GetScenariosByContextQuery(contextId));
            return HandleResult(result);
        }
    }
}
