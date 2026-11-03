using CanSayNo.Application.Common.Results;
using MediatR;

namespace CanSayNo.Application.Features.Auth.Commands.ConfirmEmailChange
{
    /// <summary>Applies a pending email change from the link sent to the new address (anonymous).</summary>
    public class ConfirmEmailChangeCommand : IRequest<Result>
    {
        /// <summary>The new address the link was sent to.</summary>
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string? IpAddress { get; set; }
    }
}
