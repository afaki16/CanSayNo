using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CanSayNo.Application.Common.Localization;
using CanSayNo.Application.Common.Results;
using CanSayNo.Application.Common.Settings;
using CanSayNo.Application.Features.Sessions.Common;
using CanSayNo.Application.Interfaces;
using CanSayNo.Domain.Common.Enums;
using CanSayNo.Domain.Common.Interfaces;
using CanSayNo.Domain.Enums;
using CanSayNo.Domain.Models;
using MediatR;

namespace CanSayNo.Application.Features.Stats.Queries.GetMyStats;

/// <summary>Anasayfa ve Profil istatistikleri: seri, haftalık aktivite, XP/seviye, rozetler.</summary>
public record GetMyStatsQuery : IRequest<Result<UserStatsDto>>;

public record UserStatsDto
{
    /// <summary>Güncel seri. Son pratik bugün ya da dün değilse 0 (DB'deki değer bir sonraki pratikte sıfırlanır).</summary>
    public int CurrentStreak { get; init; }
    public int LongestStreak { get; init; }
    public bool PracticedToday { get; init; }

    /// <summary>Başlatılan pratik sayısı.</summary>
    public int TotalSessions { get; init; }
    /// <summary>Tamamlanıp değerlendirilen pratik sayısı.</summary>
    public int TotalCompletedSessions { get; init; }

    /// <summary>Bu haftanın (TR, Pazartesi-Pazar) günleri: o gün değerlendirilmiş pratik var mı. 7 eleman.</summary>
    public List<bool> WeekActivity { get; init; } = new();
    /// <summary>Bugünün WeekActivity içindeki indeksi (TR günü; Pazartesi = 0). Cihaz saatine güvenilmez.</summary>
    public int TodayIndex { get; init; }
    /// <summary>Bu hafta değerlendirilen pratik sayısı.</summary>
    public int WeeklyProgress { get; init; }
    public int WeeklyGoal { get; init; }

    public int Level { get; init; }
    public int TotalPoints { get; init; }
    /// <summary>Mevcut seviyede biriken XP (0 ≤ x &lt; XpPerLevel).</summary>
    public int XpInLevel { get; init; }
    public int XpPerLevel { get; init; }

    public List<Badge> UnlockedBadges { get; init; } = new();
}

public class GetMyStatsQueryHandler : IRequestHandler<GetMyStatsQuery, Result<UserStatsDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IJsonStringLocalizer _localizer;
    private readonly HayirciSettings _settings;

    public GetMyStatsQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IJsonStringLocalizer localizer,
        HayirciSettings settings)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _localizer = localizer;
        _settings = settings;
    }

    public async Task<Result<UserStatsDto>> Handle(GetMyStatsQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is not int userId)
            return Result<UserStatsDto>.Failure(
                Error.Failure(ErrorCode.Unauthorized, _localizer["auth.userNotAuthenticated"]));

        var utcNow = DateTime.UtcNow;
        var offset = TimeSpan.FromHours(_settings.UsageDayUtcOffsetHours);
        var today = _settings.GetUsageDate(utcNow);
        var monday = today.AddDays(-(((int)today.DayOfWeek + 6) % 7));
        var weekStartUtc = DateTime.SpecifyKind(monday.ToDateTime(TimeOnly.MinValue) - offset, DateTimeKind.Utc);

        var completions = await _unitOfWork.Sessions.GetEvaluatedCompletionTimesAsync(
            userId, weekStartUtc, cancellationToken);
        var practiceDays = completions
            .Select(c => DateOnly.FromDateTime(c + offset))
            .ToHashSet();
        var weekActivity = Enumerable.Range(0, 7)
            .Select(i => practiceDays.Contains(monday.AddDays(i)))
            .ToList();

        // Okuma: kayıt yoksa oluşturulmaz, sıfırlarla dönülür.
        var stats = await _unitOfWork.UserStats.GetAsync(userId, cancellationToken);
        var last = stats?.LastPracticeDate;
        var streakAlive = last == today || last == today.AddDays(-1);
        var totalPoints = stats?.TotalPoints ?? 0;

        return Result<UserStatsDto>.Success(new UserStatsDto
        {
            CurrentStreak = streakAlive ? stats!.CurrentStreak : 0,
            LongestStreak = stats?.LongestStreak ?? 0,
            PracticedToday = last == today,
            TotalSessions = stats?.TotalSessions ?? 0,
            TotalCompletedSessions = stats?.TotalCompletedSessions ?? 0,
            WeekActivity = weekActivity,
            TodayIndex = today.DayNumber - monday.DayNumber,
            WeeklyProgress = completions.Count,
            WeeklyGoal = _settings.WeeklyGoal,
            Level = stats?.Level ?? 1,
            TotalPoints = totalPoints,
            XpInLevel = totalPoints % UserProgress.XpPerLevel,
            XpPerLevel = UserProgress.XpPerLevel,
            UnlockedBadges = stats?.UnlockedBadges.ToList() ?? new List<Badge>()
        });
    }
}
