using CanSayNo.Domain.Entities.Hayirci;
using CanSayNo.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CanSayNo.Infrastructure.Persistence.Seeding
{
    /// <summary>
    /// Hayırcı (CanSayNo) test senaryolarını runtime'da seed eder.
    ///
    /// EF Core <c>HasData</c> KULLANILMAZ — JSONB owned/converter alanları HasData ile sorun
    /// çıkarır ve her model değişikliğinde migration'a gömülür. Bunun yerine startup'ta
    /// (migration sonrası) idempotent ekleme yapılır: ID'si zaten DB'de olan senaryo atlanır.
    /// </summary>
    public static class HayirciSeedData
    {
        public static async Task SeedScenariosAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

            try
            {
                var seedScenarios = BuildScenarios();
                var seedIds = seedScenarios.Select(s => s.Id).ToList();

                var existingIds = await context.Scenarios
                    .Where(s => seedIds.Contains(s.Id))
                    .Select(s => s.Id)
                    .ToListAsync();

                var toAdd = seedScenarios.Where(s => !existingIds.Contains(s.Id)).ToList();

                if (toAdd.Count == 0)
                {
                    logger.LogInformation("Hayırcı senaryoları zaten mevcut. Seed atlandı.");
                    return;
                }

                await context.Scenarios.AddRangeAsync(toAdd);
                await context.SaveChangesAsync();

                logger.LogInformation("{Count} Hayırcı senaryosu seed edildi: {Ids}",
                    toAdd.Count, string.Join(", ", toAdd.Select(s => s.Id)));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hayırcı senaryoları seed edilirken hata oluştu.");
                throw;
            }
        }

        private static List<Scenario> BuildScenarios() => new()
        {
            new Scenario
            {
                Id = "is_001_cuma_mesaji",
                Title = "Cuma 18:00 Mesajı",
                ShortDescription = "Patronun hafta sonu acil iş bahanesiyle çalışma talebi",
                ContextId = ContextId.Is,
                Difficulty = 2,
                EstimatedDurationMinutes = 5,
                IsFree = true,
                IsActive = true,
                SortOrder = 1,
                Tags = new List<string> { "mesai", "patron", "haftasonu" },
                Scene = new ScenarioScene
                {
                    Setting = "Cuma akşam 18:00, hafta sonu planın var",
                    Situation = "Telefonun titriyor. Patronundan WhatsApp mesajı geldi: " +
                                "\"Müşteri toplantısı pazartesi sabah 9'a alındı, sunumu revize etmeliyiz. " +
                                "Birkaç saatini ayırabilir misin?\"",
                    UserGoal = "Hafta sonu planını korumak — ama köprüleri yakmadan",
                    Hint = "Övgüyle gelen taleplere karşı dikkatli ol; duygusal yatırım yaratır."
                },
                Character = new ScenarioCharacter
                {
                    Name = "Murat Bey",
                    Age = 45,
                    Role = "Orta düzey yönetici, doğrudan amirin",
                    Relationship = "2 yıldır birlikte çalışıyorsunuz, genelde iyi geçinirsiniz",
                    Archetype = CharacterArchetype.BabacanManipulator
                },
                PressureTactics = new ScenarioPressureTactics
                {
                    RoundOne = new List<PressureTactic> { PressureTactic.OvguVeGuven, PressureTactic.SahteZamanBaskisi },
                    RoundTwo = new List<PressureTactic> { PressureTactic.SuclulukYukleme, PressureTactic.AcilDurumYaratma },
                    RoundThree = new List<PressureTactic> { PressureTactic.PasifTehdit },
                    MaxRounds = 3
                }
            },
            new Scenario
            {
                Id = "is_002_toplanti_uzatma",
                Title = "\"Sadece bir saat\" toplantısı",
                ShortDescription = "Toplantının 3 saate uzaması, çıkmak istiyorsun",
                ContextId = ContextId.Is,
                Difficulty = 2,
                EstimatedDurationMinutes = 4,
                IsFree = true,
                IsActive = true,
                SortOrder = 2,
                Tags = new List<string> { "toplanti", "zaman", "sinirlar" },
                Scene = new ScenarioScene
                {
                    Setting = "Salı öğleden sonra, \"1 saatlik\" planlanan ekip toplantısı",
                    Situation = "\"Sadece bir saat\" denilen toplantı 2.5 saati geçti. Senin başka önemli " +
                                "işlerin bekliyor. Yönetici toplantıyı uzatmaya devam ediyor: " +
                                "\"5 dakika daha lütfen, şu konuyu da çözelim.\"",
                    UserGoal = "Toplantıdan saygıyla ayrılmak",
                    Hint = "Yapacağın işin önemini hatırlat, suçluluk hissetme."
                },
                Character = new ScenarioCharacter
                {
                    Name = "Ayşe Hanım",
                    Age = 38,
                    Role = "Proje yöneticisi",
                    Relationship = "Aynı kıdemdesiniz, profesyonel ilişki",
                    Archetype = CharacterArchetype.MasumRicaci
                },
                PressureTactics = new ScenarioPressureTactics
                {
                    RoundOne = new List<PressureTactic> { PressureTactic.SahteZamanBaskisi, PressureTactic.SosyalKanit },
                    RoundTwo = new List<PressureTactic> { PressureTactic.SuclulukYukleme },
                    RoundThree = new List<PressureTactic> { PressureTactic.KarsiTeklif },
                    MaxRounds = 3
                }
            },
            new Scenario
            {
                Id = "is_003_izin_iptal",
                Title = "Yıllık izin iptal talebi",
                ShortDescription = "Onaylanan izninin son anda iptal edilmek istenmesi",
                ContextId = ContextId.Is,
                Difficulty = 3,
                EstimatedDurationMinutes = 6,
                IsFree = true,
                IsActive = true,
                SortOrder = 3,
                Tags = new List<string> { "izin", "hak", "otorite" },
                Scene = new ScenarioScene
                {
                    Setting = "Pazartesi sabahı, 2 hafta sonra başlayacak yıllık izninin var",
                    Situation = "Müdürün ofisine çağırıldın. \"Önemli bir proje çıktı, izni 2 ay sonraya " +
                                "erteleyebilir misin? Sana güveniyorum, sen olmadan olmaz.\"",
                    UserGoal = "Önceden planlanmış izninizi korumak",
                    Hint = "İzin hakkındır, lütuf değil. Yine de profesyonel kal."
                },
                Character = new ScenarioCharacter
                {
                    Name = "Kemal Bey",
                    Age = 52,
                    Role = "Departman müdürü",
                    Relationship = "Bir yıldır birlikte çalışıyorsunuz, kıdemli",
                    Archetype = CharacterArchetype.Otoriter
                },
                PressureTactics = new ScenarioPressureTactics
                {
                    RoundOne = new List<PressureTactic> { PressureTactic.Otorite, PressureTactic.OvguVeGuven },
                    RoundTwo = new List<PressureTactic> { PressureTactic.SuclulukYukleme, PressureTactic.AcilDurumYaratma },
                    RoundThree = new List<PressureTactic> { PressureTactic.PasifTehdit },
                    MaxRounds = 3
                }
            },
            new Scenario
            {
                Id = "is_004_sen_yaparsin",
                Title = "\"Sen yaparsın\" tuzağı",
                ShortDescription = "Kendi işin değilken yardım istenmesi",
                ContextId = ContextId.Is,
                Difficulty = 3,
                EstimatedDurationMinutes = 5,
                IsFree = true,
                IsActive = true,
                SortOrder = 4,
                Tags = new List<string> { "gorev_devri", "meslektaslar" },
                Scene = new ScenarioScene
                {
                    Setting = "Çarşamba öğleden sonra, kendi işlerin yığılmış durumda",
                    Situation = "Bir iş arkadaşın masana geldi: \"Şu raporu sen yaparsan çok daha iyi olur, " +
                                "sen bu konuyu benden iyi biliyorsun. Sadece 1-2 saatlik iş.\" Aslında 4-5 saatlik " +
                                "iş ve onun sorumluluğu.",
                    UserGoal = "Kendi önceliklerini korumak, görev devrini kabul etmemek",
                    Hint = "Övgüyü kabul et, ama görevi reddet."
                },
                Character = new ScenarioCharacter
                {
                    Name = "Selin",
                    Age = 29,
                    Role = "Aynı seviyede iş arkadaşı",
                    Relationship = "Arkadaşça, ama bu tür \"ricalar\" sık geliyor",
                    Archetype = CharacterArchetype.TatliDilliSatici
                },
                PressureTactics = new ScenarioPressureTactics
                {
                    RoundOne = new List<PressureTactic> { PressureTactic.OvguVeGuven, PressureTactic.SahteZamanBaskisi },
                    RoundTwo = new List<PressureTactic> { PressureTactic.SuclulukYukleme, PressureTactic.SahteAnlayis },
                    RoundThree = new List<PressureTactic> { PressureTactic.DuyguSomurusu },
                    MaxRounds = 3
                }
            },
            new Scenario
            {
                Id = "is_005_maas_erteleme",
                Title = "Maaş zammı görüşmesini erteleme",
                ShortDescription = "Yıllık değerlendirmede zam talebini erteleme baskısı",
                ContextId = ContextId.Is,
                Difficulty = 4,
                EstimatedDurationMinutes = 7,
                IsFree = false,
                IsActive = true,
                SortOrder = 5,
                Tags = new List<string> { "maas", "kariyer", "ertelemek" },
                Scene = new ScenarioScene
                {
                    Setting = "Yıllık performans değerlendirme görüşmesi",
                    Situation = "Müdürün senin performansını övüyor ama zam konusunda diyor ki: " +
                                "\"Şirket şu an zor durumda, 6 ay daha sabret. Sana söz veriyorum, sonra her şeyi " +
                                "telafi edeceğiz.\" Halbuki son 2 yıldır benzer sözler veriliyor.",
                    UserGoal = "Zammı kararlı bir şekilde talep etmek, ertelemeyi kabul etmemek",
                    Hint = "Geçmiş vaatleri hatırlat, somut tarih iste."
                },
                Character = new ScenarioCharacter
                {
                    Name = "Tarık Bey",
                    Age = 48,
                    Role = "Doğrudan amirin",
                    Relationship = "3 yıldır birlikte çalışıyorsunuz",
                    Archetype = CharacterArchetype.BabacanManipulator
                },
                PressureTactics = new ScenarioPressureTactics
                {
                    RoundOne = new List<PressureTactic> { PressureTactic.OvguVeGuven, PressureTactic.SahteAnlayis },
                    RoundTwo = new List<PressureTactic> { PressureTactic.AcilDurumYaratma, PressureTactic.KarsiTeklif },
                    RoundThree = new List<PressureTactic> { PressureTactic.PasifTehdit },
                    MaxRounds = 3
                }
            },
            new Scenario
            {
                Id = "is_006_sirket_etkinligi",
                Title = "Cuma akşamı şirket etkinliği",
                ShortDescription = "Zorunlu hissettirilen sosyal etkinlik baskısı",
                ContextId = ContextId.Is,
                Difficulty = 1,
                EstimatedDurationMinutes = 3,
                IsFree = true,
                IsActive = true,
                SortOrder = 6,
                Tags = new List<string> { "sosyal", "sirket", "zorunluluk" },
                Scene = new ScenarioScene
                {
                    Setting = "Cuma sabahı, akşam şirket happy hour'u var",
                    Situation = "İK'dan duyuru: \"Bu akşam 18:00'da tüm ekip katılımı zorunlu, takım ruhu için " +
                                "önemli!\" Senin başka planın var ve \"zorunlu\" denilen şey aslında resmi olarak " +
                                "zorunlu değil.",
                    UserGoal = "Profesyonel kalarak katılmadığını bildirmek",
                    Hint = "Mazerete gerek yok; basit ve net olabilirsin."
                },
                Character = new ScenarioCharacter
                {
                    Name = "Esra Hanım",
                    Age = 35,
                    Role = "İK uzmanı",
                    Relationship = "Resmi mesafeli ilişki",
                    Archetype = CharacterArchetype.Otoriter
                },
                PressureTactics = new ScenarioPressureTactics
                {
                    RoundOne = new List<PressureTactic> { PressureTactic.SosyalKanit, PressureTactic.Otorite },
                    RoundTwo = new List<PressureTactic> { PressureTactic.SuclulukYukleme },
                    RoundThree = new List<PressureTactic> { PressureTactic.PasifTehdit },
                    MaxRounds = 3
                }
            },
            new Scenario
            {
                Id = "aile_001_evlenmek",
                Title = "\"Ne zaman evleneceksin?\"",
                ShortDescription = "Aile toplantısında evlilik sorgusuna karşı sınır koymak",
                ContextId = ContextId.Aile,
                Difficulty = 3,
                EstimatedDurationMinutes = 5,
                IsFree = false,
                IsActive = true,
                SortOrder = 1,
                Tags = new List<string> { "aile", "evlilik", "ozel_hayat" },
                Scene = new ScenarioScene
                {
                    Setting = "Pazar öğle yemeği, kalabalık aile sofrası",
                    Situation = "Teyzen sofranın ortasında yüksek sesle soruyor: " +
                                "\"Ee, sen ne zaman evleneceksin? Yaşın geçiyor, biz torun göreceğiz bir ara. " +
                                "Bak filanca da senden küçüktü, çoluk çocuğa karıştı bile.\"",
                    UserGoal = "Özel hayatına dair sınır koymak — sofrayı germeden",
                    Hint = "Hesap verme tuzağına düşme; kısa, net ve sıcak bir sınır yeter."
                },
                Character = new ScenarioCharacter
                {
                    Name = "Nurten Teyze",
                    Age = 58,
                    Role = "Annenin ablası",
                    Relationship = "Seni küçüklüğünden beri tanıyan, sözünü sakınmayan akraba",
                    Archetype = CharacterArchetype.DuygusalKurban
                },
                PressureTactics = new ScenarioPressureTactics
                {
                    RoundOne = new List<PressureTactic> { PressureTactic.Kiyaslama, PressureTactic.SosyalKanit },
                    RoundTwo = new List<PressureTactic> { PressureTactic.DuyguSomurusu, PressureTactic.SuclulukYukleme },
                    RoundThree = new List<PressureTactic> { PressureTactic.KisKisSessizlik },
                    MaxRounds = 3
                }
            }
        };
    }
}
