using CanSayNo.Domain.Entities;

namespace CanSayNo.Domain.Common.Interfaces.Repositories
{
    public interface IPasswordResetTokenRepository : IRepository<PasswordResetToken, int>
    {
        Task<PasswordResetToken?> GetValidTokenAsync(string token, string email);
        Task InvalidatePreviousTokensAsync(int userId);
    }
}
