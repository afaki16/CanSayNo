using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CanSayNo.Application.Common.Results;
using CanSayNo.Application.Features.Scenarios.Common;
using CanSayNo.Application.Interfaces;
using CanSayNo.Domain.Common.Interfaces;
using CanSayNo.Domain.Models;
using MediatR;

namespace CanSayNo.Application.Features.Scenarios.Queries.GetScenarios;

public class GetScenariosQueryHandler
    : IRequestHandler<GetScenariosQuery, Result<List<ScenarioListItemDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public GetScenariosQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<ScenarioListItemDto>>> Handle(
        GetScenariosQuery request, CancellationToken cancellationToken)
    {
        // 1) Aktif senaryoları filtrelerle, ContextId + SortOrder sırasında al.
        var scenarios = await _unitOfWork.Scenarios.GetActiveScenariosAsync(
            request.ContextId, request.IsFreeOnly, cancellationToken);

        // 2) User-spesifik agregaları TEK sorguda al (N+1 yok). Sadece bu senaryolarla sınırla.
        var userId = _currentUserService.UserId;
        IReadOnlyDictionary<string, ScenarioSessionAggregate> aggregates =
            userId.HasValue
                ? await _unitOfWork.Sessions.GetUserScenarioAggregatesAsync(
                    userId.Value, scenarios.Select(s => s.Id), cancellationToken)
                : new Dictionary<string, ScenarioSessionAggregate>();

        // 3) Map + user-spesifik alanları doldur.
        var dtos = scenarios.Select(s =>
        {
            var dto = _mapper.Map<ScenarioListItemDto>(s);
            if (aggregates.TryGetValue(s.Id, out var agg))
            {
                dto = dto with
                {
                    IsCompleted = agg.IsCompleted,
                    BestScore = agg.BestScore,
                    AttemptsCount = agg.AttemptsCount
                };
            }
            return dto;
        }).ToList();

        return Result<List<ScenarioListItemDto>>.Success(dtos);
    }
}
