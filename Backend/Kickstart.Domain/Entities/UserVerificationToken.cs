using Kickstart.Domain.Common.Enums;

namespace Kickstart.Domain.Entities
{
    public class UserVerificationToken : BaseEntity
    {
        public int UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public VerificationChannel Channel { get; set; }
        public VerificationPurpose Purpose { get; set; }
        public string Destination { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
        public DateTime? UsedAt { get; set; }
        public string? RequestIpAddress { get; set; }

        public User User { get; set; } = null!;

        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsValid => !IsUsed && !IsExpired;

        public void MarkAsUsed()
        {
            IsUsed = true;
            UsedAt = DateTime.UtcNow;
        }
    }
}
