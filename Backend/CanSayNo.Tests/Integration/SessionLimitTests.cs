using System.Net;
using CanSayNo.Application.Common.Settings;
using CanSayNo.Domain.Entities.Hayirci;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using static CanSayNo.Tests.Integration.TestApi;

namespace CanSayNo.Tests.Integration;

/// <summary>Premium kilidi, ücretsiz günlük limit, premium adil kullanım sınırı, LLM hız sınırı.</summary>
[Collection(ApiCollection.Name)]
public class SessionLimitTests(ApiFactory factory)
{
    private HayirciSettings Settings => factory.Services.GetRequiredService<HayirciSettings>();

    [Fact]
    public async Task Free_user_cannot_start_premium_scenario()
    {
        var user = await CreateUserAsync(factory);

        var response = await StartSessionAsync(user.Client, PremiumScenario);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("premiumRequired", await ErrorCodeAsync(response));
    }

    [Fact]
    public async Task Free_user_gets_one_practice_per_day()
    {
        var user = await CreateUserAsync(factory);
        await StartSessionIdAsync(user.Client, "is_001_cuma_mesaji");

        var second = await StartSessionAsync(user.Client, "is_002_toplanti_uzatma");

        Assert.Equal(HttpStatusCode.TooManyRequests, second.StatusCode);
        Assert.Equal("dailyLimitReached", await ErrorCodeAsync(second));
    }

    [Fact]
    public async Task Premium_user_can_start_premium_scenario()
    {
        var user = await CreateUserAsync(factory, premium: true);

        var response = await StartSessionAsync(user.Client, PremiumScenario);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Premium_user_stops_at_daily_fair_use_limit()
    {
        var user = await CreateUserAsync(factory, premium: true);
        await SetTodaysUsageAsync(user.Id, Settings.PremiumDailySessionLimit - 1);

        var last = await StartSessionAsync(user.Client, "komsu_001_anahtar_birak");
        var over = await StartSessionAsync(user.Client, "komsu_002_ev_arkadasi_sevgili");

        Assert.Equal(HttpStatusCode.OK, last.StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, over.StatusCode);
        Assert.Equal("fairUseLimitReached", await ErrorCodeAsync(over));
    }

    [Fact]
    public async Task Parallel_starts_cannot_exceed_the_limit()
    {
        var user = await CreateUserAsync(factory, premium: true);
        await SetTodaysUsageAsync(user.Id, Settings.PremiumDailySessionLimit - 1);

        string[] scenarios =
        [
            "okul_001_grup_odevi", "okul_002_odev_kopya", "okul_003_hoca_ucretsiz_is",
            "okul_004_kulup_baskanligi", "okul_005_ders_notu_israr", "dijital_001_grup_davet"
        ];
        var responses = await Task.WhenAll(scenarios.Select(s => StartSessionAsync(user.Client, s)));

        Assert.Equal(1, responses.Count(r => r.StatusCode == HttpStatusCode.OK));
        Assert.All(responses.Where(r => r.StatusCode != HttpStatusCode.OK),
            r => Assert.Equal(HttpStatusCode.TooManyRequests, r.StatusCode));

        var count = await factory.WithDbAsync(db => db.DailyUsage
            .Where(d => d.UserId == user.Id).Select(d => d.SessionsCount).SingleAsync());
        Assert.Equal(Settings.PremiumDailySessionLimit, count);
    }

    [Fact]
    public async Task Llm_endpoints_are_rate_limited_per_user()
    {
        var user = await CreateUserAsync(factory);
        var other = await CreateUserAsync(factory);
        var missing = Guid.NewGuid(); // 404 de olsa istek sayılır, LLM'e gitmez

        for (var i = 0; i < Settings.AiRequestsPerMinute; i++)
        {
            var ok = await SendMessageAsync(user.Client, missing, "selam");
            Assert.Equal(HttpStatusCode.NotFound, ok.StatusCode);
        }

        var limited = await SendMessageAsync(user.Client, missing, "selam");
        var evaluateLimited = await user.Client.PostAsync($"/api/Sessions/{missing}/evaluate", null);
        var otherUser = await SendMessageAsync(other.Client, missing, "selam");

        Assert.Equal(HttpStatusCode.TooManyRequests, limited.StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, evaluateLimited.StatusCode);
        // Sınır kullanıcı başına: aynı IP'deki başka kullanıcı etkilenmez.
        Assert.Equal(HttpStatusCode.NotFound, otherUser.StatusCode);
    }

    private Task SetTodaysUsageAsync(int userId, int sessionsCount) =>
        factory.WithDbAsync(async db =>
        {
            db.DailyUsage.Add(new DailyUsage
            {
                UserId = userId,
                Date = Settings.GetUsageDate(DateTime.UtcNow),
                SessionsCount = sessionsCount
            });
            await db.SaveChangesAsync();
        });
}
