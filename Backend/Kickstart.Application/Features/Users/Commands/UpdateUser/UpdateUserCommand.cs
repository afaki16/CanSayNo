using Kickstart.Application.Features.Users.Dtos;
using Kickstart.Application.Features.Roles.Dtos;
using Kickstart.Application.Features.Tenants.Dtos;
using Kickstart.Application.Features.Permissions.Dtos;
using Kickstart.Application.Common.Results;
using Kickstart.Domain.Common.Enums;
using MediatR;
using System;
using System.Collections.Generic;

namespace Kickstart.Application.Features.Users.Commands.UpdateUser
{
    public class UpdateUserCommand : IRequest<Result<UserListDto>>
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public UserStatus Status { get; set; }
        public string? ProfileImageUrl { get; set; }
        public List<int> RoleIds { get; set; } = new List<int>();
    }
} 
