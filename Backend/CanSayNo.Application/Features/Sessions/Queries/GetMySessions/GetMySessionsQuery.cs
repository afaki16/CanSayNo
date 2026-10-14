using CanSayNo.Application.Common.Results;
using CanSayNo.Application.Features.Sessions.Common;
using MediatR;

namespace CanSayNo.Application.Features.Sessions.Queries.GetMySessions;

public record GetMySessionsQuery : IRequest<PagedResult<SessionListItemDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
