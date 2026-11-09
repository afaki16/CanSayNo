# Yayın Kontrol Listesi (bireysel geliştirici)

İletişim: info@factech.app · Paket: `com.cansayno.app`

## ⏱ En uzun süren iş: Google'ın kapalı test şartı

Kasım 2023'ten sonra açılan **bireysel** Google Play hesaplarında, uygulama herkese açılmadan önce:

- en az **12 test kullanıcısı**, kapalı teste katılmış olarak
- **14 gün kesintisiz** uygulamayı yüklü tutmalı.

Ancak bundan sonra "üretime erişim" başvurusu yapılabiliyor; Google bunu ayrıca inceliyor. Bu yüzden:

1. Google hesabını hemen aç.
2. Android'li 12+ tanıdık bul (aile, arkadaş). Test grubu bir e-posta listesi ya da Google Grubu olabilir.
3. Kapalı testi, uygulama "yeterince iyi" olur olmaz başlat; mükemmel olmasını bekleme. Testerlar güncellemeleri otomatik alır.

## 1. Senin yapacakların

### Hesaplar
- [ ] **Google Play Console**, bireysel hesap: tek seferlik $25, kimlik doğrulaması. Geliştirici adı ve iletişim e-postası mağazada görünür.
- [ ] **Apple Developer Program**, bireysel: yıllık $99. Mağazada satıcı adı olarak **gerçek adın** görünür; D-U-N-S gerekmez.
- [ ] **Apple Small Business Program** başvurusu: yıllık geliri 1M$ altındaki geliştiriciler için Apple'ın komisyonu %30 yerine %15. Google'da abonelik komisyonu zaten %15.
- [ ] Ödeme sözleşmeleri:
  - Apple: Paid Applications Agreement, banka hesabı, vergi formu (W-8BEN).
  - Google: ödeme profili (merchant hesabı).
- [ ] **RevenueCat** hesabı (ücretsiz başlangıç).

### Hukuk ve vergi (uzmana kısa danışma önerilir)
- [ ] **Vergi:** Türkiye'de uygulama mağazası geliri için **GVK 20/B istisnası** var (mobil uygulama geliştiricileri; özel banka hesabı ve stopaj şartlı). Mali müşavire danış, mağaza ödemelerini bu hesaba yönlendir.
- [ ] **KVKK:**
  - [x] Gizlilik metninde veri sorumlusu: Alperen Faki, Bursa (açık adres yok).
  - Yurt dışına aktarım (Anthropic ve RevenueCat ABD'de) için standart sözleşme ve Kurul'a bildirim gerekip gerekmediğini teyit et.
  - VERBİS kaydının muafiyetini teyit et.
- [ ] Gizlilik politikası ve kullanım şartları taslaklarını (`docs/store/`) gözden geçir; avukat kontrolü önerilir.

### Kararlar
- [x] **Alan adı:** `cansayno.com` alındı (2026-11-08). Site `cansayno.com`, API `api.cansayno.com`, ileride web uygulaması `app.cansayno.com`.
- [x] **Yaş sınırı:** 16+ (karar 2026-11-08).
- [x] **Ücretsiz deneme:** yok (karar 2026-11-08).
- [ ] **Fiyatlar:** ₺79 aylık / ₺499 yıllık. Diğer ülkelerin fiyatları mağazada otomatik ya da elle belirlenir.

## 2. Teknik işler (birlikte yapacağımız)

### Yayından önce zorunlu
- [ ] **Canlı sunucu:**
  - API + Postgres, HTTPS, alan adı (`api.cansayno.com`).
  - Ayarlar: JWT secret, Anthropic anahtarı, RevenueCat webhook secret, CORS.
  - Otomatik yedek ve log saklama süresi.
- [ ] **E-posta gönderimi:** Doğrulama ve şifre kodları için bir SMTP sağlayıcı (SPF/DKIM ayarlı). Gönderen adresi `noreply@cansayno.com` olabilir.
- [ ] **Web sitesi → cansayno.com** (Cloudflare Workers statik site — wrangler artık Pages projesi oluşturmuyor, Workers'a yönlendiriyor). Site hazır (`Web/`, `wrangler.jsonc`, güvenlik başlıkları `_headers`, kısa adresler `_redirects`); `npm run deploy` önce `build:strict` çalıştırır, boşluk kalırsa yayınlamaz.
  1. [x] `npx wrangler@4.151.3 login` (2026-11-08, Cloudflare hesabı alperenfaki@gmail.com).
  2. [x] Ad soyad + il yazıldı (Alperen Faki, Bursa).
  3. [x] (2026-11-09, sürüm 3787832e) `cd Web && npm run deploy` — `cansayno.com` ve `www.cansayno.com` otomatik bağlanır (`wrangler.jsonc` routes, custom_domain); HTTPS birkaç dakikada aktif olur. Alan adında çakışan eski DNS kaydı varsa wrangler uyarır.
  4. [x] Doğrulandı (2026-11-09): tüm sayfalar 200, `/hesap-silme/` açık, güvenlik başlıkları (CSP, HSTS, X-Frame-Options) canlıda, `/privacy` ve `/delete-account` 301, olmayan adres 404, `www` açık.
  5. [ ] Cloudflare panelinde SSL/TLS › Edge Certificates › **Always Use HTTPS** aç — şu an `http://cansayno.com` HTTPS'e yönlenmiyor.
- [ ] **Hukuki takip:** KVKK yurt dışı aktarım (Anthropic, RevenueCat ABD) için standart sözleşme + Kurul bildirimi gerekip gerekmediğini avukata sor; Hetzner ve Brevo ile veri işleme sözleşmesini (DPA/AVV, panellerinden) imzala.
- [ ] **Yedek süresi:** Gizlilik politikası "silinen veri yedeklerden en geç 30 günde çıkar" diyor — canlı sunucuda veritabanı yedeklerini en fazla 30 gün tutacak şekilde ayarla. Güvenlik kayıtlarının 90 günlük temizliği backend'de hazır (`SecurityLogCleanupService`, `DataRetention:SecurityLogDays`).
- [x] **Uygulamaya yasal linkler** (Apple abonelik kuralı, 2026-11-08): paywall, kayıt ekranı ve Profil'de Gizlilik Politikası + Kullanım Şartları; uygulama içi tarayıcıda açılır (`lib/constants/legal.ts`, `components/ui/LegalLinks.tsx`). Site yayına girene kadar sayfa açılmaz (DNS yok).
- [x] **`app.config.ts`** (2026-11-08): `ios.supportsTablet: false`, `ios.infoPlist.ITSAppUsesNonExemptEncryption: false`, kullanılmayan Android izinleri engellendi (depolama, biyometri).
- [ ] Canlı `EXPO_PUBLIC_API_URL` (eas.json'da `https://api.cansayno.com`) ve RevenueCat anahtarları (EAS ortam değişkeni olarak).
- [x] **EAS Build** (`Mobile/eas.json`, 2026-11-08): development / preview (APK, staging) / production (otomatik sürüm numarası). **Senin yapacağın:** `npx eas-cli login` → `npx eas-cli init` (projeyi Expo hesabına bağlar, `app.config.ts`'e projectId ekler). İlk EAS derlemesinde CMake 3.31.6'nın sunucuda indirildiğini doğrula (`plugins/withAndroidCmakeVersion.js`).
- [ ] **RevenueCat:**
  - Ürünler (`cansayno_premium_monthly`, `cansayno_premium_annual`), `premium` entitlement, offering, webhook.
  - Sandbox ve kapalı testte gerçek satın alma denemesi.
- [x] **Android izinleri** (2026-11-08): kesin alarm (`SCHEDULE_EXACT_ALARM`) yok; depolama ve biyometri izinleri `blockedPermissions` ile çıkarıldı. Kalanlar: internet, bildirim, açılışta hatırlatmaları yeniden kurma (`RECEIVE_BOOT_COMPLETED`), titreşim, uygulama içi satın alma (`BILLING`), launcher rozet izinleri. `SYSTEM_ALERT_WINDOW` yalnızca debug derlemesinde.
- [ ] **İnceleme hesabı:** Canlı veritabanında `review@...`, premium açık.

### Görseller
- [x] **Uygulama ikonu + splash** (2026-11-08): balon + "dur" eli, mor→mavi geçiş. `node design/brand/generate.mjs` üretir; emulator'de ana ekran ve açılışta doğrulandı.
- [x] Google öne çıkan görsel 1024×500 ve 1024 uygulama ikonu: `docs/store/gorseller/`.
- [ ] Ekran görüntüleri: iPhone 6,9" + Android telefon (bkz. `magaza-bilgileri.md` §9).

## 3. Önerilen sıra

| Hafta | İş |
|---|---|
| 1 | Hesaplar açılır · alan adı kararı · canlı sunucu + e-posta · web sitesi yasal sayfalar · uygulamaya linkler + config + EAS |
| 1 sonu | **Google kapalı test başlar** (12 tester, 14 gün) · iOS TestFlight |
| 2–3 | Test geri bildirimleri, RevenueCat satın alma denemeleri, ekran görüntüleri, mağaza metinleri |
| 3 sonu | Google üretim başvurusu · App Store incelemesine gönderim |
