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
    /// <summary>Sets a new password with the code from <see cref="SendPasswordResetCodeCommand"/> (signed out).</summary>
    public class ResetPasswordWithCodeCommand : IRequest<Result>
    {
        public string Email { get; set; } = string.Empty;
        public int TenantId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string? IpAddress { get; set; }
    }

    public class ResetPasswordWithCodeCommandHandler : IRequestHandler<ResetPasswordWithCodeCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOtpService _otpService;
        private readonly IPasswordService _passwordService;
        private readonly IBruteForceProtectionService _bruteForceService;
        private readonly IJsonStringLocalizer _localizer;
        private readonly ILogger<ResetPasswordWithCodeCommandHandler> _logger;

        public ResetPasswordWithCodeCommandHandler(
            IUnitOfWork unitOfWork,
            IOtpService otpService,
            IPasswordService passwordService,
            IBruteForceProtectionService bruteForceService,
            IJsonStringLocalizer localizer,
            ILogger<ResetPasswordWithCodeCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _otpService = otpService;
            _passwordService = passwordService;
            _bruteForceService = bruteForceService;
            _localizer = localizer;
            _logger = logger;
        }

        public async Task<Result> Handle(ResetPasswordWithCodeCommand request, CancellationToken cancellationToken)
        {
            // Check the password first so a weak password does not burn the code
            var strengthResult = _passwordService.ValidatePasswordStrength(request.NewPassword);
            if (!strengthResult.IsSuccess)
                return Result.Failure(strengthResult.Errors);

            // Unknown email gets the same answer as a wrong code (no account enumeration)
            var user = await PasswordCodeUserLookup.FindAsync(_unitOfWork, request.Email, request.TenantId);
            if (user is null)
                return Result.Failure(Error.Failure(ErrorCode.ValidationFailed, _localizer["auth.otpInvalidOrExpired"]));

            var verify = await _otpService.VerifyAsync(user, VerificationPurpose.PasswordReset, request.Code, cancellationToken);
            if (!verify.IsSuccess)
                return verify;

            var hashResult = _passwordService.HashPassword(request.NewPassword);
            if (!hashResult.IsSuccess)
                return Result.Failure(hashResult.Errors);

            user.PasswordHash = hashResult.Value;
            user.LastSessionsRevokedAt = DateTime.UtcNow;
            // The code was delivered to this address, so using it proves ownership. A pending
            // registration is completed too; inactive/banned accounts stay as they are.
            user.EmailConfirmed = true;
            if (user.Status == UserStatus.PendingVerification)
                user.Status = UserStatus.Active;
            _unitOfWork.Users.Update(user);

            await _unitOfWork.RefreshTokens.RevokeAllUserTokensAsync(user.Id, reason: "Password reset");
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // The account owner proved control of the email; lift any lockout from every IP
            await _bruteForceService.ClearFailuresForEmailAsync(user.Email, cancellationToken);

            _logger.LogInformation("Password reset with code. UserId: {UserId}, IpAddress: {IpAddress}", user.Id, request.IpAddress);
            return Result.Success();
        }
    }
}
