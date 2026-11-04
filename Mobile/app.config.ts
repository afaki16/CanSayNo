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
    supportsTablet: true,
    // Mağaza kimliği — yayından sonra DEĞİŞTİRİLEMEZ (RevenueCat ürünleri de buna bağlı).
    bundleIdentifier: 'com.cansayno.app',
  },
  android: {
    package: 'com.cansayno.app',
    adaptiveIcon: {
      backgroundColor: '#E6F4FE',
      foregroundImage: './assets/images/android-icon-foreground.png',
      backgroundImage: './assets/images/android-icon-background.png',
      monochromeImage: './assets/images/android-icon-monochrome.png',
    },
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
        image: './assets/images/splash-icon.png',
        imageWidth: 200,
        resizeMode: 'contain',
        backgroundColor: '#ffffff',
        dark: {
          backgroundColor: '#000000',
        },
      },
    ],
    'expo-secure-store',
    'expo-font',
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
