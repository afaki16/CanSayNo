using System.Collections.Generic;
using CanSayNo.Application.Common.Results;
using CanSayNo.Application.Features.Scenarios.Common;
using CanSayNo.Domain.Enums;
using MediatR;

namespace CanSayNo.Application.Features.Scenarios.Queries.GetScenarios;

public record GetScenariosQuery : IRequest<Result<List<ScenarioListItemDto>>>
{
    /// <summary>Opsiyonel bağlam filtresi (örn. yalnızca İş senaryoları).</summary>
    public ContextId? ContextId { get; init; }

    /// <summary>true ise yalnızca ücretsiz senaryolar döner.</summary>
    public bool? IsFreeOnly { get; init; }
}
