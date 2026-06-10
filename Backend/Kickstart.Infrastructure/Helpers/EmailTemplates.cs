namespace Kickstart.Infrastructure.Helpers
{
    public static class EmailTemplates
    {
        public static string Wrap(string title, string content) => $@"<!DOCTYPE html>
<html lang=""tr"">
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>{title}</title>
</head>
<body style=""margin:0;padding:0;background:#f4f4f4;font-family:Arial,sans-serif;"">
  <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#f4f4f4;padding:40px 0;"">
    <tr>
      <td align=""center"">
        <table width=""600"" cellpadding=""0"" cellspacing=""0"" style=""background:#ffffff;border-radius:8px;padding:40px;"">
          <tr>
            <td style=""color:#333333;font-size:15px;line-height:1.7;"">
              {content}
            </td>
          </tr>
          <tr>
            <td style=""padding-top:32px;border-top:1px solid #eeeeee;color:#999999;font-size:12px;"">
              Bu e-postayı almak istemiyorsanız dikkate almayın.
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";

        public static string CtaButton(string text, string url) => $@"
<div style=""text-align:center;margin:32px 0;"">
  <a href=""{url}"" style=""background:#4F46E5;color:#ffffff;padding:14px 32px;border-radius:6px;text-decoration:none;font-weight:bold;font-size:15px;display:inline-block;"">{text}</a>
</div>";
    }
}
