using CanSayNo.Application.Features.Users.Dtos;
using CanSayNo.Application.Features.Roles.Dtos;
using CanSayNo.Application.Features.Tenants.Dtos;
using CanSayNo.Application.Features.Permissions.Dtos;
using CanSayNo.Application.Common.Results;
using CanSayNo.Domain.Common.Enums;
using MediatR;
using System;
using System.Collections.Generic;

namespace CanSayNo.Application.Features.Users.Commands.UpdateUser
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
