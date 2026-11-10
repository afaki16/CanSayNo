namespace CanSayNo.Infrastructure.Helpers
{
    /// <summary>
    /// CanSayNo e-posta iskeleti: logo + başlık, içerik, alt bilgi. Tablo tabanlı ve satır içi stilli
    /// (e-posta istemcileri CSS sınıflarını ve SVG'yi desteklemez). Marka geçişi desteklenmeyen
    /// istemcide düz mor renge düşer.
    /// </summary>
    public static class EmailTemplates
    {
        private const string LogoUrl = "https://cansayno.com/assets/apple-touch-icon.png";
        private const string SiteUrl = "https://cansayno.com";
        public const string BrandPurple = "#5B4BE0";
        private const string BrandBlue = "#2D7FF9";

        public static string Wrap(string language, string title, string preheader, string content, string footer) => $@"<!DOCTYPE html>
<html lang=""{language}"">
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>{title}</title>
</head>
<body style=""margin:0;padding:0;background:#F6F5F2;font-family:-apple-system,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;"">
  <div style=""display:none;max-height:0;overflow:hidden;opacity:0;"">{preheader}</div>
  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#F6F5F2;"">
    <tr>
      <td align=""center"" style=""padding:32px 12px;"">
        <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""max-width:560px;"">
          <tr>
            <td style=""padding:0 8px 20px 8px;"">
              <a href=""{SiteUrl}"" style=""text-decoration:none;color:#1A1A2E;font-size:20px;font-weight:700;"">
                <img src=""{LogoUrl}"" width=""36"" height=""36"" alt="""" style=""vertical-align:middle;border-radius:9px;border:0;"">
                <span style=""vertical-align:middle;padding-left:8px;"">CanSayNo</span>
              </a>
            </td>
          </tr>
          <tr>
            <td style=""background:#FFFFFF;border-radius:16px;padding:36px 32px;color:#2B2B3A;font-size:16px;line-height:1.6;"">
              {content}
            </td>
          </tr>
          <tr>
            <td style=""padding:20px 8px 0 8px;color:#8A8A99;font-size:12px;line-height:1.6;"">
              {footer}<br>
              <a href=""{SiteUrl}"" style=""color:#8A8A99;"">cansayno.com</a>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";

        public static string Heading(string text) =>
            $@"<h1 style=""margin:0 0 16px 0;font-size:22px;line-height:1.3;color:#1A1A2E;"">{text}</h1>";

        public static string Paragraph(string html) => $@"<p style=""margin:0 0 14px 0;"">{html}</p>";

        public static string Muted(string html) =>
            $@"<p style=""margin:0 0 10px 0;color:#6B6B7B;font-size:14px;"">{html}</p>";

        public static string CtaButton(string text, string url) => $@"
<table role=""presentation"" cellpadding=""0"" cellspacing=""0"" style=""margin:28px 0;"">
  <tr>
    <td style=""border-radius:12px;background:{BrandPurple};background-image:linear-gradient(135deg,{BrandPurple},{BrandBlue});"">
      <a href=""{url}"" style=""display:inline-block;padding:14px 28px;color:#FFFFFF;font-weight:700;font-size:16px;text-decoration:none;"">{text}</a>
    </td>
  </tr>
</table>";

        public static string Code(string code) => $@"
<div style=""margin:24px 0;padding:18px 0;background:#F1EFFD;border-radius:12px;text-align:center;font-size:34px;font-weight:700;letter-spacing:10px;color:{BrandPurple};font-family:'SF Mono',Consolas,Menlo,monospace;"">{code}</div>";
    }
}
