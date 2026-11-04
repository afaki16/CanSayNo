using CanSayNo.Application.Features.Account.Commands.DeleteMyAccount;
using CanSayNo.Application.Features.Account.Queries.ExportMyData;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CanSayNo.API.Controllers
{
    /// <summary>Kullanıcının kendi hesabı: veri dışa aktarma ve kalıcı silme (KVKK, App Store).</summary>
    [Authorize]
    public class AccountController : BaseController
    {
        private readonly IMediator _mediator;

        public AccountController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>Kullanıcının tüm verisi JSON olarak.</summary>
        [HttpGet("export")]
        [EnableRateLimiting("sensitive")]
        [ProducesResponseType(typeof(AccountExportDto), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> ExportMyData()
        {
            var result = await _mediator.Send(new ExportMyDataQuery());
            return HandleResult(result);
        }

        /// <summary>
        /// Hesabı ve tüm verisini kalıcı olarak siler. Gövde: { "password": "..." }.
        /// Yanlış şifre → 400; son SuperAdmin → 403.
        /// </summary>
        [HttpDelete]
        [EnableRateLimiting("sensitive")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> DeleteMyAccount([FromBody] DeleteMyAccountCommand command)
        {
            var result = await _mediator.Send(command);
            return HandleResult(result);
        }
    }
}
