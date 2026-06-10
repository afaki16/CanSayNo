using Kickstart.Application.Common.Localization;
using Kickstart.Application.Common.Results;
using Kickstart.Application.Common.Security;
using Kickstart.Application.Interfaces;
using Kickstart.Domain.Common.Enums;
using Kickstart.Domain.Common.Interfaces;
using Kickstart.Domain.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Kickstart.Application.Features.Auth.Commands.ResetPassword
{
    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordService _passwordService;
        private readonly ILogger<ResetPasswordCommandHandler> _logger;
        private readonly IJsonStringLocalizer _localizer;

        public ResetPasswordCommandHandler(
            IUnitOfWork unitOfWork,
            IPasswordService passwordService,
            ILogger<ResetPasswordCommandHandler> logger,
            IJsonStringLocalizer localizer)
        {
            _unitOfWork = unitOfWork;
            _passwordService = passwordService;
            _logger = logger;
            _localizer = localizer;
        }

        public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var normalizedEmail = request.Email?.Trim().ToLowerInvariant();
            var hashedToken = RefreshTokenHasher.Hash(request.Token);
            var resetToken = await _unitOfWork.PasswordResetTokens.GetValidTokenAsync(hashedToken, normalizedEmail);

            if (resetToken is null)
            {
                _logger.LogWarning(
                    "Password reset failed - invalid or expired token. Email: {Email}",
                    normalizedEmail);
                return Result.Failure(Error.Failure(ErrorCode.ValidationFailed, _localizer["auth.invalidOrExpiredResetLink"]));
            }

            var strengthResult = _passwordService.ValidatePasswordStrength(request.NewPassword);
            if (!strengthResult.IsSuccess)
                return Result.Failure(strengthResult.Errors);

            var hashResult = _passwordService.HashPassword(request.NewPassword);
            if (!hashResult.IsSuccess)
                return Result.Failure(hashResult.Errors);

            resetToken.User.PasswordHash = hashResult.Value;
            resetToken.User.LastSessionsRevokedAt = DateTime.UtcNow;
            resetToken.MarkAsUsed();

            _unitOfWork.Users.Update(resetToken.User);
            _unitOfWork.PasswordResetTokens.Update(resetToken);

            await _unitOfWork.RefreshTokens.RevokeAllUserTokensAsync(
                resetToken.User.Id, reason: "Password reset");

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Password reset completed. UserId: {UserId}, Email: {Email}",
                resetToken.User.Id, resetToken.User.Email);

            return Result.Success();
        }
    }
}
