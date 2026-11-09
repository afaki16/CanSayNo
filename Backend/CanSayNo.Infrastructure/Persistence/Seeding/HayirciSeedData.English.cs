using CanSayNo.Domain.Entities.Hayirci;
using CanSayNo.Domain.Enums;

namespace CanSayNo.Infrastructure.Persistence.Seeding
{
    /// <summary>
    /// İngilizce senaryolar (2026-11-09): İş 10 (ücretsiz) + diğer 7 bağlamda 5'er (premium) = 45.
    /// Genel uluslararası kültüre göre yazıldı (Türkçe senaryoların çevirisi değil).
    /// ID biçimi <c>en_&lt;bağlam&gt;_NNN_konu</c>; dil burada "en" olarak işaretlenir.
    /// Dosyalar: EnglishWork (İş), EnglishPersonal (Aile, Arkadaş, Romantik), EnglishEveryday
    /// (Satıcı, Komşu, Okul, Dijital).
    /// </summary>
    public static partial class HayirciSeedData
    {
        /// <summary>Seed edilen tüm senaryolar (Türkçe 65 + İngilizce 45). Testler içerik kurallarını bununla doğrular.</summary>
        public static List<Scenario> GetSeedScenarios() =>
            BuildScenarios()
                .Concat(BuildCatalogScenarios())
                .Concat(BuildNewContextScenarios())
                .Concat(BuildEnglishScenarios())
                .ToList();

        private static List<Scenario> BuildEnglishScenarios()
        {
            var scenarios = BuildEnglishWorkScenarios()
                .Concat(BuildEnglishPersonalScenarios())
                .Concat(BuildEnglishEverydayScenarios())
                .ToList();

            foreach (var scenario in scenarios)
                scenario.Language = ContentLanguages.English;

            return scenarios;
        }
    }
}
