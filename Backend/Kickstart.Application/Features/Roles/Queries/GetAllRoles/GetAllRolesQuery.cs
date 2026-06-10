using Kickstart.Application.Features.Users.Dtos;
using Kickstart.Application.Features.Roles.Dtos;
using Kickstart.Application.Features.Tenants.Dtos;
using Kickstart.Application.Features.Permissions.Dtos;
using Kickstart.Application.Common.Results;
using MediatR;
using System.Collections.Generic;

namespace Kickstart.Application.Features.Roles.Queries.GetAllRoles
{
    public class GetAllRolesQuery : IRequest<PagedResult<RoleDto>>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SearchTerm { get; set; }
    }
} 
