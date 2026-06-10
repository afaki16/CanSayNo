using CanSayNo.Domain.Common.Enums;
using CanSayNo.Domain.Entities;

namespace CanSayNo.Domain.Common.Interfaces.Repositories
{
    public interface IUserVerificationTokenRepository : IRepository<UserVerificationToken, int>
    {
        Task<UserVerificationToken?> GetValidTokenAsync(
            string hashedToken,
            string destination,
            VerificationChannel channel,
            VerificationPurpose purpose);

        Task InvalidatePreviousTokensAsync(
            int userId,
            VerificationChannel channel,
            VerificationPurpose purpose);
    }
}
