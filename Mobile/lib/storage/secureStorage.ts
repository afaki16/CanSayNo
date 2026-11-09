// expo-secure-store wrapper'ı. Token ve hassas veriyi şifrelenmiş halde
// saklamak için TEK erişim noktası — uygulamanın başka hiçbir yeri
// expo-secure-store'u doğrudan import etmemeli.
//
// Tüm metotlar hata durumunda uygulamayı çökertmek yerine null/false döner.

import * as SecureStore from 'expo-secure-store';

export const STORAGE_KEYS = {
  ACCESS_TOKEN: 'auth.accessToken',
  REFRESH_TOKEN: 'auth.refreshToken',
  USER: 'auth.user',
  // Hatırlatma tercihi cihaza aittir; çıkışta silinmez.
  REMINDER_PREFS: 'prefs.reminder',
  // Uygulama dili ("tr" | "en"); cihaza aittir, çıkışta silinmez. Yoksa cihaz dili kullanılır.
  LANGUAGE: 'prefs.language',
} as const;

export type StorageKey = (typeof STORAGE_KEYS)[keyof typeof STORAGE_KEYS];

// NOT: requireAuthentication KULLANILMIYOR — Expo Go'da desteklenmiyor.

async function set(key: string, value: string): Promise<void> {
  try {
    await SecureStore.setItemAsync(key, value);
  } catch (error) {
    console.error(`[secureStorage] set("${key}") başarısız:`, error);
  }
}

async function get(key: string): Promise<string | null> {
  try {
    return await SecureStore.getItemAsync(key);
  } catch (error) {
    console.error(`[secureStorage] get("${key}") başarısız:`, error);
    return null;
  }
}

async function remove(key: string): Promise<void> {
  try {
    await SecureStore.deleteItemAsync(key);
  } catch (error) {
    console.error(`[secureStorage] delete("${key}") başarısız:`, error);
  }
}

async function setJSON<T>(key: string, obj: T): Promise<void> {
  try {
    await set(key, JSON.stringify(obj));
  } catch (error) {
    console.error(`[secureStorage] setJSON("${key}") başarısız:`, error);
  }
}

async function getJSON<T>(key: string): Promise<T | null> {
  const raw = await get(key);
  if (raw === null) {
    return null;
  }
  try {
    return JSON.parse(raw) as T;
  } catch (error) {
    console.error(`[secureStorage] getJSON("${key}") parse hatası:`, error);
    return null;
  }
}

export const secureStorage = {
  set,
  get,
  delete: remove,
  setJSON,
  getJSON,
};
