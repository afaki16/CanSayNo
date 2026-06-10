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
using Kickstart.Domain.Common.Enums;
using Kickstart.Domain.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Kickstart.Application.Features.Auth.Commands.LogoutAll
{
    public class LogoutAllCommandHandler : IRequestHandler<LogoutAllCommand, Result>
    {
        private readonly IAuthService _authService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<LogoutAllCommandHandler> _logger;

        public LogoutAllCommandHandler(
            IAuthService authService,
            ICurrentUserService currentUserService,
            ILogger<LogoutAllCommandHandler> logger)
        {
            _authService = authService;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result> Handle(LogoutAllCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
            return Result<int>.Failure(Error.Failure(
               ErrorCode.NotFound,
               "User not authenticated"));

            _logger.LogWarning(
                "Logout all devices requested. UserId: {UserId}",
                userId.Value);

        return await _authService.RevokeAllUserTokensAsync(userId.Value, request.IpAddress, request.UserAgent, request.Reason);
        }
    }
} 
