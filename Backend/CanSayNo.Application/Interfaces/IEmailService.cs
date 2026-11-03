using CanSayNo.Application.Common.Results;

namespace CanSayNo.Application.Interfaces
{
    public interface IEmailService
    {
        Task<Result> SendEmailAsync(string to, string subject, string body);
        Task<Result> SendEmailConfirmationAsync(string email, string confirmationLink);
        Task<Result> SendPasswordResetAsync(string email, string firstName, string resetLink);
        Task<Result> SendEmailChangeVerificationAsync(string newEmail, string firstName, string confirmLink, int validHours);
        Task<Result> SendEmailChangedNoticeAsync(string oldEmail, string firstName, string newEmail);
        /// <summary>Sends a short one-time code for resetting or changing the password.</summary>
        Task<Result> SendPasswordCodeAsync(string email, string firstName, string code, int validMinutes);
        Task<Result> SendWelcomeEmailAsync(string email, string userName);
        Task<Result> SendBruteForceAlertAsync(
            string email,
            string firstName,
            string ipAddress,
            int failureCount,
            int lockoutMinutes);
    }
}
