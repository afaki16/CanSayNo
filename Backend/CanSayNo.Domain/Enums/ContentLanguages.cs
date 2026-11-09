namespace CanSayNo.Domain.Enums;

/// <summary>
/// Hayırcı içerik dilleri (senaryo + yapay zekâ konuşması). İki harfli ISO 639-1 kodu.
/// İstek dili Accept-Language'dan gelir (Program.cs RequestLocalization: tr, en; varsayılan tr).
/// </summary>
public static class ContentLanguages
{
    public const string Turkish = "tr";
    public const string English = "en";

    public static readonly IReadOnlyList<string> All = [Turkish, English];

    /// <summary>Bilinmeyen/boş değer Türkçe sayılır.</summary>
    public static string Normalize(string? language) =>
        string.Equals(language, English, StringComparison.OrdinalIgnoreCase) ? English : Turkish;

    public static bool IsSupported(string? language) =>
        language is not null && All.Contains(language);
}
