using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using CanSayNo.Application.Common.Results;
using CanSayNo.Application.Interfaces;
using CanSayNo.Domain.Common.Enums;
using CanSayNo.Domain.Enums;
using CanSayNo.Domain.Models;
using CanSayNo.Infrastructure.Configuration;
using CanSayNo.Infrastructure.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CanSayNo.Infrastructure.Services
{
    /// <summary>
    /// Sends account emails over SMTP. Every email exists in Turkish and English; the language is the
    /// current request's UI culture (mobile sends the app language as Accept-Language), Turkish otherwise.
    /// </summary>
    public partial class EmailService : IEmailService
    {
        private const string SupportEmail = "info@factech.app";

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

        private static bool English =>
            ContentLanguages.Normalize(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName) == ContentLanguages.English;

        private static string L(string tr, string en) => English ? en : tr;

        private static string Hello(string? firstName) => string.IsNullOrWhiteSpace(firstName)
            ? L("Merhaba,", "Hi,")
            : L($"Merhaba <strong>{Esc(firstName)}</strong>,", $"Hi <strong>{Esc(firstName)}</strong>,");

        private Task<Result> SendTemplateAsync(string to, string subject, string preheader, string content) =>
            SendEmailAsync(to, subject, EmailTemplates.Wrap(
                English ? ContentLanguages.English : ContentLanguages.Turkish,
                Esc(subject),
                preheader,
                content,
                L("Bu e-postayı CanSayNo hesabınla ilgili bir işlem nedeniyle aldın.",
                  "You received this email because of an action on your CanSayNo account.")));

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

                using var message = new MailMessage
                {
                    From = new MailAddress(_smtp.FromAddress, _smtp.FromName, Encoding.UTF8),
                    Subject = subject,
                    SubjectEncoding = Encoding.UTF8,
                    HeadersEncoding = Encoding.UTF8,
                    BodyEncoding = Encoding.UTF8
                };
                message.To.Add(to);
                // Plain-text part first, HTML last (clients show the last part they support).
                // A text alternative also keeps spam filters happier than HTML alone.
                message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(
                    ToPlainText(body), Encoding.UTF8, MediaTypeNames.Text.Plain));
                message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(
                    body, Encoding.UTF8, MediaTypeNames.Text.Html));

                await client.SendMailAsync(message);
                _logger.LogInformation("Email sent: {To}, Subject: {Subject}", to, subject);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email could not be sent: {To}", to);
                return Result.Failure(Error.Failure(ErrorCode.ValidationFailed, "Email could not be sent."));
            }
        }

        public Task<Result> SendEmailConfirmationAsync(string email, string confirmationLink)
        {
            var subject = L("E-posta adresini doğrula", "Confirm your email address");
            var content =
                EmailTemplates.Heading(subject) +
                EmailTemplates.Paragraph(L(
                    "CanSayNo hesabını etkinleştirmek için aşağıdaki butona dokun.",
                    "Tap the button below to activate your CanSayNo account.")) +
                EmailTemplates.CtaButton(L("E-postamı doğrula", "Confirm my email"), Esc(confirmationLink)) +
                EmailTemplates.Muted(L("Bu bağlantı <strong>2 saat</strong> geçerlidir.", "This link is valid for <strong>2 hours</strong>.")) +
                EmailTemplates.Muted(L(
                    "Hesabı sen oluşturmadıysan bu e-postayı yok sayabilirsin.",
                    "If you didn't create an account, you can ignore this email."));

            return SendTemplateAsync(email, subject, L("Hesabını etkinleştir", "Activate your account"), content);
        }

        public Task<Result> SendRegistrationCodeAsync(string email, string firstName, string code, int validMinutes)
        {
            var subject = L($"Doğrulama kodun: {code}", $"Your verification code: {code}");
            var content =
                EmailTemplates.Heading(L("Hesabını doğrula", "Verify your account")) +
                EmailTemplates.Paragraph(Hello(firstName)) +
                EmailTemplates.Paragraph(L(
                    "CanSayNo'ya hoş geldin! Kaydını tamamlamak için bu kodu uygulamaya gir:",
                    "Welcome to CanSayNo! Enter this code in the app to finish signing up:")) +
                EmailTemplates.Code(Esc(code)) +
                EmailTemplates.Muted(L(
                    $"Kod <strong>{validMinutes} dakika</strong> geçerlidir ve yalnızca bir kez kullanılabilir.",
                    $"The code is valid for <strong>{validMinutes} minutes</strong> and can be used once.")) +
                EmailTemplates.Muted(L(
                    "Bu kaydı sen yapmadıysan bu e-postayı yok sayabilirsin; hesap doğrulanmadan kullanılamaz.",
                    "If you didn't sign up, you can ignore this email; the account can't be used until it is verified."));

            return SendTemplateAsync(email, subject, L("Kaydını tamamlamak için kodun", "Your code to finish signing up"), content);
        }

        public Task<Result> SendPasswordResetAsync(string email, string firstName, string resetLink)
        {
            var subject = L("Şifre sıfırlama", "Reset your password");
            var content =
                EmailTemplates.Heading(subject) +
                EmailTemplates.Paragraph(Hello(firstName)) +
                EmailTemplates.Paragraph(L(
                    "Şifreni sıfırlamak için aşağıdaki butona dokun.",
                    "Tap the button below to choose a new password.")) +
                EmailTemplates.CtaButton(L("Şifremi sıfırla", "Reset password"), Esc(resetLink)) +
                EmailTemplates.Muted(L("Bu bağlantı <strong>30 dakika</strong> geçerlidir.", "This link is valid for <strong>30 minutes</strong>.")) +
                EmailTemplates.Muted(L(
                    "Bu isteği sen yapmadıysan bu e-postayı yok sayabilirsin; şifren değişmez.",
                    "If you didn't ask for this, you can ignore this email; your password won't change."));

            return SendTemplateAsync(email, subject, subject, content);
        }

        public Task<Result> SendEmailChangeVerificationAsync(string newEmail, string firstName, string confirmLink, int validHours)
        {
            var subject = L("Yeni e-posta adresini onayla", "Confirm your new email address");
            var content =
                EmailTemplates.Heading(subject) +
                EmailTemplates.Paragraph(Hello(firstName)) +
                EmailTemplates.Paragraph(L(
                    "Hesabının e-posta adresini bu adresle değiştirmek istedin. Onaylamak için aşağıdaki butona dokun.",
                    "You asked to change your account email to this address. Tap the button below to confirm.")) +
                EmailTemplates.CtaButton(L("E-postamı onayla", "Confirm email"), Esc(confirmLink)) +
                EmailTemplates.Muted(L(
                    $"Bu bağlantı <strong>{validHours} saat</strong> geçerlidir. Onaylayana kadar eski adresinle giriş yapmaya devam edersin.",
                    $"This link is valid for <strong>{validHours} hours</strong>. Until you confirm, you keep signing in with your old address.")) +
                EmailTemplates.Muted(L(
                    "Bu isteği sen yapmadıysan bu e-postayı yok sayabilirsin; e-posta adresin değişmez.",
                    "If you didn't ask for this, you can ignore this email; your email address won't change."));

            return SendTemplateAsync(newEmail, subject, subject, content);
        }

        public Task<Result> SendEmailChangedNoticeAsync(string oldEmail, string firstName, string newEmail)
        {
            var subject = L("E-posta adresin değiştirildi", "Your email address was changed");
            var content =
                EmailTemplates.Heading(subject) +
                EmailTemplates.Paragraph(Hello(firstName)) +
                EmailTemplates.Paragraph(L(
                    $"CanSayNo hesabının e-posta adresi <strong>{Esc(newEmail)}</strong> olarak değiştirildi. Bundan sonra bu adresle giriş yapacaksın.",
                    $"The email address of your CanSayNo account was changed to <strong>{Esc(newEmail)}</strong>. From now on, sign in with that address.")) +
                EmailTemplates.Muted(L(
                    $"Bu değişikliği sen yapmadıysan hesabın ele geçirilmiş olabilir; hemen <a href=\"mailto:{SupportEmail}\">{SupportEmail}</a> adresine yaz.",
                    $"If you didn't make this change, your account may be compromised; contact <a href=\"mailto:{SupportEmail}\">{SupportEmail}</a> right away."));

            return SendTemplateAsync(oldEmail, subject, subject, content);
        }

        public Task<Result> SendPasswordCodeAsync(string email, string firstName, string code, int validMinutes)
        {
            var subject = L($"Şifre kodun: {code}", $"Your password code: {code}");
            var content =
                EmailTemplates.Heading(L("Şifre doğrulama kodu", "Password verification code")) +
                EmailTemplates.Paragraph(Hello(firstName)) +
                EmailTemplates.Paragraph(L(
                    "Yeni şifreni belirlemek için bu kodu uygulamaya gir:",
                    "Enter this code in the app to set your new password:")) +
                EmailTemplates.Code(Esc(code)) +
                EmailTemplates.Muted(L(
                    $"Kod <strong>{validMinutes} dakika</strong> geçerlidir ve yalnızca bir kez kullanılabilir.",
                    $"The code is valid for <strong>{validMinutes} minutes</strong> and can be used once.")) +
                EmailTemplates.Muted(L(
                    "Bu isteği sen yapmadıysan bu e-postayı yok say ve kodu kimseyle paylaşma.",
                    "If you didn't ask for this, ignore this email and don't share the code with anyone."));

            return SendTemplateAsync(email, subject, L("Şifren için doğrulama kodu", "Verification code for your password"), content);
        }

        public Task<Result> SendEmailChangeCodeAsync(string newEmail, string firstName, string code, int validMinutes)
        {
            var subject = L($"E-posta değişikliği kodun: {code}", $"Your email change code: {code}");
            var content =
                EmailTemplates.Heading(L("E-posta değişikliği", "Email change")) +
                EmailTemplates.Paragraph(Hello(firstName)) +
                EmailTemplates.Paragraph(L(
                    "Hesabının e-posta adresini bu adresle değiştirmek için bu kodu uygulamaya gir:",
                    "Enter this code in the app to change your account email to this address:")) +
                EmailTemplates.Code(Esc(code)) +
                EmailTemplates.Muted(L(
                    $"Kod <strong>{validMinutes} dakika</strong> geçerlidir ve yalnızca bir kez kullanılabilir.",
                    $"The code is valid for <strong>{validMinutes} minutes</strong> and can be used once.")) +
                EmailTemplates.Muted(L(
                    "Bu isteği sen yapmadıysan bu e-postayı yok sayabilirsin; e-posta adresi değişmez.",
                    "If you didn't ask for this, you can ignore this email; the email address won't change."));

            return SendTemplateAsync(newEmail, subject, L("E-posta değişikliği için kodun", "Your code for the email change"), content);
        }

        public Task<Result> SendBruteForceAlertAsync(
            string email,
            string firstName,
            string ipAddress,
            int failureCount,
            int lockoutMinutes)
        {
            var subject = L("Hesabında şüpheli giriş denemeleri", "Suspicious sign-in attempts on your account");
            var content =
                EmailTemplates.Heading(subject) +
                EmailTemplates.Paragraph(Hello(firstName)) +
                EmailTemplates.Paragraph(L(
                    $"Hesabına <strong>{Esc(ipAddress)}</strong> IP adresinden <strong>{failureCount}</strong> başarısız giriş denemesi yapıldı. Güvenliğin için girişler <strong>{lockoutMinutes} dakika</strong> durduruldu.",
                    $"There were <strong>{failureCount}</strong> failed sign-in attempts on your account from IP address <strong>{Esc(ipAddress)}</strong>. To keep you safe, sign-ins are paused for <strong>{lockoutMinutes} minutes</strong>.")) +
                EmailTemplates.Paragraph(L(
                    "Bu denemeler sana ait değilse uygulamada <strong>şifreni değiştir</strong> ve başka yerde kullandığın bir şifreyse onu da değiştir.",
                    "If this wasn't you, <strong>change your password</strong> in the app, and change it anywhere else you use the same password.")) +
                EmailTemplates.Muted(L(
                    $"Sorun yaşarsan <a href=\"mailto:{SupportEmail}\">{SupportEmail}</a> adresine yazabilirsin.",
                    $"If you need help, write to <a href=\"mailto:{SupportEmail}\">{SupportEmail}</a>."));

            return SendTemplateAsync(email, subject, subject, content);
        }

        public Task<Result> SendWelcomeEmailAsync(string email, string userName)
        {
            var subject = L("CanSayNo'ya hoş geldin!", "Welcome to CanSayNo!");
            var content =
                EmailTemplates.Heading(subject) +
                EmailTemplates.Paragraph(Hello(userName)) +
                EmailTemplates.Paragraph(L(
                    "Hesabın hazır. Güvenli senaryolarda hayır demeyi pratik etmeye başlayabilirsin.",
                    "Your account is ready. You can start practising saying no in safe scenarios."));

            return SendTemplateAsync(email, subject, subject, content);
        }

        /// <summary>Rough text version of the HTML body for the text/plain part.</summary>
        internal static string ToPlainText(string html)
        {
            var body = BodyRegex().Match(html) is { Success: true } m ? m.Groups[1].Value : html;
            body = HiddenRegex().Replace(body, string.Empty);
            body = LinkRegex().Replace(body, m =>
            {
                var url = m.Groups[1].Value;
                var text = WhitespaceRegex().Replace(TagRegex().Replace(m.Groups[2].Value, string.Empty), " ").Trim();
                if (url.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
                    return text;
                return text.Length == 0 || text == url ? url : $"{text} ({url})";
            });
            body = BlockEndRegex().Replace(body, "\n");
            body = TagRegex().Replace(body, string.Empty);
            body = WebUtility.HtmlDecode(body);
            var lines = body.Split('\n').Select(l => l.Trim()).Where(l => l.Length > 0);
            return string.Join("\n\n", lines);
        }

        [GeneratedRegex(@"<body[^>]*>(.*)</body>", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
        private static partial Regex BodyRegex();

        [GeneratedRegex(@"<div style=""display:none[^""]*"">.*?</div>", RegexOptions.Singleline)]
        private static partial Regex HiddenRegex();

        [GeneratedRegex(@"<a [^>]*href=""([^""]+)""[^>]*>(.*?)</a>", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
        private static partial Regex LinkRegex();

        [GeneratedRegex(@"<br\s*/?>|</(p|h1|div|td|tr)>", RegexOptions.IgnoreCase)]
        private static partial Regex BlockEndRegex();

        [GeneratedRegex(@"<[^>]+>")]
        private static partial Regex TagRegex();

        [GeneratedRegex(@"\s+")]
        private static partial Regex WhitespaceRegex();
    }
}
