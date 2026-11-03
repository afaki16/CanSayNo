namespace CanSayNo.Infrastructure.Configuration;

/// <summary>
/// appsettings "Anthropic" bölümü. ApiKey dosyaya YAZILMAZ: geliştirmede user-secrets
/// (<c>dotnet user-secrets set "Anthropic:ApiKey" "..."</c>), sunucuda
/// <c>Anthropic__ApiKey</c> ortam değişkeni. Anahtar yoksa uygulama açılır ama
/// karakter cevapları 503 aiUnavailable döner.
/// </summary>
public class AnthropicSettings
{
    public const string SectionName = "Anthropic";

    public string ApiKey { get; set; } = string.Empty;

    public string Model { get; set; } = "claude-sonnet-5-5";

    /// <summary>
    /// Cevap başına tavan. Düşünme token'ları da buna sayılır; karakter cevabı kısa olsa da
    /// ara sıra düşünmeye pay bırakmak için 1024.
    /// </summary>
    public int MaxTokens { get; set; } = 1024;

    /// <summary>low | medium | high. Sohbet için low: basit mesajlarda düşünmeyi çoğunlukla atlar.</summary>
    public string Effort { get; set; } = "low";

    /// <summary>
    /// Değerlendirme kaliteye daha duyarlı: medium. Düşünme + JSON çıktısına yer kalsın diye 4096.
    /// </summary>
    public string EvaluationEffort { get; set; } = "medium";

    public int EvaluationMaxTokens { get; set; } = 4096;
}
