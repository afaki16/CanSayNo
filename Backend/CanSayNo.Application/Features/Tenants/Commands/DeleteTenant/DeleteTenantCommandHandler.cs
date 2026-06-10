using CanSayNo.Application.Features.Tenants.Commands.CreateTenant;
using CanSayNo.Application.Features.Tenants.Commands.UpdateTenant;
using CanSayNo.Application.Features.Tenants.Commands.DeleteTenant;
using CanSayNo.Application.Common.Localization;
using CanSayNo.Domain.Common.Interfaces;
using CanSayNo.Domain.Common.Interfaces.Repositories;
using CanSayNo.Application.Common.Results;
using CanSayNo.Domain.Models;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using CanSayNo.Domain.Common.Enums;

namespace CanSayNo.Application.Features.Tenants.Commands.DeleteTenant;

    public class DeleteTenantCommandHandler : IRequestHandler<DeleteTenantCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJsonStringLocalizer _localizer;

    public DeleteTenantCommandHandler(IUnitOfWork unitOfWork, IJsonStringLocalizer localizer)
    {
        _unitOfWork = unitOfWork;
        _localizer = localizer;
    }

    public async Task<Result<bool>> Handle(DeleteTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(request.Id);

        if (tenant == null)
        {
            return Result<bool>.Failure(Error.Failure(
                ErrorCode.NotFound,
                _localizer["tenants.notFound"]));
        }

        // Soft delete - User'ların TenantId'si null olur (DeleteBehavior.SetNull)
        _unitOfWork.Tenants.SoftDelete(tenant);
        await _unitOfWork.SaveChangesAsync();

        return Result<bool>.Success(true);
    }
}
