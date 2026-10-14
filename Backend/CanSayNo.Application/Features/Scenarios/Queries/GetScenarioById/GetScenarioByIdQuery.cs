using CanSayNo.Application.Common.Results;
using CanSayNo.Application.Features.Scenarios.Common;
using MediatR;

namespace CanSayNo.Application.Features.Scenarios.Queries.GetScenarioById;

public record GetScenarioByIdQuery(string Id) : IRequest<Result<ScenarioDetailDto>>;
