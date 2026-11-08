using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using static CanSayNo.Tests.Integration.TestApi;

namespace CanSayNo.Tests.Integration;

/// <summary>
/// 6 haneli kodlar (şifre sıfırlama): deneme hakkı DB'de atomik sayılır — paralel tahminlerle
/// 5 deneme sınırı aşılamaz; doğru kod aynı anda iki kez kullanılamaz.
/// </summary>
[Collection(ApiCollection.Name)]
public class OtpCodeTests(ApiFactory factory)
{
    private const string NewPassword = "Yeni1234!b";

    private FakeEmailService Mail => factory.Services.GetRequiredService<FakeEmailService>();

    [Fact]
    public async Task Correct_code_resets_password()
    {
        var user = await CreateUserAsync(factory);
        var client = NewClient(factory);
        var code = await RequestCodeAsync(client, user.Email);

        var reset = await ResetAsync(client, user.Email, code);

        Assert.True(reset.IsSuccessStatusCode, await reset.Content.ReadAsStringAsync());
        Assert.True((await LoginAsync(NewClient(factory), user.Email, NewPassword)).IsSuccessStatusCode);
    }

    [Fact]
    public async Task Parallel_wrong_guesses_burn_the_code()
    {
        var user = await CreateUserAsync(factory);
        var client = NewClient(factory);
        var code = await RequestCodeAsync(client, user.Email);
        var wrong = code == "000000" ? "111111" : "000000";

        // 10 eşzamanlı yanlış tahmin; her biri ayrı "cihaz"dan (IP başına limite takılmasın).
        var guesses = await Task.WhenAll(Enumerable.Range(0, 10)
            .Select(_ => ResetAsync(NewClient(factory), user.Email, wrong)));
        Assert.All(guesses, g => Assert.False(g.IsSuccessStatusCode));

        // Hak bitti: doğru kod da artık kabul edilmez.
        var correct = await ResetAsync(client, user.Email, code);
        Assert.False(correct.IsSuccessStatusCode);
        Assert.True((await LoginAsync(NewClient(factory), user.Email, Password)).IsSuccessStatusCode);
    }

    [Fact]
    public async Task Correct_code_works_only_once_even_in_parallel()
    {
        var user = await CreateUserAsync(factory);
        var code = await RequestCodeAsync(NewClient(factory), user.Email);

        var results = await Task.WhenAll(Enumerable.Range(0, 3)
            .Select(_ => ResetAsync(NewClient(factory), user.Email, code)));

        Assert.Equal(1, results.Count(r => r.IsSuccessStatusCode));
    }

    private async Task<string> RequestCodeAsync(HttpClient client, string email)
    {
        Mail.PasswordCodes.TryRemove(email, out _);
        var response = await client.PostAsJsonAsync("/api/Auth/forgot-password/code", new { email });
        Assert.True(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());
        Assert.True(Mail.PasswordCodes.TryGetValue(email, out var code), "kod e-postası gönderilmedi");
        return code!;
    }

    private static Task<HttpResponseMessage> ResetAsync(HttpClient client, string email, string code) =>
        client.PostAsJsonAsync("/api/Auth/reset-password/code", new { email, code, newPassword = NewPassword });
}
