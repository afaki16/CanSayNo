using CanSayNo.Application.Common.Results;
using CanSayNo.Application.Features.Admin.Dtos;
using MediatR;
using System.Collections.Generic;

namespace CanSayNo.Application.Features.Admin.Queries.GetActiveUsersSnapshot
{
    public class GetActiveUsersSnapshotQuery : IRequest<Result<List<ActiveUserSnapshotDto>>>
    {
        /// <summary>Optional. SuperAdmin: null = all tenants. Admin: forced to own tenant.</summary>
        public int? TenantId { get; set; }
    }
}
