using CanSayNo.Application.Features.Users.Dtos;
using CanSayNo.Application.Features.Roles.Dtos;
using CanSayNo.Application.Features.Tenants.Dtos;
using CanSayNo.Application.Features.Permissions.Dtos;
using CanSayNo.Application.Common.Results;
using MediatR;

namespace CanSayNo.Application.Features.Auth.Commands.Register
{
    public class RegisterCommand : IRequest<Result<UserDto>>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
    }
} 
