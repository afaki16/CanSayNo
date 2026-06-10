using Microsoft.Extensions.Caching.Memory;
using Kickstart.Application.Interfaces;
using Kickstart.Application.Common.Results;
using Kickstart.Domain.Common.Interfaces;

namespace Kickstart.Infrastructure.Services
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
                        var perms = new List<string> { rp.Permission.FullPermission };
                        foreach (var permType in rp.Permission.GetIndividualPermissions())
                            perms.Add($"{rp.Permission.Resource}.{permType}");
                        return perms;
                    })
                    .Distinct()
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
