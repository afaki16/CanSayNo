using System.Net;
using System.Net.Http.Headers;
using CanSayNo.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using static CanSayNo.Tests.Integration.TestApi;

namespace CanSayNo.Tests.Integration;

/// <summary>
/// İçerik dili: senaryo listesi Accept-Language'a göre gelir (varsayılan Türkçe), karakter ve
/// değerlendirme prompt'u senaryonun dilinde kurulur, hata mesajları istek dilinde döner.
/// </summary>
[Collection(ApiCollection.Name)]
public class LanguageTests(ApiFactory factory)
{
    private static void Use(HttpClient client, string language)
    {
        client.DefaultRequestHeaders.AcceptLanguage.Clear();
        client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue(language));
    }

    private static async Task<List<string>> ScenarioIdsAsync(HttpClient client, string? query = null)
    {
        var json = await ReadJsonAsync(await client.GetAsync("/api/Scenarios" + query));
        return json["data"]!.AsArray().Select(s => s!["id"]!.ToString()).ToList();
    }

    [Fact]
    public async Task Default_language_lists_only_turkish_scenarios()
    {
        var user = await CreateUserAsync(factory);

        var ids = await ScenarioIdsAsync(user.Client);

        Assert.Equal(65, ids.Count);
        Assert.DoesNotContain(ids, id => id.StartsWith("en_"));
    }

    [Fact]
    public async Task English_lists_only_english_scenarios()
    {
        var user = await CreateUserAsync(factory);
        Use(user.Client, "en-US");

        var ids = await ScenarioIdsAsync(user.Client);
        var freeIds = await ScenarioIdsAsync(user.Client, "?isFreeOnly=true");

        Assert.Equal(45, ids.Count);
        Assert.All(ids, id => Assert.StartsWith("en_", id));
        Assert.Equal(10, freeIds.Count);
        Assert.All(freeIds, id => Assert.StartsWith("en_is_", id));
    }

    [Fact]
    public async Task Unsupported_language_falls_back_to_turkish()
    {
        var user = await CreateUserAsync(factory);
        Use(user.Client, "de-DE");

        var ids = await ScenarioIdsAsync(user.Client);

        Assert.Equal(65, ids.Count);
    }

    [Fact]
    public async Task English_practice_uses_english_prompts()
    {
        var user = await CreateUserAsync(factory);
        Use(user.Client, "en");
        var chat = (FakeCharacterChatService)factory.Services.GetRequiredService<ICharacterChatService>();

        var sessionId = await StartSessionIdAsync(user.Client, "en_is_001_friday_deadline");
        var before = chat.Prompts.Count;
        var reply = await SendMessageAsync(user.Client, sessionId, "Sorry, I can't this weekend. I'll start first thing Monday.");

        Assert.Equal(HttpStatusCode.OK, reply.StatusCode);
        var prompt = chat.Prompts.Skip(before).Single(p => p.Turns[^1].Text.StartsWith("Sorry, I can't"));
        Assert.StartsWith("You are the other side of a role-play practice", prompt.Rules);
        Assert.Contains("Your character:", prompt.Scenario);
        Assert.StartsWith("Round 1/3", prompt.Round);
    }

    [Fact]
    public async Task Turkish_practice_still_uses_turkish_prompts()
    {
        var user = await CreateUserAsync(factory);
        var chat = (FakeCharacterChatService)factory.Services.GetRequiredService<ICharacterChatService>();

        var sessionId = await StartSessionIdAsync(user.Client, FreeScenario);
        var before = chat.Prompts.Count;
        await SendMessageAsync(user.Client, sessionId, "Bu hafta sonu müsait değilim.");

        var prompt = chat.Prompts.Skip(before).Single(p => p.Turns[^1].Text == "Bu hafta sonu müsait değilim.");
        Assert.StartsWith("Sen \"CanSayNo\" adlı bir mobil uygulamada", prompt.Rules);
        Assert.StartsWith("Raunt 1/3", prompt.Round);
    }

    [Fact]
    public async Task Error_messages_follow_request_language()
    {
        var user = await CreateUserAsync(factory);
        Use(user.Client, "en");

        var response = await StartSessionAsync(user.Client, "en_aile_001_holiday_visit"); // premium
        var message = (await ReadJsonAsync(response))["error"]!["errors"]!.ToJsonString();

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Contains("premium", message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotMatch("[çğıöşü]", message);
    }
}
