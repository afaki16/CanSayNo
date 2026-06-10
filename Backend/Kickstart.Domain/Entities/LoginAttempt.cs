using System;

namespace Kickstart.Domain.Entities
{
    public class LoginAttempt : BaseEntity
    {
        public string Email { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public bool Success { get; set; }
        public DateTime AttemptedAt { get; set; }
        public string? UserAgent { get; set; }
        public string? FailureReason { get; set; }
        public DateTime? ClearedAt { get; set; }
    }
}
