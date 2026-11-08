using CanSayNo.Application.Common.Settings;

namespace CanSayNo.Tests.Unit;

public class HayirciSettingsTests
{
    [Theory]
    // TR = UTC+3: 20:59 UTC hâlâ aynı gün, 21:00 UTC ertesi gün (TR gece yarısı).
    [InlineData("2026-11-08T20:59:00Z", "2026-11-08")]
    [InlineData("2026-11-08T21:00:00Z", "2026-11-09")]
    [InlineData("2026-11-08T00:30:00Z", "2026-11-08")]
    public void Usage_day_rolls_over_at_turkish_midnight(string utc, string expectedDay)
    {
        var settings = new HayirciSettings();

        var day = settings.GetUsageDate(DateTime.Parse(utc).ToUniversalTime());

        Assert.Equal(DateOnly.Parse(expectedDay), day);
    }
}
