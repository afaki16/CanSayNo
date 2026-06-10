using CanSayNo.Application.Common.Results;
using CanSayNo.Application.Features.Tenants.Dtos;
using MediatR;

namespace CanSayNo.Application.Features.Tenants.Commands.CreateTenant
{
    public class CreateTenantCommand : IRequest<Result<TenantListDto>>
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Domain { get; set; }
    public bool IsActive { get; set; } = true;
    public string ContactEmail { get; set; }
    public string ContactPhone { get; set; }
}
}