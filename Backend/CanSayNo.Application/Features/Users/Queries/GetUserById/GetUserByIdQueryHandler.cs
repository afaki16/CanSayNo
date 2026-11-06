using AutoMapper;
using CanSayNo.Application.Features.Users.Dtos;
using CanSayNo.Application.Features.Roles.Dtos;
using CanSayNo.Application.Features.Tenants.Dtos;
using CanSayNo.Application.Features.Permissions.Dtos;
using CanSayNo.Application.Features.Users.Queries.GetAllUsers;
using CanSayNo.Application.Features.Users.Queries.GetUserById;
using CanSayNo.Application.Common.Authorization;
using CanSayNo.Application.Interfaces;
using CanSayNo.Domain.Common.Interfaces;
using CanSayNo.Domain.Common.Interfaces.Repositories;
using CanSayNo.Application.Common.Results;
using CanSayNo.Domain.Common.Enums;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using CanSayNo.Domain.Models;

namespace CanSayNo.Application.Features.Users.Queries.GetUserById
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
            if (!TenantAdminVisibility.CanAccessUserTenant(_currentUserService, user))
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
