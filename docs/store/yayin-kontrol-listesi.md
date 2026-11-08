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
  - Gizlilik metnindeki `[AD SOYAD]` ve `[ADRES]` alanlarını doldur.
  - Yurt dışına aktarım (Anthropic ve RevenueCat ABD'de) için standart sözleşme ve Kurul'a bildirim gerekip gerekmediğini teyit et.
  - VERBİS kaydının muafiyetini teyit et.
- [ ] Gizlilik politikası ve kullanım şartları taslaklarını (`docs/store/`) gözden geçir; avukat kontrolü önerilir.

### Kararlar
- [ ] **Alan adı:** `cansayno.factech.app` (hemen, ücretsiz) mi, ayrı bir marka alanı mı (`cansayno.com` / `.app`)? Adres mağaza listesine ve uygulamaya yazılacağı için baştan karar vermek iyi. Web uygulaması da ileride aynı alan altında olacak.
- [x] **Yaş sınırı:** 16+ (karar 2026-11-08).
- [x] **Ücretsiz deneme:** yok (karar 2026-11-08).
- [ ] **Fiyatlar:** ₺79 aylık / ₺499 yıllık. Diğer ülkelerin fiyatları mağazada otomatik ya da elle belirlenir.

## 2. Teknik işler (birlikte yapacağımız)

### Yayından önce zorunlu
- [ ] **Canlı sunucu:**
  - API + Postgres, HTTPS, alan adı (`api.[ALAN ADI]`).
  - Ayarlar: JWT secret, Anthropic anahtarı, RevenueCat webhook secret, CORS.
  - Otomatik yedek ve log saklama süresi.
- [ ] **E-posta gönderimi:** Doğrulama ve şifre kodları için bir SMTP sağlayıcı (SPF/DKIM ayarlı). Gönderen adresi `noreply@[ALAN ADI]` olabilir.
- [ ] **Web sitesi:**
  - Tanıtım sayfası + `/gizlilik` + `/kullanim-sartlari` + `/hesap-silme`.
  - İçerik `docs/store/` belgelerinden. Statik site; ücretsiz barındırılabilir.
- [ ] **Uygulamaya yasal linkler** (Apple abonelik kuralı):
  - Paywall: Kullanım Şartları ve Gizlilik linkleri.
  - Kayıt ekranındaki "Kullanım Koşulları / Gizlilik Politikası" metni: şu an tıklanamıyor, link olmalı.
  - Profil: iki link.
- [ ] **`app.config.ts`:**
  - `ios.supportsTablet: false`: iPad ekran görüntüsü zorunluluğunu kaldırır. Uygulama iPad'e özel tasarlanmadı.
  - `ios.infoPlist.ITSAppUsesNonExemptEncryption: false`: her yüklemede ihracat şifreleme sorusunu atlar; yalnızca HTTPS kullanılıyor.
  - Canlı `EXPO_PUBLIC_API_URL` ve RevenueCat anahtarları.
- [ ] **EAS Build** (`eas.json`): development / preview / production profilleri. iOS derlemesi Mac olmadan EAS ile yapılır.
- [ ] **RevenueCat:**
  - Ürünler (`cansayno_premium_monthly`, `cansayno_premium_annual`), `premium` entitlement, offering, webhook.
  - Sandbox ve kapalı testte gerçek satın alma denemesi.
- [ ] **Android izinleri:** Derlemenin manifest'inde gereksiz izin (ör. kesin alarm) olmadığını kontrol et; Google bunları sorguluyor.
- [ ] **İnceleme hesabı:** Canlı veritabanında `review@...`, premium açık.

### Görseller
- [ ] **Uygulama ikonu + splash:** Şu an **Expo'nun varsayılan şablon görselleri** (`Mobile/assets/images/icon.png`, android-icon-*, splash-icon). Marka ikonu tasarlanmalı: 1024×1024, iOS için şeffaflıksız, Android için adaptive ön/arka plan + tek renk bildirim ikonu. Mağaza reddi ve kötü ilk izlenim sebebi.
- [ ] Google öne çıkan görsel: 1024×500.
- [ ] Ekran görüntüleri: iPhone 6,9" + Android telefon (bkz. `magaza-bilgileri.md` §9).

## 3. Önerilen sıra

| Hafta | İş |
|---|---|
| 1 | Hesaplar açılır · alan adı kararı · canlı sunucu + e-posta · web sitesi yasal sayfalar · uygulamaya linkler + config + EAS |
| 1 sonu | **Google kapalı test başlar** (12 tester, 14 gün) · iOS TestFlight |
| 2–3 | Test geri bildirimleri, RevenueCat satın alma denemeleri, ekran görüntüleri, mağaza metinleri |
| 3 sonu | Google üretim başvurusu · App Store incelemesine gönderim |
