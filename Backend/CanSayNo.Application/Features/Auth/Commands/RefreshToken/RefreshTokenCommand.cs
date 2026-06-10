using CanSayNo.Application.Features.Auth.Dtos;
using CanSayNo.Application.Common.Results;
using MediatR;

namespace CanSayNo.Application.Features.Auth.Commands.RefreshToken
{
    public class RefreshTokenCommand : IRequest<Result<LoginResponseDto>>
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
    }
} 
