using System;
using System.Threading;
using System.Threading.Tasks;
using CanSayNo.Domain.Common.Interfaces.Repositories;
using CanSayNo.Domain.Entities.Hayirci;
using CanSayNo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CanSayNo.Infrastructure.Repositories;

public class UserStatsRepository : IUserStatsRepository
{
    private readonly ApplicationDbContext _context;

    public UserStatsRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<UserStats?> GetAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserStats
            .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);
    }

    public async Task AddAsync(UserStats stats, CancellationToken cancellationToken = default)
    {
        await _context.UserStats.AddAsync(stats, cancellationToken);
    }
}
