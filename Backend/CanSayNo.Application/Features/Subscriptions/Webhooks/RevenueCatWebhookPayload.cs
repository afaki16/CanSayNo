using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CanSayNo.Application.Features.Subscriptions.Webhooks;

// RevenueCat webhook gövdesi: { "api_version": "1.0", "event": { ... } }.
// Alan adları ve anlamları RevenueCat "Event Types and Fields" belgesinden; burada yalnızca
// abonelik durumunu belirleyen alanlar var. Bilinmeyen alanlar yok sayılır.

public sealed class RevenueCatWebhookPayload
{
    [JsonPropertyName("api_version")]
    public string? ApiVersion { get; set; }

    [JsonPropertyName("event")]
    public RevenueCatEvent? Event { get; set; }
}

public sealed class RevenueCatEvent
{
    /// <summary>Olayın tekil kimliği; yeniden denemeler aynı id ve event_timestamp_ms ile gelir.</summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("event_timestamp_ms")]
    public long? EventTimestampMs { get; set; }

    [JsonPropertyName("app_user_id")]
    public string? AppUserId { get; set; }

    [JsonPropertyName("original_app_user_id")]
    public string? OriginalAppUserId { get; set; }

    [JsonPropertyName("aliases")]
    public List<string>? Aliases { get; set; }

    [JsonPropertyName("product_id")]
    public string? ProductId { get; set; }

    [JsonPropertyName("purchased_at_ms")]
    public long? PurchasedAtMs { get; set; }

    /// <summary>Süresiz/abonelik dışı ürünlerde null.</summary>
    [JsonPropertyName("expiration_at_ms")]
    public long? ExpirationAtMs { get; set; }

    /// <summary>Yalnızca BILLING_ISSUE'da; null olabilir.</summary>
    [JsonPropertyName("grace_period_expiration_at_ms")]
    public long? GracePeriodExpirationAtMs { get; set; }

    /// <summary>SANDBOX | PRODUCTION</summary>
    [JsonPropertyName("environment")]
    public string? Environment { get; set; }

    [JsonPropertyName("store")]
    public string? Store { get; set; }

    [JsonPropertyName("transaction_id")]
    public string? TransactionId { get; set; }

    [JsonPropertyName("original_transaction_id")]
    public string? OriginalTransactionId { get; set; }

    /// <summary>Yalnızca CANCELLATION'da (ör. UNSUBSCRIBE, BILLING_ERROR, CUSTOMER_SUPPORT).</summary>
    [JsonPropertyName("cancel_reason")]
    public string? CancelReason { get; set; }

    /// <summary>Yalnızca EXPIRATION'da.</summary>
    [JsonPropertyName("expiration_reason")]
    public string? ExpirationReason { get; set; }

    /// <summary>Yalnızca TRANSFER'da: işlemlerin alındığı App User ID'ler.</summary>
    [JsonPropertyName("transferred_from")]
    public List<string>? TransferredFrom { get; set; }

    /// <summary>
    /// Yalnızca TRANSFER'da: işlemleri devralan App User ID'ler. TRANSFER'da app_user_id,
    /// aliases ve transaction alanları gelmez.
    /// </summary>
    [JsonPropertyName("transferred_to")]
    public List<string>? TransferredTo { get; set; }
}
