using System.Collections.Generic;
using CanSayNo.Domain.Enums;

namespace CanSayNo.Application.Features.Scenarios.Common;

/// <summary>
/// Liste görünümü için minimal senaryo DTO'su (performans odaklı).
/// JSONB Scene/Character ve PressureTactics dahil EDİLMEZ.
/// </summary>
public record ScenarioListItemDto
{
    public string Id { get; init; } = "";
    public string Title { get; init; } = "";
    public string ShortDescription { get; init; } = "";
    public ContextId ContextId { get; init; }
    public int Difficulty { get; init; }
    public int EstimatedDurationMinutes { get; init; }
    public bool IsFree { get; init; }
    public List<string> Tags { get; init; } = new();

    // User-spesifik durumlar (handler içinde Sessions agregasından doldurulur)
    public bool IsCompleted { get; init; }
    public int? BestScore { get; init; }
    public int AttemptsCount { get; init; }
}
