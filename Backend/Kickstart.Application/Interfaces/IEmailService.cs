using Kickstart.Application.Common.Results;

namespace Kickstart.Application.Interfaces
{
    public interface IEmailService
    {
        Task<Result> SendEmailAsync(string to, string subject, string body);
        Task<Result> SendEmailConfirmationAsync(string email, string confirmationLink);
        Task<Result> SendPasswordResetAsync(string email, string firstName, string resetLink);
        Task<Result> SendWelcomeEmailAsync(string email, string userName);
        Task<Result> SendBruteForceAlertAsync(
            string email,
            string firstName,
            string ipAddress,
            int failureCount,
            int lockoutMinutes);
    }
}
