using CanSayNo.Domain.Entities.Hayirci;
using CanSayNo.Domain.Enums;
using static CanSayNo.Domain.Enums.CharacterArchetype;
using static CanSayNo.Domain.Enums.PressureTactic;

namespace CanSayNo.Infrastructure.Persistence.Seeding
{
    /// <summary>
    /// 2026-11-07'de eklenen bağlamlar: Ev &amp; Komşu, Okul &amp; Kampüs, Dijital &amp; Sosyal Medya.
    /// Her biri premium, ilk aşamada 5'er senaryo. Yazım kuralları Catalog dosyasıyla aynı.
    /// </summary>
    public static partial class HayirciSeedData
    {
        private static List<Scenario> BuildNewContextScenarios() => new()
        {
            // ───────────────────────── Ev & Komşu (premium) ─────────────────────────
            Make("komsu_001_anahtar_birak", "\"Anahtarı sende bırakayım\"",
                "Komşu tatildeyken her gün çiçek sulamanı ve kediye bakmanı istiyor",
                ContextId.Komsu, 1, 3, 1, new[] { "komsu", "iyilik", "sorumluluk" },
                "Cumartesi sabahı, apartman merdiveninde",
                "Üst kattaki komşun Nermin Hanım iki haftalığına memleketine gidiyor. Anahtarını sana bırakıp her gün " +
                "çiçeklerini sulamanı ve kedisine mama vermeni istiyor. Senin de o hafta yoğun bir iş temposun var.",
                "Her gün bakım sorumluluğunu reddetmek; istersen daha küçük bir yardım önermek",
                "Kısa ve sıcak bir hayır yeterli; yardım edebileceğin küçük bir şey varsa onu sen belirle.",
                "Canım günaydın! Ben yarın memlekete gidiyorum, iki hafta kalacağım. Anahtarı sana bıraksam, şu çiçekleri bir sulasan, Pamuk'a da mama koysan? Sana güvenim tam, başka kimseye veremem.",
                Who("Nermin Hanım", 63, "Üst kat komşun", "Yıllardır aynı apartmandasınız, bayramlarda tatlı getirir", MasumRicaci),
                T(OvguVeGuven), T(DuyguSomurusu, SosyalKanit), T(KarsiTeklif)),

            Make("komsu_002_ev_arkadasi_sevgili", "Ev arkadaşının sevgilisi taşınıyor",
                "Ev arkadaşı sevgilisinin \"birkaç haftalığına\" evde kalmasını istiyor",
                ContextId.Komsu, 3, 5, 2, new[] { "ev_arkadasi", "ortak_alan", "kira" },
                "Pazar akşamı, ortak salonda",
                "İki kişi kiraladığınız evde ev arkadaşın Ece, sevgilisi Mert'in birkaç hafta sizde kalmasını istiyor. " +
                "Mert zaten haftanın yarısı orada; banyo sırası, faturalar ve sessizlik şimdiden sorun olmaya başladı.",
                "Mert'in taşınmasına hayır demek ve ortak yaşam kuralını netleştirmek",
                "Kişiye değil düzene odaklan: \"Mert'le ilgili değil, bu evi iki kişi için tuttuk.\"",
                "Şey, sana bir şey soracaktım… Mert'in ev sahibi evi satıyormuş, yeni ev bulana kadar birkaç hafta bizde kalsa? Zaten çoğu gün burada, fark etmezsin bile.",
                Who("Ece", 25, "Ev arkadaşın", "Bir yıldır birlikte yaşıyorsunuz, aranız iyi", MasumRicaci),
                T(SahteAnlayis), T(DuyguSomurusu, AcilDurumYaratma), T(SuclulukYukleme, KarsiTeklif)),

            Make("komsu_003_yonetici_aidat", "Apartman yöneticisinin ek toplaması",
                "Yönetici, karar alınmadan yapılan bir masraf için senden para istiyor",
                ContextId.Komsu, 3, 4, 3, new[] { "apartman", "aidat", "para" },
                "Akşam, kapı zili çalıyor",
                "Apartman yöneticisi Erdal Bey kapıda. Toplantıda konuşulmadan bahçeye yeni oturma grubu almış ve daire " +
                "başına 2.500 TL istiyor. Bu masraf için ne toplantı yapıldı ne de yazılı bir karar var.",
                "Kararsız yapılan masrafı ödemeyi şimdilik reddetmek ve toplantı istemek",
                "Ödemeyi reddetmek kavga değildir; \"Toplantıda karar alınırsa öderim\" demek yeterli.",
                "İyi akşamlar komşu. Bahçeye oturma grubu aldık, çok güzel oldu, gördün mü? Daire başı 2.500 TL düşüyor, herkes verdi, bir sen kaldın. IBAN'ı göndereyim mi?",
                Who("Erdal Bey", 58, "Apartman yöneticisi", "Emekli; apartman işlerini kendi işi gibi görür", Otoriter),
                T(SosyalKanit), T(Otorite, Kiyaslama), T(PasifTehdit)),

            Make("komsu_004_gurultu_sikayeti", "Gürültü yapan komşuya geri adım atmamak",
                "Gece gürültüsünü konuştuğun komşu suçu sana çeviriyor",
                ContextId.Komsu, 4, 5, 4, new[] { "gurultu", "sinir", "karsi_suclama" },
                "Gece 00:40, alt kattaki komşunun kapısında",
                "Alt kattaki komşun Kaan haftada birkaç gece yüksek sesle müzik açıyor. Kibarca rica etmek için kapısını " +
                "çaldın. O ise \"herkes eğleniyor\" diyerek seni abartmakla suçluyor ve \"bari hafta sonları ses etme\" diyor.",
                "Gece sessizliği isteğinden vazgeçmemek, suçlamaya kapılmamak",
                "İsteğini tekrar etmek yeterli; tartışmayı kazanman gerekmiyor.",
                "Abi hayırdır, gecenin bu saatinde? Ha müzik mi? Ya ses o kadar yüksek değil ki, sen biraz hassassın galiba. Diğer komşular hiç şikâyet etmiyor.",
                Who("Kaan", 29, "Alt kat komşun", "Selamlaşırsınız, başka bir ilişkiniz yok", PasifAgresif),
                T(SosyalKanit, AsiriOzurAlan), T(Kiyaslama, KisKisSessizlik), T(KarsiTeklif)),

            Make("komsu_005_ev_sahibi_zam", "Ev sahibinin sözleşme dışı zammı",
                "Ev sahibi sözleşme bitmeden kirayı artırmak istiyor",
                ContextId.Komsu, 4, 5, 5, new[] { "ev_sahibi", "kira", "sozlesme" },
                "Hafta içi akşam, telefon araması",
                "Ev sahibin Hüseyin Bey arıyor. Sözleşmenin bitmesine yedi ay var ama kirayı bu aydan itibaren %40 " +
                "artırmak istiyor. Sözleşmede yıl içinde artış maddesi yok.",
                "Sözleşme dışı artışı reddetmek, yenileme döneminde konuşmayı önermek",
                "Sözleşmeye dayanmak kabalık değildir; sakin bir dille yazılı olanı hatırlat.",
                "Merhaba evladım, nasılsın? Bak açık konuşayım, her şey çok pahalandı. Bu aydan itibaren kirayı biraz artırmamız lazım, %40 kadar. Sen anlayışlı birisin, biliyorum.",
                Who("Hüseyin Bey", 66, "Ev sahibin", "Üç yıldır kiracısısın, ilişkiniz şimdiye kadar iyiydi", BabacanManipulator),
                T(OvguVeGuven, SahteAnlayis), T(DuyguSomurusu, SosyalKanit), T(PasifTehdit)),

            // ───────────────────────── Okul & Kampüs (premium) ─────────────────────────
            Make("okul_001_grup_odevi", "Grup ödevinde herkesin işi sende",
                "Grup arkadaşları kendi bölümlerini de sana bırakmak istiyor",
                ContextId.Okul, 2, 4, 1, new[] { "grup_odevi", "is_bolumu", "universite" },
                "Teslime üç gün kala, grup WhatsApp'ında",
                "Dört kişilik grup ödevinde bölümler paylaşılmıştı. Grup arkadaşın Berk, kendisinin ve Selin'in " +
                "bölümünü de senin yazmanı istiyor; \"sen zaten konuyu en iyi bilen kişisin\" diyor.",
                "Başkalarının bölümünü üstlenmeyi reddetmek, iş bölümüne sadık kalmak",
                "Kendi bölümünü bitireceğini söyle; başkasının işi onun sorumluluğu.",
                "Kanka selam, şimdi şöyle bi durum var, Selin hasta, ben de bu hafta stajdayım. Sen zaten konuyu en iyi bilen kişisin, bizim kısımları da yazıversen? Sunumu biz yaparız söz.",
                Who("Berk", 21, "Grup arkadaşın", "Aynı bölümdesiniz, derslerde selamlaşırsınız", MasumRicaci),
                T(OvguVeGuven, KarsiTeklif), T(AcilDurumYaratma, SosyalKanit), T(SuclulukYukleme)),

            Make("okul_002_odev_kopya", "\"Ödevini atar mısın, değiştiririm\"",
                "Sınıf arkadaşı ödevini kopyalamak için istiyor",
                ContextId.Okul, 2, 3, 2, new[] { "kopya", "akademik_durustluk", "arkadas" },
                "Gece yarısı, teslimden önceki akşam",
                "Sınıf arkadaşın İrem ödevini yetiştirememiş. Senin ödevini istiyor, \"cümleleri değiştiririm, kimse " +
                "anlamaz\" diyor. Yakalanırsanız ikinizin de notu sıfırlanabilir.",
                "Ödevi paylaşmayı reddetmek; istersen başka bir şekilde destek olmak",
                "Hayır demek arkadaşlığı bitirmez; açıklamanı kısa tut ve alternatifini sen seç.",
                "Yaa çok kötü durumdayım, ödevi hiç yetiştiremedim. Seninkini atar mısın? Söz cümleleri tamamen değiştiririm, kimse anlamaz. Bir kere bana yardım et, lütfen.",
                Who("İrem", 20, "Sınıf arkadaşın", "Derslerde yan yana oturuyorsunuz", DuygusalKurban),
                T(DuyguSomurusu), T(SahteAnlayis, SosyalKanit), T(SuclulukYukleme, KisKisSessizlik)),

            Make("okul_003_hoca_ucretsiz_is", "Hocanın \"gönüllü\" proje isteği",
                "Hoca, not karşılığı olmayan bir iş için hafta sonlarını istiyor",
                ContextId.Okul, 4, 5, 3, new[] { "hoca", "gonullu", "zaman" },
                "Ders çıkışı, hocanın odası",
                "Danışman hocan Prof. Dr. Ayten Yılmaz, kendi projesi için anket verilerini girmeni istiyor. " +
                "Haftada 10 saat sürecek, ücret ya da kredi yok ve senin de final haftan yaklaşıyor.",
                "Gönüllü işi bu dönem reddetmek, ilişkiyi koruyarak",
                "Otoriteye karşı da sınır koyabilirsin; saygılı ama net ol, gerekçeyi uzatma.",
                "Gel bakalım, otur. Sana güzel bir fırsat var: projeme veri girişi için birine ihtiyacım var, aklıma ilk sen geldin. Haftada birkaç saat, CV'ne de çok iyi yazar. Pazartesi başlarsın.",
                Who("Prof. Dr. Ayten Yılmaz", 54, "Danışman hocan", "Bitirme projende sana danışmanlık yapıyor", Otoriter),
                T(OvguVeGuven, Otorite), T(Kiyaslama, SahteZamanBaskisi), T(PasifTehdit)),

            Make("okul_004_kulup_baskanligi", "Kulüp başkanlığı teklifi",
                "Kulüp üyeleri istemediğin başkanlığı sana yüklemeye çalışıyor",
                ContextId.Okul, 3, 4, 4, new[] { "kulup", "sorumluluk", "akran_baskisi" },
                "Kulüp toplantısı sonrası, kantinde",
                "Fotoğraf kulübünün dönem sonunda başkanı mezun oluyor. Kimse aday olmayınca herkes sana dönmüş durumda. " +
                "Sen bu dönem yarı zamanlı çalışıyorsun ve başkanlık haftada en az 8 saat demek.",
                "Başkanlığı reddetmek; istersen daha küçük bir rol önermek",
                "Hayır, kulübü sevmediğin anlamına gelmez; ne kadarını yapabileceğini sen söyle.",
                "Tamam karar verdik, yeni başkan sensin! Herkes seni istiyor zaten. Yoksa kulüp kapanır, biliyorsun. Sen olmasan bu kulüp çoktan dağılırdı.",
                Who("Deniz", 22, "Kulübün mevcut başkanı", "Kulübe seni o getirdi", BabacanManipulator),
                T(OvguVeGuven, SosyalKanit), T(AcilDurumYaratma, DuyguSomurusu), T(KarsiTeklif)),

            Make("okul_005_ders_notu_israr", "Ders notlarını sürekli isteyen arkadaş",
                "Derslere gelmeyen arkadaş her hafta notlarının fotoğrafını istiyor",
                ContextId.Okul, 1, 3, 5, new[] { "ders_notu", "emek", "tekrar_eden_istek" },
                "Hafta sonu, Instagram mesajı",
                "Derslere neredeyse hiç gelmeyen Onur her hafta senden notlarının fotoğraflarını istiyor. Notları " +
                "temize çekmek senin saatlerini alıyor ve o hiç teşekkür bile etmiyor.",
                "Notları her hafta göndermeyi bırakmak",
                "Bir kalıbı bozmak için açıklama değil, net bir \"artık göndermeyeceğim\" yeter.",
                "Selam hocam 😄 bu haftanın notlarını atar mısın? Bu hafta da gelemedim, iş çıktı. Bi de geçen haftakinin 3. sayfası eksikti onu da at.",
                Who("Onur", 21, "Sınıf arkadaşın", "Sadece sınav dönemlerinde yazar", AgresifIsrarci),
                T(SahteAnlayis), T(Kiyaslama, SosyalKanit), T(KisKisSessizlik)),

            // ───────────────────────── Dijital & Sosyal Medya (premium) ─────────────────────────
            Make("dijital_001_grup_davet", "Aile WhatsApp grubu her an cevap bekliyor",
                "Gruptan hemen cevap vermediğin için sitem ediliyor",
                ContextId.Dijital, 2, 3, 1, new[] { "whatsapp", "ulasilabilirlik", "aile_grubu" },
                "Mesai saati, telefonuna bir sürü bildirim düşüyor",
                "Geniş aile WhatsApp grubunda teyzen Gülay her mesaja hemen cevap bekliyor. Toplantıdayken cevap " +
                "vermediğin için gruba \"bazıları bizi okuyup geçiyor\" yazdı ve sana özelden de yazıyor.",
                "Anında cevap verme beklentisini reddetmek, kendi zamanını belirlemek",
                "Ne zaman ulaşılabilir olduğunu söylemek yeterli; her bildirime açıklama borçlu değilsin.",
                "Görüldü atıp cevap vermiyorsun, ayıp oluyor ama. Ben gruba kuzeninin düğün fotoğraflarını attım, herkes yorum yaptı bir sen yoksun. Bu kadar mı meşgulsün?",
                Who("Gülay Teyze", 57, "Teyzen", "Aile toplantılarının organizatörü", PasifAgresif),
                T(SuclulukYukleme), T(Kiyaslama, SosyalKanit), T(KisKisSessizlik, AsiriOzurAlan)),

            Make("dijital_002_story_paylas", "\"Story'de paylaşır mısın?\"",
                "Arkadaşı işletmesinin reklamını hesabında paylaşmanı istiyor",
                ContextId.Dijital, 1, 3, 2, new[] { "instagram", "reklam", "hesap" },
                "Akşam, Instagram DM'i",
                "Arkadaşın Melis yeni açtığı takı markasının gönderisini kendi hesabında paylaşmanı ve arkadaşlarını " +
                "etiketlemeni istiyor. Hesabını kişisel tutuyorsun ve reklam paylaşmak istemiyorsun.",
                "Paylaşımı reddetmek; istersen başka bir şekilde destek olmak",
                "Hesabın senin alanın; destek olmanın tek yolu paylaşmak değil.",
                "Canımmm 💕 markamın ilk koleksiyonu çıktı! Bir story'de paylaşıp 3-4 arkadaşını etiketlesen çok sevinirim, herkes paylaşıyor, bir sen kaldın 🙈",
                Who("Melis", 27, "Üniversiteden arkadaşın", "Ara sıra buluşursunuz", TatliDilliSatici),
                T(OvguVeGuven, SosyalKanit), T(DuyguSomurusu), T(KarsiTeklif)),

            Make("dijital_003_sifre_paylasimi", "Sevgilinin telefon şifresi isteği",
                "Partner \"saklayacak bir şeyin yoksa\" diyerek telefon şifreni istiyor",
                ContextId.Dijital, 4, 5, 3, new[] { "mahremiyet", "sifre", "guven" },
                "Akşam yemeğinden sonra, evde",
                "Altı aydır birliktesin. Partnerin Emre, \"aramızda sır olmasın\" diyerek telefon ve Instagram " +
                "şifrelerini istiyor. Saklayacak bir şeyin yok ama mahremiyetini önemsiyorsun.",
                "Şifre paylaşmayı reddetmek ve güvenin bununla ölçülmediğini söylemek",
                "Mahremiyet sır saklamak değildir; sakin ve tekrar eden bir \"hayır\" yeter.",
                "Bir şey soracağım, kızmayacaksın ama. Telefonunun şifresini versene? Ben de sana veririm. Aramızda sır olmasın istiyorum, saklayacak bir şeyin yoksa sorun olmaz değil mi?",
                Who("Emre", 28, "Partnerin", "Altı aydır birliktesiniz", DuygusalKurban),
                T(SahteAnlayis), T(SuclulukYukleme, DuyguSomurusu), T(KisKisSessizlik, PasifTehdit)),

            Make("dijital_004_mesai_disi_mesaj", "Müşteriden gece sesli mesaj",
                "Serbest çalıştığın müşteri gece ve hafta sonu anında cevap bekliyor",
                ContextId.Dijital, 3, 4, 4, new[] { "freelance", "ulasilabilirlik", "musteri" },
                "Pazar sabahı 08:15, WhatsApp",
                "Serbest grafik tasarımcı olarak çalıştığın müşterin Tolga Bey sürekli gece ve hafta sonu mesaj atıyor. " +
                "Sözleşmede çalışma saatin hafta içi 10:00-18:00 yazıyor. Şimdi pazar sabahı acil revizyon istiyor.",
                "Mesai dışı talebi reddetmek, pazartesi için zaman vermek",
                "Çalışma saatini hatırlatmak profesyonelliktir; özür dilemen gerekmiyor.",
                "Günaydın, logoda küçük bir değişiklik lazım, 10 dakikalık iş. Öğlene kadar atarsan süper olur, yatırımcı toplantım var. Önceki tasarımcı hep hafta sonu da dönerdi bu arada.",
                Who("Tolga Bey", 45, "Serbest çalıştığın müşterin", "Dört aydır onun için çalışıyorsun", AgresifIsrarci),
                T(AcilDurumYaratma, KarsiTeklif), T(Kiyaslama), T(PasifTehdit)),

            Make("dijital_005_bagis_kampanyasi", "Grupta \"herkes bağış yapsın\"",
                "Ofis grubunda bağış listesine adın yazılıyor",
                ContextId.Dijital, 2, 3, 5, new[] { "bagis", "grup_baskisi", "ofis_grubu" },
                "Öğle arası, ofis WhatsApp grubu",
                "Ofis grubunda iş arkadaşın Sevgi bir hediye kampanyası başlattı ve listeye herkesin adını ve 750 TL'yi " +
                "kendisi yazdı. Senin bu ay buna bütçen yok ve gruba \"ödeyenler ✅\" listesi paylaşılıyor.",
                "Listeden çıkmak ya da kendi belirlediğin tutarı vermek",
                "Ne kadar vereceğine sen karar verirsin; gerekçe sunmak zorunda değilsin.",
                "Arkadaşlar Murat'ın doğum günü için listeyi yaptım, kişi başı 750 TL 🎁 senin adını da yazdım, IBAN'ım grupta. Bir tek senden ve Ozan'dan gelmedi, akşama kadar atarsan süper olur 🙏",
                Who("Sevgi", 34, "İş arkadaşın", "Ofisin sosyal organizatörü", TatliDilliSatici),
                T(SosyalKanit, SahteZamanBaskisi), T(Kiyaslama, SuclulukYukleme), T(KarsiTeklif)),
        };
    }
}
