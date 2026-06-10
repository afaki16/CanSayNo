using System;

namespace Kickstart.Application.Features.Admin.Dtos
{
    /// <summary>
    /// A user with at least one valid (non-revoked, non-expired) refresh token — "online" for the admin dashboard.
    /// </summary>
    public class ActiveUserSnapshotDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int? TenantId { get; set; }
        public string? TenantName { get; set; }
        public int ActiveSessionCount { get; set; }
        public DateTime? LastActivityAt { get; set; }
    }
}
