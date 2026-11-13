using CanSayNo.Domain.Enums;

namespace CanSayNo.Domain.Entities.Hayirci;

public class Session : BaseAuditableEntity
{
    // Guid PK — BaseEntity'nin int Id'sini gizler (shadow).
    public new Guid Id { get; set; } = Guid.NewGuid();

    // Relationships
    public int UserId { get; set; }            // FK -> User.Id (int)
    public User User { get; set; } = null!;

    public string ScenarioId { get; set; } = string.Empty;
    public Scenario Scenario { get; set; } = null!;

    // State
    public SessionStatus Status { get; set; } = SessionStatus.Active;
    public int CurrentRound { get; set; }

    // Content (JSONB)
    public List<SessionMessage> Messages { get; set; } = new();
    public SessionEvaluation? Evaluation { get; set; }  // null until completed

    // Metrics
    public int? TotalScore { get; set; }  // 0-100
    public int TokensUsed { get; set; }
    public int DurationSeconds { get; set; }

    // Timestamps
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}

public static class SessionMessageRoles
{
    public const string Ai = "ai";
    public const string User = "user";
}

public class SessionMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Role { get; set; } = string.Empty;  // SessionMessageRoles.Ai | SessionMessageRoles.User
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Karakter cevabının ayrı balonları (1-3 kısa mesaj; gerçek mesajlaşmadaki gibi art arda).
    /// <see cref="Content"/> bunların birleşimidir — prompt geçmişi ve değerlendirme onu kullanır.
    /// Kullanıcı mesajında, açılış mesajında ve bu alandan önceki kayıtlarda null.
    /// </summary>
    public List<string>? Parts { get; set; }

    /// <summary>Karakterin bu cevaptaki ruh hali; yalnızca karakter cevaplarında.</summary>
    public CharacterMood? Mood { get; set; }
}

// Mobile Sonuç ekranının şekli: 5 eksen 0-100 + liste halinde güçlü yönler / öneriler + koç notu.
public class SessionEvaluation
{
    public Dictionary<EvaluationCriterion, int> Scores { get; set; } = new();  // 0-100 her eksen
    public List<string> Strengths { get; set; } = new();
    public List<string> Improvements { get; set; } = new();
    public string CoachNote { get; set; } = string.Empty;
    public string? IdealResponseExample { get; set; }
    public int TotalScore { get; set; }  // 0-100

    // Değerlendirme anının kaydı: sonuç ekranı sonradan açıldığında da aynı değerler görünür.
    public int XpEarned { get; set; }
    public int StreakAfter { get; set; }
}
