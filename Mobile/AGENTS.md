# CanSayNo

AI destekli "hayır deme pratiği" mobil uygulaması. Kullanıcılar text-based roleplay
senaryolarında asertif iletişim ve sınır koymayı pratik eder. İleride sesli pratik,
gamification ve RevenueCat ile subscription eklenecek.

## Stack

- Expo SDK 54 + Expo Router 6 (file-based routing)
- React Native 0.81, React 19
- TypeScript
- Inline `StyleSheet` (NativeWind YOK — ileride değerlendirilebilir)

## Expo dokümantasyonu

Expo HAS CHANGED. Kod yazmadan önce sürüm dokümanlarına bak:
https://docs.expo.dev/versions/v54.0.0/

## Klasör yapısı

```
app/                Expo Router routes (file-based)
  _layout.tsx       Root Stack, SafeAreaProvider, StatusBar
  index.tsx         Karşılama ekranı (entry)
  (auth)/           Auth gerektirmeyen route grubu (login, register)
  (app)/            Auth gerektiren route grubu (home, ...)
components/         Paylaşılan UI bileşenleri
  ui/               Genel UI primitive'ler (Button, Input, ...)
lib/                Domain-agnostic kod
  api/              API client (axios instance, interceptors)
  constants/        Sabitler (API URL'leri, enum'lar)
  utils/            Yardımcı fonksiyonlar
hooks/              Custom React hook'ları
stores/             Zustand store'ları
types/              TypeScript type tanımları
constants/
  theme.ts          Tasarım sistemi: Colors, Spacing, FontSize, Radius
assets/             icon, splash, font dosyaları
scripts/            Yardımcı scriptler (reset-project.js, vs.)
```

## Konvansiyonlar

- **Türkçe içerik, İngilizce kod**: Kullanıcıya görünen tüm metinler (UI label, error
  mesajı, vs.) Türkçe. Değişken/fonksiyon/dosya adları İngilizce.
- **TypeScript zorunlu**: Tüm component dosyaları `.tsx`.
- **Inline style**: `StyleSheet.create` kullan, NativeWind yok.
- **Route grupları**: Auth durumuna göre `(auth)` / `(app)` ayrımı; parantezli klasör
  isimleri URL'e yansımaz.
- **Theme tokens**: Renk/spacing/font değerlerini `constants/theme.ts` üzerinden çek,
  magic number kullanma.
