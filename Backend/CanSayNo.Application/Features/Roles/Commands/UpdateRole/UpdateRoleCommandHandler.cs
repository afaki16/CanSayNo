using AutoMapper;
using CanSayNo.Application.Interfaces;
using CanSayNo.Application.Common.Localization;
using CanSayNo.Application.Features.Roles.Commands.CreateRole;
using CanSayNo.Application.Features.Roles.Commands.UpdateRole;
using CanSayNo.Application.Features.Roles.Commands.DeleteRole;
using CanSayNo.Domain.Common.Interfaces;
using CanSayNo.Domain.Common.Interfaces.Repositories;
using CanSayNo.Application.Common.Results;
using CanSayNo.Domain.Entities;
using CanSayNo.Domain.Models;
using CanSayNo.Domain.Common.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CanSayNo.Application.Features.Users.Dtos;
using CanSayNo.Application.Features.Roles.Dtos;
using CanSayNo.Application.Features.Tenants.Dtos;
using CanSayNo.Application.Features.Permissions.Dtos;

namespace CanSayNo.Application.Features.Roles.Commands.UpdateRole
{
    public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, Result<RoleDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateRoleCommandHandler> _logger;
        private readonly IPermissionService _permissionService;
        private readonly IJsonStringLocalizer _localizer;

        public UpdateRoleCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<UpdateRoleCommandHandler> logger, IPermissionService permissionService, IJsonStringLocalizer localizer)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _permissionService = permissionService;
            _localizer = localizer;
        }

        public async Task<Result<RoleDto>> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"Updating role with ID: {request.Id}");

                var role = await _unitOfWork.Roles.GetRoleWithPermissionsAsync(request.Id);

                if (role == null)
                return Result<RoleDto>.Failure(Error.Failure(
                  ErrorCode.NotFound,
                  _localizer["roles.notFound"]));

                if (role.IsSystemRole)
                return Result<RoleDto>.Failure(Error.Failure(
                ErrorCode.InvalidOperation,
                _localizer["roles.cannotModifySystem"]));

                // Check if role name already exists for another role
                var existingRole = await _unitOfWork.Roles.GetByNameAsync(request.Name);
                if (existingRole != null && existingRole.Id != request.Id)
                return Result<RoleDto>.Failure(Error.Failure(
                                ErrorCode.AlreadyExists,
                                _localizer["roles.nameExists"]));


            // Update role properties
            role.Name = request.Name;
                role.Description = request.Description;

                _unitOfWork.Roles.Update(role);

                // Clear existing permissions
                var existingRolePermissions = role.RolePermissions.ToList();
                foreach (var rolePermission in existingRolePermissions)
                {
                    _unitOfWork.Roles.RemoveRolePermission(rolePermission);
                }

                // Add new permissions if provided
                if (request.PermissionIds?.Any() == true)
                {
                    _logger.LogInformation($"Adding {request.PermissionIds.Count()} permissions to role");

                    var permissions = await _unitOfWork.Permissions.FindAsync(p => request.PermissionIds.Contains(p.Id));

                    foreach (var permission in permissions)
                    {
                        var rolePermission = new RolePermission
                        {
                            RoleId = role.Id,
                            PermissionId = permission.Id
                        };
                        await _unitOfWork.Roles.AddRolePermissionAsync(rolePermission);
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                _permissionService.ClearRolePermissionCache();
                var affectedUserIds = await _unitOfWork.Users.GetUserIdsByRoleIdAsync(request.Id);
                foreach (var userId in affectedUserIds)
                    _permissionService.ClearUserPermissionCache(userId);

                // Get updated role with permissions
                var updatedRole = await _unitOfWork.Roles.GetRoleWithPermissionsAsync(role.Id);
                var roleDto = _mapper.Map<RoleDto>(updatedRole);
                
                _logger.LogInformation($"Successfully updated role: {role.Name}");
                return Result<RoleDto>.Success(roleDto);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating role with ID: {RoleId}", request.Id);
                return Result<RoleDto>.Failure(Error.Failure(
                    ErrorCode.InternalError,
                    _localizer["roles.updateFailed"]));
        }
        }
    }
} 
