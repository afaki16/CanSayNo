using Kickstart.Domain.Common.Interfaces.Repositories;
using Kickstart.Domain.Entities;
using Kickstart.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kickstart.Infrastructure.Repositories
{
    public class LoginAttemptRepository : RepositoryBase<LoginAttempt, int>, ILoginAttemptRepository
    {
        public LoginAttemptRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<int> CountRecentFailuresAsync(
            string email,
            string ipAddress,
            DateTime since,
            CancellationToken cancellationToken = default)
        {
            return await _context.Set<LoginAttempt>()
                .Where(la => la.Email == email
                          && la.IpAddress == ipAddress
                          && la.Success == false
                          && la.AttemptedAt >= since
                          && la.ClearedAt == null)
                .CountAsync(cancellationToken);
        }

        public async Task<DateTime?> GetLastFailureTimeAsync(
            string email,
            string ipAddress,
            DateTime since,
            CancellationToken cancellationToken = default)
        {
            var lastAttempt = await _context.Set<LoginAttempt>()
                .Where(la => la.Email == email
                          && la.IpAddress == ipAddress
                          && la.Success == false
                          && la.AttemptedAt >= since
                          && la.ClearedAt == null)
                .OrderByDescending(la => la.AttemptedAt)
                .FirstOrDefaultAsync(cancellationToken);

            return lastAttempt?.AttemptedAt;
        }

        public async Task ClearFailuresAsync(
            string email,
            string ipAddress,
            DateTime since,
            CancellationToken cancellationToken = default)
        {
            var failures = await _context.Set<LoginAttempt>()
                .Where(la => la.Email == email
                          && la.IpAddress == ipAddress
                          && la.Success == false
                          && la.AttemptedAt >= since
                          && la.ClearedAt == null)
                .ToListAsync(cancellationToken);

            var now = DateTime.UtcNow;
            foreach (var failure in failures)
            {
                failure.ClearedAt = now;
            }
        }
    }
}
