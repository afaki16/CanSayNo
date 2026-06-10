namespace Kickstart.Application.Features.Auth.Dtos
{
    public class LoginRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
        /// <summary>Required when the same email exists in more than one tenant.</summary>
        public int? TenantId { get; set; }
        public string? DeviceId { get; set; }
        public string? DeviceName { get; set; }
    }
}
