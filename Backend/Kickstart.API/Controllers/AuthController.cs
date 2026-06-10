using Kickstart.API.Controllers;
using Kickstart.Application.Features.Auth.Dtos;
using Kickstart.Application.Features.Auth.Commands.Login;
using Kickstart.Application.Features.Auth.Commands.Logout;
using Kickstart.Application.Features.Auth.Commands.LogoutAll;
using Kickstart.Application.Features.Auth.Commands.LogoutDevice;
using Kickstart.Application.Features.Auth.Commands.Register;
using Kickstart.Application.Features.Auth.Commands.RefreshToken;
using Kickstart.Application.Features.Auth.Commands.RevokeSessionById;
using Kickstart.Application.Features.Auth.Commands.ChangePassword;
using Kickstart.Application.Features.Auth.Commands.ForgotPassword;
using Kickstart.Application.Features.Auth.Commands.ResetPassword;
using Kickstart.Application.Features.Auth.Commands.VerifyEmail;
using Kickstart.Application.Features.Auth.Commands.ResendVerificationEmail;
using Kickstart.Application.Features.Auth.Queries.GetUserSessions;
using Kickstart.Application.Features.Users.Queries.GetCurrentUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Kickstart.API.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : BaseController
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Login with email and password
        /// </summary>
        /// <param name="request">Login credentials</param>
        /// <returns>Access token and user information</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        [EnableRateLimiting("login")]
        [ProducesResponseType(typeof(LoginResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var command = new LoginCommand
            {
                Email = request.Email,
                Password = request.Password,
                RememberMe = request.RememberMe,
                TenantId = request.TenantId,
                DeviceId = request.DeviceId,
                DeviceName = request.DeviceName,
                IpAddress = GetIpAddress(),
                UserAgent = GetUserAgent()
            };

            var result = await _mediator.Send(command);

            // On success, also drop an HttpOnly cookie so web clients can stop storing the
            // refresh token in JS-accessible places (localStorage / non-HttpOnly cookie).
            // The body still contains the token for backwards compatibility with mobile/CLI
            // clients that don't have a cookie jar - they'll keep working unchanged.
            if (result.IsSuccess && result.Value is not null)
            {
                WriteRefreshTokenCookie(result.Value.RefreshToken, result.Value.ExpiresAt);
            }

            return HandleResult(result);
        }

        /// <summary>
        /// Register a new user account
        /// </summary>
        /// <param name="request">Registration information</param>
        /// <returns>Created user information</returns>
        [HttpPost("register")]
        [AllowAnonymous]
        [EnableRateLimiting("sensitive")]
        [ProducesResponseType(typeof(Application.Features.Users.Dtos.UserDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            var command = new RegisterCommand
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Password = request.Password,
                ConfirmPassword = request.ConfirmPassword,
                PhoneNumber = request.PhoneNumber,
                IpAddress = GetIpAddress(),
                UserAgent = GetUserAgent()
            };

            var result = await _mediator.Send(command);
            
            if (result.IsSuccess)
                return CreatedAtAction(nameof(Login), new { }, new { success = true, data = result.Value });
            
            return HandleResult(result);
        }

        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        /// <param name="request">Token refresh information</param>
        /// <returns>New access token and refresh token</returns>
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        [EnableRateLimiting("refresh")]
        [ProducesResponseType(typeof(LoginResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            // Cookie takes priority over body. Mobile/CLI clients without a cookie jar
            // can still pass refreshToken in the body.
            var refreshToken = ReadRefreshToken(request.RefreshToken);

            var command = new RefreshTokenCommand
            {
                AccessToken = request.AccessToken,
                RefreshToken = refreshToken,
                IpAddress = GetIpAddress(),
                UserAgent = GetUserAgent()
            };

            var result = await _mediator.Send(command);

            // Rotate the cookie too so the new refresh token replaces the old one.
            if (result.IsSuccess && result.Value is not null)
            {
                WriteRefreshTokenCookie(result.Value.RefreshToken, result.Value.ExpiresAt);
            }

            return HandleResult(result);
        }

        /// <summary>
        /// Logout and revoke refresh token
        /// </summary>
        /// <param name="request">Logout information</param>
        /// <returns>Success message</returns>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Logout([FromBody] LogoutRequestDto request)
        {
            // Same dual-mode read as refresh.
            var refreshToken = ReadRefreshToken(request.RefreshToken);

            var command = new LogoutCommand
            {
                RefreshToken = refreshToken,
                IpAddress = GetIpAddress(),
                UserAgent = GetUserAgent(),
                Reason = request.Reason
            };

            var result = await _mediator.Send(command);

            // Always clear the cookie on logout, even if the command failed - the user's
            // intent is clear and a stale cookie left behind is a footgun.
            ClearRefreshTokenCookie();

            return HandleResult(result);
        }

        /// <summary>
        /// Logout from all devices
        /// </summary>
        /// <returns>Success message</returns>
        [HttpPost("logout-all")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> LogoutAll()
        {
            var command = new LogoutAllCommand
            {
                IpAddress = GetIpAddress(),
                UserAgent = GetUserAgent(),
                Reason = "User requested logout from all devices"
            };

            var result = await _mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Logout from specific device
        /// </summary>
        /// <param name="deviceId">Device ID to logout from</param>
        /// <returns>Success message</returns>
        [HttpPost("logout-device/{deviceId}")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> LogoutDevice(string deviceId)
        {
            var command = new LogoutDeviceCommand
            {
                DeviceId = deviceId,
                IpAddress = GetIpAddress(),
                UserAgent = GetUserAgent(),
                Reason = "User requested logout from specific device"
            };

            var result = await _mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Get current user information (with permissions for auth restoration)
        /// </summary>
        /// <returns>Current user details including roles and permissions</returns>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(Application.Features.Users.Dtos.UserDto), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (!int.TryParse(userId, out var userIdInt))
                return Unauthorized();

            var query = new GetCurrentUserQuery { UserId = userIdInt };
            var result = await _mediator.Send(query);
            
            return HandleResult(result);
        }

        /// <summary>
        /// Get user's active sessions
        /// </summary>
        /// <returns>List of active sessions</returns>
        [HttpGet("sessions")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<SessionDto>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetUserSessions()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (!int.TryParse(userId, out var userIdInt))
                return Unauthorized();

            var query = new GetUserSessionsQuery { UserId = userIdInt };
            var result = await _mediator.Send(query);
            
            return HandleResult(result);
        }


        /// <summary>
        /// Revoke a specific session by its ID (current user only)
        /// </summary>
        [HttpPost("sessions/{sessionId}/revoke")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> RevokeSessionById(int sessionId)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var command = new RevokeSessionByIdCommand
            {
                SessionId = sessionId,
                RequestingUserId = userId,
                IpAddress = GetIpAddress(),
                UserAgent = GetUserAgent()
            };

            var result = await _mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Change password
        /// </summary>
        /// <param name="request">Password change request</param>
        /// <returns>Success message</returns>
        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
        {
            var command = new ChangePasswordCommand
            {
                CurrentPassword = request.CurrentPassword,
                NewPassword = request.NewPassword,
                IpAddress = GetIpAddress(),
                UserAgent = GetUserAgent()
            };

            var result = await _mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Forgot password
        /// </summary>
        /// <param name="request">Forgot password request</param>
        /// <returns>Success message</returns>
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        [EnableRateLimiting("sensitive")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            var command = new ForgotPasswordCommand
            {
                Email = request.Email,
                TenantId = request.TenantId,
                IpAddress = GetIpAddress(),
                UserAgent = GetUserAgent()
            };

            var result = await _mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Reset password
        /// </summary>
        /// <param name="request">Reset password request</param>
        /// <returns>Success message</returns>
        [HttpPost("reset-password")]
        [AllowAnonymous]
        [EnableRateLimiting("sensitive")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            var command = new ResetPasswordCommand
            {
                Email = request.Email,
                Token = request.Token,
                NewPassword = request.NewPassword,
                IpAddress = GetIpAddress(),
                UserAgent = GetUserAgent()
            };

            var result = await _mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Verify email address using verification token
        /// </summary>
        [HttpPost("verify-email")]
        [AllowAnonymous]
        [EnableRateLimiting("sensitive")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequestDto request)
        {
            var command = new VerifyEmailCommand
            {
                Email = request.Email,
                Token = request.Token,
                IpAddress = GetIpAddress()
            };

            var result = await _mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Resend email verification link
        /// </summary>
        [HttpPost("resend-verification")]
        [AllowAnonymous]
        [EnableRateLimiting("sensitive")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationRequestDto request)
        {
            var command = new ResendVerificationEmailCommand
            {
                Email = request.Email,
                TenantId = request.TenantId,
                IpAddress = GetIpAddress(),
                UserAgent = GetUserAgent()
            };

            var result = await _mediator.Send(command);
            return HandleResult(result);
        }

        // ??? Refresh-token cookie helpers ?????????????????????????????????????
        // We support both modes simultaneously to allow zero-downtime frontend migration:
        //   - Web clients should rely on the HttpOnly cookie (set by Login/Refresh, sent
        //     automatically by browser, never readable from JS so XSS can't steal it).
        //   - Mobile/CLI clients (no cookie jar) keep sending the token in the body.
        // The body always wins as a fallback so old clients keep working until they switch.

        private const string RefreshCookieName = "refresh_token";

        private void WriteRefreshTokenCookie(string token, DateTime expiresAtUtc)
        {
            // Secure=true requires HTTPS; allow HTTP only in Development.
            var isDev = HttpContext.RequestServices
                .GetRequiredService<IHostEnvironment>()
                .IsDevelopment();

            Response.Cookies.Append(RefreshCookieName, token, new CookieOptions
            {
                HttpOnly = true,                            // JS can't read it - XSS protection
                Secure = !isDev,                            // HTTPS-only in prod
                SameSite = SameSiteMode.Strict,             // CSRF protection
                Path = "/api/auth",                         // scoped to auth endpoints
                Expires = expiresAtUtc                      // matches token lifetime
            });
        }

        private void ClearRefreshTokenCookie()
        {
            // Same options as Append so the browser actually overwrites the existing cookie.
            // Missing options here would make this no-op for some browsers.
            var isDev = HttpContext.RequestServices
                .GetRequiredService<IHostEnvironment>()
                .IsDevelopment();

            Response.Cookies.Append(RefreshCookieName, string.Empty, new CookieOptions
            {
                HttpOnly = true,
                Secure = !isDev,
                SameSite = SameSiteMode.Strict,
                Path = "/api/auth",
                Expires = DateTimeOffset.UnixEpoch          // immediately expired
            });
        }

        private string? ReadRefreshToken(string? bodyValue)
        {
            // Cookie wins for web clients; body is the fallback for clients without a cookie jar.
            if (Request.Cookies.TryGetValue(RefreshCookieName, out var cookieValue)
                && !string.IsNullOrEmpty(cookieValue))
                return cookieValue;

            return bodyValue;
        }
    }
}
