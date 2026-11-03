using Microsoft.Extensions.Caching.Memory;
using CanSayNo.Application.Interfaces;
using CanSayNo.Application.Common.Results;
using CanSayNo.Domain.Common.Interfaces;

namespace CanSayNo.Infrastructure.Services
{
    public class PermissionService(IMemoryCache cache, IUnitOfWork unitOfWork) : IPermissionService
    {
        private const string RolePermCacheKey = "role_permissions_all";
        private const int RoleCacheMinutes = 60;
        private const int UserCacheMinutes = 15;

        private async Task<Dictionary<int, List<string>>> GetRolePermissionsMapAsync()
        {
            if (cache.TryGetValue(RolePermCacheKey, out Dictionary<int, List<string>>? map) && map is not null)
                return map;

            var roles = await unitOfWork.Roles.GetAllWithPermissionsAsync();
            map = roles.ToDictionary(
                r => r.Id,
                r => r.RolePermissions
                    .SelectMany(rp =>
                    {
                        var permission = rp.Permission;
                        // Name is the canonical policy string ("Invoices.Export"). FullPermission is
                        // Resource.Type, and a non-CRUD action has no PermissionType (it is stored as
                        // Read) — using it would grant "Invoices.Read" instead of "Invoices.Export".
                        var perms = new List<string> { permission.Name };

                        // Type-derived names only for permissions whose name IS the Resource.Type form
                        // (CRUD and flag combinations such as ReadWrite → Read/Create/Update).
                        if (string.Equals(permission.Name, permission.FullPermission, StringComparison.OrdinalIgnoreCase))
                        {
                            foreach (var permType in permission.GetIndividualPermissions())
                                perms.Add($"{permission.Resource}.{permType}");
                        }
                        return perms;
                    })
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList()
            );

            cache.Set(RolePermCacheKey, map,
               new MemoryCacheEntryOptions
               {
                   SlidingExpiration = TimeSpan.FromMinutes(RoleCacheMinutes),
                   AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(4)  // worst-case ceiling
               });

            return map;
        }

        public async Task<Result<List<string>>> GetUserPermissionsAsync(int userId)
        {
            var cacheKey = $"user_permissions_{userId}";

            if (cache.TryGetValue(cacheKey, out List<string>? cached) && cached is not null)
                return Result<List<string>>.Success(cached);

            var user = await unitOfWork.Users.GetUserWithRolesAsync(userId);
            if (user == null)
                return Result<List<string>>.Success(new List<string>());

            var rolePermMap = await GetRolePermissionsMapAsync();

            var permissions = user.UserRoles
                .Where(ur => rolePermMap.ContainsKey(ur.RoleId))
                .SelectMany(ur => rolePermMap[ur.RoleId])
                .Distinct()
                .ToList();

            cache.Set(cacheKey, permissions,
                new MemoryCacheEntryOptions
               {
                   SlidingExpiration = TimeSpan.FromMinutes(UserCacheMinutes),
                   AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)  // worst-case ceiling
               });
        
            return Result<List<string>>.Success(permissions);
        }

        public async Task<Result<bool>> HasPermissionAsync(int userId, string permission)
        {
            var result = await GetUserPermissionsAsync(userId);

            return result.Match(
                onSuccess: permissions => Result<bool>.Success(
                    permissions.Contains(permission, StringComparer.OrdinalIgnoreCase)),
                onFailure: errors => Result<bool>.Failure(errors.ToArray())
            );
        }

        public void ClearUserPermissionCache(int userId)
        {
            cache.Remove($"user_permissions_{userId}");
        }

        public void ClearRolePermissionCache()
        {
            cache.Remove(RolePermCacheKey);
        }
    }
}
