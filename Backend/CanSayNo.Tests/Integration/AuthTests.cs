using System.Net;
using static CanSayNo.Tests.Integration.TestApi;

namespace CanSayNo.Tests.Integration;

[Collection(ApiCollection.Name)]
public class AuthTests(ApiFactory factory)
{
    [Fact]
    public async Task Unverified_account_with_correct_password_is_told_to_verify_email()
    {
        var client = NewClient(factory);
        var email = NewEmail();
        await RegisterAsync(client, email);

        var response = await LoginAsync(client, email);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("emailNotConfirmed", await ErrorCodeAsync(response));
    }

    [Fact]
    public async Task Unverified_account_with_wrong_password_gets_generic_error()
    {
        var client = NewClient(factory);
        var email = NewEmail();
        await RegisterAsync(client, email);

        var response = await LoginAsync(client, email, "Wrong1234!a");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Wrong_password_and_unknown_email_look_the_same()
    {
        var user = await CreateUserAsync(factory);
        var client = NewClient(factory);

        var wrongPassword = await LoginAsync(client, user.Email, "Wrong1234!a");
        var unknownEmail = await LoginAsync(client, NewEmail("nobody"), "Wrong1234!a");

        Assert.Equal(wrongPassword.StatusCode, unknownEmail.StatusCode);
        Assert.Equal(await wrongPassword.Content.ReadAsStringAsync(), await unknownEmail.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Verified_user_can_log_in_and_read_profile()
    {
        var user = await CreateUserAsync(factory);

        var me = await ReadJsonAsync(await user.Client.GetAsync("/api/Auth/me"));

        Assert.Equal(user.Email, me["data"]!["email"]!.ToString());
    }

    [Fact]
    public async Task Duplicate_email_registration_is_rejected()
    {
        var user = await CreateUserAsync(factory);

        var again = await RegisterAsync(NewClient(factory), user.Email.ToUpperInvariant());

        Assert.False(again.IsSuccessStatusCode);
    }
}
