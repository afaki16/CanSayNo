using Kickstart.Domain.Common.Interfaces;
using Kickstart.Domain.Common.Interfaces.Repositories;
using Kickstart.Domain.Constants;
using Kickstart.Domain.Entities;
using Kickstart.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kickstart.Infrastructure.Repositories;

public class RoleRepository : RepositoryBase<Role, int>, IRoleRepository
{

    private readonly ApplicationDbContext _context;
    public RoleRepository(ApplicationDbContext context) : base(context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Role> GetByNameAsync(string name)
    {
        return await _context.Set<Role>().FirstOrDefaultAsync(r => r.Name == name);
    }

    public async Task<int> GetByIdWithNameAsync(string name)
    {
        var role = await _context.Set<Role>().FirstOrDefaultAsync(r => r.Name == name);
        return role?.Id ?? 0;
    }

    public async Task<Role> GetRoleWithPermissionsAsync(int roleId)
    {
        return await _context.Set<Role>()
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == roleId);
    }

    public async Task<Role> GetRoleWithPermissionsReadOnlyAsync(int roleId)
    {
        return await _context.Set<Role>()
            .AsNoTracking()
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == roleId);
    }

    public async Task<IEnumerable<Role>> GetAllWithPermissionsAsync()
    {
        return await _context.Set<Role>()
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .ToListAsync();
    }

    public async Task<(IEnumerable<Role> Roles, int TotalCount)> GetRolesPagedAsync(int page, int pageSize, string searchTerm = null, bool excludeSuperAdminRole = false)
    {
        var baseQuery = BuildRolesQuery(searchTerm, excludeSuperAdminRole).AsNoTracking();
        var totalCount = await baseQuery.CountAsync();

        var roles = await baseQuery
            .Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (roles, totalCount);
    }

    private IQueryable<Role> BuildRolesQuery(string searchTerm, bool excludeSuperAdminRole)
    {
        var query = _context.Set<Role>().AsQueryable();

        if (excludeSuperAdminRole)
            query = query.Where(r => r.Name != RoleNames.SuperAdmin);

        if (!string.IsNullOrEmpty(searchTerm))
        {
            var lower = searchTerm.ToLower();
            query = query.Where(r =>
                (r.Name != null && r.Name.ToLower().Contains(lower)) ||
                (r.Description != null && r.Description.ToLower().Contains(lower)));
        }

        return query;
    }

    public async Task<IEnumerable<Role>> GetNonSystemRolesWithPermissionsAsync()
    {
        return await _context.Set<Role>()
            .Where(r => !r.IsSystemRole)
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .ToListAsync();
    }

    public async Task<IEnumerable<Role>> GetRolesByUserIdAsync(int userId)
    {
        return await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Include(ur => ur.Role)
            .Select(ur => ur.Role)
            .ToListAsync();
    }

    public async Task<bool> RoleExistsAsync(string name)
    {
        return await _context.Set<Role>().AnyAsync(r => r.Name == name);
    }

    public async Task<bool> RoleHasPermissionAsync(int roleId, string permission)
    {
        return await _context.RolePermissions
            .Include(rp => rp.Role)
            .Include(rp => rp.Permission)
            .AnyAsync(rp => rp.RoleId == roleId && rp.Permission.Name == permission);
    }

    public async Task<IEnumerable<string>> GetRolePermissionsAsync(int roleId)
    {
        return await _context.RolePermissions
            .Include(rp => rp.Permission)
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => rp.Permission.Name)
            .ToListAsync();
    }

    public async Task<RolePermission> GetRolePermissionAsync(int roleId, int permissionId)
    {
        return await _context.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);
    }

    public async Task AddRolePermissionAsync(RolePermission rolePermission)
    {
        await _context.RolePermissions.AddAsync(rolePermission);
    }

    public void RemoveRolePermission(RolePermission rolePermission)
    {
        _context.RolePermissions.Remove(rolePermission);
    }
}
