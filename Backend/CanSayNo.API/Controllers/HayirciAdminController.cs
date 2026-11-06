using CanSayNo.Application.Features.Admin.Hayirci;
using CanSayNo.Application.Features.Admin.Hayirci.Scenarios;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CanSayNo.API.Controllers
{
    /// <summary>
    /// Hayırcı admin paneli: senaryo kataloğu, kullanıcı özeti, gösterge paneli.
    /// Her uç bir izne bağlı (Scenarios.*, Users.Read, Dashboard.Read); izinler startup'ta
    /// Admin ve SuperAdmin rollerine verilir. PressureTactics YALNIZCA bu uçlardan döner.
    /// </summary>
    [Authorize]
    [Route("api/admin")]
    [ApiController]
    [Produces("application/json")]
    public class HayirciAdminController : BaseController
    {
        private readonly IMediator _mediator;

        public HayirciAdminController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>Pasifler dahil tüm senaryolar, baskı taktikleri ve kullanım istatistiğiyle.</summary>
        [HttpGet("scenarios")]
        [Authorize(Policy = "scenarios.read")]
        [ProducesResponseType(typeof(IEnumerable<AdminScenarioDto>), 200)]
        public async Task<IActionResult> GetScenarios() =>
            HandleResult(await _mediator.Send(new GetAdminScenariosQuery()));

        [HttpGet("scenarios/{id}")]
        [Authorize(Policy = "scenarios.read")]
        [ProducesResponseType(typeof(AdminScenarioDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetScenario(string id) =>
            HandleResult(await _mediator.Send(new GetAdminScenarioByIdQuery(id)));

        /// <summary>Yeni senaryo. ID biçimi: &lt;bağlam&gt;_&lt;3 hane&gt;_&lt;konu&gt;; var olan ID 409.</summary>
        [HttpPost("scenarios/{id}")]
        [Authorize(Policy = "scenarios.create")]
        [ProducesResponseType(typeof(AdminScenarioDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> CreateScenario(string id, [FromBody] AdminScenarioUpsertDto body) =>
            HandleResult(await _mediator.Send(new CreateAdminScenarioCommand(id, body)));

        /// <summary>Senaryoyu tamamen günceller. Bağlam değiştirilemez.</summary>
        [HttpPut("scenarios/{id}")]
        [Authorize(Policy = "scenarios.update")]
        [ProducesResponseType(typeof(AdminScenarioDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateScenario(string id, [FromBody] AdminScenarioUpsertDto body) =>
            HandleResult(await _mediator.Send(new UpdateAdminScenarioCommand(id, body)));

        /// <summary>Aktif/pasif. Pasif senaryo mobilde görünmez; geçmiş oturumlar korunur.</summary>
        [HttpPut("scenarios/{id}/active")]
        [Authorize(Policy = "scenarios.update")]
        [ProducesResponseType(typeof(AdminScenarioDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> SetScenarioActive(string id, [FromBody] SetActiveRequest body) =>
            HandleResult(await _mediator.Send(new SetAdminScenarioActiveCommand(id, body.IsActive)));

        /// <summary>Kullanıcının Hayırcı özeti: abonelik, seri/XP, pratikler, token.</summary>
        [HttpGet("users/{userId:int}/overview")]
        [Authorize(Policy = "users.read")]
        [ProducesResponseType(typeof(AdminUserOverviewDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetUserOverview(int userId) =>
            HandleResult(await _mediator.Send(new GetAdminUserOverviewQuery(userId)));

        /// <summary>Kullanım, premium ve LLM token/maliyet (kaba tahmin) göstergeleri.</summary>
        [HttpGet("dashboard")]
        [Authorize(Policy = "dashboard.read")]
        [ProducesResponseType(typeof(AdminDashboardDto), 200)]
        public async Task<IActionResult> GetDashboard() =>
            HandleResult(await _mediator.Send(new GetAdminDashboardQuery()));

        public sealed record SetActiveRequest(bool IsActive);
    }
}
