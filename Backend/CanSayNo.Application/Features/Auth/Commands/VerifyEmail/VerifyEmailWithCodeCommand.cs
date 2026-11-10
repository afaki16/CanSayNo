using CanSayNo.Application.Common.Localization;
using CanSayNo.Application.Common.Results;
using CanSayNo.Application.Features.Auth.Commands.PasswordCodes;
using CanSayNo.Application.Interfaces;
using CanSayNo.Domain.Common.Enums;
using CanSayNo.Domain.Common.Interfaces;
using CanSayNo.Domain.Models;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CanSayNo.Application.Features.Auth.Commands.VerifyEmail
{
    /// <summary>
    /// Verifies a new account with the 6-digit code emailed at registration (mobile).
    /// Same answer for an unknown email and a wrong code (no account enumeration).
    /// </summary>
    public class VerifyEmailWithCodeCommand : IRequest<Result>
    {
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? IpAddress { get; set; }
    }

    public class VerifyEmailWithCodeCommandValidator : AbstractValidator<VerifyEmailWithCodeCommand>
    {
        public VerifyEmailWithCodeCommandValidator(IJsonStringLocalizer localizer)
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(localizer["validation.required", localizer["validation.fields.email"]])
                .EmailAddress().WithMessage(localizer["validation.emailFormat"]);

            // A malformed code gets the same message as a wrong one
            RuleFor(x => x.Code)
                .Matches("^[0-9]{6}$").WithMessage(localizer["auth.otpInvalidOrExpired"]);
        }
    }

    public class VerifyEmailWithCodeCommandHandler : IRequestHandler<VerifyEmailWithCodeCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOtpService _otpService;
        private readonly IJsonStringLocalizer _localizer;
        private readonly ILogger<VerifyEmailWithCodeCommandHandler> _logger;

        public VerifyEmailWithCodeCommandHandler(
            IUnitOfWork unitOfWork,
            IOtpService otpService,
            IJsonStringLocalizer localizer,
            ILogger<VerifyEmailWithCodeCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _otpService = otpService;
            _localizer = localizer;
            _logger = logger;
        }

        public async Task<Result> Handle(VerifyEmailWithCodeCommand request, CancellationToken cancellationToken)
        {
            var user = await PasswordCodeUserLookup.FindAsync(_unitOfWork, request.Email, tenantId: 0);
            if (user is null)
                return Result.Failure(Error.Failure(ErrorCode.ValidationFailed, _localizer["auth.otpInvalidOrExpired"]));

            // An already verified account has no active code, so this fails like a wrong code
            var verify = await _otpService.VerifyAsync(user, VerificationPurpose.RegistrationCode, request.Code, cancellationToken);
            if (!verify.IsSuccess)
                return verify;

            user.EmailConfirmed = true;
            // Only a pending registration is activated; inactive/banned accounts stay as they are
            if (user.Status == UserStatus.PendingVerification)
                user.Status = UserStatus.Active;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Email verified with code. UserId: {UserId}, IpAddress: {IpAddress}", user.Id, request.IpAddress);
            return Result.Success();
        }
    }
}
