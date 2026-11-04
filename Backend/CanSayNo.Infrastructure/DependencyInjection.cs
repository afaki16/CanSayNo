using CanSayNo.Application.Interfaces;
using CanSayNo.Application.Common.Localization;
using CanSayNo.Application.Common.Settings;
using CanSayNo.Infrastructure.Localization;
using CanSayNo.Domain.Common.Interfaces;
using CanSayNo.Domain.Common.Interfaces.Repositories;
using CanSayNo.Infrastructure.Persistence;
using CanSayNo.Infrastructure.Identity;
using CanSayNo.Infrastructure.Repositories;
using CanSayNo.Infrastructure.Services;
using CanSayNo.Infrastructure.Services.Ai;
using CanSayNo.Infrastructure.Configuration;
using CanSayNo.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Text;

namespace CanSayNo.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
        {
        // Add DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Add Repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddRepositories(typeof(DependencyInjection).Assembly);


        // Hayırcı ürün ayarları (günlük limit, abonelik AppId). Bölüm yoksa default'lar kullanılır.
        services.AddSingleton(
            configuration.GetSection(HayirciSettings.SectionName).Get<HayirciSettings>() ?? new HayirciSettings());

        services.AddSingleton(
            configuration.GetSection(RevenueCatSettings.SectionName).Get<RevenueCatSettings>() ?? new RevenueCatSettings());

        // Karakter cevapları (LLM). Anahtar yoksa servis kayıtlı kalır, cevaplar aiUnavailable döner.
        services.AddSingleton(
            configuration.GetSection(AnthropicSettings.SectionName).Get<AnthropicSettings>() ?? new AnthropicSettings());
        services.AddSingleton<AnthropicClientProvider>();
        services.AddSingleton<ICharacterChatService, AnthropicCharacterChatService>();
        services.AddSingleton<ISessionEvaluator, AnthropicSessionEvaluator>();

        // Add Memory Cache
        services.AddMemoryCache();

        // Localization — registered before JWT setup because the JWT challenge/forbidden
        // events resolve IJsonStringLocalizer to return localized error messages.
        services.AddSingleton<IJsonStringLocalizer, JsonStringLocalizer>();

        // Add Services
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IAuthService, JwtService>();
        services.AddScoped<IRevokedSessionService, RevokedSessionService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<IBruteForceProtectionService, BruteForceProtectionService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

        // Add JWT Authentication
        AddJwtAuthentication(services, configuration, environment);

            return services;
    }

    public static void AddRepositories(this IServiceCollection services, Assembly assembly)
    {
        var repositoryTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces().Any(i => i.Name.EndsWith("Repository")));

        foreach (var type in repositoryTypes)
        {
            var interfaceType = type.GetInterfaces().First(i => i.Name.EndsWith("Repository"));
            services.TryAddScoped(interfaceType, type);
        }
    }

        private static void AddJwtAuthentication(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
        {
            var jwtSettings = configuration.GetSection("JwtSettings");
            var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = !environment.IsDevelopment();
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.FromSeconds(30) // Tolerate small clock drift between server instances
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var userIdClaim = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                        var iatClaim = context.Principal?.FindFirst("iat")?.Value
                            ?? context.Principal?.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Iat)?.Value;

                        if (string.IsNullOrEmpty(userIdClaim) || string.IsNullOrEmpty(iatClaim)
                            || !int.TryParse(userIdClaim, out var userId)
                            || !long.TryParse(iatClaim, out var iatUnix))
                        {
                            return;
                        }

                        var tokenIssuedAt = DateTimeOffset.FromUnixTimeSeconds(iatUnix).UtcDateTime;
                        var revokedSessionService = context.HttpContext.RequestServices.GetService<IRevokedSessionService>();
                        if (revokedSessionService != null && await revokedSessionService.WasSessionRevokedAfterAsync(userId, tokenIssuedAt))
                        {
                            context.Fail("Session has been revoked");
                        }
                    },
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";

                        var localizer = context.HttpContext.RequestServices
                            .GetRequiredService<IJsonStringLocalizer>();

                        var result = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            success = false,
                            error = new
                            {
                                status = 401,
                                code = 1012, // ErrorCode.Unauthorized
                                errors = new Dictionary<string, string[]>
                                {
                                    { "error", new[] { localizer["auth.unauthorized"] } }
                                }
                            }
                        });
                        return context.Response.WriteAsync(result);
                    },
                    OnForbidden = context =>
                    {
                        context.Response.StatusCode = 403;
                        context.Response.ContentType = "application/json";

                        var localizer = context.HttpContext.RequestServices
                            .GetRequiredService<IJsonStringLocalizer>();

                        var result = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            success = false,
                            error = new
                            {
                                status = 403,
                                code = 1011, // ErrorCode.Forbidden
                                errors = new Dictionary<string, string[]>
                                {
                                    { "error", new[] { localizer["auth.forbidden"] } }
                                }
                            }
                        });
                        return context.Response.WriteAsync(result);
                    }
                };
            });

            services.AddAuthorization();
        }
    }
} 
