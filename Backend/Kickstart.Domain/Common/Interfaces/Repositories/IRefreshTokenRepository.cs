using Kickstart.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kickstart.Domain.Common.Interfaces.Repositories
{
    public interface IRefreshTokenRepository : IRepository<RefreshToken,int>
    {
        Task<RefreshToken> GetByTokenAsync(string token);
        Task<RefreshToken> GetActiveTokenByUserIdAsync(int userId);
        Task<IEnumerable<RefreshToken>> GetUserTokensAsync(int userId, bool includeRevoked = false);
        Task<IEnumerable<RefreshToken>> GetTokensByDeviceAsync(int userId, string deviceId);
        Task<IEnumerable<RefreshToken>> GetActiveTokensByIpAsync(string ipAddress);
        Task<int> GetActiveTokenCountAsync(int userId);
        /// <summary>
        /// Count distinct users with at least one active refresh token.
        /// When tenantId is null, counts across all tenants (SuperAdmin). Otherwise scoped to tenant.
        /// </summary>
        Task<int> GetActiveUserCountAsync(int? tenantId = null, bool excludeUsersWithSuperAdminRole = false);

        /// <summary>
        /// Get user IDs that have at least one active refresh token.
        /// </summary>
        Task<IEnumerable<int>> GetActiveUserIdsAsync(int? tenantId = null, bool excludeUsersWithSuperAdminRole = false);
        Task RevokeAllUserTokensAsync(int userId, string ipAddress = null, string userAgent = null, string reason = "User logout");
        Task RevokeTokenAsync(string token, string ipAddress = null, string userAgent = null, string reason = "Token revoked");
        Task RevokeTokensByDeviceAsync(int userId, string deviceId, string ipAddress = null, string userAgent = null, string reason = "Device logout");
        Task RevokeTokensByIpAsync(string ipAddress, string reason = "Suspicious activity");
        Task CleanupExpiredTokensAsync();
        Task CleanupRevokedTokensAsync(int daysOld = 30);
        Task<IEnumerable<RefreshToken>> GetExpiringSoonTokensAsync(int minutesThreshold = 30);
    }
} 
