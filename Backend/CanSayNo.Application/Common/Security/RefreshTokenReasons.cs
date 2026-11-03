namespace CanSayNo.Application.Common.Security
{
    public static class RefreshTokenReasons
    {
        // Set when a refresh token is replaced during rotation. Reuse detection only treats
        // tokens revoked with this reason as a possible theft.
        public const string Rotated = "Token rotated";
    }
}
