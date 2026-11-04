using CanSayNo.Application.Features.Subscriptions.Queries.GetMySubscription;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CanSayNo.API.Controllers
{
    [Authorize]
    public class SubscriptionsController : BaseController
    {
        private readonly IMediator _mediator;

        public SubscriptionsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>Giriş yapmış kullanıcının premium durumu (RevenueCat webhook'larıyla güncellenir).</summary>
        [HttpGet("me")]
        [ProducesResponseType(typeof(MySubscriptionDto), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetMySubscription()
        {
            var result = await _mediator.Send(new GetMySubscriptionQuery());
            return HandleResult(result);
        }
    }
}
