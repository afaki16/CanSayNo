using CanSayNo.Application.Common.Results;
using CanSayNo.Application.Interfaces;
using CanSayNo.Domain.Common.Enums;
using CanSayNo.Domain.Models;
using MediatR;

namespace CanSayNo.Application.Features.Admin.Hayirci;

// ───────────── Senaryolar ─────────────

public record GetAdminScenariosQuery : IRequest<Result<IReadOnlyList<AdminScenarioDto>>>;

public class GetAdminScenariosQueryHandler
    : IRequestHandler<GetAdminScenariosQuery, Result<IReadOnlyList<AdminScenarioDto>>>
{
    private readonly IAdminHayirciReadService _read;

    public GetAdminScenariosQueryHandler(IAdminHayirciReadService read) => _read = read;

    public async Task<Result<IReadOnlyList<AdminScenarioDto>>> Handle(
        GetAdminScenariosQuery request, CancellationToken cancellationToken)
    {
        var scenarios = await _read.GetAllScenariosAsync(cancellationToken);
        var usage = await _read.GetScenarioUsageAsync(cancellationToken);
        IReadOnlyList<AdminScenarioDto> dtos = scenarios
            .Select(s => AdminScenarioDto.From(s, usage.TryGetValue(s.Id, out var u) ? u : AdminScenarioUsageDto.Empty))
            .ToList();
        return Result<IReadOnlyList<AdminScenarioDto>>.Success(dtos);
    }
}

public record GetAdminScenarioByIdQuery(string Id) : IRequest<Result<AdminScenarioDto>>;

public class GetAdminScenarioByIdQueryHandler : IRequestHandler<GetAdminScenarioByIdQuery, Result<AdminScenarioDto>>
{
    private readonly IAdminHayirciReadService _read;

    public GetAdminScenarioByIdQueryHandler(IAdminHayirciReadService read) => _read = read;

    public async Task<Result<AdminScenarioDto>> Handle(GetAdminScenarioByIdQuery request, CancellationToken cancellationToken)
    {
        var scenario = (await _read.GetAllScenariosAsync(cancellationToken)).FirstOrDefault(s => s.Id == request.Id);
        if (scenario is null)
            return Result<AdminScenarioDto>.Failure(Error.Failure(ErrorCode.NotFound, "Senaryo bulunamadı."));

        var usage = await _read.GetScenarioUsageAsync(cancellationToken);
        return Result<AdminScenarioDto>.Success(
            AdminScenarioDto.From(scenario, usage.TryGetValue(scenario.Id, out var u) ? u : AdminScenarioUsageDto.Empty));
    }
}

// ───────────── Kullanıcı özeti ─────────────

public record GetAdminUserOverviewQuery(int UserId) : IRequest<Result<AdminUserOverviewDto>>;

public class GetAdminUserOverviewQueryHandler : IRequestHandler<GetAdminUserOverviewQuery, Result<AdminUserOverviewDto>>
{
    private readonly IAdminHayirciReadService _read;

    public GetAdminUserOverviewQueryHandler(IAdminHayirciReadService read) => _read = read;

    public async Task<Result<AdminUserOverviewDto>> Handle(GetAdminUserOverviewQuery request, CancellationToken cancellationToken)
    {
        var overview = await _read.GetUserOverviewAsync(request.UserId, DateTime.UtcNow, cancellationToken);
        return overview is null
            ? Result<AdminUserOverviewDto>.Failure(Error.Failure(ErrorCode.NotFound, "Kullanıcı bulunamadı."))
            : Result<AdminUserOverviewDto>.Success(overview);
    }
}

// ───────────── Gösterge paneli ─────────────

public record GetAdminDashboardQuery : IRequest<Result<AdminDashboardDto>>;

public class GetAdminDashboardQueryHandler : IRequestHandler<GetAdminDashboardQuery, Result<AdminDashboardDto>>
{
    private readonly IAdminHayirciReadService _read;

    public GetAdminDashboardQueryHandler(IAdminHayirciReadService read) => _read = read;

    public async Task<Result<AdminDashboardDto>> Handle(GetAdminDashboardQuery request, CancellationToken cancellationToken) =>
        Result<AdminDashboardDto>.Success(await _read.GetDashboardAsync(DateTime.UtcNow, cancellationToken));
}
