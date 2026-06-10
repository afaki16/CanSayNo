using AutoMapper;
using CanSayNo.Application.Features.Users.Dtos;
using CanSayNo.Application.Features.Roles.Dtos;
using CanSayNo.Application.Features.Tenants.Dtos;
using CanSayNo.Application.Features.Permissions.Dtos;
using CanSayNo.Application.Features.Users.Queries.GetAllUsers;
using CanSayNo.Application.Features.Users.Queries.GetUserById;
using CanSayNo.Application.Interfaces;
using CanSayNo.Domain.Common.Interfaces;
using CanSayNo.Domain.Common.Interfaces.Repositories;
using CanSayNo.Application.Common.Results;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CanSayNo.Application.Features.Users.Queries.GetAllUsers
{
    public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, PagedResult<UserListDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public GetAllUsersQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<PagedResult<UserListDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            // SuperAdmin sees all tenants' users, Admin/User see only their tenant's users
            int? tenantId = _currentUserService.CanAccessAllTenants ? null : _currentUserService.TenantId;
            var excludeSuperAdmins = !_currentUserService.CanAccessAllTenants;

            var (users, totalCount) = await _unitOfWork.Users.GetUsersPagedAsync(request.Page, request.PageSize, request.SearchTerm, tenantId, excludeSuperAdmins);
            var userDtos = _mapper.Map<IEnumerable<UserListDto>>(users);
            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
            return PagedResult<UserListDto>.Success(userDtos, request.Page, totalPages, totalCount);
        }
    }
} 
