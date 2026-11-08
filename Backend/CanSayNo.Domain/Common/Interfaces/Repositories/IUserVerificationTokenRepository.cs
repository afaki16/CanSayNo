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

        /// <summary>
        /// Atomically counts one verification attempt (single UPDATE). False when the token is used,
        /// expired or already has <paramref name="maxAttempts"/> attempts.
        /// </summary>
        Task<bool> TryReserveAttemptAsync(int tokenId, int maxAttempts, CancellationToken cancellationToken = default);

        /// <summary>
        /// Atomically marks the token used; false if it was already used (e.g. by a concurrent request).
        /// With <paramref name="minAttempts"/>, only when the attempt counter has reached it.
        /// </summary>
        Task<bool> TryMarkUsedAsync(int tokenId, int? minAttempts = null, CancellationToken cancellationToken = default);
    }
}
