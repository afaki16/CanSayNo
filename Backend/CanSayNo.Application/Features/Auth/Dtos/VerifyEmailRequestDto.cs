namespace CanSayNo.Application.Features.Auth.Dtos
{
    public class VerifyEmailRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}

namespace CanSayNo.Application.Features.Auth.Dtos
{
    /// <summary>Body of POST /api/Auth/verify-email/code.</summary>
    public class VerifyEmailWithCodeRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}
