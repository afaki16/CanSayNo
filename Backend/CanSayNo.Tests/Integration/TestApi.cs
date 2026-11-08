using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using CanSayNo.Domain.Common.Enums;
using CanSayNo.Domain.Entities.Hayirci;
using CanSayNo.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CanSayNo.Tests.Integration;

public sealed record TestUser(int Id, string Email, string Password, HttpClient Client);

/// <summary>Testlerde tekrar eden API adımları: kullanıcı oluşturma, giriş, JSON okuma.</summary>
public static class TestApi
{
    public const string Password = "Test1234!a";
    public const string AdminEmail = "admin@cansayno.com";
    public const string AdminPassword = "Admin123!";

    /// <summary>Ücretsiz senaryo (İş bağlamı) ve premium senaryo — seed'de her zaman var.</summary>
    public const string FreeScenario = "is_001_cuma_mesaji";
    public const string PremiumScenario = "aile_001_evlenmek";

    public static HttpClient NewClient(ApiFactory factory)
    {
        var client = factory.CreateClient();
        // Mobil gibi davran: refresh token gövdede gelir, cookie yazılmaz.
        client.DefaultRequestHeaders.Add("X-Client", "mobile");
        // Her client ayrı bir cihaz: IP başına rate limit'ler testler arasında paylaşılmaz.
        client.DefaultRequestHeaders.Add(TestClientIpStartupFilter.HeaderName, $"2001:db8::{Guid.NewGuid().ToString("N")[..4]}:{Guid.NewGuid().ToString("N")[..4]}");
        return client;
    }

    public static string NewEmail(string prefix = "user") => $"{prefix}-{Guid.NewGuid():N}@test.local";

    public static Task<HttpResponseMessage> RegisterAsync(HttpClient client, string email, string password = Password) =>
        client.PostAsJsonAsync("/api/Auth/register", new
        {
            firstName = "Test",
            lastName = "Kullanici",
            email,
            password,
            confirmPassword = password,
            phoneNumber = (string?)null
        });

    public static Task<HttpResponseMessage> LoginAsync(HttpClient client, string email, string password = Password) =>
        client.PostAsJsonAsync("/api/Auth/login", new { email, password, rememberMe = true });

    /// <summary>Kayıt → e-postayı DB'de doğrulanmış say → giriş. Dönen client yetkili.</summary>
    public static async Task<TestUser> CreateUserAsync(ApiFactory factory, bool premium = false)
    {
        var client = NewClient(factory);
        var email = NewEmail();

        var register = await RegisterAsync(client, email);
        Assert.True(register.IsSuccessStatusCode, $"register {register.StatusCode}: {await register.Content.ReadAsStringAsync()}");

        var userId = await factory.WithDbAsync(async db =>
        {
            var user = await db.Users.SingleAsync(u => u.Email == email);
            user.EmailConfirmed = true;
            user.Status = UserStatus.Active;
            if (premium)
                db.Subscriptions.Add(PremiumSubscription(user.Id));
            await db.SaveChangesAsync();
            return user.Id;
        });

        var token = await LoginForTokenAsync(client, email, Password);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return new TestUser(userId, email, Password, client);
    }

    public static Subscription PremiumSubscription(int userId) => new()
    {
        UserId = userId,
        AppId = "cansayno",
        ProductId = "test_premium",
        Status = SubscriptionStatus.Active,
        StartedAt = DateTime.UtcNow,
        ExpiresAt = DateTime.UtcNow.AddDays(1),
        Provider = "test"
    };

    public static async Task<HttpClient> AdminClientAsync(ApiFactory factory)
    {
        var client = NewClient(factory);
        var token = await LoginForTokenAsync(client, AdminEmail, AdminPassword);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public static async Task<string> LoginForTokenAsync(HttpClient client, string email, string password)
    {
        var login = await LoginAsync(client, email, password);
        var body = await ReadJsonAsync(login);
        Assert.True(login.IsSuccessStatusCode, $"login {login.StatusCode}: {body}");
        return body["data"]!["accessToken"]!.GetValue<string>();
    }

    public static async Task<JsonNode> ReadJsonAsync(HttpResponseMessage response)
    {
        var text = await response.Content.ReadAsStringAsync();
        return JsonNode.Parse(string.IsNullOrWhiteSpace(text) ? "{}" : text)!;
    }

    /// <summary>Hata gövdesindeki kod ("dailyLimitReached" gibi). Başarılı ya da kodsuz cevapta null.</summary>
    public static async Task<string?> ErrorCodeAsync(HttpResponseMessage response)
    {
        try
        {
            var json = await ReadJsonAsync(response);
            return json["error"]?["code"]?.ToString();
        }
        catch (JsonException)
        {
            return null; // rate limiter düz metin döner
        }
    }

    public static Task<HttpResponseMessage> StartSessionAsync(HttpClient client, string scenarioId) =>
        client.PostAsJsonAsync("/api/Sessions", new { scenarioId });

    public static Task<HttpResponseMessage> SendMessageAsync(HttpClient client, Guid sessionId, string content) =>
        client.PostAsJsonAsync($"/api/Sessions/{sessionId}/messages", new { content });

    public static async Task<Guid> StartSessionIdAsync(HttpClient client, string scenarioId)
    {
        var response = await StartSessionAsync(client, scenarioId);
        var body = await ReadJsonAsync(response);
        Assert.True(response.StatusCode == HttpStatusCode.OK, $"start {response.StatusCode}: {body}");
        return Guid.Parse(body["data"]!["id"]!.GetValue<string>());
    }
}
