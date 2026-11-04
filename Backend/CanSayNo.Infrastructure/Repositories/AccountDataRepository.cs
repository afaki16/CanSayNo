using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CanSayNo.Domain.Common.Interfaces.Repositories;
using CanSayNo.Domain.Entities;
using CanSayNo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CanSayNo.Infrastructure.Repositories;

public class AccountDataRepository : IAccountDataRepository
{
    private readonly ApplicationDbContext _context;

    public AccountDataRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<AccountDataSnapshot?> GetSnapshotAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Set<User>()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null)
            return null;

        var stats = await _context.UserStats
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);

        var sessions = await _context.Sessions
            .AsNoTracking()
            .Include(s => s.Scenario)
            .Where(s => s.UserId == userId)
            .OrderBy(s => s.StartedAt)
            .ToListAsync(cancellationToken);

        var dailyUsage = await _context.DailyUsage
            .AsNoTracking()
            .Where(d => d.UserId == userId)
            .OrderBy(d => d.Date)
            .ToListAsync(cancellationToken);

        var subscriptions = await _context.Subscriptions
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .OrderBy(s => s.StartedAt)
            .ToListAsync(cancellationToken);

        return new AccountDataSnapshot(user, stats, sessions, dailyUsage, subscriptions);
    }

    public async Task HardDeleteUserAsync(int userId, string email, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        // Giriş denemeleri kullanıcıya FK ile bağlı değil (e-postayla tutuluyor).
        await _context.Set<LoginAttempt>()
            .IgnoreQueryFilters()
            .Where(a => a.Email == email)
            .ExecuteDeleteAsync(cancellationToken);

        // Soft-delete filtresi atlanır: daha önce pasife alınmış kayıt da silinsin.
        var deleted = await _context.Set<User>()
            .IgnoreQueryFilters()
            .Where(u => u.Id == userId)
            .ExecuteDeleteAsync(cancellationToken);

        if (deleted != 1)
            throw new InvalidOperationException($"Expected to delete 1 user, deleted {deleted}.");

        await transaction.CommitAsync(cancellationToken);
    }
}
