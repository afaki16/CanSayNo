using CanSayNo.Application.Common.Results;
using MediatR;

namespace CanSayNo.Application.Features.Auth.Commands.RevokeSessionById
{
    public class RevokeSessionByIdCommand : IRequest<Result>
    {
        public int SessionId { get; set; }
        public int RequestingUserId { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
    }
}
