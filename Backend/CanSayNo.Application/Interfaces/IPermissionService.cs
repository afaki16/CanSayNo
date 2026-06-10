using CanSayNo.Application.Common.Results;

namespace CanSayNo.Application.Interfaces
{
    public interface IPermissionService
    {
        Task<Result<bool>> HasPermissionAsync(int userId, string permission);
        Task<Result<List<string>>> GetUserPermissionsAsync(int userId);
        void ClearUserPermissionCache(int userId);
        void ClearRolePermissionCache();
    }
}
