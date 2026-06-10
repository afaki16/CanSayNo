using Microsoft.AspNetCore.Builder;
using Kickstart.API.Middleware;

namespace Kickstart.API.Extensions
{
    public static class SecurityHeadersMiddlewareExtensions
    {
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SecurityHeadersMiddleware>();
        }
    }
}
