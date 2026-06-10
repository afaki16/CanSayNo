using AutoMapper;
using Kickstart.Application.Features.Users.Dtos;
using Kickstart.Application.Features.Roles.Dtos;
using Kickstart.Application.Features.Tenants.Dtos;
using Kickstart.Application.Features.Permissions.Dtos;
using Kickstart.Application.Features.Users.Queries.GetAllUsers;
using Kickstart.Application.Features.Users.Queries.GetUserById;
using Kickstart.Application.Common.Authorization;
using Kickstart.Application.Interfaces;
using Kickstart.Domain.Common.Interfaces;
using Kickstart.Domain.Common.Interfaces.Repositories;
using Kickstart.Application.Common.Results;
using Kickstart.Domain.Common.Enums;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Kickstart.Domain.Models;

namespace Kickstart.Application.Features.Users.Queries.GetUserById
{
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserListDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public GetUserByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<Result<UserListDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.Users.GetUserWithRolesReadOnlyAsync(request.Id);

            if (user == null)
                return Result<UserListDto>.Failure(Error.Failure(
                    ErrorCode.NotFound,
                    "User not found"));

            // Admin/User can only view users from their own tenant; SuperAdmin can view any user
            if (!_currentUserService.CanAccessAllTenants && user.TenantId != _currentUserService.TenantId)
                return Result<UserListDto>.Failure(Error.Failure(
                    ErrorCode.Forbidden,
                    "You do not have access to this user"));

            if (TenantAdminVisibility.IsHiddenFromTenantAdmin(_currentUserService, user))
                return Result<UserListDto>.Failure(Error.Failure(
                    ErrorCode.Forbidden,
                    "You do not have access to this user"));

            var userDto = _mapper.Map<UserListDto>(user);
            return Result<UserListDto>.Success(userDto);
        }
    }
} 
