using Kickstart.Domain.Common.Enums;
using Kickstart.Application.Features.Roles.Dtos;
using Kickstart.Application.Features.Permissions.Dtos;
using System;
using System.Collections.Generic;

namespace Kickstart.Application.Features.Users.Dtos
{
    public class UserDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public UserStatus Status { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool PhoneConfirmed { get; set; }
        public string? ProfileImageUrl { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? TenantId { get; set; }
        public List<RoleDto> Roles { get; set; } = new List<RoleDto>();
        public List<PermissionDto> Permissions { get; set; } = new List<PermissionDto>();
    }
} 
