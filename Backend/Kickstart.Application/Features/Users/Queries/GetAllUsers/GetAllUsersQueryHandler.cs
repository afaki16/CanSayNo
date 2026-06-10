using AutoMapper;
using Kickstart.Application.Features.Users.Dtos;
using Kickstart.Application.Features.Roles.Dtos;
using Kickstart.Application.Features.Tenants.Dtos;
using Kickstart.Application.Features.Permissions.Dtos;
using Kickstart.Application.Features.Users.Queries.GetAllUsers;
using Kickstart.Application.Features.Users.Queries.GetUserById;
using Kickstart.Application.Interfaces;
using Kickstart.Domain.Common.Interfaces;
using Kickstart.Domain.Common.Interfaces.Repositories;
using Kickstart.Application.Common.Results;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Kickstart.Application.Features.Users.Queries.GetAllUsers
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
