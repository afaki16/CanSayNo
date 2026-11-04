using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CanSayNo.Application.Common.Localization;
using CanSayNo.Application.Common.Results;
using CanSayNo.Application.Common.Settings;
using CanSayNo.Application.Features.Sessions.Common;
using CanSayNo.Application.Interfaces;
using CanSayNo.Domain.Common.Enums;
using CanSayNo.Domain.Common.Interfaces;
using CanSayNo.Domain.Entities.Hayirci;
using CanSayNo.Domain.Enums;
using CanSayNo.Domain.Models;
using MediatR;

namespace CanSayNo.Application.Features.Sessions.Commands.StartSession;

public class StartSessionCommandHandler : IRequestHandler<StartSessionCommand, Result<SessionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly IJsonStringLocalizer _localizer;
    private readonly HayirciSettings _settings;

    public StartSessionCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUserService,
        IJsonStringLocalizer localizer,
        HayirciSettings settings)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _localizer = localizer;
        _settings = settings;
    }

    public async Task<Result<SessionDto>> Handle(
        StartSessionCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is not int userId)
            return Result<SessionDto>.Failure(
                Error.Failure(ErrorCode.Unauthorized, _localizer["auth.userNotAuthenticated"]));

        var scenario = await _unitOfWork.Scenarios.GetActiveByIdAsync(request.ScenarioId, cancellationToken);
        if (scenario is null)
            return Result<SessionDto>.Failure(
                Error.Failure(ErrorCode.NotFound, _localizer["sessions.scenarioNotFound"]));

        // Premium kontrolü yeniden kullanımdan ÖNCE: aboneliği biten kullanıcı, daha önce
        // başlatıp boş bıraktığı premium oturuma da giremesin.
        var utcNow = DateTime.UtcNow;
        var isPremium = await _unitOfWork.Subscriptions.HasActiveSubscriptionAsync(
            userId, _settings.AppId, utcNow, cancellationToken);

        if (!scenario.IsFree && !isPremium)
            return Result<SessionDto>.Failure(
                Error.Failure(ErrorCode.PremiumRequired, _localizer["sessions.premiumRequired"]));

        // Pratik ekranı yeniden açıldıysa ve henüz mesaj atılmadıysa aynı oturuma devam:
        // yeni oturum açıp günlük hakkı ikinci kez yakmayız.
        var unstarted = await _unitOfWork.Sessions.GetUnstartedActiveSessionAsync(
            userId, scenario.Id, cancellationToken);
        if (unstarted is not null)
            return Result<SessionDto>.Success(_mapper.Map<SessionDto>(unstarted));

        // Kullanım herkes için sayılır (analitik); limit yalnızca premium olmayana uygulanır.
        var usageDate = _settings.GetUsageDate(utcNow);
        var usage = await _unitOfWork.DailyUsage.GetAsync(userId, usageDate, cancellationToken);

        if (!isPremium && usage is not null && usage.SessionsCount >= _settings.FreeDailySessionLimit)
            return Result<SessionDto>.Failure(
                Error.Failure(ErrorCode.DailyLimitReached, _localizer["sessions.dailyLimitReached"]));

        if (usage is null)
        {
            await _unitOfWork.DailyUsage.AddAsync(
                new DailyUsage { UserId = userId, Date = usageDate, SessionsCount = 1 },
                cancellationToken);
        }
        else
        {
            usage.SessionsCount++;
        }

        var session = new Session
        {
            UserId = userId,
            ScenarioId = scenario.Id,
            Status = SessionStatus.Active,
            CurrentRound = 0,
            StartedAt = utcNow
        };

        // Karakter sohbeti sabit açılış mesajıyla başlatır; LLM ilk kullanıcı cevabından sonra devreye girer.
        if (!string.IsNullOrWhiteSpace(scenario.Scene.OpeningMessage))
        {
            session.Messages.Add(new SessionMessage
            {
                Role = SessionMessageRoles.Ai,
                Content = scenario.Scene.OpeningMessage,
                Timestamp = utcNow
            });
        }

        await _unitOfWork.Sessions.AddAsync(session);

        var stats = await _unitOfWork.UserStats.GetAsync(userId, cancellationToken);
        if (stats is null)
            await _unitOfWork.UserStats.AddAsync(new UserStats { UserId = userId, TotalSessions = 1 }, cancellationToken);
        else
            stats.TotalSessions++;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // scenario AsNoTracking yüklendi; navigation'a bağlamak EF'in onu yeni kayıt sanmasına
        // yol açabileceği için senaryo alanları DTO'ya elle yazılır.
        var dto = _mapper.Map<SessionDto>(session) with
        {
            ScenarioTitle = scenario.Title,
            ScenarioContextId = scenario.ContextId,
            MaxRounds = scenario.PressureTactics.MaxRounds
        };

        return Result<SessionDto>.Success(dto);
    }
}
