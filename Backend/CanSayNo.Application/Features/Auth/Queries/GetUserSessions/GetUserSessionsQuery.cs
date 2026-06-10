using CanSayNo.Application.Features.Auth.Dtos;
using CanSayNo.Application.Common.Results;
using MediatR;
using System.Collections.Generic;

namespace CanSayNo.Application.Features.Auth.Queries.GetUserSessions
{
    public class GetUserSessionsQuery : IRequest<Result<IEnumerable<SessionDto>>>
    {
        public int UserId { get; set; }
    }
} 
