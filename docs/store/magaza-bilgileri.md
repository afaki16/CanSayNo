# CanSayNo: Mağaza Metinleri ve Form Cevapları

Karakter sınırları parantez içinde. Uygulama şu an **yalnızca Türkçe**; İngilizce listeleme, uygulama İngilizceye çevrilince açılmalı (yoksa İngilizce kullanıcı Türkçe uygulamayla karşılaşır ve kötü puan verir).

---

## 1. Temel bilgiler

| Alan | Değer |
|---|---|
| Uygulama adı (30) | **CanSayNo: Hayır Demeyi Öğren** (28) |
| Paket kimliği | `com.cansayno.app` (iOS + Android, değiştirilemez) |
| Birincil kategori | Eğitim (Education) |
| İkincil kategori (Apple) | Yaşam Tarzı (Lifestyle) |
| Destek e-postası | info@factech.app |
| Gizlilik politikası URL | `https://[ALAN ADI]/gizlilik` |
| Kullanım şartları URL | `https://[ALAN ADI]/kullanim-sartlari` |
| Hesap silme URL (Google) | `https://[ALAN ADI]/hesap-silme` |
| Destek / pazarlama URL | `https://[ALAN ADI]` |
| Fiyatlandırma | Ücretsiz + uygulama içi abonelik |

## 2. Türkçe listeleme

**Apple alt başlık (30):** Yapay zekâyla hayır pratiği (27)

**Google kısa açıklama (80):** Zor insanlara "hayır" demeyi yapay zekâ karakterlerle güvenle pratik et. (72)

**Apple anahtar kelimeler (100, virgülle, boşluksuz):**
`demek,sınır,koymak,asertif,iletişim,özgüven,baskı,ikna,pratik,rol,koçluk,yapay,zeka,gelişim` (91 karakter, 99 bayt; adda geçen "hayır" tekrar yazılmaz, Apple adı zaten arar)

**Uzun açıklama (4000):**

> Patronun cuma akşamı "küçük bir iş" istiyor. Akraban yine borç istiyor. Satıcı "son fırsat" diye ısrar ediyor. Ve sen yine "tamam" diyorsun.
>
> CanSayNo ile "hayır" demeyi güvenli bir ortamda, gerçek hayattaki gibi pratik edersin.
>
> **NASIL ÇALIŞIR?**
> • Bir senaryo seç: iş, aile, arkadaşlar, ilişki, satıcılar, komşular, okul ya da sosyal medya.
> • Yapay zekâ karakteriyle yazış. Karakter tıpkı gerçek hayattaki gibi ısrar eder, suçluluk hissettirir, gönlünü yapar ya da baskı kurar.
> • Pratiğin sonunda cevapların 5 eksende puanlanır: netlik, nezaket, dayanıklılık, alternatif sunma ve özür dengesi.
> • Güçlü yönlerini, geliştirebileceğin noktaları ve örnek bir cevabı gör.
>
> **NEDEN CANSAYNO?**
> • Gerçekçi karakterler: babacan manipülatörden duygusal kurbana, tatlı dilli satıcıdan otoriter patrona.
> • 65+ senaryo, 8 farklı bağlam.
> • Seri, seviye ve rozetlerle her gün biraz daha güçlen.
> • Günlük hatırlatma ile alışkanlık kazan.
> • Reklam yok. Verilerin satılmaz.
>
> **ÜCRETSİZ VE PREMIUM**
> İş & Kariyer bağlamındaki tüm senaryolar ücretsiz; günde 1 pratik yapabilirsin. Premium ile 8 bağlamın tamamını açar, her gün dilediğin kadar pratik yaparsın.
>
> Premium aylık veya yıllık otomatik yenilenen bir aboneliktir. Ödeme, satın alma onaylandığında App Store / Google Play hesabından alınır. Abonelik, dönem bitmeden en az 24 saat önce iptal edilmezse otomatik yenilenir. Aboneliğini mağaza hesap ayarlarından yönetebilir ve iptal edebilirsin.
>
> CanSayNo bir terapi ya da psikolojik danışmanlık hizmeti değildir; iletişim becerilerini pratik etmen için tasarlanmış bir araçtır.
>
> Kullanım Şartları: https://[ALAN ADI]/kullanim-sartlari
> Gizlilik Politikası: https://[ALAN ADI]/gizlilik

**Apple "Bu sürümde yenilikler":** İlk sürüm. Hayır demeye hazır mısın?

## 3. İngilizce listeleme (uygulama çevrilince)

**Name (30):** CanSayNo: Learn to Say No (25)
**Subtitle (30):** Practise boundaries with AI (27)
**Short description (80):** Practise saying "no" to pushy people with realistic AI characters. (66)
**Keywords (100):** `say,no,boundaries,assertive,assertiveness,confidence,communication,people,pleaser,roleplay,coach` (96)

Uzun açıklama: Türkçe metnin çevirisi. Uygulama İngilizceye çevrilirken birlikte yazılacak.

## 4. Abonelik ürünleri (App Store Connect + Google Play + RevenueCat)

| | Aylık | Yıllık |
|---|---|---|
| Ürün kimliği | `cansayno_premium_monthly` | `cansayno_premium_annual` |
| Fiyat (TR) | ₺79 | ₺499 |
| Apple abonelik grubu | `CanSayNo Premium` (ikisi aynı grupta) | |
| Ücretsiz deneme | [KARAR: yok / 7 gün] | |

- **RevenueCat:** entitlement `premium`. Offering `default` (current), paketler `$rc_monthly` ve `$rc_annual`; uygulama bu iki paketi okuyor.
- **Webhook:** `https://[API ALAN ADI]/api/Webhooks/revenuecat`. Authorization değeri sunucuda `RevenueCat__WebhookAuthorization` ile aynı olmalı.
- Abonelik adı/açıklaması (Apple, her dil için): "Premium: tüm bağlamlar ve günlük sınırsız pratik"

## 5. Google Play "Veri güvenliği" formu

| Soru | Cevap |
|---|---|
| Veri topluyor mu? | Evet |
| Veri üçüncü taraflarla **paylaşılıyor** mu? | Hayır. Anthropic, RevenueCat ve sunucu sağlayıcı bizim adımıza işleyen hizmet sağlayıcılardır; Google'ın tanımına göre bu "paylaşım" sayılmaz. |
| Aktarımda şifreleme | Evet (HTTPS) |
| Kullanıcı veri silme talebinde bulunabilir mi? | Evet (uygulama içi + hesap silme URL'si) |
| Hesap oluşturma | Uygulama içi, e-posta + şifre |

Toplanan veri türleri (hepsi: **toplanır, paylaşılmaz, zorunlu**; amaç **Uygulama işlevselliği** + **Hesap yönetimi**; güvenlik kayıtları için ayrıca **Dolandırıcılık önleme, güvenlik ve uyumluluk**):

- Kişisel bilgiler › **Ad**, **E-posta adresi**, **Kullanıcı kimlikleri**
- Finansal bilgiler › **Satın alma geçmişi**
- Uygulama etkinliği › **Diğer kullanıcı tarafından oluşturulan içerik** (pratik mesajları), **Uygulama etkileşimleri** (pratik sayısı, seri)
- Cihaz veya diğer kimlikler › **Cihaz veya diğer kimlikler** (IP, cihaz bilgisi; güvenlik)

Toplanmayan: konum, kişiler, fotoğraf/video, ses, sağlık, mesajlar (SMS/e-posta), takvim, web geçmişi, reklam kimliği, kilitlenme kayıtları.

## 6. Apple "Uygulama Gizliliği" (App Privacy)

- **Takip (Tracking):** Hayır
- Toplanan veriler (hepsi **kullanıcıya bağlı**, amaç **App Functionality**):
  - Contact Info › **Name**, **Email Address**
  - User Content › **Other User Content** (pratik mesajları)
  - Purchases › **Purchase History**
  - Identifiers › **User ID**
  - Usage Data › **Product Interaction**
- Kilitlenme/teşhis verisi: toplanmıyor (uygulamada crash SDK'sı yok)

## 7. Yaş derecelendirmesi

- Önerilen yaş sınırı **16+**. Romantik ilişki senaryoları içeriyor; ayrıca GDPR'da dijital rıza yaşı 16'ya kadar çıkabiliyor.
- **Apple anketi:** cinsellik/şiddet yok, kumar yok, sınırsız web erişimi yok. Yapay zekâ sohbet özelliği var; ankette bunu belirten soruya **evet** de.
- **Google IARC anketi:** kullanıcılar arası iletişim yok (kullanıcılar birbiriyle yazışmaz, yalnızca yapay zekâyla), kullanıcı içeriği başkalarıyla paylaşılmaz.
- Çıkan derece ne olursa olsun, kullanım şartlarındaki yaş sınırı 16'dır.

## 8. İnceleme notları (Apple App Review / Google)

- **Demo hesap:** `review@[ALAN ADI]` / [ŞİFRE]. Premium açık olsun ki inceleyen tüm bağlamları görebilsin.
- **Not:** "Uygulama yapay zekâ ile rol yapma pratiği sunar. Ücretsiz kullanıcı İş & Kariyer bağlamını kullanır; diğer bağlamlar abonelikle açılır. Hesap silme: Profil › Hesap bilgileri › Hesabımı sil."

## 9. Görseller

- **İkon:** 1024×1024, şeffaflıksız (Apple).
- **Google öne çıkan görsel:** 1024×500.
- **Ekran görüntüleri:**
  - iPhone 6,9": 1320×2868, en az 3.
  - Android telefon: en az 2, önerilen 4–8.
  - iPad: yalnızca `supportsTablet` açık kalırsa (bkz. kontrol listesi).
- **Önerilen ekran sırası:** bağlam seçimi → karakterle yazışma → değerlendirme sonucu → seri/rozetler → senaryo listesi.
