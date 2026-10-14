using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CanSayNo.Application.Common.Results;
using CanSayNo.Application.Features.Scenarios.Common;
using CanSayNo.Application.Interfaces;
using CanSayNo.Domain.Common.Enums;
using CanSayNo.Domain.Common.Interfaces;
using CanSayNo.Domain.Models;
using MediatR;

namespace CanSayNo.Application.Features.Scenarios.Queries.GetScenarioById;

public class GetScenarioByIdQueryHandler
    : IRequestHandler<GetScenarioByIdQuery, Result<ScenarioDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public GetScenarioByIdQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<Result<ScenarioDetailDto>> Handle(
        GetScenarioByIdQuery request, CancellationToken cancellationToken)
    {
        // Yalnızca aktif senaryo döner; pasif/yok ise NotFound (kullanıcıya kapalı içerik göstermeyiz).
        // Premium içerik premium olmayan kullanıcıya da DÖNER — istemci paywall'a yönlendirir.
        var scenario = await _unitOfWork.Scenarios.GetActiveByIdAsync(request.Id, cancellationToken);

        if (scenario is null)
            return Result<ScenarioDetailDto>.Failure(
                Error.Failure(ErrorCode.NotFound, "Senaryo bulunamadı."));

        var dto = _mapper.Map<ScenarioDetailDto>(scenario);

        // User-spesifik alanlar: bu tek senaryo için Sessions agregası.
        var userId = _currentUserService.UserId;
        if (userId.HasValue)
        {
            var aggregates = await _unitOfWork.Sessions.GetUserScenarioAggregatesAsync(
                userId.Value, new[] { scenario.Id }, cancellationToken);

            if (aggregates.TryGetValue(scenario.Id, out var agg))
            {
                dto = dto with
                {
                    IsCompleted = agg.IsCompleted,
                    BestScore = agg.BestScore,
                    AttemptsCount = agg.AttemptsCount,
                    LastAttemptAt = agg.LastAttemptAt
                };
            }
        }

        return Result<ScenarioDetailDto>.Success(dto);
    }
}
