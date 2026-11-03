using CanSayNo.Application.Common.Localization;
using CanSayNo.Application.Common.Results;
using CanSayNo.Application.Interfaces;
using CanSayNo.Domain.Common.Enums;
using CanSayNo.Domain.Common.Interfaces;
using CanSayNo.Domain.Entities;
using CanSayNo.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace CanSayNo.Infrastructure.Services
{
    public class OtpService : IOtpService
    {
        private const int CodeLength = 6;
        private const int ValidMinutes = 10;
        private const int MaxFailedAttempts = 5;
        private const int ResendCooldownSeconds = 60;

        private readonly IUnitOfWork _unitOfWork;
        private readonly IJsonStringLocalizer _localizer;
        private readonly ILogger<OtpService> _logger;
        private readonly byte[] _hashKey;

        public OtpService(
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            IJsonStringLocalizer localizer,
            ILogger<OtpService> logger)
        {
            _unitOfWork = unitOfWork;
            _localizer = localizer;
            _logger = logger;

            // A 6-digit code has only 10^6 values: a plain hash is reversed instantly if the table leaks,
            // so codes are stored as HMAC with a server-side key. Prefer a dedicated Otp:HashKey; otherwise
            // derive a separate key from the JWT secret (never use the JWT key itself for another purpose).
            var secret = configuration["Otp:HashKey"];
            _hashKey = !string.IsNullOrWhiteSpace(secret)
                ? Encoding.UTF8.GetBytes(secret)
                : HMACSHA256.HashData(
                    Encoding.UTF8.GetBytes(configuration["JwtSettings:SecretKey"]
                        ?? throw new InvalidOperationException("Otp:HashKey or JwtSettings:SecretKey must be configured.")),
                    Encoding.UTF8.GetBytes("kickstart-otp-hash-key"));
        }

        public async Task<Result<OtpIssueResult>> IssueAsync(User user, VerificationPurpose purpose, string? ipAddress, CancellationToken cancellationToken = default)
        {
            var latest = await _unitOfWork.UserVerificationTokens.GetLatestAsync(user.Id, VerificationChannel.Email, purpose);
            if (latest != null)
            {
                var secondsSince = (DateTime.UtcNow - latest.CreatedDate).TotalSeconds;
                if (secondsSince < ResendCooldownSeconds)
                {
                    var remaining = (int)Math.Ceiling(ResendCooldownSeconds - secondsSince);
                    return Result<OtpIssueResult>.Failure(Error.Failure(
                        ErrorCode.InvalidOperation,
                        _localizer["auth.otpResendCooldown", remaining]));
                }
            }

            await _unitOfWork.UserVerificationTokens.InvalidatePreviousTokensAsync(user.Id, VerificationChannel.Email, purpose);

            var code = RandomNumberGenerator.GetInt32(0, (int)Math.Pow(10, CodeLength)).ToString($"D{CodeLength}");
            await _unitOfWork.UserVerificationTokens.AddAsync(new UserVerificationToken
            {
                UserId = user.Id,
                Token = HashCode(user.Id, purpose, code),
                Channel = VerificationChannel.Email,
                Purpose = purpose,
                Destination = user.Email,
                ExpiresAt = DateTime.UtcNow.AddMinutes(ValidMinutes),
                RequestIpAddress = ipAddress
            });
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<OtpIssueResult>.Success(new OtpIssueResult(code, ValidMinutes));
        }

        public async Task<Result> VerifyAsync(User user, VerificationPurpose purpose, string code, CancellationToken cancellationToken = default)
        {
            var token = await _unitOfWork.UserVerificationTokens.GetLatestAsync(user.Id, VerificationChannel.Email, purpose);
            if (token is null || !token.IsValid)
                return Result.Failure(Error.Failure(ErrorCode.ValidationFailed, _localizer["auth.otpInvalidOrExpired"]));

            var expected = Convert.FromBase64String(token.Token);
            var actual = Convert.FromBase64String(HashCode(user.Id, purpose, (code ?? string.Empty).Trim()));
            if (!CryptographicOperations.FixedTimeEquals(expected, actual))
            {
                token.FailedAttempts++;
                var burned = token.FailedAttempts >= MaxFailedAttempts;
                if (burned)
                    token.MarkAsUsed();

                _unitOfWork.UserVerificationTokens.Update(token);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogWarning(
                    "Wrong OTP code. UserId: {UserId}, Purpose: {Purpose}, Attempts: {Attempts}, Burned: {Burned}",
                    user.Id, purpose, token.FailedAttempts, burned);

                return Result.Failure(Error.Failure(
                    ErrorCode.ValidationFailed,
                    burned ? _localizer["auth.otpTooManyAttempts"] : _localizer["auth.otpInvalidOrExpired"]));
            }

            token.MarkAsUsed();
            _unitOfWork.UserVerificationTokens.Update(token);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }

        // Binding user and purpose into the MAC keeps a code from being valid for another account or flow
        private string HashCode(int userId, VerificationPurpose purpose, string code)
        {
            var mac = HMACSHA256.HashData(_hashKey, Encoding.UTF8.GetBytes($"{userId}:{(int)purpose}:{code}"));
            return Convert.ToBase64String(mac);
        }
    }
}
