using CanSayNo.Application.Interfaces;
using CanSayNo.Application.Features.Auth.Commands.Login;
using CanSayNo.Application.Features.Auth.Commands.Logout;
using CanSayNo.Application.Features.Auth.Commands.LogoutAll;
using CanSayNo.Application.Features.Auth.Commands.LogoutDevice;
using CanSayNo.Application.Features.Auth.Commands.Register;
using CanSayNo.Application.Features.Auth.Commands.RefreshToken;
using CanSayNo.Application.Features.Auth.Commands.ChangePassword;
using CanSayNo.Application.Features.Auth.Commands.ForgotPassword;
using CanSayNo.Application.Features.Auth.Commands.ResetPassword;
using CanSayNo.Domain.Common.Interfaces;
using CanSayNo.Domain.Common.Interfaces.Repositories;
using CanSayNo.Application.Common.Results;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using CanSayNo.Domain.Common.Enums;
using CanSayNo.Domain.Models;

namespace CanSayNo.Application.Features.Auth.Commands.LogoutDevice
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
