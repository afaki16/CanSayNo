using System.IO;
using System.Text;
using CanSayNo.Application.Features.Subscriptions.Webhooks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CanSayNo.API.Controllers
{
    /// <summary>
    /// Dış servislerin bildirimleri. JWT yok; her uç kendi paylaşılan gizli değeriyle doğrular.
    /// Swagger'da gizli.
    /// </summary>
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class WebhooksController : BaseController
    {
        // RevenueCat olayları küçük; büyük gövdeyi baştan reddet.
        private const long MaxBodyBytes = 256 * 1024;

        private readonly IMediator _mediator;

        public WebhooksController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// RevenueCat webhook. 200: işlendi ya da bilerek yok sayıldı (tekrar denenmesin).
        /// 401: Authorization hatalı. 400: gövde çözülemedi. 503: gizli değer ayarlı değil.
        /// </summary>
        [HttpPost("revenuecat")]
        [RequestSizeLimit(MaxBodyBytes)]
        public async Task<IActionResult> RevenueCat()
        {
            string body;
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
                body = await reader.ReadToEndAsync();

            var result = await _mediator.Send(new ProcessRevenueCatWebhookCommand(
                Request.Headers.Authorization.ToString(), body));

            return result switch
            {
                WebhookResult.Ok => Ok(),
                WebhookResult.Unauthorized => Unauthorized(),
                WebhookResult.BadRequest => BadRequest(),
                _ => StatusCode(StatusCodes.Status503ServiceUnavailable)
            };
        }
    }
}
