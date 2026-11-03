using CanSayNo.Application.Common.Localization;
using CanSayNo.Application.Common.Results;
using CanSayNo.Application.Common.Security;
using CanSayNo.Application.Interfaces;
using CanSayNo.Domain.Common.Enums;
using CanSayNo.Domain.Common.Interfaces;
using CanSayNo.Domain.Entities;
using CanSayNo.Domain.Models;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Web;

namespace CanSayNo.Application.Features.Auth.Commands.RequestEmailChange
{
    public class RequestEmailChangeCommandHandler : IRequestHandler<RequestEmailChangeCommand, Result>
    {
        public const int LinkValidHours = 24;

        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordService _passwordService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RequestEmailChangeCommandHandler> _logger;
        private readonly IJsonStringLocalizer _localizer;

        public RequestEmailChangeCommandHandler(
            IUnitOfWork unitOfWork,
            IPasswordService passwordService,
            ICurrentUserService currentUserService,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<RequestEmailChangeCommandHandler> logger,
            IJsonStringLocalizer localizer)
        {
            _unitOfWork = unitOfWork;
            _passwordService = passwordService;
            _currentUserService = currentUserService;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
            _localizer = localizer;
        }

        public async Task<Result> Handle(RequestEmailChangeCommand request, CancellationToken cancellationToken)
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
                _logger.LogWarning("Email change rejected - incorrect current password. UserId: {UserId}", user.Id);
                return Result.Failure(Error.Failure(ErrorCode.ValidationFailed, _localizer["auth.currentPasswordIncorrect"]));
            }

            var newEmail = request.NewEmail.Trim().ToLowerInvariant();
            if (newEmail == user.Email)
                return Result.Failure(Error.Failure(ErrorCode.ValidationFailed, _localizer["auth.emailChangeSameAddress"]));

            if (await _unitOfWork.Users.EmailExistsAsync(newEmail, user.TenantId))
                return Result.Failure(Error.Failure(ErrorCode.AlreadyExists, _localizer["users.emailExists"]));

            // Only the latest request is valid
            await _unitOfWork.UserVerificationTokens.InvalidatePreviousTokensAsync(
                user.Id, VerificationChannel.Email, VerificationPurpose.EmailChange);

            var plainToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            await _unitOfWork.UserVerificationTokens.AddAsync(new UserVerificationToken
            {
                UserId = user.Id,
                Token = RefreshTokenHasher.Hash(plainToken),
                Channel = VerificationChannel.Email,
                Purpose = VerificationPurpose.EmailChange,
                Destination = newEmail,
                ExpiresAt = DateTime.UtcNow.AddHours(LinkValidHours),
                RequestIpAddress = request.IpAddress
            });
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var frontendUrl = _configuration["App:FrontendUrl"];
            var confirmLink = $"{frontendUrl}/auth/confirm-email-change?token={HttpUtility.UrlEncode(plainToken)}&email={HttpUtility.UrlEncode(newEmail)}";

            var sendResult = await _emailService.SendEmailChangeVerificationAsync(newEmail, user.FirstName, confirmLink, LinkValidHours);
            if (!sendResult.IsSuccess)
                return Result.Failure(Error.Failure(ErrorCode.InternalError, _localizer["auth.emailChangeSendFailed"]));

            _logger.LogInformation("Email change requested. UserId: {UserId}, IpAddress: {IpAddress}", user.Id, request.IpAddress);
            return Result.Success();
        }
    }
}
