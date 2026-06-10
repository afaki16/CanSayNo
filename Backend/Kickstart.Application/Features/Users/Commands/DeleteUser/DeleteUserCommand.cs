using Kickstart.Application.Common.Results;
using MediatR;
using System;

namespace Kickstart.Application.Features.Users.Commands.DeleteUser
{
    public class DeleteUserCommand : IRequest<Result>
    {
        public int Id { get; set; }
    }
} 
