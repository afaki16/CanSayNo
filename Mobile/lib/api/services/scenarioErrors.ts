// Senaryo isteklerinin kullanıcıya gösterilecek Türkçe karşılıkları.
// authErrors ile aynı kural: teknik detay (status, stack, endpoint) sızmaz.

import { API_ERROR_CODE, ApiError } from '../client';

const NETWORK = 'Bağlantı kurulamadı, internet bağlantını kontrol et';
const GENERIC = 'Senaryolar yüklenemedi, lütfen tekrar dene';
const SESSION = 'Oturumun sona ermiş, tekrar giriş yap';

export function toScenarioErrorMessage(error: unknown): string {
  if (!(error instanceof ApiError)) {
    return GENERIC;
  }
  if (error.isNetworkError) {
    return NETWORK;
  }
  if (error.status === 401) {
    return SESSION;
  }
  if (error.code === API_ERROR_CODE.NOT_FOUND || error.status === 404) {
    return 'Senaryo bulunamadı';
  }
  return GENERIC;
}

// 404'ü ekranlar ayrı ele alıyor (detay sayfasında "Geri Dön"lü boş state).
export function isNotFoundError(error: unknown): boolean {
  return (
    error instanceof ApiError &&
    (error.status === 404 || error.code === API_ERROR_CODE.NOT_FOUND)
  );
}
