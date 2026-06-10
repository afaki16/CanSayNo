using CanSayNo.Application.Common.Results;
using CanSayNo.Application.Features.Tenants.Dtos;
using MediatR;
namespace CanSayNo.Application.Features.Tenants.Queries.GetTenantById
{
    public class GetTenantByIdQuery : IRequest<Result<TenantDto>>
    {
        public int Id { get; set; }
    }
}
