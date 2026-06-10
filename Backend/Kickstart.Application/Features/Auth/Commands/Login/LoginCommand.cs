using Kickstart.Application.Features.Auth.Dtos;
using Kickstart.Application.Common.Results;
using MediatR;

namespace Kickstart.Application.Features.Auth.Commands.Login
{
    public class LoginCommand : IRequest<Result<LoginResponseDto>>
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
        /// <summary>Required when the same email exists in more than one tenant.</summary>
        public int? TenantId { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
    }
} 
