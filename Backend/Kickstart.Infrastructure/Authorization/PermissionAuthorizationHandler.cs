using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Kickstart.Application.Interfaces;

namespace Kickstart.Infrastructure.Authorization
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IPermissionService _permissionService;

        public PermissionAuthorizationHandler(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                context.Fail();
                return;
            }

            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                context.Fail();
                return;
            }

            var result = await _permissionService.HasPermissionAsync(userId, requirement.Permission);

            if (result.IsSuccess && result.Value)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
        }
    }
}
