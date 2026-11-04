using CanSayNo.Application.Common.Results;
using CanSayNo.Domain.Common.Enums;
using CanSayNo.Domain.Entities;

namespace CanSayNo.Application.Interfaces
{
    /// <summary>Issued code plus how long it stays valid (for the email text).</summary>
    public sealed record OtpIssueResult(string Code, int ValidMinutes);

    /// <summary>
    /// Short numeric one-time codes sent by email (mobile-friendly alternative to links).
    /// One active code per user and purpose; codes are stored as a keyed hash, expire quickly,
    /// are burned after a few wrong entries and cannot be re-requested within a cooldown.
    /// </summary>
    public interface IOtpService
    {
        /// <summary>
        /// Creates a new code (invalidating the previous one) and returns the plain code for the caller to send.
        /// Fails with a cooldown error if a code for the same purpose was issued moments ago.
        /// </summary>
        /// <param name="destination">Address the code is sent to and bound to; defaults to the user's current email.</param>
        Task<Result<OtpIssueResult>> IssueAsync(User user, VerificationPurpose purpose, string? ipAddress, CancellationToken cancellationToken = default, string? destination = null);

        /// <summary>
        /// Checks the code against the user's active code for the purpose and consumes it on success.
        /// A wrong code counts as a failed attempt; too many failures burn the code.
        /// </summary>
        Task<Result> VerifyAsync(User user, VerificationPurpose purpose, string code, CancellationToken cancellationToken = default);

        /// <summary>Same as <see cref="VerifyAsync"/>, and returns the destination the code was issued for.</summary>
        Task<Result<string>> VerifyForDestinationAsync(User user, VerificationPurpose purpose, string code, CancellationToken cancellationToken = default);
    }
}
