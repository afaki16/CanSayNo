using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kickstart.Application.Interfaces
{
    public interface IBruteForceProtectionService
    {
        Task<LockoutStatus> CheckLockoutAsync(
            string email,
            string ipAddress,
            CancellationToken cancellationToken = default);

        Task RecordFailedAttemptAsync(
            string email,
            string ipAddress,
            string? userAgent,
            string failureReason,
            CancellationToken cancellationToken = default);

        Task RecordSuccessfulAttemptAsync(
            string email,
            string ipAddress,
            string? userAgent,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns true if reaching this failure count crosses a lockout tier threshold.
        /// Used to decide whether to notify the user about the lockout escalation.
        /// </summary>
        bool IsAtTierThreshold(int failureCount);
    }

    public class LockoutStatus
    {
        public bool IsLockedOut { get; set; }
        public DateTime? LockoutExpiresAt { get; set; }
        public int FailureCount { get; set; }
        public int LockoutMinutes { get; set; }
        public bool ShouldNotifyUser { get; set; }
    }
}
