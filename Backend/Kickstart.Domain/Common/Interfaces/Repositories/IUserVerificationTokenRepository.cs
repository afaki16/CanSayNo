using Kickstart.Domain.Common.Enums;
using Kickstart.Domain.Entities;

namespace Kickstart.Domain.Common.Interfaces.Repositories
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
