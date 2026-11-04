// Tip-güvenli ortam değişkeni erişimi. Uygulamanın TEK env okuma noktası —
// başka hiçbir yer Constants.expoConfig.extra'ya veya process.env'e doğrudan
// dokunmamalı. Değerler app.config.ts → extra üzerinden gelir.

import Constants from 'expo-constants';
import { Platform } from 'react-native';

interface AppEnv {
  apiBaseUrl: string;
  // Platformun RevenueCat public SDK anahtarı; yoksa null (satın alma kapalı).
  revenueCatApiKey: string | null;
  appEnv: 'development' | 'staging' | 'production';
  isDev: boolean;
  isProd: boolean;
}

function getApiBaseUrl(): string {
  const configuredUrl = Constants.expoConfig?.extra?.apiBaseUrl as
    | string
    | undefined;

  if (!configuredUrl) {
    console.warn('[env] API URL yapılandırılmamış, localhost fallback kullanılıyor');
    return 'http://localhost:5156';
  }

  // Platform-aware: Android emulator host makinenin localhost'una 10.0.2.2 ile ulaşır.
  if (Platform.OS === 'android' && configuredUrl.includes('localhost')) {
    return configuredUrl.replace('localhost', '10.0.2.2');
  }

  return configuredUrl;
}

function getRevenueCatApiKey(): string | null {
  const extra = Constants.expoConfig?.extra;
  const key = (Platform.OS === 'ios' ? extra?.revenueCatIosKey : extra?.revenueCatAndroidKey) as
    | string
    | undefined;
  return key ? key : null;
}

export const env: AppEnv = {
  apiBaseUrl: getApiBaseUrl(),
  revenueCatApiKey: getRevenueCatApiKey(),
  appEnv:
    (Constants.expoConfig?.extra?.appEnv as AppEnv['appEnv']) || 'development',
  get isDev() {
    return this.appEnv === 'development';
  },
  get isProd() {
    return this.appEnv === 'production';
  },
};
