using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CanSayNo.Domain.Entities.Hayirci;

namespace CanSayNo.Domain.Common.Interfaces.Repositories;

public interface ISubscriptionRepository : IRepository<Subscription, Guid>
{
    /// <summary>
    /// Kullanıcının bu uygulama için geçerli bir aboneliği var mı? Active, Cancelled
    /// (otomatik yenileme kapalı, ödenmiş dönem sürüyor) ve GracePeriod — süresi dolmamışsa.
    /// </summary>
    Task<bool> HasActiveSubscriptionAsync(
        int userId,
        string appId,
        DateTime utcNow,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Webhook: bu mağaza işlemine ait kayıt (tracked), sahibi kim olursa olsun — işlem TRANSFER ile
    /// başka kullanıcıya geçmiş olabilir. Yoksa null.
    /// </summary>
    Task<Subscription?> GetByProviderTransactionAsync(
        string appId,
        string providerTransactionId,
        CancellationToken cancellationToken = default);

    /// <summary>Webhook TRANSFER: verilen kullanıcıların bu uygulamadaki tüm kayıtları (tracked).</summary>
    Task<List<Subscription>> GetAllForUsersAsync(
        IReadOnlyCollection<int> userIds,
        string appId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Mobil için: geçerli abonelik varsa en geç bitenini, yoksa en son kaydı döndürür (salt okunur).
    /// </summary>
    Task<Subscription?> GetCurrentAsync(
        int userId,
        string appId,
        DateTime utcNow,
        CancellationToken cancellationToken = default);
}
