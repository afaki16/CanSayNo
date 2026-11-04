using System;
using CanSayNo.Domain.Entities.Hayirci;
using CanSayNo.Domain.Enums;

namespace CanSayNo.Application.Features.Subscriptions.Webhooks;

/// <summary>
/// Bir RevenueCat olayının abonelik kaydına etkisi. Saf fonksiyon: DB'ye dokunmaz.
///
/// Erişim kuralı (HasActiveSubscriptionAsync ile uyumlu): Active, Cancelled (otomatik yenileme
/// kapalı, süre bitene kadar erişim sürer) ve GracePeriod premium sayılır — ExpiresAt geçmemişse.
/// </summary>
public static class RevenueCatEventApplier
{
    public enum Outcome
    {
        /// <summary>Kayıt güncellendi (ya da yeni kayıt dolduruldu).</summary>
        Applied,
        /// <summary>Durumu etkilemeyen olay (TEST, PRODUCT_CHANGE, SUBSCRIPTION_PAUSED, TRANSFER...).</summary>
        NoStateChange
    }

    // Erişimi başlatan/sürdüren olaylar.
    private static readonly string[] ActivatingTypes =
    {
        "INITIAL_PURCHASE", "RENEWAL", "UNCANCELLATION", "NON_RENEWING_PURCHASE",
        "SUBSCRIPTION_EXTENDED", "TEMPORARY_ENTITLEMENT_GRANT", "REFUND_REVERSED"
    };

    public static Outcome Apply(Subscription subscription, RevenueCatEvent e, DateTime utcNow)
    {
        var type = e.Type ?? string.Empty;
        var eventTime = FromMs(e.EventTimestampMs) ?? utcNow;

        if (Array.IndexOf(ActivatingTypes, type) >= 0)
        {
            subscription.Status = SubscriptionStatus.Active;
            subscription.ProductId = e.ProductId ?? subscription.ProductId;
            subscription.ExpiresAt = FromMs(e.ExpirationAtMs); // null = süresiz
            subscription.StartedAt ??= FromMs(e.PurchasedAtMs) ?? eventTime;
            subscription.CancelledAt = null;
            return Outcome.Applied;
        }

        switch (type)
        {
            case "CANCELLATION":
                subscription.CancelledAt = eventTime;
                if (e.CancelReason == "CUSTOMER_SUPPORT")
                {
                    // İade: erişim hemen kapanır.
                    subscription.Status = SubscriptionStatus.Expired;
                    subscription.ExpiresAt = eventTime;
                }
                else
                {
                    // Otomatik yenileme kapandı; ödenmiş dönem bitene kadar erişim sürer.
                    subscription.Status = SubscriptionStatus.Cancelled;
                    subscription.ExpiresAt = FromMs(e.ExpirationAtMs) ?? subscription.ExpiresAt;
                }
                return Outcome.Applied;

            case "BILLING_ISSUE":
                // Abonelik bitmedi; mağaza ödemeyi yeniden deniyor. Grace bitişi varsa ona kadar erişim.
                subscription.Status = SubscriptionStatus.GracePeriod;
                subscription.ExpiresAt = FromMs(e.GracePeriodExpirationAtMs) ?? subscription.ExpiresAt;
                return Outcome.Applied;

            case "EXPIRATION":
                subscription.Status = SubscriptionStatus.Expired;
                subscription.ExpiresAt = FromMs(e.ExpirationAtMs) ?? eventTime;
                return Outcome.Applied;

            default:
                // TEST, PRODUCT_CHANGE (yeni ürün hemen geçerli değil), SUBSCRIPTION_PAUSED
                // (erişim kaldırılmaz; EXPIRATION gelir), TRANSFER, BILLING/fiyat bildirimleri...
                return Outcome.NoStateChange;
        }
    }

    public static DateTime? FromMs(long? ms) =>
        ms is long v ? DateTimeOffset.FromUnixTimeMilliseconds(v).UtcDateTime : null;
}
