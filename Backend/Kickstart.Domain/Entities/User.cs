using Kickstart.Domain.Entities;
using Kickstart.Domain.Common.Enums;
using System;
using System.Collections.Generic;

namespace Kickstart.Domain.Entities
{
    public class User : BaseAuditableEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string? PhoneNumber { get; set; }
        public UserStatus Status { get; set; }
        public DateTime? LastLoginDate { get; set; }
        /// <summary>
        /// When admin revokes all sessions, this is set. Access tokens issued before this time are invalid.
        /// </summary>
        public DateTime? LastSessionsRevokedAt { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool PhoneConfirmed { get; set; }
        public string? ProfileImageUrl { get; set; }
        public int? TenantId { get; set; }



    // Navigation properties
    public ICollection<UserRole> UserRoles { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; }
    public Tenant? Tenant { get; set; }

    public User()
        {
            UserRoles = new HashSet<UserRole>();
            RefreshTokens = new HashSet<RefreshToken>();
            Status = UserStatus.Active;
            EmailConfirmed = false;
            PhoneConfirmed = false;
        }

        public string FullName => $"{FirstName} {LastName}".Trim();
    }
} 
