using CanSayNo.Domain.Entities.Hayirci;
using CanSayNo.Domain.Enums;

namespace CanSayNo.Application.Features.Admin.Hayirci;

// ───────────── Senaryo (admin) ─────────────
// Mobil DTO'larından farkı: PressureTactics ve IsActive dahil, pasif senaryolar da görünür.
// Bu DTO'lar YALNIZCA yetkili admin uçlarından döner.

public sealed record AdminScenarioUsageDto(int Sessions, int Evaluated, double? AverageScore, DateTime? LastPlayedAt)
{
    public static readonly AdminScenarioUsageDto Empty = new(0, 0, null, null);
}

public sealed record AdminScenarioDto(
    string Id,
    string Title,
    string ShortDescription,
    ContextId ContextId,
    string Language,
    int Difficulty,
    int EstimatedDurationMinutes,
    bool IsFree,
    bool IsActive,
    int SortOrder,
    IReadOnlyList<string> Tags,
    AdminScenarioSceneInput Scene,
    AdminScenarioCharacterInput Character,
    AdminScenarioTacticsInput PressureTactics,
    AdminScenarioUsageDto Usage,
    DateTime CreatedDate,
    DateTime? UpdatedDate)
{
    public static AdminScenarioDto From(Scenario s, AdminScenarioUsageDto usage) => new(
        s.Id, s.Title, s.ShortDescription, s.ContextId, s.Language, s.Difficulty, s.EstimatedDurationMinutes,
        s.IsFree, s.IsActive, s.SortOrder, s.Tags.ToList(),
        new AdminScenarioSceneInput
        {
            Setting = s.Scene.Setting,
            Situation = s.Scene.Situation,
            UserGoal = s.Scene.UserGoal,
            Hint = s.Scene.Hint,
            OpeningMessage = s.Scene.OpeningMessage
        },
        new AdminScenarioCharacterInput
        {
            Name = s.Character.Name,
            Age = s.Character.Age,
            Role = s.Character.Role,
            Relationship = s.Character.Relationship,
            Archetype = s.Character.Archetype
        },
        new AdminScenarioTacticsInput
        {
            RoundOne = s.PressureTactics.RoundOne.ToList(),
            RoundTwo = s.PressureTactics.RoundTwo.ToList(),
            RoundThree = s.PressureTactics.RoundThree.ToList()
        },
        usage, s.CreatedDate, s.UpdatedDate);
}

/// <summary>Senaryo ekleme/düzenleme gövdesi (ID hariç; eklemede ayrıca verilir, düzenlemede route'tan gelir).</summary>
public class AdminScenarioUpsertDto
{
    public string Title { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public ContextId ContextId { get; set; }
    /// <summary>İçerik dili ("tr" | "en"). Bağlam gibi oluşturulduktan sonra değiştirilemez.</summary>
    public string Language { get; set; } = ContentLanguages.Turkish;
    public int Difficulty { get; set; }
    public int EstimatedDurationMinutes { get; set; }
    public bool IsFree { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public List<string> Tags { get; set; } = new();
    public AdminScenarioSceneInput Scene { get; set; } = new();
    public AdminScenarioCharacterInput Character { get; set; } = new();
    public AdminScenarioTacticsInput PressureTactics { get; set; } = new();

    /// <summary>Gövdeyi varlığa uygular. JSONB alanlarına YENİ örnek atanır (değişiklik izlensin).</summary>
    public void ApplyTo(Scenario s)
    {
        s.Title = Title.Trim();
        s.ShortDescription = ShortDescription.Trim();
        s.ContextId = ContextId;
        s.Language = ContentLanguages.Normalize(Language);
        s.Difficulty = Difficulty;
        s.EstimatedDurationMinutes = EstimatedDurationMinutes;
        s.IsFree = IsFree;
        s.IsActive = IsActive;
        s.SortOrder = SortOrder;
        s.Tags = Tags.Select(t => t.Trim()).Where(t => t.Length > 0).Distinct().ToList();
        s.Scene = new ScenarioScene
        {
            Setting = Scene.Setting.Trim(),
            Situation = Scene.Situation.Trim(),
            UserGoal = Scene.UserGoal.Trim(),
            Hint = string.IsNullOrWhiteSpace(Scene.Hint) ? null : Scene.Hint.Trim(),
            OpeningMessage = Scene.OpeningMessage.Trim()
        };
        s.Character = new ScenarioCharacter
        {
            Name = Character.Name.Trim(),
            Age = Character.Age,
            Role = Character.Role.Trim(),
            Relationship = Character.Relationship.Trim(),
            Archetype = Character.Archetype,
            AvatarImageUrl = s.Character?.AvatarImageUrl
        };
        s.PressureTactics = new ScenarioPressureTactics
        {
            RoundOne = PressureTactics.RoundOne.Distinct().ToList(),
            RoundTwo = PressureTactics.RoundTwo.Distinct().ToList(),
            RoundThree = PressureTactics.RoundThree.Distinct().ToList(),
            MaxRounds = 3
        };
    }
}

public class AdminScenarioSceneInput
{
    public string Setting { get; set; } = string.Empty;
    public string Situation { get; set; } = string.Empty;
    public string UserGoal { get; set; } = string.Empty;
    public string? Hint { get; set; }
    public string OpeningMessage { get; set; } = string.Empty;
}

public class AdminScenarioCharacterInput
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty;
    public CharacterArchetype Archetype { get; set; }
}

public class AdminScenarioTacticsInput
{
    public List<PressureTactic> RoundOne { get; set; } = new();
    public List<PressureTactic> RoundTwo { get; set; } = new();
    public List<PressureTactic> RoundThree { get; set; } = new();
}

// ───────────── Kullanıcı özeti (admin) ─────────────

public sealed record AdminUserSubscriptionDto(
    string ProductId, SubscriptionStatus Status, DateTime? ExpiresAt, DateTime? StartedAt, DateTime? CancelledAt);

public sealed record AdminUserSessionDto(
    Guid Id, string ScenarioId, string ScenarioTitle, ContextId ContextId, SessionStatus Status,
    int? TotalScore, int TokensUsed, DateTime StartedAt, DateTime? CompletedAt);

public sealed record AdminUserOverviewDto(
    int UserId,
    string Email,
    string FullName,
    DateTime RegisteredAt,
    DateTime? LastLoginAt,
    DateTime? LastPracticeAt,
    bool IsPremium,
    AdminUserSubscriptionDto? Subscription,
    int Level,
    int TotalPoints,
    int CurrentStreak,
    int LongestStreak,
    IReadOnlyList<Badge> Badges,
    int TotalSessions,
    int EvaluatedSessions,
    double? AverageScore,
    long TotalTokens,
    IReadOnlyList<AdminUserSessionDto> RecentSessions);

// ───────────── Gösterge paneli ─────────────

public sealed record AdminDailyPointDto(DateOnly Date, int Sessions, int Evaluated, int ActiveUsers, long Tokens);

public sealed record AdminScenarioRankDto(string Id, string Title, ContextId ContextId, int Sessions, double? AverageScore);

public sealed record AdminDashboardDto(
    int AppUsers,
    int NewUsersLast7Days,
    int PremiumUsers,
    double PremiumRatePercent,
    int ActiveUsersToday,
    int ActiveUsersLast7Days,
    int SessionsToday,
    int EvaluatedToday,
    long TokensLast30Days,
    decimal EstimatedCostUsdLast30Days,
    decimal UsdPerMillionTokens,
    IReadOnlyList<AdminDailyPointDto> Daily,
    IReadOnlyList<AdminScenarioRankDto> TopScenarios,
    IReadOnlyList<AdminScenarioRankDto> LowestScoringScenarios);
