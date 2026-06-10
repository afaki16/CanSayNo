using CanSayNo.Application.Common.Results;
using MediatR;

namespace CanSayNo.Application.Features.Tenants.Commands.DeleteTenant;

    public class DeleteTenantCommand : IRequest<Result<bool>>
{
    public int Id { get; set; }
}
