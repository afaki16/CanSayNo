using System;
using System.Collections.Generic;
using CanSayNo.Domain.Enums;

namespace CanSayNo.Application.Features.Sessions.Common;

/// <summary>
/// Pratik oturumunun tam hali (chat + sonuç ekranı). PressureTactics içermez —
/// sadece raunt ilerlemesi için MaxRounds taşınır.
/// </summary>
public record SessionDto
{
    public Guid Id { get; init; }
    public string ScenarioId { get; init; } = "";
    public string ScenarioTitle { get; init; } = "";
    public ContextId ScenarioContextId { get; init; }
    public SessionStatus Status { get; init; }
    public int CurrentRound { get; init; }
    public int MaxRounds { get; init; }
    public List<SessionMessageDto> Messages { get; init; } = new();
    public SessionEvaluationDto? Evaluation { get; init; }
    public int? TotalScore { get; init; }
    public DateTime StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
}

public record SessionMessageDto
{
    public Guid Id { get; init; }
    public string Role { get; init; } = "";  // "ai" | "user"
    public string Content { get; init; } = "";
    public DateTime Timestamp { get; init; }
}

public record SessionEvaluationDto
{
    /// <summary>
    /// Eksen → 0-100 puan. Anahtarlar camelCase string (netlik, nezaket, dayaniklilik,
    /// alternatifSunma, ozurDengesi) — enum sözlük anahtarlarının JSON'da tutarlı çıkması için.
    /// </summary>
    public Dictionary<string, int> Scores { get; init; } = new();
    public List<string> Strengths { get; init; } = new();
    public List<string> Improvements { get; init; } = new();
    public string CoachNote { get; init; } = "";
    public string? IdealResponseExample { get; init; }
    public int TotalScore { get; init; }
}

/// <summary>"Son pratiklerin" listesi için hafif DTO (mesajlar ve değerlendirme detayı yok).</summary>
public record SessionListItemDto
{
    public Guid Id { get; init; }
    public string ScenarioId { get; init; } = "";
    public string ScenarioTitle { get; init; } = "";
    public ContextId ScenarioContextId { get; init; }
    public SessionStatus Status { get; init; }
    public int? TotalScore { get; init; }
    public DateTime StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
}
