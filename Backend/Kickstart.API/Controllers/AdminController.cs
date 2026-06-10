using Kickstart.Application.Features.Admin.Queries.GetActiveUserCount;
using Kickstart.Application.Features.Admin.Queries.GetActiveUsersSnapshot;
using Kickstart.Application.Features.Admin.Queries.GetRevokableUsers;
using Kickstart.Application.Features.Admin.Commands.RevokeUserSessions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kickstart.API.Controllers
{
    [Authorize(Policy = "RequireAdminRole")]
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class AdminController : BaseController
    {
        private readonly IMediator _mediator;

        public AdminController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get count of users with at least one active session.
        /// SuperAdmin: pass tenantId or omit for all tenants. Admin: returns count for own tenant only.
        /// </summary>
        /// <param name="tenantId">Optional. When null, SuperAdmin gets total across all tenants.</param>
        /// <returns>Active user count</returns>
        [HttpGet("active-users-count")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetActiveUserCount([FromQuery] int? tenantId = null)
        {
            var query = new GetActiveUserCountQuery { TenantId = tenantId };
            var result = await _mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// Users with at least one active refresh token (session), as a table row per user.
        /// Admin: own tenant only. SuperAdmin: all tenants, optional tenantId filter.
        /// </summary>
        [HttpGet("active-users")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetActiveUsersSnapshot([FromQuery] int? tenantId = null)
        {
            var query = new GetActiveUsersSnapshotQuery { TenantId = tenantId };
            var result = await _mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// Get users that can be revoked (active sessions, excluding SuperAdmin).
        /// For dropdown in remote logout UI.
        /// </summary>
        [HttpGet("revokable-users")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetRevokableUsers([FromQuery] int? tenantId = null)
        {
            var query = new GetRevokableUsersQuery { TenantId = tenantId };
            var result = await _mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// Revoke all sessions for a user (admin logout from all devices).
        /// Admin can only revoke users in their tenant. SuperAdmin can revoke any user.
        /// </summary>
        /// <param name="userId">User ID to revoke sessions for</param>
        /// <param name="reason">Optional reason for revocation</param>
        /// <returns>Success message</returns>
        [HttpPost("users/{userId:int}/revoke-sessions")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RevokeUserSessions(int userId, [FromQuery] string? reason = null)
        {
            var command = new RevokeUserSessionsCommand { UserId = userId, Reason = reason };
            var result = await _mediator.Send(command);
            return HandleResult(result);
        }
    }
}
