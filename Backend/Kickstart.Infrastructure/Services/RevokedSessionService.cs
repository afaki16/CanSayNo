using Kickstart.Application.Interfaces;
using Kickstart.Domain.Common.Interfaces;
using System;
using System.Threading.Tasks;

namespace Kickstart.Infrastructure.Services
{
    public class RevokedSessionService : IRevokedSessionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RevokedSessionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> WasSessionRevokedAfterAsync(int userId, DateTime tokenIssuedAtUtc)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null) return true; // User not found, treat as revoked

            if (!user.LastSessionsRevokedAt.HasValue)
                return false;

            return user.LastSessionsRevokedAt.Value > tokenIssuedAtUtc;
        }
    }
}
