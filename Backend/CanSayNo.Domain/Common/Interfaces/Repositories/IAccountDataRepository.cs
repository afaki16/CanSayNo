using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CanSayNo.Domain.Entities;
using CanSayNo.Domain.Entities.Hayirci;

namespace CanSayNo.Domain.Common.Interfaces.Repositories;

/// <summary>
/// Kullanıcının kendi hesabı üzerindeki KVKK / App Store işlemleri: verinin tamamını
/// okumak (dışa aktarma) ve hesabı tüm verisiyle kalıcı olarak silmek.
/// </summary>
public interface IAccountDataRepository
{
    /// <summary>Kullanıcıya ait tüm kayıtlar (salt okunur). Kullanıcı yoksa null.</summary>
    Task<AccountDataSnapshot?> GetSnapshotAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kullanıcıyı KALICI olarak siler (soft delete değil). Bağlı tablolar (pratikler,
    /// istatistikler, günlük kullanım, abonelik kayıtları, token'lar, roller) DB'de
    /// CASCADE ile silinir; FK'si olmayan giriş denemeleri e-postayla ayrıca silinir.
    /// Tek transaction.
    /// </summary>
    Task HardDeleteUserAsync(int userId, string email, CancellationToken cancellationToken = default);
}

public sealed record AccountDataSnapshot(
    User User,
    UserStats? Stats,
    IReadOnlyList<Session> Sessions,
    IReadOnlyList<DailyUsage> DailyUsage,
    IReadOnlyList<Subscription> Subscriptions);
