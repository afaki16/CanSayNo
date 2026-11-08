using System.Net;
using CanSayNo.Application.Interfaces;
using Microsoft.AspNetCore.Builder;
using CanSayNo.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

namespace CanSayNo.Tests.Integration;

/// <summary>
/// Gerçek API'yi (tüm middleware, auth, rate limit, EF + migration + seed) geçici bir Postgres
/// container'ına karşı çalıştırır. Geliştirme veritabanına DOKUNMAZ. LLM servisleri sahtedir —
/// testler Anthropic'e istek atmaz, para harcamaz. Docker çalışıyor olmalı.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _db = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    public async Task InitializeAsync()
    {
        await _db.StartAsync();

        // Ortam değişkenleri appsettings.*.json ve user-secrets'ı ezer; Program config'i
        // host kurulurken okuduğu için ilk CreateClient'tan ÖNCE ayarlanmalı.
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", _db.GetConnectionString());
        Environment.SetEnvironmentVariable("Anthropic__ApiKey", "");
        Environment.SetEnvironmentVariable("RevenueCat__WebhookAuthorization", WebhookSecret);
        Environment.SetEnvironmentVariable("NotificationService__Mail__Host", "");
        Environment.SetEnvironmentVariable("Logging__LogLevel__Default", "Warning");
        Environment.SetEnvironmentVariable("Logging__LogLevel__Microsoft.EntityFrameworkCore", "Warning");

        // Host'u şimdi kur: migration + seed burada bir kez çalışır.
        _ = Server;
    }

    public const string WebhookSecret = "test-webhook-secret";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Development: rate limit'ler gevşek (tüm test istekleri aynı "IP"den gelir).
        builder.UseEnvironment("Development");

        builder.ConfigureTestServices(services =>
        {
            services.AddTransient<IStartupFilter, TestClientIpStartupFilter>();

            services.RemoveAll<ICharacterChatService>();
            services.RemoveAll<ISessionEvaluator>();
            services.AddSingleton<ICharacterChatService, FakeCharacterChatService>();
            services.AddSingleton<ISessionEvaluator, FakeSessionEvaluator>();
        });
    }

    /// <summary>Test içinde doğrudan DB'ye erişim (kullanıcı aktive etme, abonelik ekleme...).</summary>
    public async Task WithDbAsync(Func<ApplicationDbContext, Task> action)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await action(db);
    }

    public async Task<T> WithDbAsync<T>(Func<ApplicationDbContext, Task<T>> action)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await action(db);
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await _db.DisposeAsync();
    }
}

/// <summary>
/// TestServer'da RemoteIpAddress boş; tüm istekler IP başına rate limit'te ("unknown") aynı
/// kovaya düşer. Her test client'ı <see cref="HeaderName"/> ile kendi IP'sini taşır — gerçek
/// hayattaki gibi farklı cihazlar. Pipeline'ın en başında (forwarded headers'tan önce) çalışır.
/// </summary>
public sealed class TestClientIpStartupFilter : IStartupFilter
{
    public const string HeaderName = "X-Test-Client-Ip";

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
    {
        app.Use(async (context, nextMiddleware) =>
        {
            if (IPAddress.TryParse(context.Request.Headers[HeaderName], out var ip))
                context.Connection.RemoteIpAddress = ip;
            await nextMiddleware();
        });
        next(app);
    };
}

/// <summary>Tüm entegrasyon testleri tek API + tek container paylaşır (her sınıf için yeniden kurmak ~10 sn).</summary>
[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<ApiFactory>
{
    public const string Name = "api";
}
