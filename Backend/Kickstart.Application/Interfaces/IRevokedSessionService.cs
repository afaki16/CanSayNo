using System;
using System.Threading.Tasks;

namespace Kickstart.Application.Interfaces
{
    /// <summary>
    /// Checks if a user's sessions were revoked after a token was issued (for immediate admin logout).
    /// </summary>
    public interface IRevokedSessionService
    {
        /// <summary>
        /// Returns true if the user's sessions were revoked after the token was issued.
        /// </summary>
        Task<bool> WasSessionRevokedAfterAsync(int userId, DateTime tokenIssuedAtUtc);
    }
}
