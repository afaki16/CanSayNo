using Kickstart.Application.Features.Users.Dtos;
using Kickstart.Application.Features.Roles.Dtos;
using Kickstart.Application.Features.Tenants.Dtos;
using Kickstart.Application.Features.Permissions.Dtos;
using Kickstart.Application.Common.Results;
using MediatR;
using System.Collections.Generic;

namespace Kickstart.Application.Features.Permissions.Queries.GetAllPermissions
{
    public class GetAllPermissionsQuery : IRequest<Result<IEnumerable<PermissionDto>>>
    {
    }
} 
