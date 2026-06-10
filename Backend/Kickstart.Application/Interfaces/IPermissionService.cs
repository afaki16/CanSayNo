using Kickstart.Application.Common.Results;

namespace Kickstart.Application.Interfaces
{
    public interface IPermissionService
    {
        Task<Result<bool>> HasPermissionAsync(int userId, string permission);
        Task<Result<List<string>>> GetUserPermissionsAsync(int userId);
        void ClearUserPermissionCache(int userId);
        void ClearRolePermissionCache();
    }
}
