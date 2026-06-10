namespace Kickstart.Application.Features.Auth.Dtos
{
    public class ResendVerificationRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public int TenantId { get; set; }
    }
}
