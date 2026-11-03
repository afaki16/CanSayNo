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
    /// <summary>Signed-in user who forgot the current password asks for a code to their email.</summary>
    public class SendPasswordChangeCodeCommand : IRequest<Result>
    {
        public string? IpAddress { get; set; }
    }

    public class SendPasswordChangeCodeCommandHandler : IRequestHandler<SendPasswordChangeCodeCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IOtpService _otpService;
        private readonly IEmailService _emailService;
        private readonly IJsonStringLocalizer _localizer;
        private readonly ILogger<SendPasswordChangeCodeCommandHandler> _logger;

        public SendPasswordChangeCodeCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IOtpService otpService,
            IEmailService emailService,
            IJsonStringLocalizer localizer,
            ILogger<SendPasswordChangeCodeCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _otpService = otpService;
            _emailService = emailService;
            _localizer = localizer;
            _logger = logger;
        }

        public async Task<Result> Handle(SendPasswordChangeCodeCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
                return Result.Failure(Error.Failure(ErrorCode.Unauthorized, _localizer["auth.userNotAuthenticated"]));

            var user = await _unitOfWork.Users.GetByIdAsync(userId.Value);
            if (user is null)
                return Result.Failure(Error.Failure(ErrorCode.NotFound, _localizer["users.notFound"]));

            // Signed in, so the cooldown can be reported as-is (nothing to hide about the account)
            var issue = await _otpService.IssueAsync(user, VerificationPurpose.PasswordChange, request.IpAddress, cancellationToken);
            if (!issue.IsSuccess)
                return Result.Failure(issue.Errors);

            var sendResult = await _emailService.SendPasswordCodeAsync(user.Email, user.FirstName, issue.Value.Code, issue.Value.ValidMinutes);
            if (!sendResult.IsSuccess)
                return Result.Failure(Error.Failure(ErrorCode.InternalError, _localizer["auth.otpSendFailed"]));

            _logger.LogInformation("Password change code sent. UserId: {UserId}", user.Id);
            return Result.Success();
        }
    }
}
