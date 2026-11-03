using CanSayNo.Domain.Common.Interfaces;
using CanSayNo.Domain.Entities;

namespace CanSayNo.Application.Features.Auth.Commands.PasswordCodes
{
    internal static class PasswordCodeUserLookup
    {
        /// <summary>
        /// Same resolution as forgot-password: look up across tenants, narrow by tenant only when the email
        /// exists in several. Returns null when no single user matches.
        /// </summary>
        public static async Task<User?> FindAsync(IUnitOfWork unitOfWork, string? email, int tenantId)
        {
            var normalizedEmail = email?.Trim().ToLowerInvariant() ?? string.Empty;
            var users = await unitOfWork.Users.GetUsersByEmailAsync(normalizedEmail);

            if (users.Count == 1)
                return users[0];
            if (users.Count > 1 && tenantId > 0)
                return users.FirstOrDefault(u => u.TenantId == tenantId);
            return null;
        }
    }
}
