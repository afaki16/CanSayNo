using Microsoft.AspNetCore.Builder;
using CanSayNo.API.Middleware;

namespace CanSayNo.API.Extensions
{
    public static class SecurityHeadersMiddlewareExtensions
    {
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SecurityHeadersMiddleware>();
        }
    }
}
