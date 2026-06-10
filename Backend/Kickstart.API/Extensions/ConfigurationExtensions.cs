namespace Kickstart.API.Extensions;

public static class ConfigurationExtensions
{
    public static void ValidateRequiredSettings(this IConfiguration config, IHostEnvironment env)
    {
        var requiredSettings = new (string Key, int MinLength)[]
        {
            ("JwtSettings:SecretKey", 32),
            ("JwtSettings:Issuer", 0),
            ("JwtSettings:Audience", 0),
            ("ConnectionStrings:DefaultConnection", 0),
        };

        var errors = new List<string>();

        foreach (var (key, minLength) in requiredSettings)
        {
            var value = config[key];
            var isInvalid = string.IsNullOrWhiteSpace(value) || (minLength > 0 && value.Length < minLength);

            if (!isInvalid) continue;

            if (env.IsDevelopment())
            {
                errors.Add($"[{key}]: appsettings.Development.json eksik veya doğru ayarlanmamış. README.md'deki Setup bölümünü kontrol edin.");
            }
            else
            {
                var envVarName = key.Replace(":", "__");
                errors.Add($"Required environment variable missing: {envVarName}. Set it in /opt/services/.env file.");
            }
        }

        if (errors.Count > 0)
            throw new InvalidOperationException(string.Join(Environment.NewLine, errors));
    }

    public static void ValidateCorsSettings(this IConfiguration config, IHostEnvironment env)
    {
        var origins = config.GetSection("CorsSettings:AllowedOrigins").Get<string[]>()
                      ?? Array.Empty<string>();

        if (origins.Length == 0 && !env.IsDevelopment())
        {
            throw new InvalidOperationException(
                "CORS configuration missing: CorsSettings:AllowedOrigins is empty. " +
                $"You must configure allowed origins in appsettings.{env.EnvironmentName}.json " +
                "or via environment variable CorsSettings__AllowedOrigins__0");
        }

        if (origins.Length == 0 && env.IsDevelopment())
        {
            // Logger is not yet built at this point; fall back to Console.
            Console.WriteLine("[WARN] CORS AllowedOrigins is empty. " +
                "API will not respond to cross-origin requests. " +
                "Add origins to appsettings.Development.json if needed.");
        }
    }
}
