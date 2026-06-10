using Kickstart.Application.Interfaces;
using Kickstart.Application.Common.Localization;
using Kickstart.Application.Common.Results;
using Kickstart.Domain.Common.Enums;
using Kickstart.Domain.Common.Interfaces;
using Kickstart.Domain.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Kickstart.Application.Features.Auth.Commands.ChangePassword
{
    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordService _passwordService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<ChangePasswordCommandHandler> _logger;
        private readonly IJsonStringLocalizer _localizer;

        public ChangePasswordCommandHandler(
            IUnitOfWork unitOfWork,
            IPasswordService passwordService,
            ICurrentUserService currentUserService,
            ILogger<ChangePasswordCommandHandler> logger,
            IJsonStringLocalizer localizer)
        {
            _unitOfWork = unitOfWork;
            _passwordService = passwordService;
            _currentUserService = currentUserService;
            _logger = logger;
            _localizer = localizer;
        }

        public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
                return Result.Failure(Error.Failure(ErrorCode.Unauthorized, _localizer["auth.userNotAuthenticated"]));

            var user = await _unitOfWork.Users.GetByIdAsync(userId.Value);
            if (user is null)
                return Result.Failure(Error.Failure(ErrorCode.NotFound, _localizer["users.notFound"]));

            var verifyResult = _passwordService.VerifyPassword(request.CurrentPassword, user.PasswordHash);
            if (!verifyResult.IsSuccess)
                return Result.Failure(verifyResult.Errors);

            if (!verifyResult.Value)
            {
                _logger.LogWarning(
                    "Change password failed - incorrect current password. UserId: {UserId}",
                    user.Id);
                return Result.Failure(Error.Failure(ErrorCode.ValidationFailed, _localizer["auth.currentPasswordIncorrect"]));
            }

            var strengthResult = _passwordService.ValidatePasswordStrength(request.NewPassword);
            if (!strengthResult.IsSuccess)
                return Result.Failure(strengthResult.Errors);

            var hashResult = _passwordService.HashPassword(request.NewPassword);
            if (!hashResult.IsSuccess)
                return Result.Failure(hashResult.Errors);

            user.PasswordHash = hashResult.Value;
            user.LastSessionsRevokedAt = DateTime.UtcNow;
            _unitOfWork.Users.Update(user);

            await _unitOfWork.RefreshTokens.RevokeAllUserTokensAsync(
                user.Id, reason: "Password changed");

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Password changed. UserId: {UserId}, Email: {Email}",
                user.Id, user.Email);

            return Result.Success();
        }
    }
}
