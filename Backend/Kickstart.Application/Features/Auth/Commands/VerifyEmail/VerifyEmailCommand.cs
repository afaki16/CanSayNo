using Kickstart.Application.Common.Results;
using MediatR;

namespace Kickstart.Application.Features.Auth.Commands.VerifyEmail
{
    public class VerifyEmailCommand : IRequest<Result>
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
    }
}
