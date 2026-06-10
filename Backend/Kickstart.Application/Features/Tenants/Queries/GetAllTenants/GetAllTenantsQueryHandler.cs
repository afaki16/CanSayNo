using AutoMapper;
using Kickstart.Domain.Common.Interfaces;
using Kickstart.Domain.Common.Interfaces.Repositories;
using Kickstart.Application.Common.Results;
using Kickstart.Application.Features.Tenants.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kickstart.Domain.Entities;
namespace Kickstart.Application.Features.Tenants.Queries.GetAllTenants
{
    public class GetAllTenantsQueryHandler : IRequestHandler<GetAllTenantsQuery, Result<IEnumerable<TenantListDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetAllTenantsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<TenantListDto>>> Handle(GetAllTenantsQuery request, CancellationToken cancellationToken)
        {
            IQueryable<Tenant> query = _unitOfWork.Tenants.GetQueryable();

            // Arama filtresi
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(t => 
                    t.Name.ToLower().Contains(searchTerm) ||
                    t.Description != null && t.Description.ToLower().Contains(searchTerm) ||
                    t.Domain != null && t.Domain.ToLower().Contains(searchTerm) ||
                    t.ContactEmail != null && t.ContactEmail.ToLower().Contains(searchTerm));
            }

            // Sayfalama ve Include
            var tenants = await query
                .Include(t => t.Users)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var tenantDtos = tenants.Select(t => new TenantListDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Domain = t.Domain,
                IsActive = t.IsActive,
                ContactEmail = t.ContactEmail,
                CreatedDate = t.CreatedDate,
                UserCount = t.Users?.Count ?? 0
            });

            return Result<IEnumerable<TenantListDto>>.Success(tenantDtos);
        }
    }
}
