using Kickstart.Domain.Common.Interfaces.Repositories;
using Kickstart.Domain.Entities;
using Kickstart.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kickstart.Infrastructure.Repositories
{
    public class PasswordResetTokenRepository : RepositoryBase<PasswordResetToken, int>, IPasswordResetTokenRepository
    {
        public PasswordResetTokenRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<PasswordResetToken?> GetValidTokenAsync(string token, string email)
        {
            return await GetQueryable()
                .Include(t => t.User)
                .FirstOrDefaultAsync(t =>
                    t.Token == token &&
                    t.User.Email == email &&
                    !t.IsUsed &&
                    t.ExpiresAt > DateTime.UtcNow);
        }

        public async Task InvalidatePreviousTokensAsync(int userId)
        {
            var activeTokens = await GetQueryable()
                .Where(t => t.UserId == userId && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();

            foreach (var token in activeTokens)
                token.MarkAsUsed();

            UpdateRange(activeTokens);
        }
    }
}
