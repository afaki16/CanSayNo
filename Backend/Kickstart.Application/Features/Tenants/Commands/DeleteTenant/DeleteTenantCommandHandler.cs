using Kickstart.Application.Features.Tenants.Commands.CreateTenant;
using Kickstart.Application.Features.Tenants.Commands.UpdateTenant;
using Kickstart.Application.Features.Tenants.Commands.DeleteTenant;
using Kickstart.Application.Common.Localization;
using Kickstart.Domain.Common.Interfaces;
using Kickstart.Domain.Common.Interfaces.Repositories;
using Kickstart.Application.Common.Results;
using Kickstart.Domain.Models;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Kickstart.Domain.Common.Enums;

namespace Kickstart.Application.Features.Tenants.Commands.DeleteTenant;

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
