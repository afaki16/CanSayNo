using CanSayNo.Domain.Enums;

namespace CanSayNo.Domain.Entities.Hayirci;

public class Subscription : BaseAuditableEntity
{
    // Guid PK — BaseEntity'nin int Id'sini gizler (shadow).
    public new Guid Id { get; set; } = Guid.NewGuid();

    public int UserId { get; set; }            // FK -> User.Id (int)
    public User User { get; set; } = null!;

    // Multi-app support (ileride başka uygulamalar için)
    public string AppId { get; set; } = "cansayno";

    // Product
    public string ProductId { get; set; } = string.Empty;  // örn: "cansayno_monthly"
    public SubscriptionStatus Status { get; set; }

    // Time
    public DateTime? ExpiresAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CancelledAt { get; set; }

    // Provider (RevenueCat)
    public string Provider { get; set; } = "RevenueCat";
    public string? ProviderUserId { get; set; }  // RevenueCat App User ID
    public string? ProviderTransactionId { get; set; }

    // Debug
    public string? RawWebhookData { get; set; }  // JSONB için string olarak
}
