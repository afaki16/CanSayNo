using CanSayNo.Application.Common.Results;
using MediatR;
using System;

namespace CanSayNo.Application.Features.Users.Commands.DeleteUser
{
    public class DeleteUserCommand : IRequest<Result>
    {
        public int Id { get; set; }
    }
} 
