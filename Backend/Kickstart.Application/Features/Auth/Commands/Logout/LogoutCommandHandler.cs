using Kickstart.Application.Interfaces;
using Kickstart.Application.Features.Auth.Commands.Login;
using Kickstart.Application.Features.Auth.Commands.Logout;
using Kickstart.Application.Features.Auth.Commands.LogoutAll;
using Kickstart.Application.Features.Auth.Commands.LogoutDevice;
using Kickstart.Application.Features.Auth.Commands.Register;
using Kickstart.Application.Features.Auth.Commands.RefreshToken;
using Kickstart.Application.Features.Auth.Commands.ChangePassword;
using Kickstart.Application.Features.Auth.Commands.ForgotPassword;
using Kickstart.Application.Features.Auth.Commands.ResetPassword;
using Kickstart.Domain.Common.Interfaces;
using Kickstart.Domain.Common.Interfaces.Repositories;
using Kickstart.Application.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Kickstart.Application.Features.Auth.Commands.Logout
{
    public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
    {
        private readonly IAuthService _authService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<LogoutCommandHandler> _logger;

        public LogoutCommandHandler(
            IAuthService authService,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork,
            ILogger<LogoutCommandHandler> logger)
        {
            _authService = authService;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            _logger.LogInformation("Logout requested. UserId: {UserId}", userId);

            // Defense-in-depth: verify the refresh token belongs to the caller.
            // Without this check, an attacker holding User A's access token could revoke
            // User B's refresh token if they ever leak it, causing a session-DoS.
            var token = await _unitOfWork.RefreshTokens.GetByTokenAsync(request.RefreshToken);
            if (token == null || token.UserId != userId)
            {
                // Don't reveal whether the token exists or belongs to someone else - return
                // success so attackers can't probe token validity. Just log and skip the revoke.
                _logger.LogWarning(
                    "Logout aborted - refresh token does not belong to caller. CallerUserId: {UserId}",
                    userId);
                return Result.Success();
            }

            return await _authService.RevokeTokenAsync(request.RefreshToken);
        }
    }
}
