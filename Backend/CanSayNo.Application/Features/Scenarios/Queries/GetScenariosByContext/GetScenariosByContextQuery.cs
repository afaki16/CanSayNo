using System.Collections.Generic;
using CanSayNo.Application.Common.Results;
using CanSayNo.Application.Features.Scenarios.Common;
using CanSayNo.Domain.Enums;
using MediatR;

namespace CanSayNo.Application.Features.Scenarios.Queries.GetScenariosByContext;

/// <summary>GetScenariosQuery'nin tek bağlama özelleşmiş hali.</summary>
public record GetScenariosByContextQuery(ContextId ContextId)
    : IRequest<Result<List<ScenarioListItemDto>>>;
