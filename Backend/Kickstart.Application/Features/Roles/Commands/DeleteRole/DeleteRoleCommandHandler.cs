using Kickstart.Application.Common.Results;
using Kickstart.Application.Common.Localization;
using Kickstart.Application.Features.Roles.Dtos;
using Kickstart.Application.Interfaces;
using Kickstart.Domain.Common.Enums;
using Kickstart.Domain.Common.Interfaces;
using Kickstart.Domain.Models;
using MediatR;


namespace Kickstart.Application.Features.Roles.Commands.DeleteRole
{

    public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPermissionService _permissionService;
        private readonly IJsonStringLocalizer _localizer;

        public DeleteRoleCommandHandler(IUnitOfWork unitOfWork, IPermissionService permissionService, IJsonStringLocalizer localizer)
        {
            _unitOfWork = unitOfWork;
            _permissionService = permissionService;
            _localizer = localizer;
        }

        public async Task<Result> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
        {
            var role = await _unitOfWork.Roles.GetByIdAsync(request.Id);

            if (role == null)
                return Result<RoleDto>.Failure(Error.Failure(
                    ErrorCode.NotFound,
                    _localizer["roles.notFound"]));

            if (role.IsSystemRole)
                return Result<RoleDto>.Failure(Error.Failure(
                    ErrorCode.InvalidOperation,
                    _localizer["roles.cannotModifySystem"]));

            // Collect affected users BEFORE delete so we can invalidate their cached
            // permission lists after the role is removed.
            var affectedUserIds = await _unitOfWork.Users.GetUserIdsByRoleIdAsync(request.Id);

            _unitOfWork.Roles.SoftDelete(role);
            await _unitOfWork.SaveChangesAsync();

            // Drop the role->permissions map (the deleted role is still in it).
            _permissionService.ClearRolePermissionCache();

            // Drop the per-user cache for everyone who had this role assigned.
            foreach (var userId in affectedUserIds)
                _permissionService.ClearUserPermissionCache(userId);

            return Result.Success();
        }
    }
} 
