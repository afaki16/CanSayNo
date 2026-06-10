using AutoMapper;
using Kickstart.Application.Features.Tenants.Commands.CreateTenant;
using Kickstart.Application.Features.Tenants.Commands.UpdateTenant;
using Kickstart.Application.Features.Tenants.Commands.DeleteTenant;
using Kickstart.Application.Features.Tenants.Dtos;
using Kickstart.Application.Common.Localization;
using Kickstart.Domain.Common.Interfaces;
using Kickstart.Domain.Common.Interfaces.Repositories;
using Kickstart.Application.Common.Results;
using Kickstart.Domain.Models;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Kickstart.Domain.Common.Enums;

namespace Kickstart.Application.Features.Tenants.Commands.UpdateTenant;

    public class UpdateTenantCommandHandler : IRequestHandler<UpdateTenantCommand, Result<TenantListDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IJsonStringLocalizer _localizer;

    public UpdateTenantCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IJsonStringLocalizer localizer)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _localizer = localizer;
    }

    public async Task<Result<TenantListDto>> Handle(UpdateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _unitOfWork.Tenants.GetTenantWithUsersAsync(request.Id);

        if (tenant == null)
        {
            return Result<TenantListDto>.Failure(Error.Failure(
                ErrorCode.NotFound,
                _localizer["tenants.notFound"]));
        }

        // İsim kontrolü (eğer değiştiyse)
        if (tenant.Name != request.Name && await _unitOfWork.Tenants.NameExistsAsync(request.Name))
        {
            return Result<TenantListDto>.Failure(Error.Failure(
                ErrorCode.AlreadyExists,
                _localizer["tenants.nameExists"]));
        }

        // Domain kontrolü (eğer değiştiyse)
        if (!string.IsNullOrEmpty(request.Domain) &&
            tenant.Domain != request.Domain &&
            await _unitOfWork.Tenants.DomainExistsAsync(request.Domain))
        {
            return Result<TenantListDto>.Failure(Error.Failure(
                ErrorCode.AlreadyExists,
                _localizer["tenants.domainExists"]));
        }

        tenant.Name = request.Name;
        tenant.Description = request.Description;
        tenant.Domain = request.Domain;
        tenant.IsActive = request.IsActive;
        tenant.ContactEmail = request.ContactEmail;
        tenant.ContactPhone = request.ContactPhone;

        _unitOfWork.Tenants.Update(tenant);
        await _unitOfWork.SaveChangesAsync();

        var tenantDto = _mapper.Map<TenantListDto>(tenant);
        tenantDto.UserCount = tenant.Users?.Count ?? 0;
        return Result<TenantListDto>.Success(tenantDto);
    }
}
