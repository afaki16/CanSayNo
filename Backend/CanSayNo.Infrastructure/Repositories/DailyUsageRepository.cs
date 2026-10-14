using System;
using System.Threading;
using System.Threading.Tasks;
using CanSayNo.Domain.Common.Interfaces.Repositories;
using CanSayNo.Domain.Entities.Hayirci;
using CanSayNo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CanSayNo.Infrastructure.Repositories;

public class DailyUsageRepository : IDailyUsageRepository
{
    private readonly ApplicationDbContext _context;

    public DailyUsageRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<DailyUsage?> GetAsync(
        int userId, DateOnly date, CancellationToken cancellationToken = default)
    {
        return await _context.DailyUsage
            .FirstOrDefaultAsync(du => du.UserId == userId && du.Date == date, cancellationToken);
    }

    public async Task AddAsync(DailyUsage dailyUsage, CancellationToken cancellationToken = default)
    {
        await _context.DailyUsage.AddAsync(dailyUsage, cancellationToken);
    }
}
