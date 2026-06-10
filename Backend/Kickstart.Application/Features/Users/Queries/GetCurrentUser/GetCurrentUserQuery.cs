using Kickstart.Application.Features.Users.Dtos;
using Kickstart.Application.Common.Results;
using MediatR;

namespace Kickstart.Application.Features.Users.Queries.GetCurrentUser
{
    public class GetCurrentUserQuery : IRequest<Result<UserDto>>
    {
        public int UserId { get; set; }
    }
}
