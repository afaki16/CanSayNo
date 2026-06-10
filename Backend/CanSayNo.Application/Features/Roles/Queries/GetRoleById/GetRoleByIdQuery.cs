using CanSayNo.Application.Features.Users.Dtos;
using CanSayNo.Application.Features.Roles.Dtos;
using CanSayNo.Application.Features.Tenants.Dtos;
using CanSayNo.Application.Features.Permissions.Dtos;
using CanSayNo.Application.Common.Results;
using MediatR;
using System;

namespace CanSayNo.Application.Features.Roles.Queries.GetRoleById
{
    public class GetRoleByIdQuery : IRequest<Result<RoleDto>>
    {
        public int Id { get; set; }
    }
} 
