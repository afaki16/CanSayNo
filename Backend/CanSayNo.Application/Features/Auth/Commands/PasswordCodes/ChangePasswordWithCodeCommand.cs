using CanSayNo.Application.Common.Localization;
using CanSayNo.Application.Common.Results;
using CanSayNo.Application.Interfaces;
using CanSayNo.Domain.Common.Enums;
using CanSayNo.Domain.Common.Interfaces;
using CanSayNo.Domain.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CanSayNo.Application.Features.Auth.Commands.PasswordCodes
{
    /// <summary>Signed-in user sets a new password with the emailed code instead of the current password.</summary>
    public class ChangePasswordWithCodeCommand : IRequest<Result>
    {
        public string Code { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class ChangePasswordWithCodeCommandHandler : IRequestHandler<ChangePasswordWithCodeCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IOtpService _otpService;
        private readonly IPasswordService _passwordService;
        private readonly IJsonStringLocalizer _localizer;
        private readonly ILogger<ChangePasswordWithCodeCommandHandler> _logger;

        public ChangePasswordWithCodeCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IOtpService otpService,
            IPasswordService passwordService,
            IJsonStringLocalizer localizer,
            ILogger<ChangePasswordWithCodeCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _otpService = otpService;
            _passwordService = passwordService;
            _localizer = localizer;
            _logger = logger;
        }

        public async Task<Result> Handle(ChangePasswordWithCodeCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
                return Result.Failure(Error.Failure(ErrorCode.Unauthorized, _localizer["auth.userNotAuthenticated"]));

            var user = await _unitOfWork.Users.GetByIdAsync(userId.Value);
            if (user is null)
                return Result.Failure(Error.Failure(ErrorCode.NotFound, _localizer["users.notFound"]));

            // Check the password first so a weak password does not burn the code
            var strengthResult = _passwordService.ValidatePasswordStrength(request.NewPassword);
            if (!strengthResult.IsSuccess)
                return Result.Failure(strengthResult.Errors);

            var verify = await _otpService.VerifyAsync(user, VerificationPurpose.PasswordChange, request.Code, cancellationToken);
            if (!verify.IsSuccess)
                return verify;

            var hashResult = _passwordService.HashPassword(request.NewPassword);
            if (!hashResult.IsSuccess)
                return Result.Failure(hashResult.Errors);

            user.PasswordHash = hashResult.Value;
            user.LastSessionsRevokedAt = DateTime.UtcNow;
            _unitOfWork.Users.Update(user);

            await _unitOfWork.RefreshTokens.RevokeAllUserTokensAsync(user.Id, reason: "Password changed");
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Password changed with code. UserId: {UserId}", user.Id);
            return Result.Success();
        }
    }
}
