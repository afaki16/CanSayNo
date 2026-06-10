using Kickstart.Application.Common.Results;
using MediatR;

namespace Kickstart.Application.Features.Auth.Commands.ResendVerificationEmail
{
    public class ResendVerificationEmailCommand : IRequest<Result>
    {
        public string Email { get; set; } = string.Empty;
        public int TenantId { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
    }
}
