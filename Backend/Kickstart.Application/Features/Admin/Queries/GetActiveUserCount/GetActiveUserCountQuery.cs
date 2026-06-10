using Kickstart.Application.Common.Results;
using MediatR;

namespace Kickstart.Application.Features.Admin.Queries.GetActiveUserCount
{
    public class GetActiveUserCountQuery : IRequest<Result<int>>
    {
        /// <summary>
        /// When null, counts across all tenants (SuperAdmin only). Otherwise scoped to tenant.
        /// </summary>
        public int? TenantId { get; set; }
    }
}
