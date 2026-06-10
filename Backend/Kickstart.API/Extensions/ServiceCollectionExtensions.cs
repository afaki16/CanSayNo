using Kickstart.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Threading.RateLimiting;

namespace Kickstart.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Kickstart API",
                    Version = "v1",
                    Description = "A base authentication API with JWT, role-based permissions, and user management",
                    Contact = new OpenApiContact
                    {
                        Name = "Kickstart Team",
                        Email = "admin@Kickstart.com"
                    }
                });

                // Include XML comments
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }

                // Add JWT Authentication to Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });

            });

            // Add CORS
            services.AddCors(options =>
            {
                options.AddPolicy("DefaultCorsPolicy", policy =>
                {
                    var allowedOrigins = configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>()
                                         ?? Array.Empty<string>();

                    // Defense-in-depth: reject wildcard. Browsers already reject "*" with AllowCredentials,
                    // but we fail loudly here so misconfiguration is caught at startup, not at request time.
                    if (allowedOrigins.Any(o => o == "*"))
                    {
                        throw new InvalidOperationException(
                            "CORS configuration error: Wildcard '*' is not allowed in CorsSettings:AllowedOrigins. " +
                            "Specify explicit origins (e.g., https://app.example.com).");
                    }

                    if (allowedOrigins.Length > 0)
                    {
                        policy.WithOrigins(allowedOrigins)
                              .AllowAnyMethod()
                              .AllowAnyHeader()
                              .AllowCredentials();
                    }
                });
            });

            // Add authorization policies
            services.AddAuthorizationPolicies();

            return services;
        }

        public static IServiceCollection AddRateLimiting(
            this IServiceCollection services,
            IHostEnvironment environment)
        {
            // Development: 10x more permissive so devs aren't blocked while testing.
            // Production/Staging keep the strict limits that protect against abuse.
            var loginLimit = environment.IsDevelopment() ? 50 : 5;
            var sensitiveLimit = environment.IsDevelopment() ? 30 : 3;
            var refreshLimit = environment.IsDevelopment() ? 200 : 20;
            var globalLimit = environment.IsDevelopment() ? 1000 : 100;

            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.OnRejected = async (context, cancellationToken) =>
                {
                    // SlidingWindowRateLimiter with QueueLimit=0 always reports MetadataName.RetryAfter = 0,
                    // because no requests are queued. Fall back to one segment duration (Window / SegmentsPerWindow)
                    // — this is the worst-case wait until the oldest segment expires and a slot frees up.
                    // Our config: Window=60s, SegmentsPerWindow=6 → 10 second fallback.
                    const int FallbackRetryAfterSeconds = 10;

                    var retryAfterSeconds = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
                        && retryAfter.TotalSeconds > 0
                            ? (int)retryAfter.TotalSeconds
                            : FallbackRetryAfterSeconds;

                    context.HttpContext.Response.Headers.RetryAfter = retryAfterSeconds.ToString();

                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILogger<RateLimiterRejectionLog>>();

                    var endpoint = context.HttpContext.Request.Path;
                    var clientIp = GetClientIp(context.HttpContext);
                    var policyName = context.HttpContext.GetEndpoint()?.Metadata
                        .GetMetadata<EnableRateLimitingAttribute>()?.PolicyName ?? "global";

                    logger.LogWarning(
                        "Rate limit exceeded. IP: {ClientIp}, Endpoint: {Endpoint}, " +
                        "Policy: {Policy}, RetryAfter: {RetryAfterSeconds}s",
                        clientIp, endpoint, policyName, retryAfterSeconds);

                    await context.HttpContext.Response.WriteAsync(
                        "Too many requests. Please try again later.", cancellationToken);
                };

                // Login policy: per-IP sliding window
                options.AddPolicy("login", httpContext =>
                    RateLimitPartition.GetSlidingWindowLimiter(
                        partitionKey: GetClientIp(httpContext),
                        factory: _ => new SlidingWindowRateLimiterOptions
                        {
                            PermitLimit = loginLimit,
                            Window = TimeSpan.FromMinutes(1),
                            SegmentsPerWindow = 6,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        }));

                options.AddPolicy("refresh", httpContext =>
                    RateLimitPartition.GetSlidingWindowLimiter(
                        partitionKey: GetClientIp(httpContext),
                        factory: _ => new SlidingWindowRateLimiterOptions
                        {
                            PermitLimit = refreshLimit,
                            Window = TimeSpan.FromMinutes(1),
                            SegmentsPerWindow = 6,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        }));

                // Sensitive policy: per-IP sliding window (forgot/reset password, register)
                options.AddPolicy("sensitive", httpContext =>
                    RateLimitPartition.GetSlidingWindowLimiter(
                        partitionKey: GetClientIp(httpContext),
                        factory: _ => new SlidingWindowRateLimiterOptions
                        {
                            PermitLimit = sensitiveLimit,
                            Window = TimeSpan.FromMinutes(1),
                            SegmentsPerWindow = 6,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        }));

                // Global default fallback applied to every request
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
                    httpContext => RateLimitPartition.GetSlidingWindowLimiter(
                        partitionKey: GetClientIp(httpContext),
                        factory: _ => new SlidingWindowRateLimiterOptions
                        {
                            PermitLimit = globalLimit,
                            Window = TimeSpan.FromMinutes(1),
                            SegmentsPerWindow = 6,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        }));
            });

            return services;
        }

        // Real client IP — relies on UseForwardedHeaders running first.
        private static string GetClientIp(HttpContext context)
        {
            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        // Marker type used purely as the logger category name for rate-limit rejections.
        private class RateLimiterRejectionLog { }

        private static void AddAuthorizationPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                // Permission policies are handled dynamically by PermissionPolicyProvider
                // (registered in Infrastructure.DependencyInjection) which evaluates them
                // against the database via PermissionAuthorizationHandler.

                // Role-based policies
                AddRoleBasedPolicies(options);

                // Custom policies
                AddCustomPolicies(options);
            });
        }

        
        private static void AddRoleBasedPolicies(AuthorizationOptions options)
        {
            // Admin role requirement
            options.AddPolicy("RequireAdminRole", policy =>
                policy.RequireRole(RoleNames.Admin, RoleNames.SuperAdmin));
            
            options.AddPolicy("RequireSuperAdminRole", policy =>
                policy.RequireRole(RoleNames.SuperAdmin));
            
            options.AddPolicy("RequireManagerRole", policy =>
                policy.RequireRole(RoleNames.Manager, RoleNames.Admin, RoleNames.SuperAdmin));
            
            options.AddPolicy("RequireUserRole", policy =>
                policy.RequireRole(RoleNames.User, RoleNames.Manager, RoleNames.Admin, RoleNames.SuperAdmin));
        }

        private static void AddCustomPolicies(AuthorizationOptions options)
        {
            // Custom business logic policies
            options.AddPolicy("RequireActiveUser", policy =>
                policy.RequireAssertion(context =>
                {
                    var user = context.User;
                    var isActive = user.HasClaim(c => c.Type == "status" && c.Value == "Active");
                    return isActive;
                }));

            options.AddPolicy("RequireEmailVerified", policy =>
                policy.RequireAssertion(context =>
                {
                    var user = context.User;
                    var emailVerified = user.HasClaim(c => c.Type == "email_verified" && c.Value == "true");
                    return emailVerified;
                }));

            // Time-based policies
            options.AddPolicy("RequireBusinessHours", policy =>
                policy.RequireAssertion(context =>
                {
                    var currentHour = DateTime.UtcNow.Hour;
                    return currentHour >= 9 && currentHour <= 17; // 9 AM - 5 PM UTC
                }));
        }
    }
} 
