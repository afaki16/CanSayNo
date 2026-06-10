using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Kickstart.API.Middleware
{
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IWebHostEnvironment _env;

        public SecurityHeadersMiddleware(RequestDelegate next, IWebHostEnvironment env)
        {
            _next = next;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var headers = context.Response.Headers;

            // Always-on security headers (all environments)

            // Prevent MIME-type sniffing
            headers["X-Content-Type-Options"] = "nosniff";

            // Prevent embedding in iframes (clickjacking protection)
            headers["X-Frame-Options"] = "DENY";

            // Limit referrer information leakage
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

            // Disable sensitive browser APIs by default
            headers["Permissions-Policy"] =
                "accelerometer=(), camera=(), geolocation=(), gyroscope=(), " +
                "magnetometer=(), microphone=(), payment=(), usb=()";

            // Permissive default CSP — derived projects SHOULD tighten this:
            // remove 'unsafe-inline', adopt nonce/hash-based script-src, restrict connect-src
            // to known API origins, and lock img-src to actual asset hosts.
            headers["Content-Security-Policy"] =
                "default-src 'self'; " +
                "script-src 'self' 'unsafe-inline'; " +
                "style-src 'self' 'unsafe-inline'; " +
                "img-src 'self' data: https:; " +
                "font-src 'self' data:; " +
                "connect-src 'self'; " +
                "frame-ancestors 'none'";

            // Production-only: Strict Transport Security
            // Forces HTTPS for 1 year, includes subdomains
            if (_env.IsProduction())
            {
                headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
            }

            // Remove server fingerprinting headers
            headers.Remove("Server");
            headers.Remove("X-Powered-By");

            await _next(context);
        }
    }
}
