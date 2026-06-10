using AutoMapper;
using Kickstart.Application.Features.Tenants.Commands.CreateTenant;
using Kickstart.Application.Features.Tenants.Commands.UpdateTenant;
using Kickstart.Application.Features.Tenants.Commands.DeleteTenant;
using Kickstart.Application.Features.Tenants.Dtos;
using Kickstart.Application.Common.Localization;
using Kickstart.Domain.Common.Interfaces;
using Kickstart.Domain.Common.Interfaces.Repositories;
using Kickstart.Application.Common.Results;
using Kickstart.Domain.Entities;
using Kickstart.Domain.Models;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Kickstart.Domain.Common.Enums;

namespace Kickstart.Application.Features.Tenants.Commands.CreateTenant;

    public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, Result<TenantListDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IJsonStringLocalizer _localizer;

    public CreateTenantCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IJsonStringLocalizer localizer)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _localizer = localizer;
    }

    public async Task<Result<TenantListDto>> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        // İsim kontrolü
        if (await _unitOfWork.Tenants.NameExistsAsync(request.Name))
        {
            return Result<TenantListDto>.Failure(Error.Failure(
                ErrorCode.AlreadyExists,
                _localizer["tenants.nameExists"]));
        }

        // Domain kontrolü (eğer verilmişse)
        if (!string.IsNullOrEmpty(request.Domain) && await _unitOfWork.Tenants.DomainExistsAsync(request.Domain))
        {
            return Result<TenantListDto>.Failure(Error.Failure(
                ErrorCode.AlreadyExists,
                _localizer["tenants.domainExists"]));
        }

        // Entity oluştur
        var tenant = new Tenant
        {
            Name = request.Name,
            Description = request.Description,
            Domain = request.Domain,
            IsActive = request.IsActive,
            ContactEmail = request.ContactEmail,
            ContactPhone = request.ContactPhone
        };

        await _unitOfWork.Tenants.AddAsync(tenant);
        await _unitOfWork.SaveChangesAsync();

        var tenantDto = _mapper.Map<TenantListDto>(tenant);
        tenantDto.UserCount = 0; // Yeni tenant'ta henüz user yok
        return Result<TenantListDto>.Success(tenantDto);
    }
}
