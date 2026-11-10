using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using CanSayNo.Domain.Common.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using static CanSayNo.Tests.Integration.TestApi;

namespace CanSayNo.Tests.Integration;

/// <summary>
/// 2026-11-07 güvenlik testinin (geçici betik) kalıcı hâli: kimlik doğrulama, başkasının
/// verisine erişim, yetki, gizli alan sızıntısı, webhook.
/// </summary>
[Collection(ApiCollection.Name)]
public class SecurityTests(ApiFactory factory)
{
    [Fact]
    public async Task Health_is_public_and_checks_database()
    {
        var response = await NewClient(factory).GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("""{"status":"ok"}""", await response.Content.ReadAsStringAsync());
    }

    [Theory]
    [InlineData("/api/Scenarios")]
    [InlineData("/api/Sessions/my")]
    [InlineData("/api/Stats/me")]
    [InlineData("/api/Subscriptions/me")]
    [InlineData("/api/Account/export")]
    public async Task Protected_endpoints_require_token(string url)
    {
        var response = await NewClient(factory).GetAsync(url);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Token_signed_with_another_key_is_rejected()
    {
        var user = await CreateUserAsync(factory);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("attacker-key-attacker-key-attacker-key-123"));
        var forged = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
            issuer: "JWTCanSayNo",
            audience: "JWTCanSayNoClients",
            claims: [new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), new Claim(ClaimTypes.Role, "SuperAdmin")],
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)));

        Assert.Equal(HttpStatusCode.Unauthorized, (await GetWithTokenAsync("/api/Stats/me", forged)).StatusCode);
    }

    [Fact]
    public async Task Unsigned_alg_none_token_is_rejected()
    {
        var user = await CreateUserAsync(factory);
        static string B64(string s) => Base64UrlEncoder.Encode(s);
        var exp = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds();
        var token = $"{B64("{\"alg\":\"none\",\"typ\":\"JWT\"}")}." +
                    $"{B64($"{{\"{ClaimTypes.NameIdentifier}\":\"{user.Id}\",\"iss\":\"JWTCanSayNo\",\"aud\":\"JWTCanSayNoClients\",\"exp\":{exp}}}")}.";

        Assert.Equal(HttpStatusCode.Unauthorized, (await GetWithTokenAsync("/api/Stats/me", token)).StatusCode);
    }

    [Fact]
    public async Task Users_cannot_touch_each_others_sessions()
    {
        var owner = await CreateUserAsync(factory);
        var attacker = await CreateUserAsync(factory);
        var sessionId = await StartSessionIdAsync(owner.Client, FreeScenario);

        var read = await attacker.Client.GetAsync($"/api/Sessions/{sessionId}");
        var message = await SendMessageAsync(attacker.Client, sessionId, "Benim artık");
        var evaluate = await attacker.Client.PostAsync($"/api/Sessions/{sessionId}/evaluate", null);

        Assert.Equal(HttpStatusCode.NotFound, read.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, message.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, evaluate.StatusCode);

        // Sahibinin oturumu değişmedi (yalnızca açılış mesajı).
        var own = await ReadJsonAsync(await owner.Client.GetAsync($"/api/Sessions/{sessionId}"));
        Assert.Single(own["data"]!["messages"]!.AsArray());
    }

    [Theory]
    [InlineData("/api/admin/scenarios")]
    [InlineData("/api/admin/dashboard")]
    [InlineData("/api/admin/users/1/overview")]
    [InlineData("/api/Users")]
    [InlineData("/api/Roles")]
    public async Task Regular_user_cannot_use_admin_endpoints(string url)
    {
        var user = await CreateUserAsync(factory);

        var response = await user.Client.GetAsync(url);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Admin_can_use_admin_endpoints()
    {
        var admin = await AdminClientAsync(factory);

        var response = await admin.GetAsync("/api/admin/scenarios");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Pressure_tactics_never_reach_the_client()
    {
        var user = await CreateUserAsync(factory);
        var sessionId = await StartSessionIdAsync(user.Client, FreeScenario);

        string[] bodies =
        [
            await user.Client.GetStringAsync("/api/Scenarios"),
            await user.Client.GetStringAsync($"/api/Scenarios/{FreeScenario}"),
            await user.Client.GetStringAsync("/api/Scenarios/by-context/is"),
            await user.Client.GetStringAsync($"/api/Sessions/{sessionId}"),
            await user.Client.GetStringAsync("/api/Account/export")
        ];

        Assert.All(bodies, body => Assert.DoesNotContain("tactic", body, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Registration_ignores_role_and_tenant_fields()
    {
        var client = NewClient(factory);
        var email = NewEmail("inject");

        var response = await client.PostAsJsonAsync("/api/Auth/register", new
        {
            firstName = "Test",
            lastName = "Kullanici",
            email,
            password = Password,
            confirmPassword = Password,
            tenantId = 1,
            roles = new[] { "SuperAdmin" },
            roleIds = new[] { 1 },
            status = "active",
            emailConfirmed = true
        });
        Assert.True(response.IsSuccessStatusCode);

        var user = await factory.WithDbAsync(db => db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .SingleAsync(u => u.Email == email));

        Assert.Null(user.TenantId);
        Assert.False(user.EmailConfirmed);
        Assert.Equal(UserStatus.PendingVerification, user.Status);
        Assert.DoesNotContain(user.UserRoles, ur => ur.Role.Name is "SuperAdmin" or "Admin");
    }

    [Fact]
    public async Task Webhook_requires_the_shared_secret()
    {
        var client = NewClient(factory);
        var payload = new { api_version = "1.0", @event = new { id = "e1", type = "INITIAL_PURCHASE", app_user_id = "1" } };

        var missing = await client.PostAsJsonAsync("/api/Webhooks/revenuecat", payload);

        var wrong = new HttpRequestMessage(HttpMethod.Post, "/api/Webhooks/revenuecat") { Content = JsonContent.Create(payload) };
        wrong.Headers.TryAddWithoutValidation("Authorization", "Bearer not-the-secret");
        var wrongResponse = await client.SendAsync(wrong);

        Assert.Equal(HttpStatusCode.Unauthorized, missing.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, wrongResponse.StatusCode);
    }

    [Fact]
    public async Task Account_delete_requires_correct_password()
    {
        var user = await CreateUserAsync(factory);

        var request = new HttpRequestMessage(HttpMethod.Delete, "/api/Account")
        {
            Content = JsonContent.Create(new { password = "Wrong1234!a" })
        };
        var response = await user.Client.SendAsync(request);

        Assert.False(response.IsSuccessStatusCode);
        Assert.True(await factory.WithDbAsync(db => db.Users.AnyAsync(u => u.Id == user.Id)));
    }

    private async Task<HttpResponseMessage> GetWithTokenAsync(string url, string token)
    {
        var client = NewClient(factory);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await client.GetAsync(url);
    }
}
