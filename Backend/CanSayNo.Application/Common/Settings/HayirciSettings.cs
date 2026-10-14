using System;

namespace CanSayNo.Application.Common.Settings;

/// <summary>
/// Hayırcı ürün ayarları — appsettings "Hayirci" bölümünden bağlanır, singleton olarak enjekte edilir.
/// </summary>
public class HayirciSettings
{
    public const string SectionName = "Hayirci";

    /// <summary>Subscriptions.AppId — multi-app abonelik ayrımı.</summary>
    public string AppId { get; set; } = "cansayno";

    /// <summary>Premium olmayan kullanıcının günde başlatabileceği pratik sayısı.</summary>
    public int FreeDailySessionLimit { get; set; } = 1;

    /// <summary>
    /// "Gün" sınırının hesaplandığı UTC ofseti. Türkiye UTC+3 (2016'dan beri yaz saati yok),
    /// böylece limit gece yarısı TR saatiyle sıfırlanır.
    /// </summary>
    public int UsageDayUtcOffsetHours { get; set; } = 3;

    /// <summary>Kullanım sayacının ait olduğu takvim günü.</summary>
    public DateOnly GetUsageDate(DateTime utcNow) =>
        DateOnly.FromDateTime(utcNow.AddHours(UsageDayUtcOffsetHours));
}
