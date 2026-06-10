using Kickstart.Application.Features.Auth.Dtos;
using Kickstart.Application.Features.Auth.Models;
using Kickstart.Application.Common.Results;
using Kickstart.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Kickstart.Application.Interfaces
{
    public interface IAuthService
    {
        Task<Result<LoginResponseDto>> LoginAsync(string email, string password, string ipAddress, string userAgent, string deviceId = null, string deviceName = null, bool rememberMe = false, int? tenantId = null);
        Task<Result<string>> GenerateAccessTokenAsync(User user);
        Task<Result<RefreshTokenIssueResult>> GenerateRefreshTokenAsync(User user, string ipAddress, string userAgent, string deviceId = null, string deviceName = null, bool rememberMe = false, DateTime? preserveExpiryDate = null);
        Task<Result<LoginResponseDto>> RefreshTokenAsync(string accessToken, string refreshToken, string ipAddress, string userAgent);
        Task<Result> RevokeTokenAsync(string refreshToken, string ipAddress = null, string userAgent = null, string reason = null);
        Task<Result> RevokeAllUserTokensAsync(int userId, string ipAddress = null, string userAgent = null, string reason = null);
        Task<Result> RevokeTokensByDeviceAsync(int userId, string deviceId, string ipAddress = null, string userAgent = null, string reason = null);
        ClaimsPrincipal GetClaimsFromExpiredToken(string token);
        Task<bool> ValidateTokenAsync(string token);
    }
} 
