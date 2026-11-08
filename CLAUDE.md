# CanSayNo (Hayırcı) — Proje Rehberi

Claude Code bu dosyayı her oturumda otomatik okur. Ürün tasarımı ve ilk geliştirme fazı claude.ai'da yapıldı; geliştirme artık buradan devam ediyor.

## Ürün

**Hayırcı** (kod adı CanSayNo): Kullanıcının AI karakterlerle rol yaparak "hayır demeyi" pratik ettiği mobil uygulama. Hedef: Türkiye + global. Solo geliştirici: Alperen.

- 8 bağlam: `is` (İş & Kariyer — ÜCRETSİZ), `aile`, `arkadas`, `romantik`, `satici` (Satıcı & Hizmet), `komsu` (Ev & Komşu), `okul` (Okul & Kampüs), `dijital` (Dijital & Sosyal Medya) — İş dışındakiler premium. Son üçü 2026-11-07'de eklendi (enum 6–8; DB'de int, migration gerekmedi). Bağlam UI metadata'sı mobilde `lib/mockData/contexts.ts`, panelde `utils/hayirciLabels.ts`; yeni bağlam eklerken ikisi + `ContextId` + admin ID regex'i (backend `AdminScenarioCommands` + `ScenarioForm.vue`) + `AdminScenarioIds.Prefix` + `dailyTask` başlıkları güncellenir. "Kaşif" rozeti = en az 5 farklı bağlam (`UserProgress.ContextExplorerContexts`)
- 7 karakter arketipi: babacanManipulator, duygusalKurban, agresifIsrarci, tatliDilliSatici, masumRicaci, pasifAgresif, otoriter
- 13 baskı taktiği (`PressureTactic` enum), 5 değerlendirme ekseni: netlik, nezaket, dayaniklilik, alternatifSunma, ozurDengesi
- Freemium: İş bağlamı tamamen ücretsiz, diğer 4 bağlam premium (RevenueCat, aylık ₺49.99 placeholder)
- v1.0 hedefi: ~50 senaryo
- Tasarım dili: friendly playful (Duolingo gamification + Headspace olgunluğu)
- **Marka (2026-11-08)**: logo = mor→mavi geçişli yuvarlak kare içinde beyaz konuşma balonu + mercan "dur" eli. Geçiş `#5B4BE0 → #2D7FF9` (sol üst → sağ alt), el `#FF7043`. Tek kaynak `design/brand/brand.mjs`; `node design/brand/generate.mjs` tüm ikon/splash/favicon/og/mağaza görsellerini üretip Mobile, Web, Frontend ve `docs/store/gorseller`'e kopyalar (elle düzenleme, betiği çalıştır; native ikon/splash değişince `expo prebuild` + yeni build). Kural: büyük marka yüzeyleri (birincil buton, başlık/görev kartları, logo, kullanıcı mesaj balonu, panel butonu/kenar çubuğu/giriş) geçiş; küçük etkileşim öğeleri (link, ikon, seçim chip'i, gönder butonu) `Colors.primary` mavisi. Durum renkleri (başarı yeşili, bilgi moru) değişmez. Mobil: `Gradients.brand`/`brandDeep`, `Colors.indigo*` (`constants/theme.ts`), logo `components/ui/BrandMark.tsx` (geometri brand.mjs ile aynı tutulmalı). Seçim süreci ve renk çalışması `design/app-icon/` (eski ok ikonu orada arşiv)

## Repo Yapısı (monorepo)

```
CanSayNo/
├── Backend/    .NET 9 Clean Architecture API (Kickstart template'inden türetildi)
├── Frontend/   Nuxt 3 admin paneli (şimdilik dokunulmuyor)
└── Mobile/     React Native + Expo SDK 54
```

## Çalıştırma

1. **Postgres**: `docker start postgres-db` (container `postgres-db`, port 5432)
2. **Backend**: `cd Backend` → `dotnet run --project CanSayNo.API` → `http://localhost:5156`, Swagger `/swagger`
   - VS/IIS Express farklı portta açılıyor (:60545). Mobile `:5156`'ya bakıyor — mobile ile test ederken `dotnet run` kullan
   - **Port çakışması**: Alperen'in Kickstart.API projesi de 5156'yı kullanıyor; Sethumo.API 5157'yi de dinleyebiliyor (2026-11-04'te test istekleri bu yüzden Sethumo'ya gitti). Test için CanSayNo'yu başka bir portta aç, öncesinde VE test sırasında `Get-NetTCPConnection -LocalPort <port>` ile sahibini kontrol et; isteklerin CanSayNo log'una düştüğünü doğrula. `/api/Sessions/my` yetkisiz istekte 401 değil 404 dönüyorsa çalışan backend CanSayNo değildir. `.env.development` değişince Metro yeniden başlatılmalı (`EXPO_PUBLIC_*` bundle'a gömülür)
   - Otomatik migration + idempotent seed startup'ta çalışır
3. **Mobile**: `cd Mobile` → `npm start` → `a` ile Android emulator (development build; ilk kez ya da native paket eklenince `npm run android` ile derleyip kur — JAVA_HOME = Android Studio `jbr`. Expo Go artık hedef değil). **Windows build notu**: `LongPathsEnabled=1` (kayıt defteri, açıldı) + CMake 3.31.6 (ninja 1.12; `plugins/withAndroidCmakeVersion.js` sabitler, SDK'ya elle kuruldu: `Sdk/cmake/3.31.6`) — varsayılan CMake 3.22.1 react-native-keyboard-controller'ın ~290 karakterlik C++ yollarında düşüyor. Emulator için hızlı build: `gradlew assembleDebug -PreactNativeArchitectures=x86_64`. Dev client'ı Metro'ya bağlamak: `adb shell am start -d "exp+cansayno://expo-development-client/?url=http%3A%2F%2F127.0.0.1%3A8082"`
   - API URL: `Mobile/.env.development` (`EXPO_PUBLIC_API_URL`). Android'de `localhost` otomatik `10.0.2.2`'ye çevrilir (`lib/config/env.ts`)

Test hesabı: `admin@cansayno.com` / `Admin123!`

## Backend

- Pattern: CQRS + MediatR + FluentValidation + AutoMapper + Result pattern, Repository + UnitOfWork
- DI: `*Repository` ile biten interface'ler reflection ile otomatik kaydolur; MediatR handler'ları ve AutoMapper profilleri assembly taramasıyla bulunur. `DependencyInjection.cs`'e elle kayıt gerekmez
- Başarılı response: `{ success, data }`. Hata: `{ success, error: { status, code, errors: { field: [...] } } }`
- Enum'lar JSON'da camelCase string (`JsonStringEnumConverter`, `Program.cs`)
- `User.Id` **int** (Guid değil) — tüm UserId FK'leri int
- Tek tenant: Kickstart multi-tenant ama Hayırcı entity'leri tenant-scoped DEĞİL. Kayıtlı kullanıcılarda `tenantId=null`, admin'de `1`; senaryolar ikisine de görünüyor (doğrulandı)
- Tek şema: `public`. Identifier'lar PascalCase → SQL'de çift tırnak şart: `SELECT "Id" FROM "Scenarios"`
- Email doğrulama zorunlu, doğrulanmadan login yok. Kullanıcıyı elle aktive etmek için hem `"EmailConfirmed" = true` hem de `"Status"` alanının pendingVerification'dan çıkması gerekiyor
- `Users.PhoneNumber` unique index'leri boş string'i dışlayan filtered index; handler'lar boş telefonu `null`'a çeviriyor

### Hayırcı Domain — `CanSayNo.Domain/Entities/Hayirci/`

| Entity | PK | JSONB alanlar |
|---|---|---|
| `Scenario` | string (`is_001_cuma_mesaji`) | scene, character, pressure_tactics, tags |
| `Session` | Guid | messages, evaluation |
| `UserStats` | UserId (1-1) | unlocked_badges |
| `Subscription` | Guid (`AppId` ile multi-app) | raw_webhook_data |
| `DailyUsage` | (UserId, Date) composite | — |

- JSONB: `System.Text.Json` + `HasConversion` + `ValueComparer` (`Configurations/Hayirci/JsonbColumnExtensions.cs`). `.OwnsOne().ToJson()` KULLANMA — `Dictionary<enum,int>` ve `List<enum>` ile kırılıyor
- Seed: `Persistence/Seeding/HayirciSeedData.cs` (ilk 7) + `HayirciSeedData.Catalog.cs` (43, kısa `Make(...)` yardımcısıyla), runtime + idempotent (HasData değil; var olan ID atlanır, mevcut senaryo GÜNCELLENMEZ). **50 senaryo (2026-11-05)**: her bağlamda 10; İş'in 10'u ücretsiz, diğerleri premium. **+15 (2026-11-07)**: `HayirciSeedData.NewContexts.cs`, yeni 3 bağlamda 5'er (toplam 65); her bağlamdan biri gerçek LLM ile uçtan uca denendi
- `PressureTactics` hiçbir DTO'da yok — sadece AI prompt'una girer, client'a asla sızmamalı

### Mevcut API

- `GET /api/Scenarios` (?contextId, ?isFreeOnly), `GET /api/Scenarios/{id}`, `GET /api/Scenarios/by-context/{contextId}` — hepsi `[Authorize]`
- Kullanıcıya özel alanlar (isCompleted, bestScore, attemptsCount, lastAttemptAt) Sessions'tan tek GroupBy sorgusuyla hesaplanıyor (N+1 yok)
- Sessions (`[Authorize]`): `POST /api/Sessions` (body `{scenarioId}`; premium senaryo + abonelik yok → 403 `premiumRequired`, ücretsiz günlük limit → 429 `dailyLimitReached`; aynı senaryoda henüz mesaj atılmamış açık oturum varsa yenisi açılmaz, o döner), `GET /api/Sessions/{id}` (başkasının oturumu → 404), `GET /api/Sessions/my?page&pageSize` (pageSize ≤ 50)
- Limit ayarları `appsettings.json` → `Hayirci` (`FreeDailySessionLimit`=1, `PremiumDailySessionLimit`=30, `AiRequestsPerMinute`=10, `UsageDayUtcOffsetHours`=3 → gün TR saatiyle döner). Kullanım `DailyUsage`'a herkes için yazılır. Ücretsiz limit → 429 `dailyLimitReached` (1014); premium "adil kullanım" sınırı → 429 `fairUseLimitReached` (1106; mobil paywall önermez). Sayaç artışı atomik (`TryIncrementSessionsAsync`, tek UPDATE ... WHERE SessionsCount < limit) — paralel isteklerle aşılamaz (2026-11-08)
- **LLM hız sınırı** (2026-11-08): `messages` ve `evaluate` uçlarında `[EnableRateLimiting("ai")]` — KULLANICI başına (JWT `NameIdentifier`, yoksa IP) dakikada 10. Bu yüzden `UseRateLimiter` artık `UseAuthentication`'dan SONRA, `UseAuthorization`'dan önce. Reddedilen istek gövdesi düz metin (kod yok); mobil 429'u "Çok hızlı gidiyorsun" diye gösterir
- Kickstart `ValidationBehavior` `PagedResult<T>` için failure üretemiyor (cast hatası → 500) — sayfalı sorgulara validator ekleme, handler'da clamp et
- Auth (Kickstart): `/api/Auth/login`, `/register`, `/refresh-token` (body `{accessToken, refreshToken}` — ikisi de zorunlu), `/logout` (body `{refreshToken, reason}`), `/me`
- Rate limiter'lar aktif (login, sensitive, refresh, availability — IP başına; ai — kullanıcı başına)
- **Veri saklama (2026-11-08)**: `SecurityLogCleanupService` (Infrastructure/Services/Maintenance, hosted service; açılıştan 5 dk sonra, sonra her 24 saat) 90 günden eski giriş denemelerini ve süresi 90+ gün önce dolmuş refresh/doğrulama/şifre sıfırlama kayıtlarını siler. Ayar `DataRetention:SecurityLogDays` — gizlilik politikası §5'teki süreyle aynı tutulmalı. Test: `SecurityLogCleanupTests`

### Testler — `Backend/CanSayNo.Tests` (2026-11-08)

- Çalıştır: `cd Backend` → `dotnet test CanSayNo.Tests` (~1.5 dk). **Docker açık olmalı**: entegrasyon testleri Testcontainers ile geçici `postgres:16-alpine` açar, iş bitince siler — dev DB'ye dokunmaz. Anthropic'e istek atmaz (sahte `ICharacterChatService`/`ISessionEvaluator`, `Integration/Fakes.cs`)
- `Unit/`: saf mantık — `UserProgress` (seri/XP/rozet), `RevenueCatEventApplier`, TR gün sınırı
- `Integration/`: gerçek pipeline (`WebApplicationFactory<Program>`, Development ortamı, migration + seed). `ApiFactory` ayarları ortam değişkeniyle ezer (bağlantı, boş Anthropic anahtarı, webhook secret). Her test client'ı `X-Test-Client-Ip` ile ayrı IP taşır (yalnızca testte eklenen `TestClientIpStartupFilter`) — yoksa IP başına limitlere takılır. Kullanıcı: API'den kayıt + DB'de e-posta onayı (`TestApi.CreateUserAsync`, `premium: true` abonelik ekler)
- Kapsam: auth (madde 28, hesap sayımı, kopya e-posta, web cookie / mobil gövde refresh akışı, OTP deneme sınırı — sahte `IEmailService` kodu yakalar), güvenlik (sahte/`alg:none` JWT, IDOR, admin uçları 403, taktik sızıntısı, kayıtta rol enjeksiyonu, webhook secret, şifresiz silme), limitler (premium kilidi, günlük limit, adil kullanım, paralel başlatma, LLM hız sınırı), uçtan uca pratik (raunt, kapanış, değerlendirme idempotent + 0-100 sıkıştırma, istatistik/rozet)
- Yeni iş kuralı/uç eklerken buraya test ekle; geçici betik yazmak yerine

## Mobile

- Expo Router (file-based), Zustand, TanStack Query, axios, react-hook-form + zod, expo-secure-store, phosphor-react-native, Reanimated
- `lib/api/`: client (Bearer interceptor), refreshInterceptor (401 → refresh → retry; eşzamanlı 401'ler tek refresh; login/register refresh akışının dışında), queryClient, `apiGet` / `apiPost` / `apiPostVoid`, endpoint sabitleri `endpoints.ts`
- `lib/api/services/`: authService, scenarioService, hata → Türkçe mesaj eşlemeleri
- `hooks/queries/useScenarios.ts`: hiyerarşik `scenarioKeys` — session yazdıktan sonra `invalidateQueries({ queryKey: scenarioKeys.all })`
- `stores/authStore.ts`: secure storage; sadece `StoredUser` saklanır (roles/permissions değil)
- **Gerçek veriden**: auth, senaryo listesi/detayı, bağlam kartı sayıları, pratik başlatma (`hooks/useStartPractice.ts` → POST /api/Sessions; premium → paywall, günlük limit → Alert), Anasayfa "Son Pratiklerin" ve Profil ortalama skoru (GET /api/Sessions/my)
- **Gerçek veriden (LLM)**: pratik chat'i (`useSession` + `useSendMessage` → POST /api/Sessions/{id}/messages; hata → bant + mesaj input'a geri) ve Sonuç ekranı (`useEvaluateSession` → POST /api/Sessions/{id}/evaluate). `aiResponses.ts` / `evaluation.ts` mock'ları silindi; eksen etiketleri `lib/constants/evaluation.ts`
- **İstatistikler**: Anasayfa/Profil seri, haftalık gün çizelgesi, "Bu Hafta", seviye/XP, rozetler → `useMyStats` (GET /api/Stats/me). `userStats` mock'u silindi; `lib/mockData` artık sadece `contexts`. Haftanın günleri ve "bugün" backend'de TR saatine göre (`weekActivity`, `todayIndex`) — cihaz saatine güvenilmez. Profil "Usta" rozeti = `persistent` (50 pratik)
- **Auth guard (2026-11-04)**: kök layout'ta `Stack.Protected` — girişli kullanıcı `index`/`(auth)`'u, girişsiz kullanıcı `(app)`'i göremez; login/logout/refresh hatasında yönlendirmeyi router yapar. Açılışta saklı oturum arka planda `/api/Auth/me` ile doğrulanır (`hooks/useSessionValidation.ts`, açılışı bloklamaz). Refresh sırasında ağ hatası oturumu SİLMEZ (çevrimdışı açılış); yalnızca sunucu reddederse silinir. Emulator'de 4 senaryo doğrulandı
- **E-posta değişikliği (kodla, 2026-11-04)**: `POST /api/Auth/profile/email/code` (`{newEmail, currentPassword}`, yeni adrese 6 haneli kod, `VerificationPurpose.EmailChangeCode`=6, kod hedef adrese bağlı) → `POST /api/Auth/profile/email/confirm` (`{code}`). Onayda tüm oturumlar kapanır + eski adrese bildirim; mobil `clearAuth` yapar, kullanıcı yeni adresle girer. Link akışı (web) aynen duruyor
- **Bildirimler (yerel hatırlatma, 2026-11-06)**: `expo-notifications`, sunucu push'u YOK. Profil → Bildirimler (`(app)/notifications`): aç/kapat + saat (varsayılan kapalı; açınca izin istenir; Android 13+ hiç sorulmamış izni de `denied` bildirir → `canAskAgain` ile `askable`/`blocked` ayrılır, `blocked` ise "Ayarları aç"). Plan saf fonksiyon `lib/notifications/reminderPlan.ts`: önümüzdeki 7 gün için günde 1 tek seferlik bildirim; bugün pratik yapıldıysa/saat geçtiyse yarından başlar; ilki seriyi anar. `useReminderSync` ((app) layout) tercih/istatistik değişince ve uygulama öne gelince planı baştan kurar, çıkışta iptal eder. Eşitlemeler SIRALI (`queue`) — yoksa iç içe geçip aynı güne iki hatırlatma kuruluyordu. Tercih cihaza ait (`prefs.reminder`, secure store). Uygulama açıkken bildirim gösterilmez. Android alarmları kesin değil (+1 saat pencere). Emulator'da alarmlar `dumpsys alarm` ile doğrulandı; bildirimin ekrana düşüşü görülmedi (emulator saati değiştirilemedi)
- **Bugünkü görev**: `lib/utils/dailyTask.ts` (saf fonksiyon) senaryo listesinden seçer — hiç denenmemiş → yarım kalmış → en düşük skorlu; yalnızca ücretsiz senaryolar (mobil abonelik durumunu bilmiyor)
- **Oturum değişimi**: giriş/çıkış/refresh başarısız → `queryClient.resetQueries()` (kök layout); önceki hesabın verisi yeni hesaba görünmez
- **Klavye**: `react-native-keyboard-controller` (Expo Go'da var) — kök layout'ta `KeyboardProvider`, ekranlarda kütüphanenin `KeyboardAvoidingView`'i (`behavior="padding"`). RN'ninkini KULLANMA: Android edge-to-edge'de klavye kapanınca boşluk bırakıyor
- Pratik ekranı `sessionId` query param'ı alır ve Sonuç ekranına geçirir. LLM uçları için axios timeout'u istek bazında uzatıldı (mesaj 45 sn, değerlendirme 90 sn)
- `lib/mockData/contexts.ts` kalıcı UI metadata (isim, renk, ikon) — mock değil, ileride `lib/constants/`'a taşınabilir
- Register'da phoneNumber `null` gönderiliyor (formda alan yok)

## Çalışma Kuralları

- Türkçe iletişim; kod ve identifier'lar İngilizce
- Adım adım ilerle, her adımdan sonra doğrula; büyük işleri parçalara böl
- Tahminle değil gerçek sözleşmeyle çalış (Swagger / mevcut kod). Bir şeyi değiştirmeden önce mevcut kodu oku
- Kickstart pattern'lerine sadık kal
- DB'yi değiştiren, migration ekleyen veya backend'i yeniden başlatan bir şey yapmadan önce sor
- Mock kodu yoruma alma, sil
- Her sprint sonunda aşağıdaki "Şu Anki Durum" bölümünü güncelle

## Şu Anki Durum (2026-10-14)

**Tamamlandı**: backend kurulumu, domain modeli, scenarios read API, enum string serialization, mobile 3 parçalık entegrasyon (API altyapısı, auth, senaryolar), phoneNumber unique index düzeltmesi.

**Yeni tasarım uygulandı (2026-10-14)**: Claude Design "CanSayNo Ekranlar" — mavi `#2D7FF9` + mercan palet (2026-11-08'den beri marka yüzeyleri mor→mavi geçiş, bkz. Ürün › Marka), Satoshi font (`assets/fonts/`, `useFonts` ile; ağırlık → dosya eşlemesi `components/ui/Text.tsx`'te — RN `Text` yerine her yerde bunu kullan). Tüm ekranlar yenilendi + yeni Sonuç ekranı (`scenarios/result/[scenarioId]`, mock `lib/mockData/evaluation.ts`). Marka adı UI'da "CanSayNo". Paywall fiyatı ₺79 aylık / ₺499 yıllık. Tüm bu iş `feature/scenarios-redesign` branch'inde commit'lendi (main'e henüz merge edilmedi). Kayıtta "Şifre tekrar" alanı kaldırıldı (confirmPassword = password gönderiliyor).

**Kickstart template yükseltmeleri (2026-11-03)**: `../Kickstart/TEMPLATE_UPGRADES.md`'den 11, 14, 15, 16, 17, 18, 21 ve "refresh reuse detection" kontrol maddesi uygulandı, API ile doğrulandı. Yeni migration'lar: `DropPermissionResourceTypeUniqueIndex`, `AddFailedAttemptsToUserVerificationToken`. Yeni uçlar: `PUT /api/Auth/profile`, `PUT /api/Auth/profile/email`, `POST /api/Auth/confirm-email-change`, `POST /api/Auth/forgot-password/code`, `reset-password/code`, `password-change/code`, `password-change/confirm`, `check-availability`. Alınmayanlar: multi-tenant maddeleri (3, 12, 13, 22, 23), 20, Nuxt maddeleri, 2, 19, 24; 9 farklı biçimde zaten var (string hata kodları). Kickstart'tan madde alırken dikkat: CanSayNo'da `ErrorCode` 1014/1015 = DailyLimitReached/PremiumRequired (Kickstart'ta başka anlamda). **E-posta benzersizliği GLOBAL (2026-11-04)**: `EmailExistsAsync(email, excludeUserId)` tenant'a bakmaz — kayıt, `check-availability`, e-posta değişikliği (kod + link), admin kullanıcı oluşturma/güncelleme seed (`tenantId=1`) adreslerini de dolu sayar. DB'de de global: tek `IX_Users_Email` (unique, `NOT IsDeleted`; migration `MakeUserEmailGloballyUnique`, 2026-11-05). Eşzamanlı yarışta index'in 23505'i `GlobalExceptionHandler`'da 409 `users.emailExists`'e çevrilir (telefon index'leri için `users.phoneExists`). E-posta değişikliği onay linki (`/auth/confirm-email-change`) için web sayfası yok; mobil KODLA değiştirir (aşağıda). **Madde 26 (2026-11-06)**: mobil her isteğe `X-Client: mobile` gönderir (`lib/api/client.ts` `CLIENT_HEADERS`, refresh çağrısı dahil); backend bu başlıkta refresh cookie'si yazmaz/okumaz (`AuthController` `IsMobileClient`) — RN cookie'yi düz metin saklıyordu. **Web'de gövdeden kaldırıldı (2026-11-08)**: başlıksız (tarayıcı/panel) login ve refresh cevabında `refreshToken` null, token yalnızca HttpOnly cookie'de (`DeliverRefreshToken`) — XSS cevaptan okuyamaz. Panel zaten cookie + boş gövdeyle çalışıyordu; `TokenDeliveryTests` doğruluyor. `change-password` artık `sensitive` rate limit'te. **OTP denemeleri atomik (2026-11-08)**: deneme kod karşılaştırılmadan ÖNCE tek UPDATE ile sayılır (`TryReserveAttemptAsync`), kod `TryMarkUsedAsync` ile bir kez kullanılır — eskiden paralel yanlış tahminler 5 deneme sınırını aşıyordu (`OtpCodeTests` eski kodda düşüyor, yenide geçiyor). Madde 27 gereksiz: e-posta global benzersiz, çok hesaplı login dalına düşülemez. **Madde 28 (2026-11-07)**: doğrulama bekleyen hesap doğru şifreyle login'de 1013 alır (eskiden genel 1012; mobil 1013'ü "E-postanı doğrulaman gerekiyor" diye gösteriyor). Küçük düzeltmelerden yalnızca kullanıcı formunda telefonun zorunlu olmaması alındı (mobil kullanıcıların telefonu yok, admin düzenleyemiyordu); diğerleri (rol detay penceresi, "Tümünden Çık", koyu tema butonu) Kickstart'ın yeni arayüzüne bağlı, CanSayNo paneli hâlâ Vuetify. Kontrol listesi 3/3 temiz (legacy timestamp yok, büyük harfli e-posta 0, reuse detection yalnızca rotation'da)

**Güvenlik testi (2026-11-07)**: canlı API'ye 40 kontrol (betik oturum scratchpad'inde, `sec-test.mjs`): kayıtta rol/tenant enjeksiyonu, sahte/`alg:none`/rolü değiştirilmiş JWT, oturumlarda IDOR (oku/mesaj/değerlendir/export), normal kullanıcıyla 16 admin ucu, PressureTactics sızıntısı, premium/günlük limit (6 eşzamanlı istek → 1 oturum), pasif senaryo, girdi (501 karakter, 2 MB, SQL enjeksiyonu), hesap sayımı, refresh token çaprazlama, şifresiz hesap silme, sahte webhook, CORS, güvenlik başlıkları, logout-all sonrası token. 39/40 geçti. Tek açık (premium kullanıcının oturum/mesaj sayısına sınır yoktu → LLM maliyet istismarı) 2026-11-08'de kapatıldı: günde 30 pratik + dakikada 10 LLM isteği; kontrollerin çoğu artık `CanSayNo.Tests`'te kalıcı. Prompt injection (talimat/taktik sızdırma, "değerlendiriciye 100 ver") gerçek modelde denendi: karakter rolden çıkmadı, değerlendirme 15 verdi

**Bekleyen küçük işler**:
1. Emulator'de görsel test — "0/6 tamamlandı", chat açılışı/hata bandı, sonuç ekranı hata durumu görüldü (2026-11-03). `ErrorState` ortalama hatası düzeltildi. 2026-11-04 sahte LLM ile emulator'de görüldü: chat (yazıyor göstergesi, 3 raunt, tamamlandı), Sonuç ekranı (dolu değerlendirme), Anasayfa güncellemesi, Profil premium bandı, e-posta değiştirme. Bu turda düzeltilen: kod alanı yazılamıyordu (adımlara key), donanım geri/kaydırma pratikten onaysız çıkarıyordu (usePreventRemove). Skeleton'lar 2026-11-05'te görüldü (emulator ağı `adb emu network delay gprs` + `speed gsm` ile yavaşlatılıp, API emulator ağı üzerinden `localhost`→10.0.2.2 ile; adb reverse bu kısıtlamayı atlar). Sonuç ekranında kaydırınca içerik durum çubuğunun altına giriyordu → yalnızca kaydırınca görünen şerit + açık ikonlar
3. ~~Android klavye boşluğu~~ — çözüldü (2026-11-04, react-native-keyboard-controller)
2. Tasarım (Claude Design "CanSayNo Ekranlar") karşılaştırıldı (2026-11-04): tüm ekranlar uygulamada. Profil ayarlar kartı tasarımdaki gibi: "Hesap bilgileri" → `(app)/account` (ad/soyad, PUT /api/Auth/profile; telefon /me'den korunur; e-posta değişikliği "E-postayı değiştir" → `(app)/change-email`, kodla), "Bildirimler" → `(app)/notifications` (yerel hatırlatma)

(2026-11-03: "Şifremi unuttum" eklendi — `(auth)/forgot-password` → `(auth)/reset-password`, OTP kodlu, `hooks/usePasswordReset.ts`; emulator'de uçtan uca doğrulandı. Şifre kuralı `lib/validation/password.ts`'te, kayıtla ortak. Dev'de kodun e-postayla gelmesi için `NotificationService:Mail` ayarı gerekli — boşsa mail gitmez; test için mailpit `localhost:1025`)

(2026-10-14: `is_005_maas_erteleme` tekrar `IsFree=true` yapıldı; test kullanıcıları (id 4, 8–17) silindi — DB'de sadece seed kullanıcıları 1–3 kaldı)

## Sıradaki Büyük İş: Sessions + LLM Proxy

Chat ekranı şu an kullanıcı ne yazarsa yazsın script'li cevap veriyor. Hedef:

- `POST /api/sessions` — oturum başlat, DailyUsage ile ücretsiz limit kontrolü
- `POST /api/sessions/{id}/messages` — kullanıcı mesajı → Anthropic API → karakter cevabı
- `POST /api/sessions/{id}/evaluate` — 5 eksende değerlendirme, `Session.Evaluation`'a yaz, `UserStats` güncelle
- `GET /api/sessions/{id}`, `GET /api/sessions/my`
- Anthropic API key sadece backend'de (user secrets / appsettings.Development.json), asla mobile'da
- Sonra mobile chat ekranını bu endpoint'lere bağla, `aiResponses.ts`'i sil

**Alınan kararlar (2026-10-14)**:
- **Model**: karakter cevapları ve değerlendirme için `claude-sonnet-5-5` (resmi Anthropic C# SDK)
- **Ücretsiz limit**: günde **1 pratik** (`DailyUsage`); premium günde 30 (adil kullanım, 2026-11-08)
- **Değerlendirme şeması** yeni Sonuç ekranına göre: 5 eksen (netlik, nezaket, dayaniklilik, alternatifSunma, ozurDengesi) **0–100**, `strengths[]`, `improvements[]`, `coachNote`, opsiyonel `idealResponseExample`, `totalScore` 0–100. Mevcut `SessionEvaluation` buna göre güncellenecek (JSONB — migration muhtemelen gerekmez). Structured outputs ile alınacak
- **Karakter prompt'u**: sabit kural bloğu (cache'lenir) + senaryo bloğu (sahne, karakter, arketip, o rauntun taktikleri). Kısa, doğal, Türkçe mesajlaşma dili
- **Raunt yönetimi**: backend sayar; her kullanıcı mesajı = 1 raunt, o rauntun `PressureTactics` listesi prompt'a girer; `MaxRounds` dolunca kapanış mesajı + session Completed
- **Limitler**: cevap başına düşük `max_tokens`, kullanıcı mesajı ≤ 500 karakter (backend de doğrular), session başına token tavanı (`TokensUsed`)

**Uygulama sırası**: (1) Sessions CRUD + günlük limit (LLM'siz) → (2) messages endpoint + Anthropic çağrısı → (3) evaluate + UserStats → (4) mobile chat/sonuç bağlama, mock'ları silme → (5) Anasayfa/Profil gerçek istatistik.

**Durum (2026-11-04)**: 1–5 yazıldı. 5. adım (GET /api/Stats/me + Anasayfa/Profil) API ve emulator'de doğrulandı. 1–4: LLM'siz yollar API ve emulator'de doğrulandı (açılış mesajı, 500 karakter, 400/404/409/503, sayaçlar, hata UI'ı). **Gerçek LLM ile doğrulandı (2026-11-07)**: `Anthropic:ApiKey` user-secrets'ta (`dotnet user-secrets set "Anthropic:ApiKey" "..." --project CanSayNo.API`; Max aboneliği API'yi kapsamaz, console.anthropic.com kredisi). 2 uçtan uca pratik (is_001 güçlü cevaplar → 77 puan, is_005 pes eden cevaplar → 18 puan): karakter taktikleri doğal uyguluyor, kapanış doğru, değerlendirme ayırt ediyor. Cevap 2–5 sn, değerlendirme ~10 sn. Pratik başına ~11.5k token (DB `TokensUsed`), gerçek maliyet ≈ $0.03. Cache çalışıyor: karakter sabit bloğu (~2.1k) 2. rauntan itibaren `cache_read`; değerlendirme bloğu (~1.9k) 5 dk içinde başka oturumlarla da paylaşılıyor. Test betiği: kayıt → mailpit doğrulama → 3 mesaj → evaluate → `DELETE /api/Account`.
- Backend: `CharacterPromptBuilder` / `EvaluationPromptBuilder` (prompt'lar), `AnthropicClientProvider` + `AnthropicCharacterChatService` + `AnthropicSessionEvaluator` (Infrastructure/Services/Ai), `UserProgress` (seri/XP/seviye/rozet, saf fonksiyon). Ayarlar: `Anthropic` (Model, MaxTokens 1024, Effort low; değerlendirme medium/4096), `Hayirci` (MaxUserMessageLength 500, SessionTokenLimit 40000)
- Açılış mesajı `Scene.OpeningMessage` (JSONB); seed açılışta boş olanları doldurur
- Hayırcı'ya özel hata kodları 1100+: AiUnavailable 503, AiResponseRefused 422, SessionNotActive 409, SessionTokenLimitReached 429, SessionNotCompleted 409
- **Eşzamanlılık (2026-11-04)**: `ISessionLock` (bellek içi, oturum başına; tek API örneğinde geçerli). Mesaj: kilit doluysa beklemeden 409 `sessionBusy` (1105), LLM'e gitmez. Değerlendirme: ikinci istek 85 sn'ye kadar bekler, sonra kayıtlı sonucu döner (LLM ve XP bir kez). Yatay ölçeklenirse DB kilidine geçilmeli
- **Sahte Anthropic ile test**: `Anthropic:BaseUrl` (boş = gerçek API) yerel bir sahte sunucuya yönlendirilebilir; anahtarsız uçtan uca akış böyle doğrulandı (3 raunt + kapanış + değerlendirme, istek şekli: model, effort, `cache_control`, json_schema). Sonnet 5.5'te `thinking` gönderilmez — varsayılan adaptive. Gerçek modelin cevap kalitesi ve gerçek `cache_read` hâlâ doğrulanmadı
- Rozetler: `WorkMaster` (İş bağlamında 10 değerlendirilmiş pratik) ve `ContextExplorer` (5 bağlamın hepsinde ≥1) backend'de kazanılıyor; Profil tasarım gereği yalnızca 4 rozet gösteriyor
- Bilinen eksikler: server-side refusal fallback eklenmedi (yalnızca cyber/frontier_llm'i kapsıyor)

## Admin Paneli (Hayırcı, 2026-11-06)

- **Backend** `HayirciAdminController` (`/api/admin`): `GET scenarios` (pasifler dahil, PressureTactics + kullanım istatistiği), `GET/POST/PUT scenarios/{id}`, `PUT scenarios/{id}/active`, `GET users/{id}/overview`, `GET dashboard`. İzinler `Scenarios.Read/Create/Update`, `Dashboard.Read` (startup seed Admin + SuperAdmin'e verir; kullanıcı özeti `Users.Read`). Okumalar `IAdminHayirciReadService` (Infrastructure, AsNoTracking), yazmalar UnitOfWork. Senaryo silme YOK (geçmiş oturumlar bağlı) — pasif yapılır. Bağlam değiştirilemez; yeni ID biçimi `<bağlam>_<3 hane>_<konu>`. Seed mevcut senaryoyu ezmediği için panelde yapılan düzenleme kalıcıdır
- Gösterge: uygulama kullanıcısı = `TenantId null`; gün sınırı TR; 14 günlük seri, 30 gün token. Maliyet KABA TAHMİN (`Hayirci:AdminEstimatedUsdPerMillionTokens`=3; girdi/çıktı ayrı tutulmuyor)
- **Frontend** (Nuxt): menü "Hayırcı › Senaryolar" (`pages/scenarios`, `components/Scenarios/ScenarioForm.vue`), Kullanıcı detayında "Hayırcı" sekmesi (`UserHayirciOverview.vue`), Anasayfa gerçek göstergeler (mock kartlar ve gelir grafiği silindi). API `composables/useHayirciAdmin.ts`, enum etiketleri `utils/hayirciLabels.ts`
- Kullanıcı durumu API'den metin gelir (`"active"`); `utils/userStatus.ts` çevirir — eskiden aktif kullanıcılar "Pasif" görünüyordu; frontend `UserStatus` enum'ı backend'e (0..3) hizalandı
- Çalıştırma: 3000'i Sethumo kullanıyor → `API_BASE_URL=http://localhost:5177 npx nuxt dev --port 3100 --host 127.0.0.1` + API'ye `CorsSettings__AllowedOrigins__0=http://localhost:3100`. `nuxt typecheck` çalışmaz (tsconfig yok). Emulator Chrome'unda dev modu donuyor; `nuxt build` + `node .output/server/index.mjs` ile denendi
- **Tenant görünürlüğü (2026-11-06)**: Admin kendi tenant'ına EK OLARAK tenant'sız (mobil) kullanıcıları görür/düzenler/siler — `TenantAdminVisibility.CanAccessUserTenant`, liste `GetUsersPagedAsync(includeTenantless: true)`. Başka tenant'lar ve SuperAdmin'ler Admin'e kapalı kalır
- **Marka (2026-11-08)**: Kickstart yeşili/amberi kaldırıldı. Primary marka mavisi (`theme.css` + `plugins/vuetify.client.ts`), secondary mercan `#FF7043` (koyu temada container üstü yazı açık mercan), büyük yüzeyler mor→mavi geçiş: `data.json` `theme.colors.primary` (→ `--theme-gradient*`, butonlar `btn-gradient-primary`, giriş/kayıt zeminleri), kenar çubuğu seçili öğesi (`layouts/default.vue` `.sidebar-active-pill`), giriş sayfası (`components/Auth/LoginPage.vue` `--lp-*`), profil başlığı ve gösterge profil kartı. Durum renkleri (başarı yeşili, bilgi moru, aktif anahtarı) değişmedi. Logo/favicon `design/brand/generate.mjs`'ten. Giriş sonrası sayfalar (gösterge, senaryolar, kullanıcılar, profil, koyu tema) headless Chrome + DevTools Protocol betiğiyle doğrulandı. Not: panel `127.0.0.1:3100`'den açılırsa CORS `localhost:3100`'e izin verdiği için "Network Error" — `localhost` kullan

## Mağaza Hazırlığı (2026-11-08)

- Bireysel geliştirici hesabı, iletişim `info@factech.app`. Alan adı `cansayno.com` (2026-11-08): site kök alan, API `api.`, ileride web uygulaması `app.` (cookie için aynı site şart)
- Taslaklar `docs/store/`: gizlilik (TR/EN, koddaki gerçek veri envanterine göre: reklam/analitik/crash SDK'sı YOK), kullanım şartları (TR/EN), Google hesap silme sayfası, mağaza metinleri + Veri güvenliği / App Privacy cevapları, `yayin-kontrol-listesi.md`. `[AD SOYAD]`, `[ADRES]`, sunucu/e-posta sağlayıcı yer tutucu
- Yayın engelleri: Google bireysel hesapta 12 tester × 14 gün kapalı test; canlı sunucu + SMTP yok; site yayında değil (cansayno.com DNS yok); EAS projesi bağlanmadı (`eas login` + `eas init` kullanıcıda)
- **Yapıldı (2026-11-08)**: yasal linkler paywall + kayıt + Profil'de (`lib/constants/legal.ts` adresler, `lib/utils/openLegalPage.ts` uygulama içi tarayıcı, `components/ui/LegalLinks.tsx`); `ios.supportsTablet: false` + `ITSAppUsesNonExemptEncryption: false`; `android.blockedPermissions` (depolama, biyometri — kullanılmıyor); `Mobile/eas.json` (development / preview APK staging / production `autoIncrement`, `appVersionSource: remote`, API `https://api.cansayno.com`). Premium "sınırsız" ifadeleri adil kullanımla (günde 30) uyumlu hale getirildi — paywall avantajı, limit mesajları (mobil + backend tr/en), mağaza abonelik açıklaması
- Uygulama yalnızca Türkçe — İngilizce listeleme çeviriden sonra
- Kararlar: yaş 16+, ücretsiz deneme YOK, iPad desteği kapalı başlayacak (Apple yayından sonra cihaz desteğini kaldırmaya izin vermiyor; sonradan açılabilir)
- **Web sitesi `Web/`** (statik, cansayno.com): `npm run build` → `dist/` (ana sayfa + `/gizlilik/`, `/kullanim-sartlari/`, `/hesap-silme/`, `/en/privacy/`, `/en/terms/`, 404). Yasal sayfalar `docs/store/*.md`'den `marked` ile üretilir — metni orada düzelt. `build:strict` doldurulmamış `[YER TUTUCU]` kalırsa durur (yayından önce bunu kullan). Renk/font uygulamayla aynı (Satoshi, `Mobile/assets/fonts`'tan kopyalanır). `npm run preview` → :4173. Headless Chrome 500px'ten dar pencere açmıyor; telefon görünümünü 375px iframe ile kontrol et. **Yayın**: Cloudflare Pages (alan adı Cloudflare'de), proje `cansayno`; `npm run deploy` = `build:strict` + `wrangler@4.151.3 pages deploy dist`. `src/_headers` (sıkı CSP — sitede JS yok; style-src 'unsafe-inline' yalnızca style özniteliği için), `src/_redirects` (/privacy, /terms, /delete-account). Adımlar `docs/store/yayin-kontrol-listesi.md`. Yasal metinlerde sağlayıcılar Hetzner (Almanya) + Brevo (Fransa) yazıyor — canlı sunucu bunlarla kurulacak

## Diğer Bekleyenler

- ~~Premium gating + RevenueCat webhook~~ — backend yapıldı (2026-11-04), sahte olaylarla 31 test geçti:
  - `POST /api/Webhooks/revenuecat` (anonim, Swagger'da gizli). `Authorization` başlığı `RevenueCat:WebhookAuthorization` ile sabit-zamanlı karşılaştırılır (ham değer ya da `Bearer <değer>` kabul); ayar boşsa 503. Ayarla: `dotnet user-secrets set "RevenueCat:WebhookAuthorization" "..." --project CanSayNo.API`. Webhook başlık/yeniden deneme belgesi okunamadı — gerçek panelde doğrula
  - Olay → durum: `RevenueCatEventApplier` (saf). Satın alma/yenileme/uncancel → Active; CANCELLATION → Cancelled (süre bitene kadar erişim sürer), `cancel_reason=CUSTOMER_SUPPORT` (iade) → hemen Expired; BILLING_ISSUE → GracePeriod; EXPIRATION → Expired. TEST/PRODUCT_CHANGE/PAUSED/TRANSFER durum değiştirmez. Satır `original_transaction_id` ile anahtarlanır; aynı/daha eski olay (`id`, `event_timestamp_ms`) yok sayılır; ham gövde `RawWebhookData`'ya. Kullanıcı `original_app_user_id` → `app_user_id` → `aliases` içindeki ilk sayısal ve var olan id; anonim kullanıcı yok sayılır. SANDBOX olaylar `RevenueCat:AcceptSandboxEvents=false` iken yok sayılır
  - Premium = Active/Cancelled/GracePeriod ve `ExpiresAt` gelecekte/null. StartSession premium kontrolünü boş oturumu yeniden kullanmadan ÖNCE yapar
  - `GET /api/Subscriptions/me` → `{isPremium, productId, status, expiresAt, willRenew}`. Mobil: `useMySubscription` — Profil premium bandı ("Premium aktif" + yenileme/bitiş tarihi), Bugünkü görev premium'da tüm senaryolardan seçer
  - **Mobil satın alma (2026-11-05)**: `react-native-purchases` + `expo-dev-client` → artık **development build** (`npm run android` = `expo run:android`; Expo Go'da SDK "browser mode"a düşer, satın alma yok). Paket kimliği `com.cansayno.app` (iOS+Android, değiştirilemez). Tek temas noktası `lib/purchases/purchases.ts`: SDK yalnızca girişli kullanıcıyla `appUserID = String(User.Id)` yapılandırılır, çıkışta `logOut` (`hooks/usePurchasesIdentity.ts`, kök layout). Paywall fiyatları `current` offering'in `annual`/`monthly` paketlerinden (`hooks/usePremiumPurchase.ts`); satın alma/geri yükleme sonrası `/api/Subscriptions/me` 8×2 sn yoklanır, premium görünürse `scenarioKeys` yenilenir, görünmezse "birkaç dakika içinde" denir. Anahtarlar `EXPO_PUBLIC_REVENUECAT_IOS_KEY` / `EXPO_PUBLIC_REVENUECAT_ANDROID_KEY` (public SDK anahtarı; boşsa paywall "Satın alma bu sürümde kapalı"). Android `MainActivity` launchMode `singleTop` (`plugins/withAndroidLaunchMode.js`; singleTask ödeme doğrulamasında satın almayı iptal edebiliyor). `android/` `ios/` üretilir, gitignore'da
  - Eksik: RevenueCat anahtarları, ürün ID'leri, offering (annual + monthly), webhook secret; gerçek/Test Store satın alma hiç denenmedi (2026-11-05: development build emulator'da açıldı, giriş + paywall "Satın alma bu sürümde kapalı" durumu görüldü, native hata yok). Geri yükleme başka hesaba bağlı aboneliği TRANSFER olayıyla taşır — backend TRANSFER'ı şu an yok sayıyor
- ~~Hesap silme + veri dışa aktarma~~ — yapıldı (2026-11-04): `GET /api/Account/export` (JSON; şifre hash'i/token/ham webhook yok), `DELETE /api/Account` (gövde `{password}`, KALICI silme — FK'ler CASCADE, LoginAttempts e-postayla; son SuperAdmin 403). Mobil: Hesap bilgileri ekranında "Verilerimi indir" (expo-file-system + expo-sharing, paylaşım menüsü) ve iki adımlı "Hesabımı sil". Silme mağaza aboneliğini iptal etmez — kullanıcıya söyleniyor
- ~~Mobile için refresh token ömrü~~ — mobil login her zaman `rememberMe: true` gönderiyor → 60 gün (`RefreshTokenExpiryInDaysRememberMe`). Yenileme bitiş tarihini korur (kayan süre yok): 60 gün sonra yeniden giriş
- Granular permission (`Scenarios.Read`)
- Karakter illüstrasyonları — Leonardo.ai ile 2 örnek üretildi ama stilleri tutarsız; şimdilik Phosphor placeholder
