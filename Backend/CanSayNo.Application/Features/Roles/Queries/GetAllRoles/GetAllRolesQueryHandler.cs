using AutoMapper;
using CanSayNo.Application.Features.Roles.Queries.GetAllRoles;
using CanSayNo.Application.Features.Roles.Queries.GetRoleById;
using CanSayNo.Application.Interfaces;
using CanSayNo.Domain.Common.Interfaces;
using CanSayNo.Domain.Common.Interfaces.Repositories;
using CanSayNo.Application.Common.Results;
using CanSayNo.Application.Features.Users.Dtos;
using CanSayNo.Application.Features.Roles.Dtos;
using CanSayNo.Application.Features.Tenants.Dtos;
using CanSayNo.Application.Features.Permissions.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CanSayNo.Application.Features.Roles.Queries.GetAllRoles
{
    public class GetAllRolesQueryHandler : IRequestHandler<GetAllRolesQuery, PagedResult<RoleDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAllRolesQueryHandler> _logger;
        private readonly ICurrentUserService _currentUserService;

        public GetAllRolesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetAllRolesQueryHandler> logger, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<PagedResult<RoleDto>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting roles with permissions (paged)...");

                var hideSuperAdmin = !_currentUserService.CanAccessAllTenants;
                var (roles, totalCount) = await _unitOfWork.Roles.GetRolesPagedAsync(request.Page, request.PageSize, request.SearchTerm, hideSuperAdmin);
                var roleDtos = _mapper.Map<IEnumerable<RoleDto>>(roles);
                var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

                _logger.LogInformation("Found {TotalCount} roles total, returning page {Page}", totalCount, request.Page);

                return PagedResult<RoleDto>.Success(roleDtos, request.Page, totalPages, totalCount);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all roles");
                throw;
            }
        }
    }
} 
