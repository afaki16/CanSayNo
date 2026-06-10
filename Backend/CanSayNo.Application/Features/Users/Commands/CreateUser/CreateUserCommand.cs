using CanSayNo.Application.Features.Users.Dtos;
using CanSayNo.Application.Features.Roles.Dtos;
using CanSayNo.Application.Features.Tenants.Dtos;
using CanSayNo.Application.Features.Permissions.Dtos;
using CanSayNo.Application.Common.Results;
using CanSayNo.Domain.Common.Enums;
using MediatR;
using System;
using System.Collections.Generic;   

namespace CanSayNo.Application.Features.Users.Commands.CreateUser
{
    public class CreateUserCommand : IRequest<Result<UserListDto>>
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
