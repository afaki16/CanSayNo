using CanSayNo.Application.Common.Results;
using CanSayNo.Application.Features.Tenants.Dtos;
using MediatR;

namespace CanSayNo.Application.Features.Tenants.Commands.UpdateTenant;

    public class UpdateTenantCommand : IRequest<Result<TenantListDto>>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Domain { get; set; }
    public bool IsActive { get; set; }
    public string ContactEmail { get; set; }
    public string ContactPhone { get; set; }
}
