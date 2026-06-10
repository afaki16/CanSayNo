using Kickstart.Application.Interfaces;
using Kickstart.Application.Common.Results;
using Kickstart.Domain.Common.Enums;
using Kickstart.Domain.Common.Interfaces;
using Kickstart.Domain.Common.Interfaces.Repositories;
using Kickstart.Domain.Models;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Kickstart.Application.Features.Admin.Queries.GetActiveUserCount
{
    public class GetActiveUserCountQueryHandler : IRequestHandler<GetActiveUserCountQuery, Result<int>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetActiveUserCountQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Result<int>> Handle(GetActiveUserCountQuery request, CancellationToken cancellationToken)
        {
            // SuperAdmin can pass null or any tenantId. Admin can only query their own tenant.
            int? tenantId = request.TenantId;
            if (!_currentUserService.CanAccessAllTenants)
            {
                if (tenantId.HasValue && tenantId != _currentUserService.TenantId)
                    return Result<int>.Failure(Error.Failure(ErrorCode.Forbidden, "You can only view active users in your own tenant"));
                tenantId = _currentUserService.TenantId;
            }

            var excludeSuperAdmins = !_currentUserService.CanAccessAllTenants;
            var count = await _unitOfWork.RefreshTokens.GetActiveUserCountAsync(tenantId, excludeSuperAdmins);
            return Result<int>.Success(count);
        }
    }
}
