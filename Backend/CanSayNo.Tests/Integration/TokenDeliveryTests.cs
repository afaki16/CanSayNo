using System.Net;
using System.Net.Http.Json;
using static CanSayNo.Tests.Integration.TestApi;

namespace CanSayNo.Tests.Integration;

/// <summary>
/// Refresh token teslimi (Kickstart #26 + "web'de gövdeden kaldır"): tarayıcı (admin paneli) token'ı
/// YALNIZCA HttpOnly cookie'de alır, gövdede null gelir; mobil (X-Client: mobile) gövdede alır, cookie almaz.
/// Panel akışı: login → boş gövdeli refresh (cookie ile) → logout.
/// </summary>
[Collection(ApiCollection.Name)]
public class TokenDeliveryTests(ApiFactory factory)
{
    private const string CookieName = "refresh_token";

    [Fact]
    public async Task Web_login_puts_refresh_token_only_in_httponly_cookie()
    {
        var user = await CreateUserAsync(factory);
        var web = NewClient(factory, mobile: false);

        var login = await LoginAsync(web, user.Email);
        var body = await ReadJsonAsync(login);

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        Assert.False(string.IsNullOrEmpty(body["data"]!["accessToken"]?.ToString()));
        Assert.Null(body["data"]!["refreshToken"]);

        var setCookie = RefreshSetCookie(login);
        Assert.NotNull(setCookie);
        Assert.Contains("httponly", setCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=strict", setCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("path=/api/auth", setCookie, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Web_refresh_with_cookie_and_empty_body_rotates_the_cookie()
    {
        var user = await CreateUserAsync(factory);
        var web = NewClient(factory, mobile: false);
        var first = CookieValue(RefreshSetCookie(await LoginAsync(web, user.Email))!);

        // Panelin yaptığı gibi: gövde {} — token yalnızca cookie'de.
        var refresh = await PostWithCookieAsync(web, "/api/Auth/refresh-token", first);
        var body = await ReadJsonAsync(refresh);

        Assert.Equal(HttpStatusCode.OK, refresh.StatusCode);
        Assert.False(string.IsNullOrEmpty(body["data"]!["accessToken"]?.ToString()));
        Assert.Null(body["data"]!["refreshToken"]);
        var second = CookieValue(RefreshSetCookie(refresh)!);
        Assert.NotEqual(first, second);

        // Döndürülmüş (eski) token tekrar kullanılamaz.
        var reuse = await PostWithCookieAsync(web, "/api/Auth/refresh-token", first);
        Assert.False(reuse.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Web_logout_clears_cookie_and_token_stops_working()
    {
        var user = await CreateUserAsync(factory);
        var web = NewClient(factory, mobile: false);
        var login = await LoginAsync(web, user.Email);
        var token = CookieValue(RefreshSetCookie(login)!);
        var accessToken = (await ReadJsonAsync(login))["data"]!["accessToken"]!.ToString();

        // Panel logout'u access token + cookie ile çağırır.
        var logout = await PostWithCookieAsync(web, "/api/Auth/logout", token, accessToken);
        var cleared = RefreshSetCookie(logout);
        var refresh = await PostWithCookieAsync(web, "/api/Auth/refresh-token", token);

        Assert.True(logout.IsSuccessStatusCode, await logout.Content.ReadAsStringAsync());
        Assert.NotNull(cleared);
        Assert.Equal(string.Empty, CookieValue(cleared));
        Assert.False(refresh.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Mobile_gets_refresh_token_in_body_and_no_cookie()
    {
        var user = await CreateUserAsync(factory);
        var mobile = NewClient(factory);

        var login = await LoginAsync(mobile, user.Email);
        var data = (await ReadJsonAsync(login))["data"]!;

        Assert.Null(RefreshSetCookie(login));
        var refreshToken = data["refreshToken"]!.ToString();
        Assert.False(string.IsNullOrEmpty(refreshToken));

        var refresh = await mobile.PostAsJsonAsync("/api/Auth/refresh-token",
            new { accessToken = data["accessToken"]!.ToString(), refreshToken });
        var refreshed = (await ReadJsonAsync(refresh))["data"]!;

        Assert.Equal(HttpStatusCode.OK, refresh.StatusCode);
        Assert.Null(RefreshSetCookie(refresh));
        Assert.False(string.IsNullOrEmpty(refreshed["refreshToken"]?.ToString()));
        Assert.NotEqual(refreshToken, refreshed["refreshToken"]!.ToString());
    }

    private static string? RefreshSetCookie(HttpResponseMessage response) =>
        response.Headers.TryGetValues("Set-Cookie", out var values)
            ? values.FirstOrDefault(v => v.StartsWith(CookieName + "=", StringComparison.Ordinal))
            : null;

    private static string CookieValue(string setCookie) =>
        Uri.UnescapeDataString(setCookie[(CookieName.Length + 1)..].Split(';')[0]);

    private static Task<HttpResponseMessage> PostWithCookieAsync(
        HttpClient client, string url, string token, string? accessToken = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(new { }) };
        request.Headers.Add("Cookie", $"{CookieName}={Uri.EscapeDataString(token)}");
        if (accessToken is not null)
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        return client.SendAsync(request);
    }
}
