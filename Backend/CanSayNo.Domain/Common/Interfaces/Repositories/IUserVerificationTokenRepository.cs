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

        /// <summary>Most recently created token for the user/channel/purpose, used or not (null if none).</summary>
        Task<UserVerificationToken?> GetLatestAsync(
            int userId,
            VerificationChannel channel,
            VerificationPurpose purpose);

        Task InvalidatePreviousTokensAsync(
            int userId,
            VerificationChannel channel,
            VerificationPurpose purpose);
    }
}
