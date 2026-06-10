using Kickstart.Application.Features.Admin.Dtos;
using Kickstart.Application.Interfaces;
using Kickstart.Application.Common.Results;
using Kickstart.Domain.Common.Interfaces;
using Kickstart.Domain.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kickstart.Application.Features.Admin.Queries.GetRevokableUsers
{
    public class GetRevokableUsersQueryHandler : IRequestHandler<GetRevokableUsersQuery, Result<IEnumerable<RevokableUserDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetRevokableUsersQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Result<IEnumerable<RevokableUserDto>>> Handle(GetRevokableUsersQuery request, CancellationToken cancellationToken)
        {
            int? tenantId = request.TenantId;
            if (!_currentUserService.CanAccessAllTenants)
            {
                if (tenantId.HasValue && tenantId != _currentUserService.TenantId)
                    return Result<IEnumerable<RevokableUserDto>>.Failure(
                        Domain.Models.Error.Failure(Domain.Common.Enums.ErrorCode.Forbidden, "You can only view users in your own tenant"));
                tenantId = _currentUserService.TenantId;
            }

            var excludeSuperAdmins = !_currentUserService.CanAccessAllTenants;
            var activeUserIds = (await _unitOfWork.RefreshTokens.GetActiveUserIdsAsync(tenantId, excludeSuperAdmins)).ToList();
            if (activeUserIds.Count == 0)
                return Result<IEnumerable<RevokableUserDto>>.Success(new List<RevokableUserDto>());

            var users = await _unitOfWork.Users.GetQueryable()
                .Where(u => activeUserIds.Contains(u.Id))
                .Where(u => !u.UserRoles.Any(ur => ur.Role.Name == RoleNames.SuperAdmin))
                .Select(u => new RevokableUserDto
                {
                    Id = u.Id,
                    FullName = u.FirstName + " " + u.LastName,
                    Email = u.Email
                })
                .OrderBy(u => u.FullName)
                .ToListAsync(cancellationToken);

            return Result<IEnumerable<RevokableUserDto>>.Success(users);
        }
    }
}
