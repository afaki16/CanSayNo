using Kickstart.Application.Common.Results;
using Kickstart.Application.Interfaces;
using Kickstart.Domain.Common.Enums;
using Kickstart.Domain.Models;
using Kickstart.Infrastructure.Configuration;
using Kickstart.Infrastructure.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace Kickstart.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _smtp;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _smtp = configuration.GetSection("NotificationService:Mail").Get<SmtpSettings>()
                    ?? new SmtpSettings();
            _logger = logger;
        }

        // Shortcut to keep template strings short.
        // EVERY user-supplied value embedded in HTML email content MUST go through this.
        // Without it, a user named "<a href='phishing.com'>click</a>" can turn a system
        // email into a phishing page.
        private static string Esc(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);

        public async Task<Result> SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                using var client = new SmtpClient(_smtp.Host, _smtp.Port)
                {
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_smtp.Username, _smtp.Password),
                    EnableSsl = _smtp.EnableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };

                var message = new MailMessage
                {
                    From = new MailAddress(_smtp.FromAddress, _smtp.FromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                message.To.Add(to);

                await client.SendMailAsync(message);
                _logger.LogInformation("E-posta gonderildi: {To}, Konu: {Subject}", to, subject);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "E-posta gonderilemedi: {To}", to);
                return Result.Failure(Error.Failure(ErrorCode.ValidationFailed, "E-posta gonderilemedi."));
            }
        }

        public async Task<Result> SendEmailConfirmationAsync(string email, string confirmationLink)
        {
            const string subject = "E-posta Adresinizi Dogrulayin";

            var content = $@"
<h2 style=""color:#333;"">E-posta Dogrulama</h2>
<p>Hesabinizi aktif etmek icin asagidaki butona tiklayin.</p>
{EmailTemplates.CtaButton("E-postami Dogrula", Esc(confirmationLink))}
<p style=""color:#666;"">Bu istegi siz yapmadiysaniz dikkate almayin.</p>";

            return await SendEmailAsync(email, subject, EmailTemplates.Wrap(subject, content));
        }

        public async Task<Result> SendPasswordResetAsync(string email, string firstName, string resetLink)
        {
            const string subject = "Sifre Sifirlama";

            var content = $@"
<h2 style=""color:#333;"">Sifre Sifirlama</h2>
<p>Merhaba <strong>{Esc(firstName)}</strong>,</p>
<p>Sifrenizi sifirlamak icin asagidaki butona tiklayin.</p>
{EmailTemplates.CtaButton("Sifremi Sifirla", Esc(resetLink))}
<p style=""color:#666;"">Bu link <strong>30 dakika</strong> gecerlidir.</p>
<p style=""color:#666;"">Bu istegi siz yapmadiysaniz dikkate almayin.</p>";

            return await SendEmailAsync(email, subject, EmailTemplates.Wrap(subject, content));
        }

        public async Task<Result> SendBruteForceAlertAsync(
            string email,
            string firstName,
            string ipAddress,
            int failureCount,
            int lockoutMinutes)
        {
            const string subject = "Supheli Giris Aktivitesi Tespit Edildi";

            var content = $@"
<h2 style=""color:#333;"">Hesabinizda Supheli Aktivite</h2>
<p>Merhaba <strong>{Esc(firstName)}</strong>,</p>
<p>Hesabiniza <strong>{Esc(ipAddress)}</strong> IP adresinden <strong>{failureCount}</strong> basarisiz giris denemesi tespit edildi.</p>
<p>Guvenlik amaciyla hesabiniz <strong>{lockoutMinutes} dakika</strong> sureyle gecici olarak kilitlendi. Bu sure sonunda tekrar giris yapabilirsiniz.</p>

<h3 style=""color:#333;margin-top:24px;"">Bu islemi siz yapmadiysaniz:</h3>
<ul style=""color:#555;"">
  <li>Sifrenizi <strong>derhal degistirin</strong>.</li>
  <li>Hesap guvenliginizi gozden gecirin.</li>
  <li>Iki adimli dogrulama mevcut oldugunda etkinlestirin.</li>
</ul>

<h3 style=""color:#333;margin-top:24px;"">Guvenlik Ipuclari</h3>
<ul style=""color:#555;"">
  <li>Guclu ve benzersiz sifreler kullanin.</li>
  <li>Sifrenizi asla kimseyle paylasmayin.</li>
  <li>Phishing e-postalarina karsi dikkatli olun.</li>
</ul>

<p style=""color:#666;margin-top:24px;"">Guvenlik Ekibi</p>";

            return await SendEmailAsync(email, subject, EmailTemplates.Wrap(subject, content));
        }

        public async Task<Result> SendWelcomeEmailAsync(string email, string userName)
        {
            const string subject = "Hos Geldiniz!";

            var content = $@"
<h2 style=""color:#333;"">Hos Geldiniz, <strong>{Esc(userName)}</strong>!</h2>
<p>Hesabiniz basariyla olusturuldu. Artik platformumuzu kullanabilirsiniz.</p>
<p style=""color:#666;"">Iyi kullanimlar dileriz.</p>";

            return await SendEmailAsync(email, subject, EmailTemplates.Wrap(subject, content));
        }
    }
}