using CanSayNo.Application.Features.Users.Dtos;
using CanSayNo.Application.Features.Roles.Dtos;
using CanSayNo.Application.Features.Tenants.Dtos;
using CanSayNo.Application.Features.Permissions.Dtos;
using CanSayNo.Application.Common.Results;
using MediatR;
using System.Collections.Generic;

namespace CanSayNo.Application.Features.Permissions.Queries.GetAllPermissions
{
    public class GetAllPermissionsQuery : IRequest<Result<IEnumerable<PermissionDto>>>
    {
    }
} 
