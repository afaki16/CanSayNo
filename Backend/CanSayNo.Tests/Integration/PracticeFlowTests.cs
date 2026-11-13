using System.Net;
using System.Net.Http.Json;
using static CanSayNo.Tests.Integration.TestApi;

namespace CanSayNo.Tests.Integration;

[Collection(ApiCollection.Name)]
public class PracticeFlowTests(ApiFactory factory)
{
    [Fact]
    public async Task Full_practice_start_messages_complete_evaluate_updates_stats()
    {
        var user = await CreateUserAsync(factory);
        var sessionId = await StartSessionIdAsync(user.Client, FreeScenario);

        // Açılış mesajı karakterden gelir.
        var session = await ReadJsonAsync(await user.Client.GetAsync($"/api/Sessions/{sessionId}"));
        Assert.Equal("ai", session["data"]!["messages"]![0]!["role"]!.ToString());
        var maxRounds = session["data"]!["maxRounds"]!.GetValue<int>();
        Assert.True(maxRounds > 0);

        // Değerlendirme, pratik bitmeden yapılamaz.
        var early = await user.Client.PostAsync($"/api/Sessions/{sessionId}/evaluate", null);
        Assert.Equal(HttpStatusCode.Conflict, early.StatusCode);
        Assert.Equal("sessionNotCompleted", await ErrorCodeAsync(early));

        string status = "active";
        for (var round = 1; round <= maxRounds; round++)
        {
            var reply = await SendMessageAsync(user.Client, sessionId, $"Hayır, bu sefer olmaz. ({round})");
            var body = await ReadJsonAsync(reply);
            Assert.True(reply.StatusCode == HttpStatusCode.OK, body.ToJsonString());
            Assert.Equal(round, body["data"]!["currentRound"]!.GetValue<int>());
            status = body["data"]!["status"]!.ToString();

            // Karakter cevabı ayrı balonlar + ruh hali olarak gelir; Content birleşimidir.
            var last = body["data"]!["messages"]!.AsArray().Last()!;
            Assert.Equal("ai", last["role"]!.ToString());
            Assert.Equal(FakeCharacterChatService.ReplyParts, last["parts"]!.AsArray().Select(p => p!.ToString()));
            Assert.Equal("insistent", last["mood"]!.ToString());
            Assert.Equal(FakeCharacterChatService.ReplyText, last["content"]!.ToString());
        }
        Assert.Equal("completed", status);

        // Balonlar ve ruh hali kalıcı (JSONB): oturum sonradan açılınca da aynı.
        var reloaded = await ReadJsonAsync(await user.Client.GetAsync($"/api/Sessions/{sessionId}"));
        var reloadedLast = reloaded["data"]!["messages"]!.AsArray().Last()!;
        Assert.Equal(2, reloadedLast["parts"]!.AsArray().Count);
        Assert.Equal("insistent", reloadedLast["mood"]!.ToString());
        // Açılış mesajı tek balon: parts boş (AutoMapper null listeyi [] yapar), mood yok.
        Assert.Empty(reloaded["data"]!["messages"]![0]!["parts"]!.AsArray());
        Assert.Null(reloaded["data"]!["messages"]![0]!["mood"]);

        // Bitmiş pratiğe mesaj atılamaz.
        var afterEnd = await SendMessageAsync(user.Client, sessionId, "Bir şey daha");
        Assert.Equal(HttpStatusCode.Conflict, afterEnd.StatusCode);
        Assert.Equal("sessionNotActive", await ErrorCodeAsync(afterEnd));

        var callsBefore = Evaluator.Calls;
        var evaluated = await ReadJsonAsync(await user.Client.PostAsync($"/api/Sessions/{sessionId}/evaluate", null));
        var evaluation = evaluated["data"]!["evaluation"]!;
        Assert.Equal(FakeSessionEvaluator.ExpectedTotalScore, evaluation["totalScore"]!.GetValue<int>());
        // Aralık dışı puan 0-100'e sıkıştırılır.
        Assert.All(evaluation["scores"]!.AsObject(), kv => Assert.InRange(kv.Value!.GetValue<int>(), 0, 100));

        // İdempotent: ikinci çağrı LLM'e gitmez, aynı puanı döner.
        var again = await ReadJsonAsync(await user.Client.PostAsync($"/api/Sessions/{sessionId}/evaluate", null));
        Assert.Equal(FakeSessionEvaluator.ExpectedTotalScore, again["data"]!["evaluation"]!["totalScore"]!.GetValue<int>());
        Assert.Equal(callsBefore + 1, Evaluator.Calls);

        var stats = (await ReadJsonAsync(await user.Client.GetAsync("/api/Stats/me")))["data"]!;
        Assert.Equal(1, stats["totalCompletedSessions"]!.GetValue<int>());
        Assert.Equal(1, stats["currentStreak"]!.GetValue<int>());
        Assert.True(stats["practicedToday"]!.GetValue<bool>());
        Assert.Contains("firstNo", stats["unlockedBadges"]!.AsArray().Select(b => b!.ToString()));
    }

    [Fact]
    public async Task Message_longer_than_500_characters_is_rejected()
    {
        var user = await CreateUserAsync(factory);
        var sessionId = await StartSessionIdAsync(user.Client, FreeScenario);

        var response = await SendMessageAsync(user.Client, sessionId, new string('a', 501));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Reopening_unstarted_practice_returns_same_session_without_using_daily_quota()
    {
        var user = await CreateUserAsync(factory);

        var first = await StartSessionIdAsync(user.Client, FreeScenario);
        var second = await StartSessionIdAsync(user.Client, FreeScenario);

        Assert.Equal(first, second);
    }

    [Fact]
    public async Task My_sessions_lists_only_own_sessions()
    {
        var a = await CreateUserAsync(factory);
        var b = await CreateUserAsync(factory);
        var aSession = await StartSessionIdAsync(a.Client, FreeScenario);
        await StartSessionIdAsync(b.Client, FreeScenario);

        var list = await ReadJsonAsync(await a.Client.GetAsync("/api/Sessions/my?page=1&pageSize=50"));
        var ids = list["data"]!["items"]!.AsArray().Select(s => s!["id"]!.ToString()).ToList();

        Assert.Equal([aSession.ToString()], ids);
    }

    private FakeSessionEvaluator Evaluator =>
        (FakeSessionEvaluator)factory.Services.GetService(typeof(Application.Interfaces.ISessionEvaluator))!;
}
