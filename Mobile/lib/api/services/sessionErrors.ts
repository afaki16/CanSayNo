// Pratik oturumu isteklerinin kullanıcıya gösterilecek Türkçe karşılıkları.
// Premium ve günlük limit hataları ayrı "kind" olarak döner — ekranlar bunlara
// özel aksiyon gösterir (paywall'a yönlendirme gibi).

import { API_ERROR_CODE, ApiError } from '../client';

const NETWORK = 'Bağlantı kurulamadı, internet bağlantını kontrol et';
const GENERIC = 'Pratik başlatılamadı, lütfen tekrar dene';
const SESSION = 'Oturumun sona ermiş, tekrar giriş yap';
const DAILY_LIMIT =
  'Bugünkü ücretsiz pratik hakkını kullandın. Yarın tekrar gel ya da premium ile sınırsız pratik yap.';

export type StartSessionErrorKind = 'premiumRequired' | 'dailyLimitReached' | 'other';

export interface StartSessionError {
  kind: StartSessionErrorKind;
  message: string;
}

export function toStartSessionError(error: unknown): StartSessionError {
  if (!(error instanceof ApiError)) {
    return { kind: 'other', message: GENERIC };
  }
  if (error.code === API_ERROR_CODE.PREMIUM_REQUIRED || error.status === 403) {
    return { kind: 'premiumRequired', message: error.message };
  }
  if (error.code === API_ERROR_CODE.DAILY_LIMIT_REACHED || error.status === 429) {
    return { kind: 'dailyLimitReached', message: DAILY_LIMIT };
  }
  if (error.isNetworkError) {
    return { kind: 'other', message: NETWORK };
  }
  if (error.status === 401) {
    return { kind: 'other', message: SESSION };
  }
  if (error.code === API_ERROR_CODE.NOT_FOUND || error.status === 404) {
    return { kind: 'other', message: 'Senaryo bulunamadı' };
  }
  return { kind: 'other', message: GENERIC };
}

// Liste/detay okuma hataları (Son pratikler, oturum detayı).
export function toSessionErrorMessage(error: unknown): string {
  if (!(error instanceof ApiError)) {
    return 'Pratikler yüklenemedi';
  }
  if (error.isNetworkError) {
    return NETWORK;
  }
  if (error.status === 401) {
    return SESSION;
  }
  if (error.status === 404) {
    return 'Pratik bulunamadı';
  }
  return 'Pratikler yüklenemedi';
}
