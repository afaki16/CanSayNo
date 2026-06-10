using CanSayNo.Application.Common.Results;
using MediatR;

namespace CanSayNo.Application.Features.Admin.Commands.RevokeUserSessions
{
    public class RevokeUserSessionsCommand : IRequest<Result>
    {
        public int UserId { get; set; }
        public string? Reason { get; set; }
    }
}
