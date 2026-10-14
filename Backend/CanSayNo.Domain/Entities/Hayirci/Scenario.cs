using CanSayNo.Domain.Enums;

namespace CanSayNo.Domain.Entities.Hayirci;

public class Scenario : BaseAuditableEntity
{
    // Identity — string PK (örn: "is_001_cuma_mesaji"). UI'da da bu ID kullanılıyor.
    // BaseEntity'nin int Id'sini gizler (shadow); EF en türemiş Id'yi anahtar olarak kullanır.
    public new string Id { get; set; } = string.Empty;

    // Basic info
    public string Title { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public ContextId ContextId { get; set; }
    public int Difficulty { get; set; }  // 1-5
    public int EstimatedDurationMinutes { get; set; }

    // Access control
    public bool IsFree { get; set; }
    public bool IsActive { get; set; } = true;

    // Complex data (JSONB)
    public ScenarioScene Scene { get; set; } = new();
    public ScenarioCharacter Character { get; set; } = new();
    public ScenarioPressureTactics PressureTactics { get; set; } = new();

    // Metadata
    public List<string> Tags { get; set; } = new();
    public int SortOrder { get; set; }

    // Navigation
    public ICollection<Session> Sessions { get; set; } = new HashSet<Session>();
}

// Owned types / value objects for JSONB
public class ScenarioScene
{
    public string Setting { get; set; } = string.Empty;
    public string Situation { get; set; } = string.Empty;
    public string UserGoal { get; set; } = string.Empty;
    public string? Hint { get; set; }
}

public class ScenarioCharacter
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty;
    public CharacterArchetype Archetype { get; set; }
    public string? AvatarImageUrl { get; set; }  // null = placeholder
}

public class ScenarioPressureTactics
{
    public List<PressureTactic> RoundOne { get; set; } = new();
    public List<PressureTactic> RoundTwo { get; set; } = new();
    public List<PressureTactic> RoundThree { get; set; } = new();
    public int MaxRounds { get; set; } = 3;
}
