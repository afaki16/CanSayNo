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
- Auth (Kickstart): `/api/Auth/login`, `/register`, `/refresh-token` (body `{accessToken, refreshToken}` — ikisi de zorunlu), `/logout` (body `{refreshToken, reason}`), `/me`
- Rate limiter'lar aktif (login, sensitive, refresh)

## Mobile

- Expo Router (file-based), Zustand, TanStack Query, axios, react-hook-form + zod, expo-secure-store, phosphor-react-native, Reanimated
- `lib/api/`: client (Bearer interceptor), refreshInterceptor (401 → refresh → retry; eşzamanlı 401'ler tek refresh; login/register refresh akışının dışında), queryClient, `apiGet` / `apiPost` / `apiPostVoid`, endpoint sabitleri `endpoints.ts`
- `lib/api/services/`: authService, scenarioService, hata → Türkçe mesaj eşlemeleri
- `hooks/queries/useScenarios.ts`: hiyerarşik `scenarioKeys` — session yazdıktan sonra `invalidateQueries({ queryKey: scenarioKeys.all })`
- `stores/authStore.ts`: secure storage; sadece `StoredUser` saklanır (roles/permissions değil)
- **Gerçek veriden**: auth, senaryo listesi/detayı, bağlam kartı sayıları
- **Hâlâ mock**: chat AI cevapları (`lib/mockData/aiResponses.ts`, gerçek senaryo ID'leriyle anahtarlı), `userStats`, `recentActivity`, sonuç ekranı değerlendirmesi (`evaluation.ts`)
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

**Bekleyen küçük işler**:
1. Emulator'de görsel test — yeni tasarımın tüm ekranları + senaryo ekranlarının skeleton, error state, "Tekrar Dene" ve "0/6 tamamlandı" davranışları ekranda gözle görülmedi
2. Tasarımda olup henüz eklenmeyenler: "Şifremi unuttum" (mobil ekran yok, backend endpoint var), profilde "Hesap bilgileri" / "Bildirimler" satırları (hedef ekran yok)

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

## Diğer Bekleyenler

- Premium gating (backend tarafında erişim kontrolü)
- RevenueCat webhook handler (`RawWebhookData` yazarken JSON validasyonu)
- Hesap silme + veri dışa aktarma (App Store / KVKK zorunlu)
- Mobile auth guard (`app/(app)/_layout.tsx` isAuthenticated kontrolü yok)
- Rehydrate'te token'ı `/api/Auth/me` ile doğrulama
- Mobile için refresh token ömrü (şu an 7 gün, 30–90 gün olmalı)
- Granular permission (`Scenarios.Read`)
- Karakter illüstrasyonları — Leonardo.ai ile 2 örnek üretildi ama stilleri tutarsız; şimdilik Phosphor placeholder
