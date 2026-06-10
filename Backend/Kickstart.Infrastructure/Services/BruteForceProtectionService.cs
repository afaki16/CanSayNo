using Kickstart.Application.Interfaces;
using Kickstart.Domain.Common.Interfaces;
using Kickstart.Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kickstart.Infrastructure.Services
{
    public class BruteForceProtectionService : IBruteForceProtectionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<BruteForceProtectionService> _logger;

        private static readonly (int Threshold, int LockoutMinutes)[] Tiers = new[]
        {
            (3, 1),        // 3 failures -> 1 minute (minor friction, attacker slow-down)
            (5, 5),        // 5 failures -> 5 minutes
            (8, 30),       // 8 failures -> 30 minutes
            (12, 240),     // 12 failures -> 4 hours
            (20, 1440),    // 20 failures -> 24 hours
            (30, 10080)    // 30 failures -> 7 days (manual admin intervention recommended)
        };

        private static readonly TimeSpan FailureWindow = TimeSpan.FromHours(24);

        public BruteForceProtectionService(
            IUnitOfWork unitOfWork,
            ILogger<BruteForceProtectionService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<LockoutStatus> CheckLockoutAsync(
            string email,
            string ipAddress,
            CancellationToken cancellationToken = default)
        {
            var since = DateTime.UtcNow - FailureWindow;

            var failureCount = await _unitOfWork.LoginAttempts
                .CountRecentFailuresAsync(email, ipAddress, since, cancellationToken);

            var lockoutMinutes = GetLockoutMinutesForFailures(failureCount);

            if (lockoutMinutes == 0)
            {
                return new LockoutStatus
                {
                    IsLockedOut = false,
                    FailureCount = failureCount
                };
            }

            var lastFailureTime = await _unitOfWork.LoginAttempts
                .GetLastFailureTimeAsync(email, ipAddress, since, cancellationToken);

            if (!lastFailureTime.HasValue)
            {
                return new LockoutStatus
                {
                    IsLockedOut = false,
                    FailureCount = failureCount
                };
            }

            var lockoutExpiresAt = lastFailureTime.Value.AddMinutes(lockoutMinutes);
            var isLockedOut = DateTime.UtcNow < lockoutExpiresAt;

            return new LockoutStatus
            {
                IsLockedOut = isLockedOut,
                LockoutExpiresAt = isLockedOut ? lockoutExpiresAt : null,
                FailureCount = failureCount,
                LockoutMinutes = lockoutMinutes
            };
        }

        public async Task RecordFailedAttemptAsync(
            string email,
            string ipAddress,
            string? userAgent,
            string failureReason,
            CancellationToken cancellationToken = default)
        {
            var since = DateTime.UtcNow - FailureWindow;

            var failureCountBefore = await _unitOfWork.LoginAttempts
                .CountRecentFailuresAsync(email, ipAddress, since, cancellationToken);

            var attempt = new LoginAttempt
            {
                Email = email,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Success = false,
                FailureReason = failureReason,
                AttemptedAt = DateTime.UtcNow
            };

            await _unitOfWork.LoginAttempts.AddAsync(attempt);

            var failureCountAfter = failureCountBefore + 1;

            if (IsTierTransition(failureCountBefore, failureCountAfter))
            {
                _logger.LogWarning(
                    "Brute force lockout triggered. Email: {Email}, IP: {IpAddress}, " +
                    "FailureCount: {FailureCount}, LockoutMinutes: {LockoutMinutes}",
                    email, ipAddress, failureCountAfter,
                    GetLockoutMinutesForFailures(failureCountAfter));
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task RecordSuccessfulAttemptAsync(
            string email,
            string ipAddress,
            string? userAgent,
            CancellationToken cancellationToken = default)
        {
            var since = DateTime.UtcNow - FailureWindow;

            await _unitOfWork.LoginAttempts
                .ClearFailuresAsync(email, ipAddress, since, cancellationToken);

            var attempt = new LoginAttempt
            {
                Email = email,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Success = true,
                AttemptedAt = DateTime.UtcNow
            };

            await _unitOfWork.LoginAttempts.AddAsync(attempt);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private static int GetLockoutMinutesForFailures(int failureCount)
        {
            var matchingTier = Tiers
                .Where(t => failureCount >= t.Threshold)
                .OrderByDescending(t => t.Threshold)
                .FirstOrDefault();

            return matchingTier.LockoutMinutes;
        }

        private static bool IsTierTransition(int countBefore, int countAfter)
        {
            foreach (var tier in Tiers)
            {
                if (countBefore < tier.Threshold && countAfter >= tier.Threshold)
                    return true;
            }
            return false;
        }

        public bool IsAtTierThreshold(int failureCount)
        {
            // Returns true exactly when failureCount equals one of the tier thresholds.
            // Used by AuthService to trigger one-time notifications when a user crosses
            // into a new lockout tier - we don't want to spam them on every failed attempt.
            return Tiers.Any(t => t.Threshold == failureCount);
        }
    }
}
