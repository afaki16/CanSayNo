using System;
using System.Collections.Generic;
using CanSayNo.Domain.Enums;

namespace CanSayNo.Application.Features.Scenarios.Common;

/// <summary>
/// Detay görünümü için tam senaryo DTO'su.
/// PressureTactics KASITLI olarak dahil edilmez — bu veri yalnızca AI prompt'una geçer,
/// kullanıcıya gösterilmez; istemcinin bunu bilmesine gerek yoktur ve sızdırılması
/// senaryonun "oyununu" bozar.
/// </summary>
public record ScenarioDetailDto
{
    public string Id { get; init; } = "";
    public string Title { get; init; } = "";
    public string ShortDescription { get; init; } = "";
    public ContextId ContextId { get; init; }
    public int Difficulty { get; init; }
    public int EstimatedDurationMinutes { get; init; }
    public bool IsFree { get; init; }
    public ScenarioSceneDto Scene { get; init; } = new();
    public ScenarioCharacterDto Character { get; init; } = new();
    public List<string> Tags { get; init; } = new();

    // User-spesifik
    public bool IsCompleted { get; init; }
    public int? BestScore { get; init; }
    public int AttemptsCount { get; init; }
    public DateTime? LastAttemptAt { get; init; }
}

public record ScenarioSceneDto
{
    public string Setting { get; init; } = "";
    public string Situation { get; init; } = "";
    public string UserGoal { get; init; } = "";
    public string? Hint { get; init; }
}

public record ScenarioCharacterDto
{
    public string Name { get; init; } = "";
    public int Age { get; init; }
    public string Role { get; init; } = "";
    public string Relationship { get; init; } = "";
    public CharacterArchetype Archetype { get; init; }
    public string? AvatarImageUrl { get; init; }
}
