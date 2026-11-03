using CanSayNo.Application.Common.Results;
using MediatR;

namespace CanSayNo.Application.Features.Auth.Commands.RequestEmailChange
{
    /// <summary>
    /// The signed-in user asks to move their account to a new email. Nothing changes until the link
    /// sent to the new address is confirmed (see ConfirmEmailChangeCommand).
    /// </summary>
    public class RequestEmailChangeCommand : IRequest<Result>
    {
        public string NewEmail { get; set; } = string.Empty;
        public string CurrentPassword { get; set; } = string.Empty;
        public string? IpAddress { get; set; }
    }
}
