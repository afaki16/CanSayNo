using Kickstart.Application.Interfaces;
using Kickstart.Application.Features.Auth.Dtos;
using Kickstart.Application.Common.Results;
using MediatR;

namespace Kickstart.Application.Features.Auth.Commands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponseDto>>
    {
        private readonly IAuthService _authService;

        public LoginCommandHandler(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task<Result<LoginResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            return await _authService.LoginAsync(request.Email, request.Password, request.IpAddress, request.UserAgent, request.DeviceId, request.DeviceName, request.RememberMe, request.TenantId);
        }
    }
} 
