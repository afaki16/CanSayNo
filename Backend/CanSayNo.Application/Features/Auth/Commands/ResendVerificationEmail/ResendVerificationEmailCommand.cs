using CanSayNo.Application.Common.Results;
using MediatR;

namespace CanSayNo.Application.Features.Auth.Commands.ResendVerificationEmail
{
    public class ResendVerificationEmailCommand : IRequest<Result>
    {
        public string Email { get; set; } = string.Empty;
        public int TenantId { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
    }
}
