namespace Kickstart.Application.Common.Security
{
    public static class SecurityConstants
    {
        // Pre-computed BCrypt hash for timing-attack protection.
        // Used by login flow when the user does not exist (or hash is missing/invalid)
        // so that BCrypt.Verify time is constant regardless of user existence.
        // SAFE to commit — this is a known dummy, not any real user's hash.
        // Hash of: "dummy_password_for_timing_protection" with cost factor 12.
        public const string DummyBCryptHash =
            "$2a$12$sd7Fk21uxekyjSgbU3XSle4m4NAEIlpC4ghUQ56tZ/tmQy2rpG9dS";
    }
}
