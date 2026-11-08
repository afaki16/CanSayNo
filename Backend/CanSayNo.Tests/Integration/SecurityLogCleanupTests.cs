using CanSayNo.Domain.Entities;
using CanSayNo.Infrastructure.Services.Maintenance;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using static CanSayNo.Tests.Integration.TestApi;

namespace CanSayNo.Tests.Integration;

/// <summary>
/// Gizlilik politikası §5: güvenlik kayıtları en fazla 90 gün saklanır. Temizlik yalnızca eski
/// kayıtları siler; geçerli oturum anahtarlarına ve yeni giriş denemelerine dokunmaz.
/// </summary>
[Collection(ApiCollection.Name)]
public class SecurityLogCleanupTests(ApiFactory factory)
{
    [Fact]
    public async Task Purge_deletes_only_records_older_than_retention()
    {
        var user = await CreateUserAsync(factory);
        var now = DateTime.UtcNow;
        var email = $"cleanup-{Guid.NewGuid():N}@test.local";
        var oldToken = $"old-{Guid.NewGuid():N}";
        var liveToken = $"live-{Guid.NewGuid():N}";

        await factory.WithDbAsync(async db =>
        {
            db.LoginAttempts.AddRange(
                new LoginAttempt { Email = email, IpAddress = "203.0.113.1", AttemptedAt = now.AddDays(-91) },
                new LoginAttempt { Email = email, IpAddress = "203.0.113.1", AttemptedAt = now.AddDays(-89) });
            db.RefreshTokens.AddRange(
                new RefreshToken { UserId = user.Id, Token = oldToken, ExpiryDate = now.AddDays(-95) },
                new RefreshToken { UserId = user.Id, Token = liveToken, ExpiryDate = now.AddDays(30) });
            await db.SaveChangesAsync();
        });

        using (var scope = factory.Services.CreateScope())
        {
            var cleanup = scope.ServiceProvider.GetRequiredService<SecurityLogCleanup>();
            var result = await cleanup.PurgeAsync(now);
            Assert.True(result.LoginAttempts >= 1);
            Assert.True(result.RefreshTokens >= 1);
        }

        var (attempts, tokens) = await factory.WithDbAsync(async db => (
            await db.LoginAttempts.IgnoreQueryFilters().Where(a => a.Email == email).Select(a => a.AttemptedAt).ToListAsync(),
            await db.RefreshTokens.IgnoreQueryFilters().Where(t => t.Token == oldToken || t.Token == liveToken)
                .Select(t => t.Token).ToListAsync()));

        Assert.Single(attempts);
        Assert.True(attempts[0] > now.AddDays(-90));
        Assert.Equal([liveToken], tokens);

        // Kullanıcının asıl oturumu hâlâ çalışır.
        Assert.True((await user.Client.GetAsync("/api/Auth/me")).IsSuccessStatusCode);
    }
}
