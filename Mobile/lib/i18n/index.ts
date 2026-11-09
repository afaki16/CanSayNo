// Uygulama dili (i18next). Kaynak dil Türkçe; İngilizce dosyalar aynı şekli taşımak ZORUNDA
// (tip kontrolü eksik/fazla anahtarı yakalar, bkz. locales/types.ts).
//
// Dil seçimi: kullanıcının kaydettiği tercih → yoksa cihaz dili (Türkçe ise tr, değilse en).
// Seçilen dil her API isteğinde Accept-Language olarak gider (lib/api/client.ts); backend senaryo
// listesini ve hata mesajlarını buna göre döner.
//
// Bileşenlerde: const { t } = useTranslation(); t('profile.title')
// Bileşen dışı saf fonksiyonlarda: import { t } from '@/lib/i18n' (çağrı anındaki dil).

import i18n from 'i18next';
import { getLocales } from 'expo-localization';
import { initReactI18next } from 'react-i18next';

import { secureStorage, STORAGE_KEYS } from '@/lib/storage/secureStorage';

import { resources } from './resources';

export type AppLanguage = 'tr' | 'en';
export const APP_LANGUAGES: readonly AppLanguage[] = ['tr', 'en'];

function isAppLanguage(value: unknown): value is AppLanguage {
  return value === 'tr' || value === 'en';
}

/** Cihaz dili Türkçe ise Türkçe, diğer her dilde İngilizce. */
export function deviceLanguage(): AppLanguage {
  return getLocales()[0]?.languageCode === 'tr' ? 'tr' : 'en';
}

i18n.use(initReactI18next).init({
  resources,
  lng: deviceLanguage(),
  fallbackLng: 'tr',
  interpolation: { escapeValue: false }, // React zaten kaçırır
  returnNull: false,
});

/** Şu anki dil (yalnızca tr | en). */
export function currentLanguage(): AppLanguage {
  return isAppLanguage(i18n.language) ? i18n.language : 'tr';
}

/** Açılışta kayıtlı tercihi uygular; yoksa cihaz dili kalır. Kök layout bekler. */
export async function loadStoredLanguage(): Promise<void> {
  const saved = await secureStorage.get(STORAGE_KEYS.LANGUAGE);
  if (isAppLanguage(saved) && saved !== i18n.language) {
    await i18n.changeLanguage(saved);
  }
}

/** Kullanıcının dil seçimi: uygular ve cihazda saklar (çıkışta silinmez). */
export async function setAppLanguage(language: AppLanguage): Promise<void> {
  await i18n.changeLanguage(language);
  await secureStorage.set(STORAGE_KEYS.LANGUAGE, language);
}

/** Tarih/sayı biçimlendirme için yerel ayar. */
export function currentLocale(): string {
  return currentLanguage() === 'en' ? 'en-US' : 'tr-TR';
}

export const t = i18n.t.bind(i18n);
export default i18n;
