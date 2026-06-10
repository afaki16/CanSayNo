using System;

namespace Kickstart.Application.Interfaces
{
    public interface ICurrentUserService
    {
        int? UserId { get; }
        int? TenantId { get; }
        string Email { get; }
        string FullName { get; }
        bool IsAuthenticated { get; }
        bool IsInRole(string role);
        /// <summary>
        /// True when user can access all tenants (e.g. SuperAdmin). When false, user is scoped to their own tenant.
        /// </summary>
        bool CanAccessAllTenants { get; }
    }
} 
