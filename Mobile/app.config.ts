import { ConfigContext, ExpoConfig } from 'expo/config';

// Expo dinamik config. Eskiden app.json'daydı; ortam değişkenlerini
// `extra` altına taşıyabilmek için TypeScript config'e çevrildi.
// Tüm runtime env erişimi tek noktadan: lib/config/env.ts → Constants.expoConfig.extra.

export default ({ config }: ConfigContext): ExpoConfig => ({
  ...config,
  name: 'CanSayNo',
  slug: 'CanSayNo',
  version: '1.0.0',
  orientation: 'portrait',
  icon: './assets/images/icon.png',
  scheme: 'cansayno',
  userInterfaceStyle: 'automatic',
  newArchEnabled: true,
  ios: {
    // iPad'de iPhone görünümünde çalışır. Ekranlar iPad için tasarlanmadı; Apple yayından sonra
    // desteklenen cihazı KALDIRMAYA izin vermez, ama sonradan eklemeye izin verir → kapalı başla.
    supportsTablet: false,
    infoPlist: {
      // Yalnızca HTTPS (standart, muaf şifreleme) — her yüklemede ihracat uyumluluğu sorusu sorulmaz.
      ITSAppUsesNonExemptEncryption: false,
    },
    // Mağaza kimliği — yayından sonra DEĞİŞTİRİLEMEZ (RevenueCat ürünleri de buna bağlı).
    bundleIdentifier: 'com.cansayno.app',
  },
  android: {
    package: 'com.cansayno.app',
    // İkon dosyaları design/brand/generate.mjs ile üretilir; elle değiştirme, betiği çalıştır. Arka plan görseli
    // mor→mavi geçiş; düz renk yalnızca görsel yüklenemezse görünür.
    adaptiveIcon: {
      backgroundColor: '#5B4BE0',
      foregroundImage: './assets/images/android-icon-foreground.png',
      backgroundImage: './assets/images/android-icon-background.png',
      monochromeImage: './assets/images/android-icon-monochrome.png',
    },
    // Paketlerin eklediği ama kullanılmayan izinler (Play incelemesinde soru doğurur):
    // depolama — veri dışa aktarımı yalnızca uygulamanın önbelleğine yazar (hooks/useAccountData.ts);
    // biyometri — expo-secure-store requireAuthentication kullanılmıyor (lib/storage/secureStorage.ts).
    blockedPermissions: [
      'android.permission.READ_EXTERNAL_STORAGE',
      'android.permission.WRITE_EXTERNAL_STORAGE',
      'android.permission.USE_BIOMETRIC',
      'android.permission.USE_FINGERPRINT',
    ],
    edgeToEdgeEnabled: true,
    predictiveBackGestureEnabled: false,
  },
  web: {
    output: 'static',
    favicon: './assets/images/favicon.png',
  },
  plugins: [
    'expo-router',
    [
      'expo-splash-screen',
      {
        // Logo (yuvarlak kare) uygulamanın zemin renginde; koyu temada mürekkep rengi.
        image: './assets/images/splash-icon.png',
        imageWidth: 120,
        resizeMode: 'contain',
        backgroundColor: '#F8F7F4',
        dark: {
          backgroundColor: '#16181D',
        },
      },
    ],
    'expo-secure-store',
    'expo-font',
    // Yerel hatırlatmalar. Android küçük ikon: tek renk (beyaz + şeffaf) olmalı.
    [
      'expo-notifications',
      { icon: './assets/images/android-icon-monochrome.png', color: '#5B4BE0' },
    ],
    // RevenueCat: Android'de varsayılan singleTask, kullanıcı ödemeyi doğrulamak için
    // uygulamadan çıkıp dönünce satın almayı iptal edebiliyor.
    './plugins/withAndroidLaunchMode',
    // Windows'ta uzun C++ yolları için yeni CMake/ninja (LongPathsEnabled=1 gerekir).
    './plugins/withAndroidCmakeVersion',
  ],
  experiments: {
    typedRoutes: true,
    reactCompiler: true,
  },
  extra: {
    // Tüm ortam değişkenleri burada — Constants.expoConfig.extra ile erişilir.
    // Tip-güvenli erişim için lib/config/env.ts kullan, doğrudan okuma.
    apiBaseUrl: process.env.EXPO_PUBLIC_API_URL || 'http://localhost:5156',
    appEnv: process.env.EXPO_PUBLIC_APP_ENV || 'development',
    // RevenueCat public SDK anahtarları (gizli değil; secret/webhook anahtarı ASLA buraya girmez).
    // Boşsa satın alma kapalı kalır — paywall bunu gösterir.
    revenueCatIosKey: process.env.EXPO_PUBLIC_REVENUECAT_IOS_KEY || '',
    revenueCatAndroidKey: process.env.EXPO_PUBLIC_REVENUECAT_ANDROID_KEY || '',
  },
});
