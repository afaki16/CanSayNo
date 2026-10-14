using CanSayNo.Domain.Enums;

namespace CanSayNo.Domain.Entities.Hayirci;

public class UserStats : BaseAuditableEntity
{
    // PK + FK -> User.Id (int). Inherited int Id is ignored in configuration.
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    // Streak
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public DateOnly? LastPracticeDate { get; set; }

    // Counts
    public int TotalSessions { get; set; }
    public int TotalCompletedSessions { get; set; }

    // Gamification
    public int TotalPoints { get; set; }  // XP
    public int Level { get; set; } = 1;
    public List<Badge> UnlockedBadges { get; set; } = new();
}
