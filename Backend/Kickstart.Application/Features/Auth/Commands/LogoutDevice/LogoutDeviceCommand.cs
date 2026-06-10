using Kickstart.Application.Common.Results;
using MediatR;

namespace Kickstart.Application.Features.Auth.Commands.LogoutDevice
{
    public class LogoutDeviceCommand : IRequest<Result>
    {
        public string DeviceId { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string Reason { get; set; }
    }
} 
