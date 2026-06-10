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
using System.Threading;
using System.Threading.Tasks;
using Kickstart.Domain.Common.Enums;
using Kickstart.Domain.Models;

namespace Kickstart.Application.Features.Auth.Commands.LogoutDevice
{
    public class LogoutDeviceCommandHandler : IRequestHandler<LogoutDeviceCommand, Result>
    {
        private readonly IAuthService _authService;
        private readonly ICurrentUserService _currentUserService;

        public LogoutDeviceCommandHandler(IAuthService authService, ICurrentUserService currentUserService)
        {
            _authService = authService;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(LogoutDeviceCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
            return Result<int>.Failure(Error.Failure(
                   ErrorCode.NotFound,
                   "User not authenticated"));

        return await _authService.RevokeTokensByDeviceAsync(userId.Value, request.DeviceId, request.IpAddress, request.UserAgent, request.Reason);
        }
    }
} 
