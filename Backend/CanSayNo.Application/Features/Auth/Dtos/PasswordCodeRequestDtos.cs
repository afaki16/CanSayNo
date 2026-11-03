namespace CanSayNo.Application.Features.Auth.Dtos
{
    public class ResetPasswordWithCodeRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public int TenantId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class ChangePasswordWithCodeRequestDto
    {
        public string Code { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
