using Kickstart.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kickstart.Domain.Common.Interfaces.Repositories
{
    public interface IUserRepository : IRepository<User,int>
    {
        Task<User> GetByEmailAsync(string email, int? tenantId);
        Task<IReadOnlyList<User>> GetUsersByEmailAsync(string email);
        Task<IReadOnlyList<User>> GetUsersByEmailWithPermissionsAsync(string email);
        Task<User> GetUserWithRolesAsync(int userId);

        /// <summary>
        /// Read-only variant of GetUserWithRolesAsync - returns the user with no change tracking.
        /// Use this from Query handlers; use GetUserWithRolesAsync from Command handlers that
        /// will modify the returned entity.
        /// </summary>
        Task<User> GetUserWithRolesReadOnlyAsync(int userId);
        Task<User> GetUserWithPermissionsAsync(int userId);
        Task<bool> EmailExistsAsync(string email, int? tenantId);
        /// <param name="excludeUserId">If set, that user is ignored (for updates).</param>
        Task<bool> PhoneExistsAsync(string phoneNumber, int? tenantId, int? excludeUserId = null);
        Task<UserRole> GetUserRoleAsync(int userId, int roleId);
        Task AddUserRoleAsync(UserRole userRole);
        void RemoveUserRole(UserRole userRole);
        Task<(IEnumerable<User> Users, int TotalCount)> GetUsersPagedAsync(int page, int pageSize, string searchTerm = null, int? tenantId = null, bool excludeUsersWithSuperAdminRole = false);
        Task<IEnumerable<int>> GetUserIdsByRoleIdAsync(int roleId);
        Task<int> CountActiveSuperAdminsAsync();
    }
} 
