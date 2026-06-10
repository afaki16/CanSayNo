using Kickstart.Application.Features.Admin.Dtos;
using Kickstart.Application.Common.Results;
using MediatR;
using System.Collections.Generic;

namespace Kickstart.Application.Features.Admin.Queries.GetRevokableUsers
{
    public class GetRevokableUsersQuery : IRequest<Result<IEnumerable<RevokableUserDto>>>
    {
        /// <summary>
        /// Optional. SuperAdmin can filter by tenant. Admin uses own tenant.
        /// </summary>
        public int? TenantId { get; set; }
    }
}
