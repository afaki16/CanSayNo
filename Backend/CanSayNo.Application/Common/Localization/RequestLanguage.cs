using System.Globalization;
using CanSayNo.Domain.Enums;

namespace CanSayNo.Application.Common.Localization;

/// <summary>
/// İsteğin içerik dili. RequestLocalization middleware'i Accept-Language'a göre (desteklenen: tr, en;
/// varsayılan tr) CurrentUICulture'ı ayarlar — mobil her istekte uygulamanın dilini gönderir.
/// Hata mesajları (IJsonStringLocalizer) da aynı kültürü kullanır.
/// </summary>
public static class RequestLanguage
{
    public static string Current =>
        ContentLanguages.Normalize(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);
}
