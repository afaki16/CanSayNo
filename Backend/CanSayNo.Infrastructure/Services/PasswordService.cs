using CanSayNo.Application.Interfaces;
using CanSayNo.Application.Common.Localization;
using BCrypt.Net;
using CanSayNo.Application.Common.Security;
using CanSayNo.Domain.Common.Interfaces;
using CanSayNo.Domain.Common.Interfaces.Repositories;
using CanSayNo.Domain.Entities;
using CanSayNo.Application.Common.Results;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using CanSayNo.Domain.Models;
using CanSayNo.Domain.Common.Enums;

namespace CanSayNo.Infrastructure.Services
{
    public class PasswordService : IPasswordService
    {
        private readonly ILogger<PasswordService> _logger;
        private readonly IJsonStringLocalizer _localizer;

        public PasswordService(ILogger<PasswordService> logger, IJsonStringLocalizer localizer)
        {
            _logger = logger;
            _localizer = localizer;
        }

    public Result<string> HashPassword(string password)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(password))
                return Result<string>.Failure(Error.Failure(
                    ErrorCode.ValidationFailed,
                    _localizer["password.empty"]));

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
            return Result<string>.Success(hashedPassword);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to hash password");
            return Result<string>.Failure(Error.Failure(
                ErrorCode.InternalError,
                _localizer["password.hashFailed"]));
        }
    }

    public Result<bool> VerifyPassword(string password, string hashedPassword)
    {
        // Always exercise BCrypt to keep verify time constant — a fast-path early
        // return on null/empty/malformed hash would leak timing information that an
        // attacker can use for user enumeration.
        var safePassword = password ?? string.Empty;

        if (string.IsNullOrWhiteSpace(hashedPassword))
        {
            try { BCrypt.Net.BCrypt.Verify(safePassword, SecurityConstants.DummyBCryptHash); }
            catch { /* dummy verify — result discarded */ }
            return Result<bool>.Success(false);
        }

        try
        {
            var isValid = BCrypt.Net.BCrypt.Verify(safePassword, hashedPassword);
            return Result<bool>.Success(isValid);
        }
        catch (SaltParseException)
        {
            try { BCrypt.Net.BCrypt.Verify(safePassword, SecurityConstants.DummyBCryptHash); }
            catch { /* dummy verify — result discarded */ }
            return Result<bool>.Success(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify password");
            return Result<bool>.Failure(Error.Failure(
                ErrorCode.InternalError,
                _localizer["password.verifyFailed"]));
        }
    }

    public Result<string> GenerateRandomPassword(int length = 12)
        {
            try
            {
                if (length < 8)
                return Result<string>.Failure(
            Error.Failure(ErrorCode.ValidationFailed, _localizer["password.tooShort"]));

            const string lowercase = "abcdefghijklmnopqrstuvwxyz";
                const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                const string digits = "0123456789";
                const string special = "!@#$%^&*";
                
                var password = new StringBuilder();

                password.Append(lowercase[RandomNumberGenerator.GetInt32(lowercase.Length)]);
                password.Append(uppercase[RandomNumberGenerator.GetInt32(uppercase.Length)]);
                password.Append(digits[RandomNumberGenerator.GetInt32(digits.Length)]);
                password.Append(special[RandomNumberGenerator.GetInt32(special.Length)]);

                string allChars = lowercase + uppercase + digits + special;
                for (int i = 4; i < length; i++)
                    password.Append(allChars[RandomNumberGenerator.GetInt32(allChars.Length)]);

                var chars = password.ToString().ToCharArray();
                for (int i = chars.Length - 1; i > 0; i--)
                {
                    int j = RandomNumberGenerator.GetInt32(i + 1);
                    (chars[i], chars[j]) = (chars[j], chars[i]);
                }

                return Result<string>.Success(new string(chars));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate random password");
                return Result<string>.Failure(
                    Error.Failure(ErrorCode.InternalError, _localizer["password.generationFailed"]));
            }
        }

    public Result<bool> ValidatePasswordStrength(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return Result<bool>.Failure(
                Error.Failure(ErrorCode.ValidationFailed, _localizer["password.empty"]));

        if (password.Length < 8)
            return Result<bool>.Failure(
                Error.Failure(ErrorCode.ValidationFailed, _localizer["password.tooShort"]));

        if (!Regex.IsMatch(password, @"[a-z]"))
            return Result<bool>.Failure(
                Error.Failure(ErrorCode.ValidationFailed, _localizer["password.missingLowercase"]));

        if (!Regex.IsMatch(password, @"[A-Z]"))
            return Result<bool>.Failure(
                Error.Failure(ErrorCode.ValidationFailed, _localizer["password.missingUppercase"]));

        if (!Regex.IsMatch(password, @"\d"))
            return Result<bool>.Failure(
                Error.Failure(ErrorCode.ValidationFailed, _localizer["password.missingDigit"]));

        return Result<bool>.Success(true);
    }

}
} 
