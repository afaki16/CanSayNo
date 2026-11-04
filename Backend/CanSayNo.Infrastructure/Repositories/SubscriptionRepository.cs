using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CanSayNo.Domain.Common.Interfaces.Repositories;
using CanSayNo.Domain.Entities.Hayirci;
using CanSayNo.Domain.Enums;
using CanSayNo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CanSayNo.Infrastructure.Repositories;

public class SubscriptionRepository : RepositoryBase<Subscription, Guid>, ISubscriptionRepository
{
    private readonly ApplicationDbContext _context;

    public SubscriptionRepository(ApplicationDbContext context) : base(context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<bool> HasActiveSubscriptionAsync(
        int userId,
        string appId,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .AsNoTracking()
            .AnyAsync(IsCurrent(userId, appId, utcNow), cancellationToken);
    }

    // GracePeriod: ödeme sorunu yaşanırken store erişimi sürdürür — premium sayılır.
    // Cancelled: otomatik yenileme kapalı ama ödenmiş dönem bitmedi — premium sayılır.
    // ExpiresAt null: süresiz (örn. ömür boyu / promosyon).
    private static System.Linq.Expressions.Expression<Func<Subscription, bool>> IsCurrent(
        int userId, string appId, DateTime utcNow) =>
        s => s.UserId == userId
             && s.AppId == appId
             && (s.Status == SubscriptionStatus.Active
                 || s.Status == SubscriptionStatus.Cancelled
                 || s.Status == SubscriptionStatus.GracePeriod)
             && (s.ExpiresAt == null || s.ExpiresAt > utcNow);

    public async Task<Subscription?> GetByProviderTransactionAsync(
        int userId,
        string appId,
        string providerTransactionId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId
                                      && s.AppId == appId
                                      && s.ProviderTransactionId == providerTransactionId,
                cancellationToken);
    }

    public async Task<Subscription?> GetCurrentAsync(
        int userId,
        string appId,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        var current = await _context.Subscriptions
            .AsNoTracking()
            .Where(IsCurrent(userId, appId, utcNow))
            .OrderByDescending(s => s.ExpiresAt == null)
            .ThenByDescending(s => s.ExpiresAt)
            .FirstOrDefaultAsync(cancellationToken);
        if (current is not null)
            return current;

        return await _context.Subscriptions
            .AsNoTracking()
            .Where(s => s.UserId == userId && s.AppId == appId)
            .OrderByDescending(s => s.UpdatedDate ?? s.CreatedDate)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
