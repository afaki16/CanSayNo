# CanSayNo (Hayırcı) — Proje Rehberi

Claude Code bu dosyayı her oturumda otomatik okur. Ürün tasarımı ve ilk geliştirme fazı claude.ai'da yapıldı; geliştirme artık buradan devam ediyor.

## Ürün

**Hayırcı** (kod adı CanSayNo): Kullanıcının AI karakterlerle rol yaparak "hayır demeyi" pratik ettiği mobil uygulama. Hedef: Türkiye + global. Solo geliştirici: Alperen.

- 5 bağlam: `is` (İş & Kariyer — ÜCRETSİZ), `aile`, `arkadas`, `romantik`, `satici` (dördü premium)
- 7 karakter arketipi: babacanManipulator, duygusalKurban, agresifIsrarci, tatliDilliSatici, masumRicaci, pasifAgresif, otoriter
- 13 baskı taktiği (`PressureTactic` enum), 5 değerlendirme ekseni: netlik, nezaket, dayaniklilik, alternatifSunma, ozurDengesi
- Freemium: İş bağlamı tamamen ücretsiz, diğer 4 bağlam premium (RevenueCat, aylık ₺49.99 placeholder)
- v1.0 hedefi: ~50 senaryo
- Tasarım dili: friendly playful (Duolingo gamification + Headspace olgunluğu)

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
   - **Port çakışması**: Alperen'in Kickstart.API projesi de 5156'yı kullanıyor. Test öncesi `Get-NetTCPConnection -LocalPort 5156` ile kontrol et; `/api/Sessions/my` yetkisiz istekte 401 değil 404 dönüyorsa çalışan backend CanSayNo değildir
   - Otomatik migration + idempotent seed startup'ta çalışır
3. **Mobile**: `cd Mobile` → `npm start` → `a` ile Android emulator
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
- Seed: `Persistence/Seeding/HayirciSeedData.cs`, runtime + idempotent (HasData değil). DB'de 7 senaryo: `is_001`…`is_006` + `aile_001_evlenmek`
- `PressureTactics` hiçbir DTO'da yok — sadece AI prompt'una girer, client'a asla sızmamalı

### Mevcut API

- `GET /api/Scenarios` (?contextId, ?isFreeOnly), `GET /api/Scenarios/{id}`, `GET /api/Scenarios/by-context/{contextId}` — hepsi `[Authorize]`
- Kullanıcıya özel alanlar (isCompleted, bestScore, attemptsCount, lastAttemptAt) Sessions'tan tek GroupBy sorgusuyla hesaplanıyor (N+1 yok)
- Sessions (`[Authorize]`): `POST /api/Sessions` (body `{scenarioId}`; premium senaryo + abonelik yok → 403 `premiumRequired`, ücretsiz günlük limit → 429 `dailyLimitReached`; aynı senaryoda henüz mesaj atılmamış açık oturum varsa yenisi açılmaz, o döner), `GET /api/Sessions/{id}` (başkasının oturumu → 404), `GET /api/Sessions/my?page&pageSize` (pageSize ≤ 50)
- Limit ayarları `appsettings.json` → `Hayirci` (`FreeDailySessionLimit`, `UsageDayUtcOffsetHours`=3 → gün TR saatiyle döner). Kullanım `DailyUsage`'a herkes için yazılır, limit sadece premium olmayana uygulanır
- Kickstart `ValidationBehavior` `PagedResult<T>` için failure üretemiyor (cast hatası → 500) — sayfalı sorgulara validator ekleme, handler'da clamp et
- Auth (Kickstart): `/api/Auth/login`, `/register`, `/refresh-token` (body `{accessToken, refreshToken}` — ikisi de zorunlu), `/logout` (body `{refreshToken, reason}`), `/me`
- Rate limiter'lar aktif (login, sensitive, refresh)

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
- **Hâlâ sabit**: Anasayfa "Bugünkü görev" (`is_001`), profil satırları "Hesap bilgileri"/"Bildirimler" yok
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

**Yeni tasarım uygulandı (2026-10-14)**: Claude Design "CanSayNo Ekranlar" — mavi `#2D7FF9` + mercan palet, Satoshi font (`assets/fonts/`, `useFonts` ile; ağırlık → dosya eşlemesi `components/ui/Text.tsx`'te — RN `Text` yerine her yerde bunu kullan). Tüm ekranlar yenilendi + yeni Sonuç ekranı (`scenarios/result/[scenarioId]`, mock `lib/mockData/evaluation.ts`). Marka adı UI'da "CanSayNo". Paywall fiyatı ₺79 aylık / ₺499 yıllık. Tüm bu iş `feature/scenarios-redesign` branch'inde commit'lendi (main'e henüz merge edilmedi). Kayıtta "Şifre tekrar" alanı kaldırıldı (confirmPassword = password gönderiliyor).

**Kickstart template yükseltmeleri (2026-11-03)**: `../Kickstart/TEMPLATE_UPGRADES.md`'den 11, 14, 15, 16, 17, 18, 21 ve "refresh reuse detection" kontrol maddesi uygulandı, API ile doğrulandı. Yeni migration'lar: `DropPermissionResourceTypeUniqueIndex`, `AddFailedAttemptsToUserVerificationToken`. Yeni uçlar: `PUT /api/Auth/profile`, `PUT /api/Auth/profile/email`, `POST /api/Auth/confirm-email-change`, `POST /api/Auth/forgot-password/code`, `reset-password/code`, `password-change/code`, `password-change/confirm`, `check-availability`. Alınmayanlar: multi-tenant maddeleri (3, 12, 13, 22, 23), 20, Nuxt maddeleri, 2, 19, 24; 9 farklı biçimde zaten var (string hata kodları). Kickstart'tan madde alırken dikkat: CanSayNo'da `ErrorCode` 1014/1015 = DailyLimitReached/PremiumRequired (Kickstart'ta başka anlamda). Kapsam notu: seed kullanıcıları `tenantId=1`, mobil kullanıcılar `null`; `check-availability` ve e-posta değişikliği yalnızca aynı tenant kapsamında çakışma arar. E-posta değişikliği onay linki (`/auth/confirm-email-change`) için henüz web/mobil ekranı yok.

**Bekleyen küçük işler**:
1. Emulator'de görsel test — "0/6 tamamlandı", chat açılışı/hata bandı, sonuç ekranı hata durumu görüldü (2026-11-03). `ErrorState` ortalama hatası düzeltildi. Skeleton'lar ve diğer ekranlar hâlâ gözle görülmedi
3. Android'de klavye kapandıktan sonra input'un altında boşluk kalıyor (`KeyboardAvoidingView` + edge-to-edge; chat ve auth ekranları). `behavior="padding"` çözmedi; muhtemelen `react-native-keyboard-controller` gerekiyor
2. Tasarımda olup henüz eklenmeyenler: profilde "Hesap bilgileri" / "Bildirimler" satırları (hedef ekran yok; "Hesap bilgileri" için backend `PUT /api/Auth/profile` hazır)

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
- **Ücretsiz limit**: günde **1 pratik** (`DailyUsage`); premium sınırsız
- **Değerlendirme şeması** yeni Sonuç ekranına göre: 5 eksen (netlik, nezaket, dayaniklilik, alternatifSunma, ozurDengesi) **0–100**, `strengths[]`, `improvements[]`, `coachNote`, opsiyonel `idealResponseExample`, `totalScore` 0–100. Mevcut `SessionEvaluation` buna göre güncellenecek (JSONB — migration muhtemelen gerekmez). Structured outputs ile alınacak
- **Karakter prompt'u**: sabit kural bloğu (cache'lenir) + senaryo bloğu (sahne, karakter, arketip, o rauntun taktikleri). Kısa, doğal, Türkçe mesajlaşma dili
- **Raunt yönetimi**: backend sayar; her kullanıcı mesajı = 1 raunt, o rauntun `PressureTactics` listesi prompt'a girer; `MaxRounds` dolunca kapanış mesajı + session Completed
- **Limitler**: cevap başına düşük `max_tokens`, kullanıcı mesajı ≤ 500 karakter (backend de doğrular), session başına token tavanı (`TokensUsed`)

**Uygulama sırası**: (1) Sessions CRUD + günlük limit (LLM'siz) → (2) messages endpoint + Anthropic çağrısı → (3) evaluate + UserStats → (4) mobile chat/sonuç bağlama, mock'ları silme → (5) Anasayfa/Profil gerçek istatistik.

**Durum (2026-11-04)**: 1–5 yazıldı. 5. adım (GET /api/Stats/me + Anasayfa/Profil) API ve emulator'de doğrulandı. 1–4: LLM'siz yollar API ve emulator'de doğrulandı (açılış mesajı, 500 karakter, 400/404/409/503, sayaçlar, hata UI'ı). **Gerçek LLM cevabı ve değerlendirme henüz hiç çalıştırılmadı** — `Anthropic:ApiKey` yok (`dotnet user-secrets set "Anthropic:ApiKey" "..." --project CanSayNo.API`). Anahtar gelince: 3 raunt uçtan uca + kapanış + değerlendirme + `cache_read` kontrolü.
- Backend: `CharacterPromptBuilder` / `EvaluationPromptBuilder` (prompt'lar), `AnthropicClientProvider` + `AnthropicCharacterChatService` + `AnthropicSessionEvaluator` (Infrastructure/Services/Ai), `UserProgress` (seri/XP/seviye/rozet, saf fonksiyon). Ayarlar: `Anthropic` (Model, MaxTokens 1024, Effort low; değerlendirme medium/4096), `Hayirci` (MaxUserMessageLength 500, SessionTokenLimit 40000)
- Açılış mesajı `Scene.OpeningMessage` (JSONB); seed açılışta boş olanları doldurur
- Hayırcı'ya özel hata kodları 1100+: AiUnavailable 503, AiResponseRefused 422, SessionNotActive 409, SessionTokenLimitReached 429, SessionNotCompleted 409
- Bilinen eksikler: aynı rauntta eşzamanlı iki istek ikisi de LLM'e gidebilir (token tavanı sınırlıyor); `WorkMaster` / `ContextExplorer` rozetleri yok; server-side refusal fallback eklenmedi (yalnızca cyber/frontier_llm'i kapsıyor)

## Diğer Bekleyenler

- Premium gating (backend tarafında erişim kontrolü)
- RevenueCat webhook handler (`RawWebhookData` yazarken JSON validasyonu)
- Hesap silme + veri dışa aktarma (App Store / KVKK zorunlu)
- Mobile için refresh token ömrü (şu an 7 gün, 30–90 gün olmalı)
- Granular permission (`Scenarios.Read`)
- Karakter illüstrasyonları — Leonardo.ai ile 2 örnek üretildi ama stilleri tutarsız; şimdilik Phosphor placeholder
