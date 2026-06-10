using CanSayNo.Application.Features.Admin.Dtos;
using CanSayNo.Application.Common.Results;
using MediatR;
using System.Collections.Generic;

namespace CanSayNo.Application.Features.Admin.Queries.GetRevokableUsers
{
    public class GetRevokableUsersQuery : IRequest<Result<IEnumerable<RevokableUserDto>>>
    {
        /// <summary>
        /// Optional. SuperAdmin can filter by tenant. Admin uses own tenant.
        /// </summary>
        public int? TenantId { get; set; }
    }
}
