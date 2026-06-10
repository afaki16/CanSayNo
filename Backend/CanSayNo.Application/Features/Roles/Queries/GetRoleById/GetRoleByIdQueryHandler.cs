using AutoMapper;
using CanSayNo.Application.Features.Roles.Queries.GetAllRoles;
using CanSayNo.Application.Features.Roles.Queries.GetRoleById;
using CanSayNo.Application.Interfaces;
using CanSayNo.Domain.Common.Interfaces;
using CanSayNo.Domain.Common.Interfaces.Repositories;
using CanSayNo.Application.Common.Results;
using CanSayNo.Domain.Models;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using CanSayNo.Application.Features.Users.Dtos;
using CanSayNo.Application.Features.Roles.Dtos;
using CanSayNo.Application.Features.Tenants.Dtos;
using CanSayNo.Application.Features.Permissions.Dtos;
using CanSayNo.Domain.Common.Enums;
using CanSayNo.Domain.Constants;

namespace CanSayNo.Application.Features.Roles.Queries.GetRoleById
{
    public class GetRoleByIdQueryHandler : IRequestHandler<GetRoleByIdQuery, Result<RoleDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public GetRoleByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<Result<RoleDto>> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
        {
            var role = await _unitOfWork.Roles.GetRoleWithPermissionsReadOnlyAsync(request.Id);

            if (role == null)
            return Result<RoleDto>.Failure(Error.Failure(
                       ErrorCode.NotFound,
                       "Role not found"));

            if (!_currentUserService.CanAccessAllTenants && role.Name == RoleNames.SuperAdmin)
                return Result<RoleDto>.Failure(Error.Failure(ErrorCode.Forbidden, "You do not have access to this role"));

        var roleDto = _mapper.Map<RoleDto>(role);
            return Result<RoleDto>.Success(roleDto);
    }
    }
} 
