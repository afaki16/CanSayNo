using Kickstart.Application.Common.Results;
using Kickstart.Application.Features.Tenants.Dtos;
using MediatR;
namespace Kickstart.Application.Features.Tenants.Queries.GetTenantById
{
    public class GetTenantByIdQuery : IRequest<Result<TenantDto>>
    {
        public int Id { get; set; }
    }
}
