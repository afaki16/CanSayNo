namespace Kickstart.Application.Features.Auth.Dtos
{
    public class ForgotPasswordRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public int TenantId { get; set; }
    }
} 
