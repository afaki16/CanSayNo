using Kickstart.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kickstart.Domain.Common.Interfaces.Repositories
{
    public interface IRoleRepository : IRepository<Role,int>
    {
        Task<Role> GetByNameAsync(string name);
        Task<Role> GetRoleWithPermissionsAsync(int roleId);

        /// <summary>
        /// Read-only variant of GetRoleWithPermissionsAsync - returns the role with no change tracking.
        /// Use this from Query handlers; use GetRoleWithPermissionsAsync from Command handlers that
        /// will modify the returned entity.
        /// </summary>
        Task<Role> GetRoleWithPermissionsReadOnlyAsync(int roleId);
        Task<int> GetByIdWithNameAsync(string name);
        Task<IEnumerable<Role>> GetAllWithPermissionsAsync();
        Task<(IEnumerable<Role> Roles, int TotalCount)> GetRolesPagedAsync(int page, int pageSize, string searchTerm = null, bool excludeSuperAdminRole = false);
        Task<IEnumerable<Role>> GetNonSystemRolesWithPermissionsAsync();
        Task<IEnumerable<Role>> GetRolesByUserIdAsync(int userId);
        Task<bool> RoleExistsAsync(string name);
        Task<bool> RoleHasPermissionAsync(int roleId, string permission);
        Task<IEnumerable<string>> GetRolePermissionsAsync(int roleId);
        Task<RolePermission> GetRolePermissionAsync(int roleId, int permissionId);
        Task AddRolePermissionAsync(RolePermission rolePermission);
        void RemoveRolePermission(RolePermission rolePermission);
    }
} 
