using AutoMapper;
using CanSayNo.Application.Features.Users.Dtos;
using CanSayNo.Application.Features.Roles.Dtos;
using CanSayNo.Application.Features.Tenants.Dtos;
using CanSayNo.Application.Features.Permissions.Dtos;
using CanSayNo.Application.Features.Roles.Commands.CreateRole;
using CanSayNo.Application.Features.Roles.Commands.UpdateRole;
using CanSayNo.Application.Features.Roles.Commands.DeleteRole;
using CanSayNo.Application.Common.Localization;
using CanSayNo.Domain.Common.Interfaces;
using CanSayNo.Domain.Common.Interfaces.Repositories;
using CanSayNo.Application.Common.Results;
using CanSayNo.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CanSayNo.Domain.Models;
using CanSayNo.Domain.Common.Enums;

namespace CanSayNo.Application.Features.Roles.Commands.CreateRole
{
    public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, Result<RoleDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateRoleCommandHandler> _logger;
        private readonly IJsonStringLocalizer _localizer;

        public CreateRoleCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CreateRoleCommandHandler> logger, IJsonStringLocalizer localizer)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _localizer = localizer;
        }

        public async Task<Result<RoleDto>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"Creating new role: {request.Name}");

                // Check if role name already exists
                if (await _unitOfWork.Roles.RoleExistsAsync(request.Name))
                return Result<RoleDto>.Failure(Error.Failure(
                  ErrorCode.AlreadyExists,
                  _localizer["roles.nameExists"]));
            

                // Create role and assign permissions in a single transaction
                var role = new Role
                {
                    Name = request.Name,
                    Description = request.Description,
                    IsSystemRole = false
                };

                await _unitOfWork.Roles.AddAsync(role);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Created role with ID: {role.Id}");

                // Assign permissions to role if provided
                if (request.PermissionIds?.Any() == true)
                {
                    _logger.LogInformation($"Adding {request.PermissionIds.Count()} permissions to new role. Permission IDs: {string.Join(", ", request.PermissionIds)}");

                    var permissions = await _unitOfWork.Permissions.FindAsync(p => request.PermissionIds.Contains(p.Id));

                    foreach (var permission in permissions)
                    {
                        var rolePermission = new RolePermission
                        {
                            RoleId = role.Id,
                            PermissionId = permission.Id,
                            CreatedDate = DateTime.UtcNow,
                            IsDeleted = false
                        };
                        await _unitOfWork.Roles.AddRolePermissionAsync(rolePermission);
                    }

                    await _unitOfWork.SaveChangesAsync();
                    _logger.LogInformation($"Saved {permissions.Count()} role permissions to database");
                }

                // Get role with permissions for mapping
                var roleWithPermissions = await _unitOfWork.Roles.GetRoleWithPermissionsAsync(role.Id);
                _logger.LogInformation($"Retrieved role with {roleWithPermissions?.RolePermissions?.Count ?? 0} permissions");
                
                var roleDto = _mapper.Map<RoleDto>(roleWithPermissions);
                _logger.LogInformation($"Mapped role DTO with {roleDto?.Permissions?.Count ?? 0} permissions");
                
                _logger.LogInformation($"Successfully created role: {role.Name}");
                return Result<RoleDto>.Success(roleDto);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating role: {RoleName}", request.Name);
                return Result<RoleDto>.Failure(Error.Failure(
                    ErrorCode.InternalError,
                    _localizer["roles.createFailed"]));
            }
        }
    }
} 
