using Kickstart.Application.Common.Results;
using MediatR;

namespace Kickstart.Application.Features.Auth.Commands.ChangePassword
{
    public class ChangePasswordCommand : IRequest<Result>
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
    }
} 
