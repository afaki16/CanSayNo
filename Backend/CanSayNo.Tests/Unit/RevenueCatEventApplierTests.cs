using CanSayNo.Application.Features.Subscriptions.Webhooks;
using CanSayNo.Domain.Entities.Hayirci;
using CanSayNo.Domain.Enums;
using static CanSayNo.Application.Features.Subscriptions.Webhooks.RevenueCatEventApplier;

namespace CanSayNo.Tests.Unit;

public class RevenueCatEventApplierTests
{
    private static readonly DateTime Now = new(2026, 11, 8, 12, 0, 0, DateTimeKind.Utc);
    private static long Ms(DateTime d) => new DateTimeOffset(d).ToUnixTimeMilliseconds();

    private static RevenueCatEvent Event(string type, DateTime? expires = null, string? cancelReason = null) => new()
    {
        Id = Guid.NewGuid().ToString(),
        Type = type,
        EventTimestampMs = Ms(Now),
        ProductId = "cansayno_monthly",
        PurchasedAtMs = Ms(Now),
        ExpirationAtMs = expires is { } e ? Ms(e) : null,
        CancelReason = cancelReason
    };

    [Theory]
    [InlineData("INITIAL_PURCHASE")]
    [InlineData("RENEWAL")]
    [InlineData("UNCANCELLATION")]
    public void Activating_events_make_subscription_active(string type)
    {
        var sub = new Subscription { Status = SubscriptionStatus.Cancelled, CancelledAt = Now.AddDays(-1) };

        var outcome = Apply(sub, Event(type, Now.AddMonths(1)), Now);

        Assert.Equal(Outcome.Applied, outcome);
        Assert.Equal(SubscriptionStatus.Active, sub.Status);
        Assert.Equal(Now.AddMonths(1), sub.ExpiresAt);
        Assert.Null(sub.CancelledAt);
        Assert.Equal("cansayno_monthly", sub.ProductId);
    }

    [Fact]
    public void Cancellation_keeps_access_until_period_ends()
    {
        var end = Now.AddDays(10);
        var sub = new Subscription { Status = SubscriptionStatus.Active, ExpiresAt = end };

        Apply(sub, Event("CANCELLATION", end), Now);

        Assert.Equal(SubscriptionStatus.Cancelled, sub.Status);
        Assert.Equal(end, sub.ExpiresAt);
        Assert.Equal(Now, sub.CancelledAt);
    }

    [Fact]
    public void Refund_ends_access_immediately()
    {
        var sub = new Subscription { Status = SubscriptionStatus.Active, ExpiresAt = Now.AddDays(10) };

        Apply(sub, Event("CANCELLATION", Now.AddDays(10), cancelReason: "CUSTOMER_SUPPORT"), Now);

        Assert.Equal(SubscriptionStatus.Expired, sub.Status);
        Assert.Equal(Now, sub.ExpiresAt);
    }

    [Fact]
    public void Billing_issue_moves_to_grace_period()
    {
        var graceEnd = Now.AddDays(3);
        var sub = new Subscription { Status = SubscriptionStatus.Active, ExpiresAt = Now };
        var e = Event("BILLING_ISSUE");
        e.GracePeriodExpirationAtMs = Ms(graceEnd);

        Apply(sub, e, Now);

        Assert.Equal(SubscriptionStatus.GracePeriod, sub.Status);
        Assert.Equal(graceEnd, sub.ExpiresAt);
    }

    [Fact]
    public void Expiration_expires()
    {
        var sub = new Subscription { Status = SubscriptionStatus.Active, ExpiresAt = Now.AddDays(1) };

        Apply(sub, Event("EXPIRATION", Now), Now);

        Assert.Equal(SubscriptionStatus.Expired, sub.Status);
    }

    [Theory]
    [InlineData("TEST")]
    [InlineData("PRODUCT_CHANGE")]
    [InlineData("SUBSCRIPTION_PAUSED")]
    [InlineData("TRANSFER")]
    [InlineData("SOMETHING_NEW")]
    public void Other_events_do_not_change_state(string type)
    {
        var sub = new Subscription { Status = SubscriptionStatus.Active, ExpiresAt = Now.AddDays(5) };

        var outcome = Apply(sub, Event(type, Now.AddYears(1)), Now);

        Assert.Equal(Outcome.NoStateChange, outcome);
        Assert.Equal(SubscriptionStatus.Active, sub.Status);
        Assert.Equal(Now.AddDays(5), sub.ExpiresAt);
    }
}
