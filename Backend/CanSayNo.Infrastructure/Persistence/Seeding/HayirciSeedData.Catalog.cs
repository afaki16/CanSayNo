using CanSayNo.Domain.Entities.Hayirci;
using CanSayNo.Domain.Enums;
using static CanSayNo.Domain.Enums.CharacterArchetype;
using static CanSayNo.Domain.Enums.PressureTactic;

namespace CanSayNo.Infrastructure.Persistence.Seeding
{
    /// <summary>
    /// v1.0 senaryo kataloğu (ilk 7 senaryonun devamı). Her bağlamda 10 senaryo, toplam 50.
    /// İş bağlamı ücretsiz, diğer dört bağlam premium.
    ///
    /// Yazım kuralları: açılış mesajı karakterin ağzından, doğal mesajlaşma Türkçesi;
    /// raunt taktikleri baskıyı kademeli artırır (1: yumuşak giriş, 2: duygusal/aciliyet,
    /// 3: son koz). Hint kullanıcıya görünür; PressureTactics asla görünmez.
    /// </summary>
    public static partial class HayirciSeedData
    {
        private static Scenario Make(
            string id,
            string title,
            string shortDescription,
            ContextId context,
            int difficulty,
            int minutes,
            int sortOrder,
            string[] tags,
            string setting,
            string situation,
            string userGoal,
            string hint,
            string opening,
            ScenarioCharacter character,
            PressureTactic[] roundOne,
            PressureTactic[] roundTwo,
            PressureTactic[] roundThree) => new()
        {
            Id = id,
            Title = title,
            ShortDescription = shortDescription,
            ContextId = context,
            Difficulty = difficulty,
            EstimatedDurationMinutes = minutes,
            IsFree = context == ContextId.Is,
            IsActive = true,
            SortOrder = sortOrder,
            Tags = tags.ToList(),
            Scene = new ScenarioScene
            {
                Setting = setting,
                Situation = situation,
                UserGoal = userGoal,
                Hint = hint,
                OpeningMessage = opening
            },
            Character = character,
            PressureTactics = new ScenarioPressureTactics
            {
                RoundOne = roundOne.ToList(),
                RoundTwo = roundTwo.ToList(),
                RoundThree = roundThree.ToList(),
                MaxRounds = 3
            }
        };

        private static ScenarioCharacter Who(string name, int age, string role, string relationship, CharacterArchetype archetype) =>
            new() { Name = name, Age = age, Role = role, Relationship = relationship, Archetype = archetype };

        private static PressureTactic[] T(params PressureTactic[] tactics) => tactics;

        private static List<Scenario> BuildCatalogScenarios() => new()
        {
            // ───────────────────────── İş & Kariyer (ücretsiz) ─────────────────────────
            Make("is_007_gece_mesaji", "Gece 22:30 \"hızlıca bakar mısın\"",
                "Mesai sonrası küçük görünen ama saatler sürecek bir iş isteği",
                ContextId.Is, 2, 4, 7, new[] { "mesai", "dijital_sinir", "takim_lideri" },
                "Salı gecesi 22:30, evdesin ve yatmaya hazırlanıyorsun",
                "Takım liderinden Slack mesajı geliyor: \"Uyumadıysan şu dashboard'daki rakamlara hızlıca bakar mısın? " +
                "Yarın sabah yönetime gidecek.\" Rakamların kaynağını bulmak en az iki saat sürer ve bu ay üçüncü kez oluyor.",
                "Bu gece çalışmayı reddetmek ve sabah için makul bir zaman önermek",
                "Mesaja hemen cevap verme zorunluluğun yok; ne zaman bakabileceğini net söylemen yeter.",
                "Uyumadıysan şu dashboard'daki rakamlara hızlıca bakabilir misin? Yarın sabah 9'da yönetime gidiyor, sadece kontrol etmek lazım. 10 dakikalık iş 🙏",
                Who("Can", 34, "Takım lideri", "Bir yıldır aynı ekiptesiniz, iyi anlaşırsınız ama sınırları pek dikkate almaz", MasumRicaci),
                T(SahteZamanBaskisi, OvguVeGuven), T(AcilDurumYaratma, SosyalKanit), T(SuclulukYukleme)),

            Make("is_008_ek_gorev", "Ayrılan çalışanın işleri",
                "İşten ayrılan arkadaşının tüm sorumlulukları ek ücretsiz sana veriliyor",
                ContextId.Is, 4, 6, 8, new[] { "is_yuku", "ucret", "otorite" },
                "Perşembe sabahı, birebir görüşme",
                "Ekipten Deniz ayrıldı. Müdürün, yerine kimse alınmayacağını ve Deniz'in iki müşterisinin de sana " +
                "devredileceğini söylüyor. Ne unvan ne maaş değişikliği var; senin iş yükün zaten dolu.",
                "Ek yükü olduğu gibi kabul etmemek; ya önceliklendirme ya da karşılık talep etmek",
                "\"Hayır\" demek yerine \"bunu yaparsam şu düşer\" diye somut takas konuşabilirsin.",
                "Deniz'in ayrıldığını duymuşsundur. Bütçe yüzünden yerine kimseyi alamıyoruz, o yüzden onun iki müşterisini sana veriyorum. Sen bu işi zaten herkesten iyi biliyorsun, eminim halledersin.",
                Who("Gülay Hanım", 47, "Birim müdürü", "Üç yıldır müdürün; kararlarını genelde tartışmaya açmaz", Otoriter),
                T(Otorite, OvguVeGuven), T(AcilDurumYaratma, Kiyaslama), T(PasifTehdit)),

            Make("is_009_borc_isteyen_meslektas", "Maaş gününe kadar borç",
                "İş arkadaşı maaş gününe kadar yüklü bir borç istiyor",
                ContextId.Is, 3, 5, 9, new[] { "para", "meslektas", "borc" },
                "Öğle arası, ofis mutfağı",
                "Yan masadaki iş arkadaşın kimse yokken yanına geliyor ve maaş gününe kadar 15.000 TL istiyor. " +
                "Geçen yıl verdiğin 3.000 TL'yi ancak aylar sonra, sen hatırlatınca geri ödemişti.",
                "Borç vermeyi reddetmek ve iş ilişkisini zedelememek",
                "Neden veremediğini uzun uzun açıklamak zorunda değilsin.",
                "Senden bir şey isteyeceğim ama kimseye söyleme lütfen. Ev sahibi kirayı artırdı, bu ay çok sıkıştım. Maaş gününe kadar 15 bin verebilir misin? Yemin ederim ilk gün geri öderim.",
                Who("Burak", 31, "Aynı departmanda uzman", "İki yıldır yan masadasın, arada birlikte öğle yemeği yersiniz", DuygusalKurban),
                T(DuyguSomurusu, SahteZamanBaskisi), T(SuclulukYukleme, AcilDurumYaratma), T(KisKisSessizlik)),

            Make("is_010_nobet_degisimi", "Üçüncü kez nöbet değişimi",
                "Ekip arkadaşı hafta sonu nöbetini yine seninle değiştirmek istiyor",
                ContextId.Is, 2, 4, 10, new[] { "nobet", "haftasonu", "meslektas" },
                "Çarşamba, ekip sohbet grubu",
                "Ekip arkadaşın bu hafta sonuki nöbetini senin nöbetinle değiştirmek istiyor. Son iki ayda üçüncü " +
                "kez soruyor ve önceki değişimlerin hiçbirini sana geri vermedi. Bu hafta sonu ailenle planın var.",
                "Değişimi reddetmek, örüntüyü nazikçe adlandırmak",
                "Önceki iki değişimi hatırlatmak suçlama değil, gerçeği söylemektir.",
                "Bu hafta sonu nöbeti benimle değiştirebilir misin? Kuzenimin nişanı var, gitmem lazım. Sen zaten pek bir yere gitmiyorsun hafta sonları 😅",
                Who("Ozan", 28, "Aynı vardiyadaki ekip arkadaşı", "Bir buçuk yıldır aynı ekiptesiniz", PasifAgresif),
                T(SosyalKanit, KarsiTeklif), T(SuclulukYukleme, SahteAnlayis), T(KisKisSessizlik, PasifTehdit)),

            // ───────────────────────── Aile & Akraba ─────────────────────────
            Make("aile_002_bayram_ziyareti", "Bayramın tamamı bizde",
                "Annen bayramın her gününü onlarla geçirmeni bekliyor",
                ContextId.Aile, 3, 5, 2, new[] { "bayram", "anne", "zaman" },
                "Bayramdan bir hafta önce, telefon görüşmesi",
                "Bayramın ilk günü ailene gideceksin; ikinci gün arkadaşlarınla kısa bir kaçamak planladın. " +
                "Annen bunu duyunca telefonda sesi değişiyor.",
                "Planını korumak, annene sevgini hissettirerek",
                "Sevgini göstermek ile her isteği kabul etmek aynı şey değil.",
                "Bayramda ikinci gün yokmuşsun öyle mi? Ben de herkese dört gün birlikteyiz dedim. Teyzenler de gelecek. Senin için dolma bile yapacaktım...",
                Who("Annen (Hülya)", 56, "Annen", "Sıcak ama kırılgan; uzaklaştığını hissedince içine kapanır", DuygusalKurban),
                T(DuyguSomurusu, SosyalKanit), T(SuclulukYukleme, Kiyaslama), T(KisKisSessizlik)),

            Make("aile_003_kardes_borc", "Kardeşin yeni borç isteği",
                "Önceki borcunu ödemeyen kardeşin tekrar para istiyor",
                ContextId.Aile, 4, 6, 3, new[] { "kardes", "para", "borc" },
                "Pazartesi akşamı, kardeşinden gelen sesli arama",
                "Kardeşin yeni bir telefon ve kurs için 20.000 TL istiyor. Geçen yıl \"iki ayda öderim\" dediği " +
                "25.000 TL'nin hâlâ yarısını geri vermedi. Annen de araya girip \"kardeşin o, yardım et\" diyor.",
                "Yeni borcu reddetmek, kardeşinle bağı koparmadan",
                "Geçmiş borcu konuşmak kin tutmak değildir; sınırının gerekçesidir.",
                "Abla/abi bir şey isteyeceğim ama kızma. Kurs başlıyor, bir de telefonum bozuldu, 20 bin lazım. Bu sefer kesin öderim, söz. Annem de sana sorarsın dedi.",
                Who("Emre", 23, "Küçük kardeşin", "Onu çok seviyorsun; ailede hep korunan taraf o oldu", MasumRicaci),
                T(OvguVeGuven, Otorite), T(DuyguSomurusu, SuclulukYukleme), T(Kiyaslama, KisKisSessizlik)),

            Make("aile_004_meslek_secimi", "\"Bu meslek iş mi?\"",
                "Baban kariyer değişikliğini kabullenmiyor",
                ContextId.Aile, 4, 6, 4, new[] { "kariyer", "baba", "kararlar" },
                "Akşam yemeği, ailecek sofradasınız",
                "Mühendislikten ayrılıp grafik tasarıma geçme kararını açıkladın. Baban yemeği bırakıp " +
                "kararını sorgulamaya başlıyor; okul masraflarını ve elalemi hatırlatıyor.",
                "Kararını savunmak ama hesap verme sorgusuna girmemek",
                "Onay istemiyorsun, bilgi veriyorsun. Kararın senin.",
                "Dört yıl mühendislik okut, sonra çiz çiz para kazan. Bu nasıl karar? Ben senin yaşındayken iki çocuk büyütüyordum. Akıllı ol, bu hevesi bırak.",
                Who("Baban (Hasan)", 61, "Baban", "Seni seviyor ama kararlarını kendi doğrularıyla tartar", Otoriter),
                T(Otorite, Kiyaslama), T(SuclulukYukleme, SosyalKanit), T(PasifTehdit)),

            Make("aile_005_cocuk_bakimi", "Her hafta sonu bakıcılık",
                "Ablan çocuklarını her hafta sonu sana bırakmak istiyor",
                ContextId.Aile, 2, 4, 5, new[] { "kardes", "cocuk", "zaman" },
                "Cuma öğleden sonra, ablandan gelen mesaj",
                "Ablan son bir aydır her cumartesi çocuklarını sana bırakıyor. Bu hafta sonu kendine ayırmak istiyorsun " +
                "ama ablan yine soruyor, üstelik bu sefer pazar da var.",
                "Bu hafta sonu reddetmek ve kalıcı bir düzen konuşmak",
                "Bir kez \"hayır\" demek ilişkiyi bozmaz; sürekli \"evet\" içten içe bozar.",
                "Canımm bu hafta sonu da çocuklar sende kalabilir mi? Hem cumartesi hem pazar. Bizim düğün var, biliyorsun seni çok seviyorlar, teyzem/dayım diye tutturuyorlar 💕",
                Who("Ablan (Derya)", 37, "Ablan", "Yakınsınız; o yardım istemeyi doğal görüyor", MasumRicaci),
                T(OvguVeGuven, DuyguSomurusu), T(SahteAnlayis, SuclulukYukleme), T(Kiyaslama)),

            Make("aile_006_habersiz_ziyaret", "Kayınvalidenin anahtar isteği",
                "Kayınvaliden habersiz gelebilmek için evinin anahtarını istiyor",
                ContextId.Aile, 3, 5, 6, new[] { "kayinvalide", "mahremiyet", "ev" },
                "Pazar ziyareti, kahve sonrası",
                "Kayınvaliden son iki ziyaretinde habersiz geldi. Şimdi \"lazım olur\" diyerek yedek anahtar istiyor. " +
                "Eşin şu an odada değil.",
                "Anahtarı vermemek, saygılı ama net kalmak",
                "\"Biz böyle rahat ediyoruz\" demek yeterli bir gerekçedir.",
                "Bir yedek anahtarı bana versenize. Ne olur ne olmaz, bir şey lazım olur, çiçekleri sularım. Benim kendi evim gibi zaten burası, değil mi?",
                Who("Sevim Hanım", 63, "Kayınvaliden", "Görünüşte tatlı; sınır konunca alındığını belli eder", PasifAgresif),
                T(SahteAnlayis, OvguVeGuven), T(SuclulukYukleme, DuyguSomurusu), T(KisKisSessizlik, AsiriOzurAlan)),

            Make("aile_007_tasinma", "Evden ayrılma kararı",
                "Annen ayrı eve çıkma kararına karşı çıkıyor",
                ContextId.Aile, 5, 7, 7, new[] { "anne", "bagimsizlik", "tasinma" },
                "Kira sözleşmesini imzalamadan önceki akşam",
                "Kendi evine çıkmak için ev buldun ve yarın sözleşmeyi imzalayacaksın. Annene söylediğinde " +
                "ağlamaya başlıyor ve \"daha evlenmeden ayrı ev mi olur\" diyor.",
                "Kararından vazgeçmeden annenin duygularını tanımak",
                "Annenin üzüntüsü gerçek olabilir; bu, kararını yanlış yapmaz.",
                "Ne gerek var ayrı eve? Burada neyin eksik? Elalem ne der, evlenmeden ayrı eve çıkmış derler. Ben sensiz bu evde ne yaparım...",
                Who("Annen (Fatma)", 54, "Annen", "Çok bağlısınız; bağımsızlığını kendine yönelik ret gibi algılıyor", DuygusalKurban),
                T(DuyguSomurusu, SosyalKanit), T(SuclulukYukleme, KarsiTeklif), T(KisKisSessizlik, AsiriOzurAlan)),

            Make("aile_008_kefil", "Amcanın kefillik isteği",
                "Amcan yüklü bir kredi için kefil olmanı istiyor",
                ContextId.Aile, 4, 6, 8, new[] { "para", "kefil", "akraba" },
                "Amcanın iş yerinde, çay eşliğinde",
                "Amcan dükkânını büyütmek için 500.000 TL kredi çekecek ve bankanın kefil istediğini söylüyor. " +
                "Daha önce başka bir akrabanın kefilliği yüzünden zor duruma düştüğünü biliyorsun.",
                "Kefil olmayı reddetmek, aile içi ilişkiyi korumak",
                "Kefillik borcun kendisidir; \"sadece imza\" değildir.",
                "Yeğenim, sen bizim aklı başında olanımızsın. Banka kefil istiyor, sadece bir imza. Hiçbir risk yok, ben her ay öderim. Baban da olsa hemen imzalardı.",
                Who("Amcan (Rıza)", 59, "Babanın kardeşi", "Çocukluğundan beri sana harçlık veren, sevilen amca", BabacanManipulator),
                T(OvguVeGuven, SahteAnlayis), T(Kiyaslama, SuclulukYukleme), T(PasifTehdit)),

            Make("aile_009_sofra_israri", "\"Bir tabak daha ye\"",
                "Babaannen sofrada yemek ve kilo yorumlarıyla ısrar ediyor",
                ContextId.Aile, 1, 3, 9, new[] { "yemek", "beden", "buyukler" },
                "Babaannenin evinde akşam yemeği",
                "Doydun ama babaannen tabağını tekrar dolduruyor. Bir yandan da \"çok zayıfladın, hasta mısın\" " +
                "diye herkesin önünde vücudun hakkında yorum yapıyor.",
                "Yemeği nazikçe reddetmek ve beden yorumlarına sınır koymak",
                "Sıcak bir ton ve kısa cümleler, büyüklerle en iyi sınırdır.",
                "Ay bu ne hal, eriyip gitmişsin! Hasta mısın yoksa? Al şu tabağı bitir, sana özel yaptım. Benim yemeğimi beğenmiyor musun yoksa?",
                Who("Babaannen", 79, "Babaannen", "Seni çok seviyor; sevgisini yemekle gösteriyor", BabacanManipulator),
                T(DuyguSomurusu, OvguVeGuven), T(SuclulukYukleme, SosyalKanit), T(KisKisSessizlik)),

            Make("aile_010_yazlik_tatili", "Zorunlu aile tatili",
                "Baban tüm ailenin tatilini yazlıkta geçirmesini bekliyor",
                ContextId.Aile, 2, 4, 10, new[] { "tatil", "baba", "plan" },
                "Mayıs, aile WhatsApp grubu",
                "Baban gruba \"Bu yaz herkes temmuzda yazlıkta, iki hafta\" yazdı. Sen aylardır kendi tatilini " +
                "planlıyorsun ve uçak biletlerini aldın. Kardeşlerin \"tamam baba\" diye cevap verdi bile.",
                "Kendi tatilini korumak, kısa bir ziyaret önermek",
                "Herkesin kabul etmesi senin de kabul etmen gerektiği anlamına gelmez.",
                "Bu yaz istisnasız herkes temmuzda yazlıkta, iki hafta. Dedeniz de son yazlarından birini yaşıyor, toplanalım. Kardeşlerin tamam dedi, bir sen kaldın.",
                Who("Baban (Ahmet)", 63, "Baban", "Aile birliğini her şeyin üstünde tutar; plan yapıp duyurur", Otoriter),
                T(Otorite, SosyalKanit), T(DuyguSomurusu, SuclulukYukleme), T(Kiyaslama, PasifTehdit)),

            // ───────────────────────── Arkadaş & Sosyal ─────────────────────────
            Make("arkadas_001_borc", "Arkadaşın üçüncü borcu",
                "Yakın arkadaşın bu yıl üçüncü kez borç istiyor",
                ContextId.Arkadas, 3, 5, 1, new[] { "para", "borc", "yakin_arkadas" },
                "Cumartesi öğleden sonra, kafede buluşma",
                "Lise arkadaşın bu yıl iki kez borç aldı; birini ödedi, diğerini ödemedi. Şimdi tatil için " +
                "10.000 TL istiyor ve \"sen yoksan kimden isteyeyim\" diyor.",
                "Borç vermeyi reddetmek, arkadaşlığı korumak",
                "Arkadaşlığın değeri borç vermeye bağlı değil.",
                "Kanka çok ayıp olacak biliyorum ama 10 bin lazım. Tatil fırsatı çıktı, kaçırırsam bir daha olmaz. Senden başka kimseye soramam ki, sen benim kardeşimsin.",
                Who("Kerem", 29, "Lise arkadaşın", "On yıldır yakın arkadaşsınız", DuygusalKurban),
                T(OvguVeGuven, SahteZamanBaskisi), T(DuyguSomurusu, SuclulukYukleme), T(KisKisSessizlik)),

            Make("arkadas_002_tasinma_yardimi", "Pazar sabahı taşınma",
                "Son dakika taşınmaya yardım çağrısı",
                ContextId.Arkadas, 1, 3, 2, new[] { "yardim", "son_dakika", "haftasonu" },
                "Cumartesi gecesi 23:00, mesaj",
                "Arkadaşın yarın sabah 7'de taşınacağını ve nakliyecinin iptal ettiğini yazıyor. Yarın senin tek " +
                "dinlenme günün ve öğlen önemli bir randevun var.",
                "Bu kez yardım etmeyi reddetmek ya da sınırlı bir katkı önermek",
                "Tamamen \"hayır\" ya da kısmi \"evet\" — ikisi de geçerli; önemli olan senin seçmen.",
                "Acil durum!! Nakliyeci iptal etti, yarın sabah 7'de taşınıyorum. Gelir misin? Sadece birkaç koli var, öğlene biter. Pizza benden 🍕",
                Who("Ece", 27, "Üniversiteden arkadaşın", "Sık görüşürsünüz; plan yapmayı sevmez", MasumRicaci),
                T(AcilDurumYaratma, SahteZamanBaskisi), T(SosyalKanit, SuclulukYukleme), T(KarsiTeklif)),

            Make("arkadas_003_gece_israri", "\"Bir tek içip kalkarız\"",
                "Yorgunken arkadaş grubu dışarı çıkman için ısrar ediyor",
                ContextId.Arkadas, 2, 4, 3, new[] { "sosyal", "yorgunluk", "grup_baskisi" },
                "Cuma 21:00, arkadaş grubunun sohbeti",
                "Zor bir haftanın sonunda evde dinlenmek istiyorsun. Arkadaş grubu dışarıda ve seni çağırıyor; " +
                "\"hep sen eksiksin\" mesajları art arda geliyor.",
                "Evde kalma kararını savunmak, ileriye dönük bir plan önermek",
                "Yorgunluk yeterli bir gerekçedir; kanıtlamana gerek yok.",
                "Hadi ama ya herkes burada! Bir tek içip kalkarız, söz. Sen hep son anda ekiyorsun bizi, artık seni davet etmeyeceğiz 😤",
                Who("Mert", 30, "Arkadaş grubunun organizatörü", "Gruptaki en ısrarcı ve neşeli kişi", AgresifIsrarci),
                T(SosyalKanit, KarsiTeklif), T(SuclulukYukleme, Kiyaslama), T(PasifTehdit)),

            Make("arkadas_004_araba_odunc", "Arabanı ödünç isteyen arkadaş",
                "Arkadaşın hafta sonu arabanı ödünç almak istiyor",
                ContextId.Arkadas, 3, 5, 4, new[] { "esya", "araba", "guven" },
                "Perşembe akşamı, telefon",
                "Arkadaşın hafta sonu kamp için arabanı istiyor. Ehliyeti yeni ve arabanın sigortası yalnızca " +
                "seni kapsıyor. Geçen sefer ödünç verdiğin bisikleti bozuk getirmişti.",
                "Arabayı vermemek, alternatif önermek isteyip istemediğine sen karar ver",
                "Eşyanı korumak bencillik değil, sorumluluktur.",
                "Kanka hafta sonu arabanı verir misin? Kampa gideceğiz, ben çok dikkatli kullanırım, biliyorsun. Benzinini de full yaparım, tertemiz getiririm!",
                Who("Tolga", 26, "Mahalleden arkadaşın", "Eğlenceli ama biraz dikkatsiz", TatliDilliSatici),
                T(OvguVeGuven, KarsiTeklif), T(SahteAnlayis, SosyalKanit), T(SuclulukYukleme)),

            Make("arkadas_005_sir", "\"Bana söyleyebilirsin\"",
                "Bir arkadaşın, başka bir arkadaşının sırrını anlatman için baskı yapıyor",
                ContextId.Arkadas, 3, 4, 5, new[] { "sir", "dedikodu", "guven" },
                "Kahve molası, ikiniz başbaşasınız",
                "Ortak arkadaşınız Zeynep sana ayrılık sürecini anlattı ve kimseye söylememeni istedi. Başka bir " +
                "arkadaşın bir şeyler sezmiş, detayları öğrenmek için seni sıkıştırıyor.",
                "Sırrı korumak, merakı geri çevirmek",
                "\"Bu benim anlatacağım bir şey değil\" demek tam bir cümledir.",
                "Zeynep'le Barış ayrıldı değil mi? Yüzünden belli. Hadi bana söyleyebilirsin, ben kime söyleyeceğim? Zaten herkes biliyor gibi bir şey.",
                Who("Pelin", 28, "Arkadaş grubundan", "Samimisiniz ama dedikoduyu sever", PasifAgresif),
                T(SosyalKanit, SahteAnlayis), T(OvguVeGuven, Kiyaslama), T(KisKisSessizlik)),

            Make("arkadas_006_hesap_bolme", "Eşit bölünen pahalı hesap",
                "Az yemene rağmen hesabın eşit bölünmesi bekleniyor",
                ContextId.Arkadas, 2, 4, 6, new[] { "para", "restoran", "grup" },
                "Doğum günü yemeği, hesap masaya geldi",
                "Sekiz kişilik grupta herkes şarap ve ana yemek söyledi; sen sadece salata ve su aldın. Organizatör " +
                "hesabı eşit bölmeyi öneriyor: kişi başı 2.400 TL. Senin payın 400 TL civarı.",
                "Kendi payını ödemeyi önermek, tuhaf hissettirmeden",
                "Kendi payını ödemek cimrilik değil; adil olmaktır.",
                "Tamam arkadaşlar hesap geldi, kolaylık olsun eşit bölelim, kişi başı 2400. IBAN'ı gruba atıyorum, herkes şimdi göndersin lütfen 🙏",
                Who("Cansu", 31, "Doğum günü organizatörü", "Sosyal, hızlı ve pratik; detaylarla uğraşmayı sevmez", TatliDilliSatici),
                T(SosyalKanit, SahteZamanBaskisi), T(SahteAnlayis, Kiyaslama), T(SuclulukYukleme)),

            Make("arkadas_007_ev_arkadasi", "Ev arkadaşının misafiri",
                "Ev arkadaşının sevgilisi bir hafta daha kalmak istiyor",
                ContextId.Arkadas, 3, 5, 7, new[] { "ev_arkadasi", "ev", "faturalar" },
                "Salı akşamı, mutfak",
                "Ev arkadaşının sevgilisi iki haftadır neredeyse her gün evde. Faturalar arttı, banyo sırası " +
                "uzadı. Şimdi ev arkadaşın onun bir hafta daha kalmasını istiyor.",
                "Kalışı reddetmek ve ev kurallarını konuşmak",
                "Ortak alan ortak karar ister; rahatsızlığını söylemek hakkın.",
                "Bir şey soracağım... Ali'nin ev sahibi tadilat yapıyor, bir hafta daha bizde kalabilir mi? Zaten hep odadayız, fark etmiyorsun bile bence.",
                Who("Dilan", 25, "Ev arkadaşın", "Bir yıldır aynı evdesiniz, genelde iyi anlaşırsınız", MasumRicaci),
                T(SahteAnlayis, AcilDurumYaratma), T(DuyguSomurusu, KarsiTeklif), T(KisKisSessizlik)),

            Make("arkadas_008_ortak_is", "Birikimini ortak işe yatır",
                "Eski bir arkadaşın birikimini kafe ortaklığına yatırmanı istiyor",
                ContextId.Arkadas, 5, 7, 8, new[] { "para", "yatirim", "risk" },
                "Akşam yemeği, arkadaşının evinde",
                "Eski arkadaşın bir kafe açmak istiyor ve senden 300.000 TL ortaklık payı istiyor. İş planı yok, " +
                "daha önce iki girişimi batmış. Birikimin ev peşinatı için.",
                "Yatırımı reddetmek, arkadaşının hayaline saygı göstererek",
                "Birinin hayalini desteklemek, parasını vermek zorunda olmak demek değil.",
                "Sana çok heyecanlı bir şey anlatacağım! Kafe işi nihayet oluyor. Ortağım olur musun? 300 bin koyarsın, bir yılda katlarız. Bu fırsat bir daha gelmez, inan bana.",
                Who("Serkan", 33, "Eski iş arkadaşın", "Karizmatik, iyimser; fikirlerine çok inanır", BabacanManipulator),
                T(OvguVeGuven, SahteZamanBaskisi), T(SosyalKanit, KarsiTeklif), T(SuclulukYukleme, PasifTehdit)),

            Make("arkadas_009_dugun_yuku", "Düğün hazırlığının tamamı",
                "Arkadaşın düğün hazırlıklarının yükünü sana bırakıyor",
                ContextId.Arkadas, 3, 5, 9, new[] { "dugun", "yardim", "sinirlar" },
                "Düğüne iki ay kala, sesli mesaj",
                "Nedimesi/sağdıcı olmayı kabul ettin ama son haftalarda organizasyon, tedarikçi araması ve davetiye " +
                "dağıtımı da sana kaldı. Şimdi bekârlığa veda partisinin tüm planlamasını ve masrafını istiyor.",
                "Ek sorumluluğu reddetmek, rolüne net sınır çizmek",
                "\"Şu kadarını yapabilirim\" diye sınır çizmek dostluğu korur.",
                "Kankaa, bekarlığa veda partisini de sen organize eder misin? Mekan, süsleme, pasta... Masrafları da sonra hallederiz. Sen en iyi arkadaşımsın, başkasına emanet edemem 🥹",
                Who("Gizem", 29, "En yakın arkadaşın", "Çocukluk arkadaşın; heyecanlı, dağınık", DuygusalKurban),
                T(OvguVeGuven, DuyguSomurusu), T(SuclulukYukleme, AcilDurumYaratma), T(AsiriOzurAlan)),

            Make("arkadas_010_odev", "\"Ödevini atar mısın?\"",
                "Sınıf arkadaşın ödevini kopyalamak istiyor",
                ContextId.Arkadas, 2, 4, 10, new[] { "okul", "odev", "durustluk" },
                "Teslimden bir gece önce, mesaj",
                "Haftalardır üzerinde çalıştığın proje ödevini sınıf arkadaşın \"sadece fikir almak için\" istiyor. " +
                "Geçen dönem bir başkasının ödevinin kopyalandığı ortaya çıkmış ve ikisi de sıfır almıştı.",
                "Ödevi paylaşmamak, yardım edeceksen sınırını sen belirlemek",
                "Emeğini korumak, yardım etmeyi reddetmek değildir.",
                "Ödevini bir atar mısın? Kopyalamayacağım yemin ederim, sadece nasıl yaptığına bakacağım. Hoca zaten okumuyor ki. Herkes birbirine atıyor 😅",
                Who("Berk", 21, "Sınıf arkadaşın", "Derslerde yan yana oturursunuz", AgresifIsrarci),
                T(SosyalKanit, SahteZamanBaskisi), T(OvguVeGuven, SuclulukYukleme), T(PasifTehdit)),

            // ───────────────────────── Romantik İlişkiler ─────────────────────────
            Make("romantik_001_telefon_sifresi", "\"Şifreni ver o zaman\"",
                "Partnerin güven gerekçesiyle telefon şifreni istiyor",
                ContextId.Romantik, 4, 6, 1, new[] { "mahremiyet", "guven", "telefon" },
                "Akşam evde, film izlerken",
                "Telefonuna gelen bir bildirime takılan partnerin, \"saklayacak bir şeyin yoksa\" diyerek " +
                "telefon şifreni istiyor. Sana güvensizlik duygusunu daha önce de dile getirmişti.",
                "Mahremiyetini korumak, güven konusunu ayrıca konuşmayı önermek",
                "Güven şifreyle değil davranışla kurulur; bunu söyleyebilirsin.",
                "Kimdi o? Neden yüzünü çevirdin? Saklayacak bir şeyin yoksa şifreni ver o zaman. Ben sana her şeyimi gösteririm, sen niye göstermiyorsun?",
                Who("Partnerin (Deniz)", 28, "Bir buçuk yıllık partnerin", "Birbirinize bağlısınız ama Deniz kıskançlıkla boğuşuyor", PasifAgresif),
                T(Kiyaslama, SuclulukYukleme), T(DuyguSomurusu, AsiriOzurAlan), T(KisKisSessizlik, PasifTehdit)),

            Make("romantik_002_eski_sevgili", "Eski sevgilinin barışma ısrarı",
                "Ayrıldığın kişi sürekli mesajla geri dönmeni istiyor",
                ContextId.Romantik, 4, 6, 2, new[] { "ayrilik", "eski_sevgili", "net_olmak" },
                "Gece yarısı, uzun bir mesaj",
                "Üç ay önce düşünerek ayrıldın. Eski sevgilin haftalardır mesaj atıyor; bu gece de " +
                "\"bir şans daha\" diye uzun bir mesaj yazdı.",
                "Kararını net söylemek, kapıyı aralık bırakmamak",
                "Belirsiz bir cevap umut demektir; net ve nazik ol.",
                "Uyuyamıyorum. Seninle geçirdiğimiz her şeyi düşünüyorum. Değiştim, gerçekten. Sadece bir kahve, bir kere konuşalım. Bana bir şans borçlusun bence...",
                Who("Eski sevgilin (Arda)", 30, "Eski sevgilin", "İki yıl birlikte kaldınız; ayrılık senin kararındı", DuygusalKurban),
                T(DuyguSomurusu, KarsiTeklif), T(SuclulukYukleme, OvguVeGuven), T(KisKisSessizlik, AsiriOzurAlan)),

            Make("romantik_003_arkadas_gecesi", "Arkadaş gecesine surat",
                "Partnerin arkadaşlarınla buluşmana içerliyor",
                ContextId.Romantik, 3, 5, 3, new[] { "arkadaslar", "kiskanclik", "ozgurluk" },
                "Cuma akşamı, hazırlanırken",
                "Haftalardır planlanan kızlar/erkekler gecesine hazırlanıyorsun. Partnerin \"tamam git\" diyor ama " +
                "tonu soğuk, kısa cevaplar veriyor ve \"bu akşam yalnız kalırım artık\" diyor.",
                "Planına gitmek, partnerinin duygusunu sahiplenmeden",
                "Onun duygusu onun sorumluluğu; seninki ise planına sadık kalmak.",
                "Tamam git, eğlen. Ben de yalnız otururum artık. Zaten son zamanlarda arkadaşların hep benden önemli. Neyse, önemli değil.",
                Who("Partnerin (Ozan)", 29, "İki yıllık partnerin", "Sevgi dolu ama yalnız kalmaktan hoşlanmıyor", PasifAgresif),
                T(SahteAnlayis, SuclulukYukleme), T(DuyguSomurusu, Kiyaslama), T(KisKisSessizlik)),

            Make("romantik_004_birlikte_yasamak", "Üçüncü ayda aynı ev",
                "Partnerin henüz hazır değilken birlikte yaşamayı istiyor",
                ContextId.Romantik, 3, 5, 4, new[] { "iliski_hizi", "ev", "kararlar" },
                "Pazar kahvaltısı",
                "Üç aydır birliktesiniz ve ilişkiden memnunsun. Partnerin kirası bitince sana taşınmak istiyor. " +
                "Sen bu adım için henüz hazır değilsin.",
                "Taşınmayı şimdilik reddetmek, ilişkiye bağlılığını göstermek",
                "\"Şimdi değil\" demek \"seni istemiyorum\" demek değildir.",
                "Bir fikrim var! Kiram ay sonunda bitiyor, sana taşınsam? Zaten hep birlikteyiz, iki kira ödemek saçmalık. Herkes böyle yapıyor artık 🥰",
                Who("Partnerin (Ceren)", 27, "Üç aylık partnerin", "İlişki hızlı ilerledi; Ceren planları sever", MasumRicaci),
                T(OvguVeGuven, SosyalKanit), T(SahteZamanBaskisi, KarsiTeklif), T(DuyguSomurusu, SuclulukYukleme)),

            Make("romantik_005_ikinci_bulusma", "İkinci buluşma ısrarı",
                "İlgini çekmeyen biri ikinci buluşma için ısrar ediyor",
                ContextId.Romantik, 2, 4, 5, new[] { "flort", "net_olmak", "uygulama" },
                "Buluşmadan iki gün sonra, mesaj",
                "Uygulamada tanıştığın biriyle bir kahve içtin; kibar biriydi ama aranızda bir şey hissetmedin. " +
                "Şimdi ikinci buluşma için tarih soruyor ve cevap vermediğinde ard arda yazıyor.",
                "İlgilenmediğini kibarca ama net söylemek",
                "Uzun açıklama ısrara kapı açar; kısa ve net ol.",
                "Selam! Cumartesi boş musun? Geçen sefer çok güzeldi bence. Bu sefer yemeğe gidelim, ben ısmarlıyorum. Hayır deme ama 😄",
                Who("Kaan", 31, "Uygulamada tanıştığın kişi", "Bir kez buluştunuz", AgresifIsrarci),
                T(OvguVeGuven, KarsiTeklif), T(SuclulukYukleme, SahteAnlayis), T(PasifTehdit)),

            Make("romantik_006_masraflar", "Tüm masraflar senden",
                "Partnerin harcamaların hep senden çıkmasını bekliyor",
                ContextId.Romantik, 3, 5, 6, new[] { "para", "adalet", "iliski" },
                "Ay sonu, kartının ekstresine bakarken",
                "Son aylarda yemek, tatil ve hediyelerin neredeyse tamamını sen ödüyorsun. Partnerin gelir sahibi " +
                "ama yeni bir hafta sonu kaçamağının parasını da senden bekliyor.",
                "Masrafları adil paylaşmayı konuşmak",
                "Para konuşmak romantizmi öldürmez; içten içe biriken kırgınlık öldürür.",
                "Hafta sonu için Kapadokya'da harika bir otel buldum! Balon turu da var 😍 Sen rezervasyonu yapar mısın, benim kart limitim doldu bu ay.",
                Who("Partnerin (Melis)", 28, "Bir yıllık partnerin", "Eğlenceli, plan yapan taraf; para konusunu açmaz", DuygusalKurban),
                T(OvguVeGuven, SahteZamanBaskisi), T(DuyguSomurusu, Kiyaslama), T(SuclulukYukleme, KisKisSessizlik)),

            Make("romantik_007_aile_tanisma", "\"Bu hafta sonu ailemle tanış\"",
                "Partnerin hazır olmadığın bir aile tanışmasında ısrar ediyor",
                ContextId.Romantik, 2, 4, 7, new[] { "aile", "iliski_hizi", "hazir_olmak" },
                "Çarşamba akşamı, telefon",
                "Partnerin bu pazar ailesinin büyük yemeğine gelmeni istiyor. İlişki dört aylık ve sen bu adımı " +
                "biraz daha ileride atmak istiyorsun. Partnerin ailesine geleceğini çoktan söylemiş.",
                "Davete şimdilik hayır demek, tanışma için ileri bir zaman önermek",
                "Başkasının verdiği söz senin sözün değildir.",
                "Pazar annemlerin evinde yemek var, sen de geliyorsun! Anneme söyledim bile, çok heyecanlı. Gelmezsen bana çok kızar, beni zor durumda bırakma ne olur.",
                Who("Partnerin (Yiğit)", 30, "Dört aylık partnerin", "Aile odaklı ve heyecanlı", AgresifIsrarci),
                T(SosyalKanit, SahteZamanBaskisi), T(SuclulukYukleme, DuyguSomurusu), T(PasifTehdit)),

            Make("romantik_008_takip_listesi", "\"Onu takipten çıkar\"",
                "Partnerin sosyal medyada kimi takip edeceğine karışıyor",
                ContextId.Romantik, 4, 5, 8, new[] { "kiskanclik", "sosyal_medya", "kontrol" },
                "Pazar öğleden sonra, kanepede",
                "Partnerin Instagram'da takip ettiğin birkaç kişiyi gösterip onları takipten çıkarmanı istiyor; " +
                "aralarında yıllardır arkadaşın olan biri de var.",
                "Kontrol talebini reddetmek, kıskançlığı konuşmaya açık olmak",
                "Duygusunu dinleyebilirsin; davranışını değiştirmek zorunda değilsin.",
                "Bu kim? Neden takip ediyorsun? Beni seviyorsan bunları takipten çıkarırsın, zor bir şey değil ki. Ben senin için yapardım.",
                Who("Partnerin (Ege)", 27, "Bir yıllık partnerin", "Kıskanç; güvensizliğini kontrolle yönetmeye çalışıyor", Otoriter),
                T(Kiyaslama, SuclulukYukleme), T(DuyguSomurusu, Otorite), T(PasifTehdit, KisKisSessizlik)),

            Make("romantik_009_arkadas_tatili", "Arkadaşlarla tatile itiraz",
                "Partnerin, aylar önce planladığın arkadaş tatiline gitmemeni istiyor",
                ContextId.Romantik, 3, 5, 9, new[] { "tatil", "arkadaslar", "ozgurluk" },
                "Tatile bir hafta kala, akşam yemeği",
                "Arkadaşlarınla dört günlük tatil aylar önce planlandı, biletler alındı. Partnerin şimdi bu tatilin " +
                "\"ilişkiye saygısızlık\" olduğunu söylüyor ve iptal etmeni istiyor.",
                "Tatile gitme kararını korumak, partnerinin endişesini dinleyerek",
                "Önceden konuşulmuş bir planı son anda tartışmaya açmak senin hatan değil.",
                "Gerçekten gidecek misin yani? Ben olsam senin için iptal ederdim. Bu ilişkiye saygısızlık bence. Sen gidersen ben de ne yapacağımı bilmiyorum.",
                Who("Partnerin (Bora)", 31, "İki yıllık partnerin", "Bağlı ama ayrı kalınca kaygılanıyor", PasifAgresif),
                T(Kiyaslama, SuclulukYukleme), T(DuyguSomurusu, KarsiTeklif), T(PasifTehdit)),

            Make("romantik_010_ozur_talebi", "Bitmeyen özür talebi",
                "Küçük bir anlaşmazlık sonrası partnerin abartılı özür bekliyor",
                ContextId.Romantik, 4, 6, 10, new[] { "ozur", "tartisma", "denge" },
                "Tartışmanın ertesi sabahı",
                "Dün akşam yemeğe 20 dakika geç kaldın ve haber verdin, özür diledin. Partnerin hâlâ küs; " +
                "\"gerçekten pişman olsan böyle davranmazdın\" diyerek sürekli yeni özürler bekliyor.",
                "Bir kez özür dilediğini kabul edip daha fazlasını reddetmek",
                "Gerçek bir özür bir kez yeter; ölçüyü kaybetmemek de sağlıklı ilişkinin parçası.",
                "Günaydın demekle her şey düzelmiyor. Dün beni nasıl beklettiğinin farkında değilsin bence. Gerçekten pişman olsan bunu telafi etmek için bir şeyler yapardın.",
                Who("Partnerin (Su)", 28, "İki yıllık partnerin", "Duygusal tepkileri güçlü; kırgınlığı uzun sürer", DuygusalKurban),
                T(AsiriOzurAlan, SuclulukYukleme), T(DuyguSomurusu, KisKisSessizlik), T(AsiriOzurAlan, PasifTehdit)),

            // ───────────────────────── Satıcı & Pazarlama ─────────────────────────
            Make("satici_001_operator", "Operatörden \"size özel\" paket",
                "Çağrı merkezi temsilcisi paket yükseltmesi dayatıyor",
                ContextId.Satici, 1, 3, 1, new[] { "telefon", "kampanya", "abonelik" },
                "Salı öğleden sonra, telefon araması",
                "Mobil operatörünün temsilcisi arıyor ve \"sadece size özel\" daha pahalı bir pakete geçmeni " +
                "istiyor. Mevcut paketin ihtiyacını karşılıyor.",
                "Teklifi reddetmek ve aramayı kısa tutmak",
                "Satıcıya gerekçe borçlu değilsin; \"ilgilenmiyorum, teşekkürler\" yeterli.",
                "Merhabalar, size özel bir müjdemiz var! Sadece bugüne özel, 40 GB ek internet ve sınırsız konuşma, aylık sadece 99 TL farkla. Hemen tanımlayayım mı?",
                Who("Temsilci Gökhan", 26, "Operatör çağrı merkezi temsilcisi", "Tanımadığın biri; satış hedefi var", TatliDilliSatici),
                T(SahteZamanBaskisi, OvguVeGuven), T(SosyalKanit, KarsiTeklif), T(SahteAnlayis)),

            Make("satici_002_galeri_paket", "Galeride ek paket satışı",
                "Araba satın alırken pahalı ek garanti ve paketler dayatılıyor",
                ContextId.Satici, 3, 5, 2, new[] { "araba", "ek_paket", "pazarlik" },
                "Galeride, sözleşme masasında",
                "Kararlaştırdığın fiyatla arabayı almaya geldin. Satış danışmanı sözleşmeyi hazırlarken 35.000 TL'lik " +
                "uzatılmış garanti, kaplama ve bakım paketini \"herkes alıyor\" diyerek ekliyor.",
                "Ek paketleri reddetmek, anlaşılan fiyata sadık kalmak",
                "Konuşulan fiyatı yazılı olarak teyit et; eklenen her kalem ayrı bir karardır.",
                "Sözleşmeniz hazır! Ben size uzatılmış garanti, seramik kaplama ve 3 yıllık bakım paketini de ekledim. Müşterilerimizin %90'ı alıyor, ileride çok rahat edersiniz.",
                Who("Hakan Bey", 42, "Galeri satış danışmanı", "Bugün tanıştınız", AgresifIsrarci),
                T(SosyalKanit, Otorite), T(AcilDurumYaratma, KarsiTeklif), T(PasifTehdit)),

            Make("satici_003_spor_salonu", "Üyelik iptali",
                "Spor salonu üyeliğini iptal etmek istiyorsun, temsilci ikna etmeye çalışıyor",
                ContextId.Satici, 2, 4, 3, new[] { "abonelik", "iptal", "spor" },
                "Spor salonunun resepsiyonu",
                "Taşındığın için spor salonu üyeliğini iptal etmeye geldin. Üyelik sorumlusu seni bir masaya " +
                "oturtuyor ve iptal yerine indirim, dondurma ve kişisel antrenör teklifleri sıralıyor.",
                "İptali tamamlamak, tekliflere kapılmamak",
                "Amacını baştan net söyle ve aynı cümleyi tekrar etmekten çekinme.",
                "Ooo hoş geldiniz! İptal mi? Ama hedeflerinize çok yaklaşmıştınız. Size özel 3 ay %50 indirim yapabilirim, bir de ücretsiz PT seansı. Vazgeçmeyin bence!",
                Who("Selim", 29, "Üyelik sorumlusu", "Salonda birkaç kez selamlaştınız", TatliDilliSatici),
                T(OvguVeGuven, KarsiTeklif), T(SahteAnlayis, SuclulukYukleme), T(SahteZamanBaskisi)),

            Make("satici_004_kapi_satisi", "Kapıdaki su arıtma satıcısı",
                "Kapına gelen satıcı \"ücretsiz test\" bahanesiyle içeri girmek istiyor",
                ContextId.Satici, 2, 3, 4, new[] { "kapi", "guvenlik", "israr" },
                "Cumartesi sabahı, kapı zili",
                "Kapıda yaka kartlı biri \"belediyeyle ortak su kalitesi testi\" yaptığını söylüyor ve içeri girip " +
                "musluk suyunu test etmek istiyor. Aslında su arıtma cihazı satıyor.",
                "İçeri almamak ve konuşmayı kısa tutmak",
                "Kapını açmak zorunda değilsin; güvenliğin nezaketten önce gelir.",
                "Günaydın efendim! Mahallede ücretsiz su kalitesi testi yapıyoruz, belediyeyle ortak çalışıyoruz. Komşularınızın hepsi yaptırdı, sadece 5 dakika, içeri girip musluğunuza bakacağım.",
                Who("Satıcı Volkan", 35, "Kapıdan kapıya satıcı", "Tanımadığın biri", AgresifIsrarci),
                T(Otorite, SosyalKanit), T(AcilDurumYaratma, SahteAnlayis), T(PasifTehdit)),

            Make("satici_005_magaza_ceket", "\"Bu ceket tam size göre\"",
                "Mağazada satıcı denediğin pahalı ceketi almaya ikna etmeye çalışıyor",
                ContextId.Satici, 1, 3, 5, new[] { "magaza", "alisveris", "iltifat" },
                "AVM'de bir giyim mağazası, kabin önü",
                "Merak edip denediğin ceket bütçenin çok üstünde. Satış danışmanı iltifatlar yağdırıyor ve " +
                "\"son beden\" olduğunu söylüyor.",
                "Ceketi almadan, rahatça mağazadan çıkmak",
                "İltifata teşekkür edip kararını değiştirmeyebilirsin.",
                "Vay, bu ceket sanki sizin için dikilmiş! Rengi de çok yakıştı. Bu son beden bu arada, akşama kalmaz. Kasaya götüreyim mi?",
                Who("Satış danışmanı Aslı", 24, "Mağaza satış danışmanı", "Bugün tanıştınız", TatliDilliSatici),
                T(OvguVeGuven, SahteZamanBaskisi), T(SosyalKanit, KarsiTeklif), T(SahteAnlayis)),

            Make("satici_006_banka_sigorta", "Krediye eklenen sigorta",
                "Banka kredi verirken istemediğin sigortayı zorunlu gibi sunuyor",
                ContextId.Satici, 3, 5, 6, new[] { "banka", "kredi", "sigorta" },
                "Banka şubesi, müşteri temsilcisinin masası",
                "İhtiyaç kredisi için şubedesin. Müşteri temsilcisi evrakları hazırlarken hayat sigortası ve kart " +
                "aidatlı bir paketi \"kredinin onayı için gerekli\" diyerek ekliyor.",
                "Zorunlu olmayan ürünleri reddetmek, krediyi yalnız almak",
                "\"Zorunlu olduğunu yazılı gösterebilir misiniz?\" diye sormak hakkın.",
                "Krediniz onaylandı, tebrikler! Yalnız onay için hayat sigortası ve Platin kart paketini de tanımlıyorum, sistem böyle istiyor. Şuraya imzanızı alayım.",
                Who("Müşteri temsilcisi Nilay", 33, "Banka müşteri temsilcisi", "Bankanın müşterisisin, ilk kez görüşüyorsunuz", Otoriter),
                T(Otorite, SahteZamanBaskisi), T(SosyalKanit, AcilDurumYaratma), T(PasifTehdit)),

            Make("satici_007_emlakci_kapora", "\"Hemen kapora verin\"",
                "Emlakçı evi kaçırmamak için hemen kapora istiyor",
                ContextId.Satici, 4, 5, 7, new[] { "emlak", "kapora", "aceleye_getirme" },
                "Kiralık daire gezisi, salon",
                "Gezdiğin daire güzel ama tapuyu, aidatı ve sözleşmeyi görmedin. Emlakçı \"üç kişi daha bakacak\" " +
                "diyerek şimdi 20.000 TL kapora istiyor.",
                "Belgeleri görmeden kapora vermemek",
                "Acele ettiren her teklif bir duraklama sebebidir.",
                "Bakın açık konuşayım, bu daire bugün gider. Öğleden sonra üç aile daha gelecek. Şimdi 20 bin kapora verin, daireyi sizin adınıza tutayım. Yoksa ben bir şey yapamam.",
                Who("Emlakçı Recep Bey", 50, "Emlak danışmanı", "Bugün tanıştınız", AgresifIsrarci),
                T(SahteZamanBaskisi, SosyalKanit), T(AcilDurumYaratma, OvguVeGuven), T(PasifTehdit)),

            Make("satici_008_network_marketing", "Eski arkadaştan \"iş fırsatı\"",
                "Eski bir okul arkadaşın seni çok katlı pazarlamaya katmaya çalışıyor",
                ContextId.Satici, 4, 6, 8, new[] { "network_marketing", "arkadas", "para" },
                "Yıllar sonra buluştuğunuz bir kafe",
                "Yıllardır görüşmediğin okul arkadaşın \"kahve içelim\" dedi. Muhabbet kısa sürede bir \"iş fırsatına\" " +
                "dönüyor: başlangıç paketi 25.000 TL ve altına yeni kişiler katman gerekiyor.",
                "Teklifi reddetmek, buluşmayı tatlıya bağlamak",
                "Arkadaşlığı ve satışı ayırmak senin hakkın.",
                "Seni görünce aklıma ilk sen geldin, sen hep girişimciydin! Benim ekipte yer var. Başlangıç paketi sadece 25 bin, 3 ayda çıkarıyorsun. Bak ben geçen ay araba aldım.",
                Who("Gamze", 30, "Eski okul arkadaşın", "Okulda yakındınız, sonra yollar ayrıldı", TatliDilliSatici),
                T(OvguVeGuven, SosyalKanit), T(Kiyaslama, SahteZamanBaskisi), T(SuclulukYukleme, DuyguSomurusu)),

            Make("satici_009_tamirci", "Gereksiz ek onarım",
                "Servis, sadece bakım için getirdiğin araca pahalı ek işlem öneriyor",
                ContextId.Satici, 3, 4, 9, new[] { "servis", "tamir", "uzmanlik" },
                "Oto servis, kabul masası",
                "Arabanı yalnızca periyodik bakım için getirdin. Servis danışmanı fren disklerinin ve debriyajın " +
                "\"acilen\" değişmesi gerektiğini söylüyor; 6 ay önce başka serviste kontrol ettirmiştin ve sorun yoktu.",
                "Ek işlemi şimdilik reddetmek, sadece bakımı yaptırmak",
                "İkinci görüş istemek, uzmanlığa saygısızlık değildir.",
                "Bakımı yaptık ama fren diskleri ve debriyaj bitmiş, bu şekilde kullanmanızı önermem. Toplam 28 bin tutuyor, bugün yaparsak yarın teslim ederiz. Güvenliğiniz için söylüyorum.",
                Who("Servis danışmanı Erkan", 44, "Oto servis danışmanı", "Bu servise ilk kez geldin", Otoriter),
                T(Otorite, AcilDurumYaratma), T(SahteAnlayis, SosyalKanit), T(PasifTehdit)),

            Make("satici_010_bagis_standi", "AVM'deki bağış standı",
                "Dernek görevlisi düzenli aylık bağış talimatı imzalatmak istiyor",
                ContextId.Satici, 2, 3, 10, new[] { "bagis", "duygusal_baski", "avm" },
                "AVM koridoru, bir dernek standı",
                "Bir dernek görevlisi seni durduruyor ve kredi kartınla aylık 300 TL'lik düzenli bağış talimatı " +
                "imzalamanı istiyor. Kime, nasıl bağış yapacağına kendin karar vermek istiyorsun.",
                "Talimatı imzalamamak, suçluluk hissetmeden yoluna devam etmek",
                "Bağış yapıp yapmamak ve nasıl yapacağın senin kararın.",
                "Merhaba, bir dakikanızı alabilir miyim? Günde sadece 10 lira, bir çocuğun eğitimini değiştiriyor. Bugün buradan geçen pek çok kişi destek oldu, sizin gibi duyarlı birinin de olacağına eminim.",
                Who("Dernek görevlisi Elif", 23, "Sokakta bağış toplayan gönüllü", "Tanımadığın biri", DuygusalKurban),
                T(OvguVeGuven, SosyalKanit), T(DuyguSomurusu, SuclulukYukleme), T(KarsiTeklif))
        };
    }
}
