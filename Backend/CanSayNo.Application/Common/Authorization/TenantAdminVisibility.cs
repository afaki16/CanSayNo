using System.Linq;
using CanSayNo.Application.Interfaces;
using CanSayNo.Domain.Constants;
using CanSayNo.Domain.Entities;

namespace CanSayNo.Application.Common.Authorization
{
    /// <summary>
    /// Tenant Admin, kendi tenant'ında bile SuperAdmin rolündeki kullanıcıları görmemeli / yönetmemeli.
    /// SuperAdmin (<see cref="ICurrentUserService.CanAccessAllTenants"/>) tüm kullanıcılara erişir.
    /// </summary>
    public static class TenantAdminVisibility
    {
        public static bool UserHasSuperAdminRole(User user) =>
            user?.UserRoles?.Any(ur => ur.Role?.Name == RoleNames.SuperAdmin) == true;

        public static bool IsHiddenFromTenantAdmin(ICurrentUserService current, User user) =>
            !current.CanAccessAllTenants && UserHasSuperAdminRole(user);

        /// <summary>
        /// Hayırcı: mobil kayıt olan uygulama kullanıcıları tenant'sız (TenantId null); Hayırcı verisi
        /// tenant-scoped değil. Tenant Admin kendi tenant'ına EK OLARAK tenant'sız kullanıcıları da
        /// görür/yönetir (başka tenant'ları değil). SuperAdmin her şeye erişir.
        /// </summary>
        public static bool CanAccessUserTenant(ICurrentUserService current, User user) =>
            current.CanAccessAllTenants
            || user.TenantId == null
            || user.TenantId == current.TenantId;
    }
}
