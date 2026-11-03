namespace CanSayNo.Domain.Common.Enums;

public enum VerificationPurpose
{
    Registration = 1,
    EmailChange = 2,
    PhoneChange = 3,
    /// <summary>OTP code for "forgot password" (signed-out user, e.g. mobile apps).</summary>
    PasswordReset = 4,
    /// <summary>OTP code for changing the password when signed in but the current password is forgotten.</summary>
    PasswordChange = 5
}
