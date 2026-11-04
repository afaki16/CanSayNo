using System;
using System.Collections.Generic;
using System.Linq;
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
using Microsoft.Extensions.Logging;

namespace CanSayNo.Application.Features.Sessions.Commands.EvaluateSession;

public class EvaluateSessionCommandHandler : IRequestHandler<EvaluateSessionCommand, Result<SessionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly IJsonStringLocalizer _localizer;
    private readonly HayirciSettings _settings;
    private readonly ISessionEvaluator _evaluator;
    private readonly ISessionLock _sessionLock;
    private readonly ILogger<EvaluateSessionCommandHandler> _logger;

    public EvaluateSessionCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUserService,
        IJsonStringLocalizer localizer,
        HayirciSettings settings,
        ISessionEvaluator evaluator,
        ISessionLock sessionLock,
        ILogger<EvaluateSessionCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _localizer = localizer;
        _settings = settings;
        _evaluator = evaluator;
        _sessionLock = sessionLock;
        _logger = logger;
    }

    private static readonly TimeSpan EvaluationLockWait = TimeSpan.FromSeconds(85);

    public async Task<Result<SessionDto>> Handle(EvaluateSessionCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is not int userId)
            return Fail(ErrorCode.Unauthorized, "auth.userNotAuthenticated");

        // Sonuç ekranı aynı anda iki kez isterse ikincisi birincinin bitmesini bekler, sonra
        // kayıtlı değerlendirmeyi döner: LLM bir kez çağrılır, XP bir kez yazılır.
        // Bekleme mobilin 90 sn'lik istek zaman aşımının altında tutulur.
        using var sessionLock = await _sessionLock.TryAcquireAsync(
            request.SessionId, EvaluationLockWait, cancellationToken);
        if (sessionLock is null)
            return Fail(ErrorCode.SessionBusy, "sessions.busy");

        var session = await _unitOfWork.Sessions.GetUserSessionAsync(
            request.SessionId, userId, asNoTracking: false, cancellationToken);
        if (session is null)
            return Fail(ErrorCode.NotFound, "sessions.notFound");

        // İdempotent: sonuç ekranı yeniden açıldığında tekrar LLM çağrılmaz, XP ikinci kez yazılmaz.
        if (session.Evaluation is not null)
            return Result<SessionDto>.Success(_mapper.Map<SessionDto>(session));

        if (session.Status != SessionStatus.Completed)
            return Fail(ErrorCode.SessionNotCompleted, "sessions.notCompleted");

        var prompt = EvaluationPromptBuilder.Build(session.Scenario, session.Messages);
        var reply = await _evaluator.EvaluateAsync(prompt, cancellationToken);
        session.TokensUsed += reply.TokensUsed;

        if (reply.Outcome != AiCallOutcome.Success || reply.Evaluation is null)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogWarning("Session evaluation failed. SessionId: {SessionId}, Outcome: {Outcome}",
                session.Id, reply.Outcome);
            return Fail(ErrorCode.AiUnavailable, "sessions.evaluationUnavailable");
        }

        var evaluation = ToEvaluation(reply.Evaluation);
        session.Evaluation = evaluation;
        session.TotalScore = evaluation.TotalScore;

        var utcNow = DateTime.UtcNow;
        var stats = await _unitOfWork.UserStats.GetAsync(userId, cancellationToken);
        if (stats is null)
        {
            // Bu özellikten önce başlatılmış oturumlar için kayıt olmayabilir.
            stats = new UserStats { UserId = userId, TotalSessions = 1 };
            await _unitOfWork.UserStats.AddAsync(stats, cancellationToken);
        }

        // Bu oturum henüz kaydedilmediği için DB sayımına elle eklenir.
        var evaluatedByContext = new Dictionary<ContextId, int>(
            await _unitOfWork.Sessions.GetEvaluatedCountsByContextAsync(userId, cancellationToken));
        var contextId = session.Scenario.ContextId;
        evaluatedByContext[contextId] = evaluatedByContext.GetValueOrDefault(contextId) + 1;

        var progress = UserProgress.ApplyCompletedSession(
            stats,
            evaluation.TotalScore,
            _settings.GetUsageDate(utcNow),
            utcNow.AddHours(_settings.UsageDayUtcOffsetHours).Hour,
            evaluatedByContext);

        evaluation.XpEarned = progress.XpEarned;
        evaluation.StreakAfter = stats.CurrentStreak;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Session evaluated. SessionId: {SessionId}, TotalScore: {TotalScore}, XP: {Xp}, NewBadges: {Badges}",
            session.Id, evaluation.TotalScore, progress.XpEarned, string.Join(",", progress.NewBadges));

        return Result<SessionDto>.Success(_mapper.Map<SessionDto>(session));
    }

    /// <summary>
    /// Ham çıktıyı doğrular: bilinmeyen eksenler atılır, eksik eksen 0 sayılır, puanlar 0-100'e
    /// sıkıştırılır (şema sayısal sınır desteklemiyor). Toplam puan eksenlerin ortalamasıdır.
    /// </summary>
    private static SessionEvaluation ToEvaluation(RawEvaluation raw)
    {
        var scores = EvaluationPromptBuilder.CriterionKeys.ToDictionary(
            kv => kv.Value,
            kv => Math.Clamp(raw.Scores.TryGetValue(kv.Key, out var v) ? v : 0, 0, 100));

        static List<string> Clean(IEnumerable<string> items) =>
            items.Select(s => s.Trim()).Where(s => s.Length > 0).Take(3).ToList();

        return new SessionEvaluation
        {
            Scores = scores,
            Strengths = Clean(raw.Strengths),
            Improvements = Clean(raw.Improvements),
            CoachNote = raw.CoachNote.Trim(),
            IdealResponseExample = string.IsNullOrWhiteSpace(raw.IdealResponseExample)
                ? null
                : raw.IdealResponseExample.Trim(),
            TotalScore = (int)Math.Round(scores.Values.Average(), MidpointRounding.AwayFromZero)
        };
    }

    private Result<SessionDto> Fail(ErrorCode code, string messageKey) =>
        Result<SessionDto>.Failure(Error.Failure(code, _localizer[messageKey]));
}
