using System;
using System.Threading;
using System.Threading.Tasks;
using CanSayNo.Domain.Entities.Hayirci;

namespace CanSayNo.Domain.Common.Interfaces.Repositories;

public interface ISubscriptionRepository : IRepository<Subscription, Guid>
{
    /// <summary>
    /// Kullanıcının bu uygulama için geçerli (Active / GracePeriod ve süresi dolmamış)
    /// bir aboneliği var mı?
    /// </summary>
    Task<bool> HasActiveSubscriptionAsync(
        int userId,
        string appId,
        DateTime utcNow,
        CancellationToken cancellationToken = default);
}
