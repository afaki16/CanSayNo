using CanSayNo.Application.Features.Users.Dtos;
using CanSayNo.Application.Common.Results;
using MediatR;

namespace CanSayNo.Application.Features.Users.Queries.GetCurrentUser
{
    public class GetCurrentUserQuery : IRequest<Result<UserDto>>
    {
        public int UserId { get; set; }
    }
}
