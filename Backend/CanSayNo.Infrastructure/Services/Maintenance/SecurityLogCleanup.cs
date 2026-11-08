using CanSayNo.Domain.Entities;
using CanSayNo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CanSayNo.Infrastructure.Services.Maintenance;

/// <summary>Veri saklama ayarları — appsettings "DataRetention".</summary>
public class DataRetentionSettings
{
    public const string SectionName = "DataRetention";

    /// <summary>
    /// Güvenlik kayıtlarının (giriş denemeleri, süresi dolmuş oturum anahtarları ve kodlar) en uzun
    /// saklanma süresi. Gizlilik politikasında yazan süreyle (docs/store/gizlilik-politikasi.md §5)
    /// AYNI tutulmalı.
    /// </summary>
    public int SecurityLogDays { get; set; } = 90;

    /// <summary>Temizliğin çalışma aralığı (saat).</summary>
    public int IntervalHours { get; set; } = 24;
}

/// <summary>
/// Saklama süresini aşan güvenlik kayıtlarını siler. Yalnızca artık hiçbir işe yaramayan kayıtlar
/// gider: süresi dolalı en az <see cref="DataRetentionSettings.SecurityLogDays"/> gün olmuş anahtar
/// ve kodlar (refresh token en fazla 60 gün geçerli — reuse detection'a dokunmaz) ve o kadar eski
/// giriş denemeleri. Soft-delete filtresi atlanır: silinmiş işaretli kayıtlar da temizlenir.
/// </summary>
public class SecurityLogCleanup(ApplicationDbContext db, DataRetentionSettings settings)
{
    public sealed record Result(int LoginAttempts, int RefreshTokens, int VerificationTokens, int PasswordResetTokens)
    {
        public int Total => LoginAttempts + RefreshTokens + VerificationTokens + PasswordResetTokens;
    }

    public async Task<Result> PurgeAsync(DateTime utcNow, CancellationToken cancellationToken = default)
    {
        var cutoff = utcNow.AddDays(-settings.SecurityLogDays);

        var loginAttempts = await db.Set<LoginAttempt>().IgnoreQueryFilters()
            .Where(a => a.AttemptedAt < cutoff)
            .ExecuteDeleteAsync(cancellationToken);

        var refreshTokens = await db.Set<RefreshToken>().IgnoreQueryFilters()
            .Where(t => t.ExpiryDate < cutoff)
            .ExecuteDeleteAsync(cancellationToken);

        var verificationTokens = await db.Set<UserVerificationToken>().IgnoreQueryFilters()
            .Where(t => t.ExpiresAt < cutoff)
            .ExecuteDeleteAsync(cancellationToken);

        var passwordResetTokens = await db.Set<PasswordResetToken>().IgnoreQueryFilters()
            .Where(t => t.ExpiresAt < cutoff)
            .ExecuteDeleteAsync(cancellationToken);

        return new Result(loginAttempts, refreshTokens, verificationTokens, passwordResetTokens);
    }
}

/// <summary>Temizliği uygulama açıldıktan kısa süre sonra ve sonra her gün çalıştırır.</summary>
public class SecurityLogCleanupService(
    IServiceScopeFactory scopeFactory,
    DataRetentionSettings settings,
    ILogger<SecurityLogCleanupService> logger) : BackgroundService
{
    private static readonly TimeSpan StartupDelay = TimeSpan.FromMinutes(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await Task.Delay(StartupDelay, stoppingToken);
            using var timer = new PeriodicTimer(TimeSpan.FromHours(Math.Max(1, settings.IntervalHours)));
            do
            {
                await RunOnceAsync(stoppingToken);
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException)
        {
            // Uygulama kapanıyor.
        }
    }

    private async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var cleanup = scope.ServiceProvider.GetRequiredService<SecurityLogCleanup>();
            var result = await cleanup.PurgeAsync(DateTime.UtcNow, cancellationToken);
            if (result.Total > 0)
                logger.LogInformation(
                    "Security log cleanup ({Days} days): {LoginAttempts} login attempts, {RefreshTokens} refresh tokens, " +
                    "{VerificationTokens} verification tokens, {PasswordResetTokens} password reset tokens deleted",
                    settings.SecurityLogDays, result.LoginAttempts, result.RefreshTokens,
                    result.VerificationTokens, result.PasswordResetTokens);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Bir sonraki turda tekrar denenir; uygulamayı düşürmez.
            logger.LogError(ex, "Security log cleanup failed");
        }
    }
}
