namespace CanSayNo.Application.Common.Settings;

/// <summary>
/// appsettings "RevenueCat" bölümü. WebhookAuthorization dosyaya YAZILMAZ: geliştirmede
/// user-secrets (<c>RevenueCat:WebhookAuthorization</c>), sunucuda
/// <c>RevenueCat__WebhookAuthorization</c>. Boşsa webhook ucu kapalıdır (503).
/// </summary>
public class RevenueCatSettings
{
    public const string SectionName = "RevenueCat";

    /// <summary>
    /// RevenueCat panelinde webhook için girilen Authorization değeri. Gelen başlık bu değere
    /// birebir ya da "Bearer {değer}" olarak eşit olmalı.
    /// </summary>
    public string WebhookAuthorization { get; set; } = string.Empty;

    /// <summary>SANDBOX ortamındaki olaylar işlensin mi (geliştirme/test için true).</summary>
    public bool AcceptSandboxEvents { get; set; }
}
