using Kickstart.Application.Common.Localization;
using Kickstart.Application.Common.Results;
using Kickstart.Application.Common.Security;
using Kickstart.Domain.Common.Enums;
using Kickstart.Domain.Common.Interfaces;
using Kickstart.Domain.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Kickstart.Application.Features.Auth.Commands.VerifyEmail
{
    public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<VerifyEmailCommandHandler> _logger;
        private readonly IJsonStringLocalizer _localizer;

        public VerifyEmailCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<VerifyEmailCommandHandler> logger,
            IJsonStringLocalizer localizer)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _localizer = localizer;
        }

        public async Task<Result> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
        {
            var normalizedEmail = request.Email?.Trim().ToLowerInvariant() ?? string.Empty;
            var hashedToken = RefreshTokenHasher.Hash(request.Token);

            var token = await _unitOfWork.UserVerificationTokens.GetValidTokenAsync(
                hashedToken,
                normalizedEmail,
                VerificationChannel.Email,
                VerificationPurpose.Registration);

            if (token is null)
            {
                _logger.LogWarning(
                    "Email verification failed - invalid or expired token. Email: {Email}, IpAddress: {IpAddress}",
                    normalizedEmail, request.IpAddress);
                return Result.Failure(Error.Failure(
                    ErrorCode.ValidationFailed,
                    _localizer["auth.invalidOrExpiredVerificationLink"]));
            }

            token.User.EmailConfirmed = true;
            token.User.Status = UserStatus.Active;
            token.MarkAsUsed();

            _unitOfWork.Users.Update(token.User);
            _unitOfWork.UserVerificationTokens.Update(token);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Email verified. UserId: {UserId}, Email: {Email}, IpAddress: {IpAddress}",
                token.User.Id, token.User.Email, request.IpAddress);

            return Result.Success();
        }
    }
}
