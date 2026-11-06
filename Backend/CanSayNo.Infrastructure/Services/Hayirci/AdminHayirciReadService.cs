using CanSayNo.Application.Common.Settings;
using CanSayNo.Application.Features.Admin.Hayirci;
using CanSayNo.Application.Interfaces;
using CanSayNo.Domain.Entities.Hayirci;
using CanSayNo.Domain.Enums;
using CanSayNo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CanSayNo.Infrastructure.Services.Hayirci;

/// <summary>
/// Admin raporları. Gün sınırları Hayirci:UsageDayUtcOffsetHours'a göre (TR saati).
/// "Uygulama kullanıcısı" = TenantId null (mobil kayıt); seed/admin hesapları (tenant 1) sayılmaz.
///
/// Gösterge sorguları son 30 günün oturumlarını tek projeksiyonla belleğe alıp gruplar —
/// v1 ölçeği için yeterli; hacim büyürse günlük özet tablosuna geçilmeli.
/// </summary>
public class AdminHayirciReadService : IAdminHayirciReadService
{
    private const int DailySeriesDays = 14;
    private const int CostWindowDays = 30;
    private const int RankCount = 5;
    private const int MinSessionsForLowestRank = 3;

    private readonly ApplicationDbContext _context;
    private readonly HayirciSettings _settings;

    public AdminHayirciReadService(ApplicationDbContext context, IOptions<HayirciSettings> settings)
    {
        _context = context;
        _settings = settings.Value;
    }

    public async Task<IReadOnlyList<Scenario>> GetAllScenariosAsync(CancellationToken cancellationToken = default) =>
        await _context.Scenarios
            .AsNoTracking()
            .OrderBy(s => s.ContextId)
            .ThenBy(s => s.SortOrder)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyDictionary<string, AdminScenarioUsageDto>> GetScenarioUsageAsync(
        CancellationToken cancellationToken = default)
    {
        var rows = await _context.Sessions
            .AsNoTracking()
            .GroupBy(s => s.ScenarioId)
            .Select(g => new
            {
                ScenarioId = g.Key,
                Sessions = g.Count(),
                Evaluated = g.Count(s => s.TotalScore != null),
                AverageScore = g.Average(s => (double?)s.TotalScore),
                LastPlayedAt = g.Max(s => (DateTime?)s.StartedAt)
            })
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(
            r => r.ScenarioId,
            r => new AdminScenarioUsageDto(r.Sessions, r.Evaluated, Round(r.AverageScore), r.LastPlayedAt));
    }

    public async Task<AdminUserOverviewDto?> GetUserOverviewAsync(
        int userId, DateTime utcNow, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new { u.Id, u.Email, u.FirstName, u.LastName, u.CreatedDate, u.LastLoginDate })
            .FirstOrDefaultAsync(cancellationToken);
        if (user is null)
            return null;

        var stats = await _context.UserStats.AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);

        var subscription = await _context.Subscriptions.AsNoTracking()
            .Where(s => s.UserId == userId && s.AppId == _settings.AppId)
            .OrderByDescending(s => s.ExpiresAt == null)
            .ThenByDescending(s => s.ExpiresAt)
            .FirstOrDefaultAsync(cancellationToken);

        var isPremium = await _context.Subscriptions.AsNoTracking()
            .AnyAsync(IsCurrentPremium(_settings.AppId, utcNow, userId), cancellationToken);

        var sessionTotals = await _context.Sessions.AsNoTracking()
            .Where(s => s.UserId == userId)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Evaluated = g.Count(s => s.TotalScore != null),
                AverageScore = g.Average(s => (double?)s.TotalScore),
                Tokens = g.Sum(s => (long)s.TokensUsed),
                LastPracticeAt = g.Max(s => (DateTime?)s.StartedAt)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var recent = await _context.Sessions.AsNoTracking()
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.StartedAt)
            .Take(10)
            .Select(s => new AdminUserSessionDto(
                s.Id, s.ScenarioId, s.Scenario.Title, s.Scenario.ContextId, s.Status,
                s.TotalScore, s.TokensUsed, s.StartedAt, s.CompletedAt))
            .ToListAsync(cancellationToken);

        return new AdminUserOverviewDto(
            user.Id,
            user.Email,
            $"{user.FirstName} {user.LastName}".Trim(),
            user.CreatedDate,
            user.LastLoginDate,
            sessionTotals?.LastPracticeAt,
            isPremium,
            subscription is null
                ? null
                : new AdminUserSubscriptionDto(subscription.ProductId, subscription.Status, subscription.ExpiresAt,
                    subscription.StartedAt, subscription.CancelledAt),
            stats?.Level ?? 1,
            stats?.TotalPoints ?? 0,
            stats?.CurrentStreak ?? 0,
            stats?.LongestStreak ?? 0,
            stats?.UnlockedBadges.ToList() ?? new List<Badge>(),
            sessionTotals?.Total ?? 0,
            sessionTotals?.Evaluated ?? 0,
            Round(sessionTotals?.AverageScore),
            sessionTotals?.Tokens ?? 0,
            recent);
    }

    public async Task<AdminDashboardDto> GetDashboardAsync(DateTime utcNow, CancellationToken cancellationToken = default)
    {
        var offset = TimeSpan.FromHours(_settings.UsageDayUtcOffsetHours);
        var today = DateOnly.FromDateTime(utcNow + offset);
        DateOnly LocalDay(DateTime utc) => DateOnly.FromDateTime(utc + offset);
        // Yerel günün başlangıcının UTC karşılığı.
        // Npgsql timestamptz parametresi için Kind=Utc şart.
        DateTime DayStartUtc(DateOnly day) =>
            DateTime.SpecifyKind(day.ToDateTime(TimeOnly.MinValue) - offset, DateTimeKind.Utc);

        var appUsers = await _context.Users.AsNoTracking().CountAsync(u => u.TenantId == null, cancellationToken);
        var weekAgoUtc = DayStartUtc(today.AddDays(-6));
        var newUsers = await _context.Users.AsNoTracking()
            .CountAsync(u => u.TenantId == null && u.CreatedDate >= weekAgoUtc, cancellationToken);

        var premiumUsers = await _context.Subscriptions.AsNoTracking()
            .Where(IsCurrentPremium(_settings.AppId, utcNow, null))
            .Select(s => s.UserId)
            .Distinct()
            .CountAsync(cancellationToken);

        var windowStartUtc = DayStartUtc(today.AddDays(-(CostWindowDays - 1)));
        var sessions = await _context.Sessions.AsNoTracking()
            .Where(s => s.StartedAt >= windowStartUtc)
            .Select(s => new { s.UserId, s.StartedAt, s.TotalScore, s.TokensUsed })
            .ToListAsync(cancellationToken);

        var byDay = sessions.GroupBy(s => LocalDay(s.StartedAt)).ToDictionary(g => g.Key, g => g.ToList());

        var daily = Enumerable.Range(0, DailySeriesDays)
            .Select(i => today.AddDays(-(DailySeriesDays - 1) + i))
            .Select(day =>
            {
                var items = byDay.TryGetValue(day, out var list) ? list : new();
                return new AdminDailyPointDto(
                    day,
                    items.Count,
                    items.Count(s => s.TotalScore != null),
                    items.Select(s => s.UserId).Distinct().Count(),
                    items.Sum(s => (long)s.TokensUsed));
            })
            .ToList();

        var todayItems = byDay.TryGetValue(today, out var t) ? t : new();
        var activeLast7 = sessions
            .Where(s => LocalDay(s.StartedAt) >= today.AddDays(-6))
            .Select(s => s.UserId)
            .Distinct()
            .Count();

        var tokens30 = sessions.Sum(s => (long)s.TokensUsed);
        var rate = _settings.AdminEstimatedUsdPerMillionTokens;
        var cost30 = Math.Round(tokens30 / 1_000_000m * rate, 2);

        var usage = await GetScenarioUsageAsync(cancellationToken);
        var scenarios = await _context.Scenarios.AsNoTracking()
            .Select(s => new { s.Id, s.Title, s.ContextId })
            .ToListAsync(cancellationToken);
        var ranked = scenarios
            .Where(s => usage.ContainsKey(s.Id))
            .Select(s => new AdminScenarioRankDto(s.Id, s.Title, s.ContextId, usage[s.Id].Sessions, usage[s.Id].AverageScore))
            .ToList();

        var top = ranked.OrderByDescending(r => r.Sessions).ThenBy(r => r.Title).Take(RankCount).ToList();
        // En düşük puanlılar: tek iki denemeyle sıralama yanıltmasın diye en az N değerlendirme.
        var lowest = ranked
            .Where(r => r.AverageScore != null && usage[r.Id].Evaluated >= MinSessionsForLowestRank)
            .OrderBy(r => r.AverageScore)
            .Take(RankCount)
            .ToList();

        return new AdminDashboardDto(
            appUsers,
            newUsers,
            premiumUsers,
            appUsers == 0 ? 0 : Math.Round(premiumUsers * 100.0 / appUsers, 1),
            todayItems.Select(s => s.UserId).Distinct().Count(),
            activeLast7,
            todayItems.Count,
            todayItems.Count(s => s.TotalScore != null),
            tokens30,
            cost30,
            rate,
            daily,
            top,
            lowest);
    }

    // SubscriptionRepository.IsCurrent ile aynı kural: Active/Cancelled/GracePeriod ve süresi dolmamış.
    private static System.Linq.Expressions.Expression<Func<Subscription, bool>> IsCurrentPremium(
        string appId, DateTime utcNow, int? userId) =>
        s => (userId == null || s.UserId == userId)
             && s.AppId == appId
             && (s.Status == SubscriptionStatus.Active
                 || s.Status == SubscriptionStatus.Cancelled
                 || s.Status == SubscriptionStatus.GracePeriod)
             && (s.ExpiresAt == null || s.ExpiresAt > utcNow);

    private static double? Round(double? value) => value is null ? null : Math.Round(value.Value, 1);
}
