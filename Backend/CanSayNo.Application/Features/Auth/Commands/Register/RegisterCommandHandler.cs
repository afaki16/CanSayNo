using CanSayNo.Application.Interfaces;
using AutoMapper;
using CanSayNo.Application.Features.Auth.Commands.Login;
using CanSayNo.Application.Features.Auth.Commands.Logout;
using CanSayNo.Application.Features.Auth.Commands.LogoutAll;
using CanSayNo.Application.Features.Auth.Commands.LogoutDevice;
using CanSayNo.Application.Features.Auth.Commands.Register;
using CanSayNo.Application.Features.Auth.Commands.RefreshToken;
using CanSayNo.Application.Features.Auth.Commands.ChangePassword;
using CanSayNo.Application.Features.Auth.Commands.ForgotPassword;
using CanSayNo.Application.Features.Auth.Commands.ResetPassword;
using CanSayNo.Application.Common.Security;
using CanSayNo.Application.Common.Localization;
using CanSayNo.Domain.Common.Interfaces;
using CanSayNo.Domain.Constants;
using CanSayNo.Domain.Common.Interfaces.Repositories;
using CanSayNo.Application.Common.Results;
using CanSayNo.Domain.Entities;
using CanSayNo.Domain.Common.Enums;
using CanSayNo.Domain.Models;
using CanSayNo.Application.Features.Users.Dtos;
using CanSayNo.Application.Features.Roles.Dtos;
using CanSayNo.Application.Features.Tenants.Dtos;
using CanSayNo.Application.Features.Permissions.Dtos;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace CanSayNo.Application.Features.Auth.Commands.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<UserDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordService _passwordService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly ILogger<RegisterCommandHandler> _logger;
        private readonly IJsonStringLocalizer _localizer;

        public RegisterCommandHandler(
            IUnitOfWork unitOfWork,
            IPasswordService passwordService,
            IEmailService emailService,
            IConfiguration configuration,
            IMapper mapper,
            ILogger<RegisterCommandHandler> logger,
            IJsonStringLocalizer localizer)
        {
            _unitOfWork = unitOfWork;
            _passwordService = passwordService;
            _emailService = emailService;
            _configuration = configuration;
            _mapper = mapper;
            _logger = logger;
            _localizer = localizer;
        }

        public async Task<Result<UserDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var normalizedEmail = request.Email?.Trim().ToLowerInvariant();

            // Public register: users without a tenant share one uniqueness scope (TenantId null)
            if (await _unitOfWork.Users.EmailExistsAsync(normalizedEmail, tenantId: null))
             return Result<UserDto>.Failure(Error.Failure(
                  ErrorCode.AlreadyExists,
                  _localizer["users.emailExists"]));

            if (!string.IsNullOrWhiteSpace(request.PhoneNumber) &&
                await _unitOfWork.Users.PhoneExistsAsync(request.PhoneNumber, tenantId: null))
                return Result<UserDto>.Failure(Error.Failure(
                    ErrorCode.AlreadyExists,
                    _localizer["users.phoneExists"]));

            // Validate password strength (single source of truth: PasswordService)
            var strengthResult = _passwordService.ValidatePasswordStrength(request.Password);
            if (!strengthResult.IsSuccess)
                return Result<UserDto>.Failure(strengthResult.Errors);

        // Hash password
        var passwordResult = _passwordService.HashPassword(request.Password);
            if (!passwordResult.IsSuccess)
            return Result<UserDto>.Failure(Error.Failure(
            ErrorCode.AlreadyExists,
            $"{passwordResult.Error}"));

        // Create user
        var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = normalizedEmail,
                PasswordHash = passwordResult.Value,
                // Blank phone is stored as NULL, never "": the unique index treats NULLs as
                // distinct but would collide on repeated empty strings.
                PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim(),
                Status = UserStatus.PendingVerification,
                EmailConfirmed = false
            };

            // Assign default User role
            var userRole = await _unitOfWork.Roles.GetByNameAsync(RoleNames.User);
            if (userRole != null)
            {
                user.UserRoles.Add(new UserRole { RoleId = userRole.Id });
            }

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "User registered. UserId: {UserId}, Email: {Email}, TenantId: {TenantId}",
                user.Id, normalizedEmail, user.TenantId);

            await IssueVerificationEmailAsync(user, request.IpAddress, cancellationToken);

            var userDto = _mapper.Map<UserDto>(user);
            return Result<UserDto>.Success(userDto);
        }

        private async Task IssueVerificationEmailAsync(User user, string ipAddress, CancellationToken cancellationToken)
        {
            var plainToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            var hashedToken = RefreshTokenHasher.Hash(plainToken);

            var verificationToken = new UserVerificationToken
            {
                UserId = user.Id,
                Token = hashedToken,
                Channel = VerificationChannel.Email,
                Purpose = VerificationPurpose.Registration,
                Destination = user.Email,
                ExpiresAt = DateTime.UtcNow.AddHours(2),
                RequestIpAddress = ipAddress
            };

            await _unitOfWork.UserVerificationTokens.AddAsync(verificationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var frontendUrl = _configuration["App:FrontendUrl"];
            var verifyLink = $"{frontendUrl}/auth/verify-email?token={HttpUtility.UrlEncode(plainToken)}&email={HttpUtility.UrlEncode(user.Email)}";

            try
            {
                var sendResult = await _emailService.SendEmailConfirmationAsync(user.Email, verifyLink);
                if (!sendResult.IsSuccess)
                {
                    _logger.LogError(
                        "Failed to send verification email after register. UserId: {UserId}, Email: {Email}",
                        user.Id, user.Email);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Exception while sending verification email after register. UserId: {UserId}, Email: {Email}",
                    user.Id, user.Email);
            }
        }
    }
} 
