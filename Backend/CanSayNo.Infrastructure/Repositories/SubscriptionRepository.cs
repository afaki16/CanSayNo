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
        // GracePeriod: ödeme sorunu yaşanırken store erişimi sürdürür — premium sayılır.
        // ExpiresAt null: süresiz (örn. promosyon) abonelik.
        return await _context.Subscriptions
            .AsNoTracking()
            .AnyAsync(s => s.UserId == userId
                           && s.AppId == appId
                           && (s.Status == SubscriptionStatus.Active
                               || s.Status == SubscriptionStatus.GracePeriod)
                           && (s.ExpiresAt == null || s.ExpiresAt > utcNow),
                cancellationToken);
    }
}
