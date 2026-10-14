using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CanSayNo.Application.Common.Results;
using CanSayNo.Application.Features.Scenarios.Common;
using CanSayNo.Application.Features.Scenarios.Queries.GetScenarios;
using MediatR;

namespace CanSayNo.Application.Features.Scenarios.Queries.GetScenariosByContext;

public class GetScenariosByContextQueryHandler
    : IRequestHandler<GetScenariosByContextQuery, Result<List<ScenarioListItemDto>>>
{
    private readonly IMediator _mediator;

    public GetScenariosByContextQueryHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public Task<Result<List<ScenarioListItemDto>>> Handle(
        GetScenariosByContextQuery request, CancellationToken cancellationToken)
    {
        // Tek bağlam = GetScenariosQuery'nin ContextId filtresi. Mantığı tek yerde tut.
        return _mediator.Send(
            new GetScenariosQuery { ContextId = request.ContextId },
            cancellationToken);
    }
}
