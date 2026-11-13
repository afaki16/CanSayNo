using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using static CanSayNo.Tests.Integration.TestApi;

namespace CanSayNo.Tests.Integration;

/// <summary>
/// RevenueCat webhook'u uçtan uca: satın alma premium açar; "Satın alımları geri yükle" başka
/// hesaptan yapılınca gelen TRANSFER aboneliği yeni hesaba taşır.
/// </summary>
[Collection(ApiCollection.Name)]
public class RevenueCatWebhookTests(ApiFactory factory)
{
    private static long Ms(DateTime d) => new DateTimeOffset(d).ToUnixTimeMilliseconds();

    [Fact]
    public async Task Purchase_makes_user_premium()
    {
        var user = await CreateUserAsync(factory);
        var transaction = Guid.NewGuid().ToString("N");

        await SendAsync(PurchaseEvent(user.Id.ToString(), transaction, DateTime.UtcNow));

        Assert.True(await IsPremiumAsync(user));
    }

    [Fact]
    public async Task Transfer_moves_subscription_to_the_restoring_account()
    {
        var oldAccount = await CreateUserAsync(factory);
        var newAccount = await CreateUserAsync(factory);
        var transaction = Guid.NewGuid().ToString("N");
        var now = DateTime.UtcNow;

        await SendAsync(PurchaseEvent(oldAccount.Id.ToString(), transaction, now.AddMinutes(-10)));
        Assert.True(await IsPremiumAsync(oldAccount));

        var transfer = new
        {
            id = Guid.NewGuid().ToString(),
            type = "TRANSFER",
            event_timestamp_ms = Ms(now.AddMinutes(-5)),
            store = "PLAY_STORE",
            environment = "PRODUCTION",
            transferred_from = new[] { oldAccount.Id.ToString() },
            transferred_to = new[] { newAccount.Id.ToString() }
        };
        await SendAsync(transfer);
        await SendAsync(transfer); // yeniden deneme zararsız

        Assert.True(await IsPremiumAsync(newAccount));
        Assert.False(await IsPremiumAsync(oldAccount));

        // Sonraki yenileme hâlâ eski hesabı original_app_user_id olarak taşıyabilir: kayıt yeni hesapta kalmalı.
        await SendAsync(new
        {
            id = Guid.NewGuid().ToString(),
            type = "RENEWAL",
            event_timestamp_ms = Ms(now),
            app_user_id = newAccount.Id.ToString(),
            original_app_user_id = oldAccount.Id.ToString(),
            aliases = new[] { oldAccount.Id.ToString(), newAccount.Id.ToString() },
            product_id = "cansayno_monthly",
            purchased_at_ms = Ms(now),
            expiration_at_ms = Ms(now.AddMonths(1)),
            environment = "PRODUCTION",
            store = "PLAY_STORE",
            transaction_id = Guid.NewGuid().ToString("N"),
            original_transaction_id = transaction
        });

        Assert.True(await IsPremiumAsync(newAccount));
        Assert.False(await IsPremiumAsync(oldAccount));
        Assert.Equal(1, await factory.WithDbAsync(db =>
            db.Subscriptions.CountAsync(s => s.ProviderTransactionId == transaction)));
    }

    [Fact]
    public async Task Transfer_to_unknown_user_is_ignored()
    {
        var owner = await CreateUserAsync(factory);
        var transaction = Guid.NewGuid().ToString("N");
        await SendAsync(PurchaseEvent(owner.Id.ToString(), transaction, DateTime.UtcNow.AddMinutes(-1)));

        await SendAsync(new
        {
            id = Guid.NewGuid().ToString(),
            type = "TRANSFER",
            event_timestamp_ms = Ms(DateTime.UtcNow),
            environment = "PRODUCTION",
            transferred_from = new[] { owner.Id.ToString() },
            transferred_to = new[] { "$RCAnonymousID:abc" }
        });

        Assert.True(await IsPremiumAsync(owner));
    }

    private static object PurchaseEvent(string appUserId, string transaction, DateTime at) => new
    {
        id = Guid.NewGuid().ToString(),
        type = "INITIAL_PURCHASE",
        event_timestamp_ms = Ms(at),
        app_user_id = appUserId,
        original_app_user_id = appUserId,
        product_id = "cansayno_monthly",
        purchased_at_ms = Ms(at),
        expiration_at_ms = Ms(at.AddMonths(1)),
        environment = "PRODUCTION",
        store = "PLAY_STORE",
        transaction_id = transaction,
        original_transaction_id = transaction
    };

    private async Task SendAsync(object @event)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/Webhooks/revenuecat")
        {
            Content = JsonContent.Create(new { api_version = "1.0", @event })
        };
        request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {ApiFactory.WebhookSecret}");

        var response = await NewClient(factory).SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static async Task<bool> IsPremiumAsync(TestUser user)
    {
        var body = await ReadJsonAsync(await user.Client.GetAsync("/api/Subscriptions/me"));
        return body["data"]!["isPremium"]!.GetValue<bool>();
    }
}
