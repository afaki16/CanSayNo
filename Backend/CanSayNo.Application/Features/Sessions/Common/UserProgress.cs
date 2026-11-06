using System;
using System.Collections.Generic;
using System.Linq;
using CanSayNo.Domain.Entities.Hayirci;
using CanSayNo.Domain.Enums;

namespace CanSayNo.Application.Features.Sessions.Common;

/// <summary>
/// Değerlendirilen bir pratiğin kullanıcının istatistiklerine etkisi: seri, XP, seviye, rozetler.
/// Saf fonksiyon — DB'ye dokunmaz; çağıran kaydeder. Her oturum için yalnızca bir kez çağrılmalı
/// (değerlendirme idempotent olduğu için ikinci kez çalışmaz).
/// </summary>
public static class UserProgress
{
    /// <summary>Seviye başına XP (mobil "240 / 500 XP" gösterimiyle uyumlu).</summary>
    public const int XpPerLevel = 500;

    /// <summary>Tamamlanan her pratik için sabit XP.</summary>
    public const int CompletionXp = 10;

    /// <summary>Puan katkısı: toplam puanın %40'ı → pratik başına 10-50 XP.</summary>
    public const double ScoreXpFactor = 0.4;

    /// <summary>WorkMaster: İş bağlamında bu kadar değerlendirilmiş pratik.</summary>
    public const int WorkMasterSessions = 10;

    // Bağlam sayısı arttıkça "hepsi" şartı rozeti erişilmez kılmasın diye sabit eşik.
    public const int ContextExplorerContexts = 5;

    public sealed record Result(int XpEarned, IReadOnlyList<Badge> NewBadges);

    /// <param name="practiceDate">Pratiğin yerel (TR) takvim günü.</param>
    /// <param name="localHour">Pratiğin yerel (TR) saati, 0-23.</param>
    /// <param name="evaluatedByContext">Bağlam başına değerlendirilmiş pratik sayısı, BU pratik dahil.</param>
    public static Result ApplyCompletedSession(
        UserStats stats,
        int totalScore,
        DateOnly practiceDate,
        int localHour,
        IReadOnlyDictionary<ContextId, int> evaluatedByContext)
    {
        stats.TotalCompletedSessions++;

        // Seri: aynı gün tekrar pratik seriyi değiştirmez; dün pratik yapıldıysa artar; aksi halde 1'den başlar.
        if (stats.LastPracticeDate != practiceDate)
        {
            stats.CurrentStreak = stats.LastPracticeDate == practiceDate.AddDays(-1)
                ? stats.CurrentStreak + 1
                : 1;
            stats.LastPracticeDate = practiceDate;
        }
        stats.LongestStreak = Math.Max(stats.LongestStreak, stats.CurrentStreak);

        var xp = CompletionXp + (int)Math.Round(totalScore * ScoreXpFactor, MidpointRounding.AwayFromZero);
        stats.TotalPoints += xp;
        stats.Level = 1 + stats.TotalPoints / XpPerLevel;

        var newBadges = new List<Badge>();
        void Unlock(Badge badge, bool condition)
        {
            if (condition && !stats.UnlockedBadges.Contains(badge))
                newBadges.Add(badge);
        }

        Unlock(Badge.FirstNo, stats.TotalCompletedSessions >= 1);
        Unlock(Badge.ThreeDayStreak, stats.CurrentStreak >= 3);
        Unlock(Badge.SevenDayStreak, stats.CurrentStreak >= 7);
        Unlock(Badge.ThirtyDayStreak, stats.CurrentStreak >= 30);
        Unlock(Badge.PerfectScore, totalScore >= 100);
        Unlock(Badge.NightOwl, localHour >= 22 || localHour < 4);
        Unlock(Badge.EarlyBird, localHour >= 5 && localHour < 8);
        Unlock(Badge.Persistent, stats.TotalCompletedSessions >= 50);
        Unlock(Badge.WorkMaster,
            evaluatedByContext.TryGetValue(ContextId.Is, out var workCount) && workCount >= WorkMasterSessions);
        Unlock(Badge.ContextExplorer,
            evaluatedByContext.Count(kv => kv.Value > 0) >= ContextExplorerContexts);

        if (newBadges.Count > 0)
        {
            // JSONB listesi: yeni örnek atamak değişikliğin izlenmesini garanti eder.
            stats.UnlockedBadges = new List<Badge>(stats.UnlockedBadges);
            stats.UnlockedBadges.AddRange(newBadges);
        }

        return new Result(xp, newBadges);
    }
}
