using Kickstart.Application.Features.Auth.Dtos;
using Kickstart.Application.Common.Results;
using MediatR;
using System.Collections.Generic;

namespace Kickstart.Application.Features.Auth.Queries.GetUserSessions
{
    public class GetUserSessionsQuery : IRequest<Result<IEnumerable<SessionDto>>>
    {
        public int UserId { get; set; }
    }
} 
