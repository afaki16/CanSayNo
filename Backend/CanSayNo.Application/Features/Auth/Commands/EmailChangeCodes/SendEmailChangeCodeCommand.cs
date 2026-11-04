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
    /// Signed-in user asks to move the account to a new address; a code goes to the NEW inbox.
    /// Code-based twin of RequestEmailChangeCommand (link) for the mobile app.
    /// </summary>
    public class SendEmailChangeCodeCommand : IRequest<Result>
    {
        public string NewEmail { get; set; } = string.Empty;
        public string CurrentPassword { get; set; } = string.Empty;
        public string? IpAddress { get; set; }
    }

    public class SendEmailChangeCodeCommandValidator : AbstractValidator<SendEmailChangeCodeCommand>
    {
        public SendEmailChangeCodeCommandValidator(IJsonStringLocalizer localizer)
        {
            RuleFor(x => x.NewEmail)
                .NotEmpty().WithMessage(localizer["validation.required", localizer["validation.fields.email"]])
                .EmailAddress().WithMessage(localizer["validation.emailFormat"])
                .MaximumLength(100).WithMessage(localizer["validation.maxLength", localizer["validation.fields.email"], 100]);

            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage(localizer["validation.required", localizer["validation.fields.password"]]);
        }
    }

    public class SendEmailChangeCodeCommandHandler : IRequestHandler<SendEmailChangeCodeCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IPasswordService _passwordService;
        private readonly IOtpService _otpService;
        private readonly IEmailService _emailService;
        private readonly IJsonStringLocalizer _localizer;
        private readonly ILogger<SendEmailChangeCodeCommandHandler> _logger;

        public SendEmailChangeCodeCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IPasswordService passwordService,
            IOtpService otpService,
            IEmailService emailService,
            IJsonStringLocalizer localizer,
            ILogger<SendEmailChangeCodeCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _passwordService = passwordService;
            _otpService = otpService;
            _emailService = emailService;
            _localizer = localizer;
            _logger = logger;
        }

        public async Task<Result> Handle(SendEmailChangeCodeCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
                return Result.Failure(Error.Failure(ErrorCode.Unauthorized, _localizer["auth.userNotAuthenticated"]));

            var user = await _unitOfWork.Users.GetByIdAsync(userId.Value);
            if (user is null)
                return Result.Failure(Error.Failure(ErrorCode.NotFound, _localizer["users.notFound"]));

            // Re-authenticate: a stolen session alone must not be able to move the account to another inbox
            var verifyResult = _passwordService.VerifyPassword(request.CurrentPassword, user.PasswordHash);
            if (!verifyResult.IsSuccess)
                return Result.Failure(verifyResult.Errors);
            if (!verifyResult.Value)
            {
                _logger.LogWarning("Email change code rejected - incorrect current password. UserId: {UserId}", user.Id);
                return Result.Failure(Error.Failure(ErrorCode.ValidationFailed, _localizer["auth.currentPasswordIncorrect"]));
            }

            var newEmail = request.NewEmail.Trim().ToLowerInvariant();
            if (newEmail == user.Email)
                return Result.Failure(Error.Failure(ErrorCode.ValidationFailed, _localizer["auth.emailChangeSameAddress"]));

            if (await _unitOfWork.Users.EmailExistsAsync(newEmail))
                return Result.Failure(Error.Failure(ErrorCode.AlreadyExists, _localizer["users.emailExists"]));

            var issue = await _otpService.IssueAsync(
                user, VerificationPurpose.EmailChangeCode, request.IpAddress, cancellationToken, destination: newEmail);
            if (!issue.IsSuccess)
                return Result.Failure(issue.Errors);

            var sendResult = await _emailService.SendEmailChangeCodeAsync(
                newEmail, user.FirstName, issue.Value.Code, issue.Value.ValidMinutes);
            if (!sendResult.IsSuccess)
                return Result.Failure(Error.Failure(ErrorCode.InternalError, _localizer["auth.emailChangeSendFailed"]));

            _logger.LogInformation("Email change code sent. UserId: {UserId}, IpAddress: {IpAddress}", user.Id, request.IpAddress);
            return Result.Success();
        }
    }
}
