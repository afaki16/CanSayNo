using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using static CanSayNo.Tests.Integration.TestApi;

namespace CanSayNo.Tests.Integration;

/// <summary>
/// Mobil kayıt doğrulaması 6 haneli kodla (web sayfası gerekmez); web istemcisi linkle devam eder.
/// </summary>
[Collection(ApiCollection.Name)]
public class RegistrationCodeTests(ApiFactory factory)
{
    private FakeEmailService Mail => factory.Services.GetRequiredService<FakeEmailService>();

    [Fact]
    public async Task Mobile_registration_is_verified_with_the_emailed_code()
    {
        var client = NewClient(factory);
        var email = NewEmail();
        var register = await RegisterAsync(client, email);
        Assert.True(register.IsSuccessStatusCode, await register.Content.ReadAsStringAsync());
        Assert.True(Mail.RegistrationCodes.TryGetValue(email, out var code), "kayıt kodu gönderilmedi");

        Assert.Equal("emailNotConfirmed", await ErrorCodeAsync(await LoginAsync(client, email)));

        var verify = await VerifyAsync(client, email, code!);
        Assert.True(verify.IsSuccessStatusCode, await verify.Content.ReadAsStringAsync());
        Assert.True((await LoginAsync(client, email)).IsSuccessStatusCode);

        // Kod tek kullanımlık
        Assert.Equal(HttpStatusCode.BadRequest, (await VerifyAsync(client, email, code!)).StatusCode);
    }

    [Fact]
    public async Task Wrong_code_and_unknown_email_get_the_same_answer()
    {
        var client = NewClient(factory);
        var email = NewEmail();
        await RegisterAsync(client, email);
        var code = Mail.RegistrationCodes[email];
        var wrong = code == "000000" ? "111111" : "000000";

        var wrongCode = await VerifyAsync(client, email, wrong);
        var unknownEmail = await VerifyAsync(client, NewEmail(), wrong);

        Assert.Equal(HttpStatusCode.BadRequest, wrongCode.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, unknownEmail.StatusCode);
        Assert.Equal(await ErrorMessageAsync(wrongCode), await ErrorMessageAsync(unknownEmail));
        Assert.Equal("emailNotConfirmed", await ErrorCodeAsync(await LoginAsync(client, email)));
    }

    [Fact]
    public async Task Resend_within_cooldown_keeps_the_first_code_valid()
    {
        var client = NewClient(factory);
        var email = NewEmail();
        await RegisterAsync(client, email);
        var first = Mail.RegistrationCodes[email];

        var resend = await client.PostAsJsonAsync("/api/Auth/resend-verification", new { email });

        Assert.True(resend.IsSuccessStatusCode);
        Assert.True((await VerifyAsync(client, email, first)).IsSuccessStatusCode);
    }

    [Fact]
    public async Task Web_registration_still_gets_a_link_not_a_code()
    {
        var email = NewEmail();
        var register = await RegisterAsync(NewClient(factory, mobile: false), email);

        Assert.True(register.IsSuccessStatusCode, await register.Content.ReadAsStringAsync());
        Assert.False(Mail.RegistrationCodes.ContainsKey(email));
    }

    private static Task<HttpResponseMessage> VerifyAsync(HttpClient client, string email, string code) =>
        client.PostAsJsonAsync("/api/Auth/verify-email/code", new { email, code });

    private static async Task<string?> ErrorMessageAsync(HttpResponseMessage response) =>
        (await ReadJsonAsync(response))["error"]?.ToJsonString();
}
