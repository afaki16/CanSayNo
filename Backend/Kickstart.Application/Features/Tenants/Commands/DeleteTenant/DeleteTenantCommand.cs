using Kickstart.Application.Common.Results;
using MediatR;

namespace Kickstart.Application.Features.Tenants.Commands.DeleteTenant;

    public class DeleteTenantCommand : IRequest<Result<bool>>
{
    public int Id { get; set; }
}
