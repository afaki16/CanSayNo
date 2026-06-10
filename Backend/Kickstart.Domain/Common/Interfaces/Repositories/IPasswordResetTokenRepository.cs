using Kickstart.Domain.Entities;

namespace Kickstart.Domain.Common.Interfaces.Repositories
{
    public interface IPasswordResetTokenRepository : IRepository<PasswordResetToken, int>
    {
        Task<PasswordResetToken?> GetValidTokenAsync(string token, string email);
        Task InvalidatePreviousTokensAsync(int userId);
    }
}
