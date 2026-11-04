using CanSayNo.Application.Common.Localization;
using CanSayNo.Application.Common.Results;
using CanSayNo.Application.Interfaces;
using CanSayNo.Domain.Common.Enums;
using CanSayNo.Domain.Common.Interfaces;
using CanSayNo.Domain.Models;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CanSayNo.Application.Features.Auth.Commands.EmailChangeCodes
{
    /// <summary>
    /// Completes the email change with the code sent to the new address. Signs out everywhere
    /// (tokens carry the old email) and notifies the old inbox — same effects as the link flow.
    /// </summary>
    public class ConfirmEmailChangeWithCodeCommand : IRequest<Result>
    {
        public string Code { get; set; } = string.Empty;
        public string? IpAddress { get; set; }
    }

    public class ConfirmEmailChangeWithCodeCommandValidator : AbstractValidator<ConfirmEmailChangeWithCodeCommand>
    {
        public ConfirmEmailChangeWithCodeCommandValidator(IJsonStringLocalizer localizer)
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage(localizer["auth.otpInvalidOrExpired"])
                .Matches("^[0-9]{6}$").WithMessage(localizer["auth.otpInvalidOrExpired"]);
        }
    }

    public class ConfirmEmailChangeWithCodeCommandHandler : IRequestHandler<ConfirmEmailChangeWithCodeCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IOtpService _otpService;
        private readonly IEmailService _emailService;
        private readonly IJsonStringLocalizer _localizer;
        private readonly ILogger<ConfirmEmailChangeWithCodeCommandHandler> _logger;

        public ConfirmEmailChangeWithCodeCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IOtpService otpService,
            IEmailService emailService,
            IJsonStringLocalizer localizer,
            ILogger<ConfirmEmailChangeWithCodeCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _otpService = otpService;
            _emailService = emailService;
            _localizer = localizer;
            _logger = logger;
        }

        public async Task<Result> Handle(ConfirmEmailChangeWithCodeCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
                return Result.Failure(Error.Failure(ErrorCode.Unauthorized, _localizer["auth.userNotAuthenticated"]));

            var user = await _unitOfWork.Users.GetByIdAsync(userId.Value);
            if (user is null)
                return Result.Failure(Error.Failure(ErrorCode.NotFound, _localizer["users.notFound"]));

            var verify = await _otpService.VerifyForDestinationAsync(
                user, VerificationPurpose.EmailChangeCode, request.Code, cancellationToken);
            if (!verify.IsSuccess)
                return Result.Failure(verify.Errors);

            var newEmail = verify.Value;

            // The address may have been taken by someone else since the code was sent
            if (await _unitOfWork.Users.EmailExistsAsync(newEmail))
                return Result.Failure(Error.Failure(ErrorCode.AlreadyExists, _localizer["users.emailExists"]));

            var oldEmail = user.Email;
            user.Email = newEmail;
            // The code reached the new inbox, so the new address is verified
            user.EmailConfirmed = true;
            // Tokens carry the old email: sign out everywhere, sign in again with the new address
            user.LastSessionsRevokedAt = DateTime.UtcNow;
            _unitOfWork.Users.Update(user);

            await _unitOfWork.RefreshTokens.RevokeAllUserTokensAsync(user.Id, reason: "Email changed");
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Tell the previous owner of the inbox; if this was not them, they now know the account moved
            await _emailService.SendEmailChangedNoticeAsync(oldEmail, user.FirstName, newEmail);

            _logger.LogInformation("Email changed with code. UserId: {UserId}, IpAddress: {IpAddress}", user.Id, request.IpAddress);
            return Result.Success();
        }
    }
}
