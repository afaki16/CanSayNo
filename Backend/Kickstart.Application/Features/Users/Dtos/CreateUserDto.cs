using Kickstart.Domain.Common.Enums;
using System;
using System.Collections.Generic;

namespace Kickstart.Application.Features.Users.Dtos
{
    public class CreateUserDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string? PhoneNumber { get; set; }
        public UserStatus Status { get; set; } = UserStatus.Active;
        public List<int> RoleIds { get; set; } = new List<int>();
        /// <summary>
        /// Optional. Only SuperAdmin can specify. If null, user is created in current user's tenant.
        /// </summary>
        public int? TenantId { get; set; }
    }
} 
