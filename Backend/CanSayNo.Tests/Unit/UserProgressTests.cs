using CanSayNo.Application.Features.Sessions.Common;
using CanSayNo.Domain.Entities.Hayirci;
using CanSayNo.Domain.Enums;

namespace CanSayNo.Tests.Unit;

public class UserProgressTests
{
    private static readonly DateOnly Today = new(2026, 11, 8);
    private const int Noon = 12;

    private static Dictionary<ContextId, int> Contexts(params (ContextId Id, int Count)[] items) =>
        items.ToDictionary(i => i.Id, i => i.Count);

    [Fact]
    public void First_practice_starts_streak_and_unlocks_FirstNo()
    {
        var stats = new UserStats();

        var result = UserProgress.ApplyCompletedSession(stats, 80, Today, Noon, Contexts((ContextId.Is, 1)));

        Assert.Equal(1, stats.TotalCompletedSessions);
        Assert.Equal(1, stats.CurrentStreak);
        Assert.Equal(1, stats.LongestStreak);
        Assert.Equal(Today, stats.LastPracticeDate);
        Assert.Contains(Badge.FirstNo, result.NewBadges);
    }

    [Fact]
    public void Xp_is_completion_bonus_plus_forty_percent_of_score()
    {
        var stats = new UserStats();

        var result = UserProgress.ApplyCompletedSession(stats, 75, Today, Noon, Contexts());

        // 10 + round(75 * 0.4) = 40
        Assert.Equal(40, result.XpEarned);
        Assert.Equal(40, stats.TotalPoints);
        Assert.Equal(1, stats.Level);
    }

    [Fact]
    public void Level_rises_every_500_xp()
    {
        var stats = new UserStats { TotalPoints = 480 };

        UserProgress.ApplyCompletedSession(stats, 50, Today, Noon, Contexts());

        Assert.Equal(510, stats.TotalPoints);
        Assert.Equal(2, stats.Level);
    }

    [Fact]
    public void Practice_on_consecutive_day_extends_streak()
    {
        var stats = new UserStats { CurrentStreak = 2, LongestStreak = 2, LastPracticeDate = Today.AddDays(-1) };

        var result = UserProgress.ApplyCompletedSession(stats, 60, Today, Noon, Contexts());

        Assert.Equal(3, stats.CurrentStreak);
        Assert.Equal(3, stats.LongestStreak);
        Assert.Contains(Badge.ThreeDayStreak, result.NewBadges);
    }

    [Fact]
    public void Second_practice_same_day_does_not_change_streak()
    {
        var stats = new UserStats { CurrentStreak = 4, LongestStreak = 4, LastPracticeDate = Today };

        UserProgress.ApplyCompletedSession(stats, 60, Today, Noon, Contexts());

        Assert.Equal(4, stats.CurrentStreak);
    }

    [Fact]
    public void Gap_of_a_day_resets_streak_but_keeps_longest()
    {
        var stats = new UserStats { CurrentStreak = 6, LongestStreak = 6, LastPracticeDate = Today.AddDays(-2) };

        UserProgress.ApplyCompletedSession(stats, 60, Today, Noon, Contexts());

        Assert.Equal(1, stats.CurrentStreak);
        Assert.Equal(6, stats.LongestStreak);
    }

    [Theory]
    [InlineData(23, Badge.NightOwl)]
    [InlineData(2, Badge.NightOwl)]
    [InlineData(6, Badge.EarlyBird)]
    public void Time_of_day_badges(int localHour, Badge expected)
    {
        var result = UserProgress.ApplyCompletedSession(new UserStats(), 50, Today, localHour, Contexts());

        Assert.Contains(expected, result.NewBadges);
    }

    [Fact]
    public void Already_unlocked_badge_is_not_returned_again()
    {
        var stats = new UserStats { TotalCompletedSessions = 3, UnlockedBadges = [Badge.FirstNo] };

        var result = UserProgress.ApplyCompletedSession(stats, 50, Today, Noon, Contexts());

        Assert.DoesNotContain(Badge.FirstNo, result.NewBadges);
        Assert.Single(stats.UnlockedBadges, Badge.FirstNo);
    }

    [Fact]
    public void ContextExplorer_needs_five_different_contexts()
    {
        var four = Contexts((ContextId.Is, 3), (ContextId.Aile, 1), (ContextId.Arkadas, 1), (ContextId.Komsu, 1));
        var five = Contexts((ContextId.Is, 3), (ContextId.Aile, 1), (ContextId.Arkadas, 1), (ContextId.Komsu, 1),
            (ContextId.Dijital, 1));

        var withFour = UserProgress.ApplyCompletedSession(new UserStats(), 50, Today, Noon, four);
        var withFive = UserProgress.ApplyCompletedSession(new UserStats(), 50, Today, Noon, five);

        Assert.DoesNotContain(Badge.ContextExplorer, withFour.NewBadges);
        Assert.Contains(Badge.ContextExplorer, withFive.NewBadges);
    }

    [Fact]
    public void WorkMaster_needs_ten_work_practices()
    {
        var nine = UserProgress.ApplyCompletedSession(new UserStats(), 50, Today, Noon, Contexts((ContextId.Is, 9)));
        var ten = UserProgress.ApplyCompletedSession(new UserStats(), 50, Today, Noon, Contexts((ContextId.Is, 10)));

        Assert.DoesNotContain(Badge.WorkMaster, nine.NewBadges);
        Assert.Contains(Badge.WorkMaster, ten.NewBadges);
    }

    [Fact]
    public void Perfect_score_badge_only_at_100()
    {
        var at99 = UserProgress.ApplyCompletedSession(new UserStats(), 99, Today, Noon, Contexts());
        var at100 = UserProgress.ApplyCompletedSession(new UserStats(), 100, Today, Noon, Contexts());

        Assert.DoesNotContain(Badge.PerfectScore, at99.NewBadges);
        Assert.Contains(Badge.PerfectScore, at100.NewBadges);
    }
}
