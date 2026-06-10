namespace Kickstart.Infrastructure.Configuration
{
    public class JwtSettings
    {
        public string SecretKey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int ExpiryInMinutes { get; set; }
        public int RefreshTokenExpiryInDays { get; set; }
        public int RefreshTokenExpiryInDaysRememberMe { get; set; } = 30;
    }
} 
