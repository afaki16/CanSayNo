using Kickstart.Application.Common.Results;
using Kickstart.Application.Features.Admin.Dtos;
using Kickstart.Application.Interfaces;
using Kickstart.Domain.Common.Enums;
using Kickstart.Domain.Common.Interfaces;
using Kickstart.Domain.Constants;
using Kickstart.Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kickstart.Application.Features.Admin.Queries.GetActiveUsersSnapshot
{
    public class GetActiveUsersSnapshotQueryHandler : IRequestHandler<GetActiveUsersSnapshotQuery, Result<List<ActiveUserSnapshotDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetActiveUsersSnapshotQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Result<List<ActiveUserSnapshotDto>>> Handle(GetActiveUsersSnapshotQuery request, CancellationToken cancellationToken)
        {
            int? tenantId = request.TenantId;
            if (!_currentUserService.CanAccessAllTenants)
            {
                if (tenantId.HasValue && tenantId != _currentUserService.TenantId)
                    return Result<List<ActiveUserSnapshotDto>>.Failure(
                        Error.Failure(ErrorCode.Forbidden, "You can only view active users in your own tenant"));
                tenantId = _currentUserService.TenantId;
            }

            var now = DateTime.UtcNow;

            var usersQuery = _unitOfWork.Users.GetQueryable().Where(x => !x.IsDeleted);
            if (!_currentUserService.CanAccessAllTenants)
                usersQuery = usersQuery.Where(u => !u.UserRoles.Any(ur => ur.Role.Name == RoleNames.SuperAdmin));

            var list = await (
                from rt in _unitOfWork.RefreshTokens.GetQueryable()
                where !rt.IsRevoked && rt.ExpiryDate > now
                join u in usersQuery on rt.UserId equals u.Id
                where !tenantId.HasValue || u.TenantId == tenantId.Value
                join tenant in _unitOfWork.Tenants.GetQueryable() on u.TenantId equals tenant.Id into tj
                from tenant in tj.DefaultIfEmpty()
                group rt by new
                {
                    u.Id, u.FirstName, u.LastName, u.Email, u.TenantId,
                    TenantName = (string?)tenant.Name
                } into g
                orderby g.Key.TenantName, g.Key.LastName, g.Key.FirstName
                select new ActiveUserSnapshotDto
                {
                    UserId             = g.Key.Id,
                    FullName           = g.Key.FirstName + " " + g.Key.LastName,
                    Email              = g.Key.Email,
                    TenantId           = g.Key.TenantId,
                    TenantName         = g.Key.TenantName,
                    ActiveSessionCount = g.Count(),
                    LastActivityAt     = g.Max(rt => rt.CreatedDate)
                })
                .ToListAsync(cancellationToken);

            return Result<List<ActiveUserSnapshotDto>>.Success(list);
        }
    }
}
