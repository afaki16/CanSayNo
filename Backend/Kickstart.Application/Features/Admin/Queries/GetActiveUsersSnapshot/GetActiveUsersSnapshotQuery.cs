using Kickstart.Application.Common.Results;
using Kickstart.Application.Features.Admin.Dtos;
using MediatR;
using System.Collections.Generic;

namespace Kickstart.Application.Features.Admin.Queries.GetActiveUsersSnapshot
{
    public class GetActiveUsersSnapshotQuery : IRequest<Result<List<ActiveUserSnapshotDto>>>
    {
        /// <summary>Optional. SuperAdmin: null = all tenants. Admin: forced to own tenant.</summary>
        public int? TenantId { get; set; }
    }
}
