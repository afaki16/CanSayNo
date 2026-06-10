namespace Kickstart.API.Configuration
{
    public class RateLimitSettings
    {
        public int LoginPermitLimit { get; set; } = 5;
        public int SensitivePermitLimit { get; set; } = 3;
        public int GlobalPermitLimit { get; set; } = 100;
        public int WindowMinutes { get; set; } = 1;
    }
}
