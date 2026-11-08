// Pratik oturumu isteklerinin kullanıcıya gösterilecek Türkçe karşılıkları.
// Premium ve günlük limit hataları ayrı "kind" olarak döner — ekranlar bunlara
// özel aksiyon gösterir (paywall'a yönlendirme gibi).

import { API_ERROR_CODE, ApiError } from '../client';

const NETWORK = 'Bağlantı kurulamadı, internet bağlantını kontrol et';
const GENERIC = 'Pratik başlatılamadı, lütfen tekrar dene';
const SESSION = 'Oturumun sona ermiş, tekrar giriş yap';
const DAILY_LIMIT =
  'Bugünkü ücretsiz pratik hakkını kullandın. Yarın tekrar gel ya da premium ile sınırsız pratik yap.';

// Kullanıcı başına dakikalık istek sınırı (backend "ai" rate limit politikası; gövdesi kodsuz).
const TOO_FAST = 'Çok hızlı gidiyorsun. Birkaç saniye bekleyip tekrar dene.';

export type StartSessionErrorKind =
  | 'premiumRequired'
  | 'dailyLimitReached'
  | 'fairUseLimitReached'
  | 'other';

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
  // Premium adil kullanım sınırı da 429 — premium kullanıcıya paywall önerilmesin diye önce bakılır.
  if (error.code === API_ERROR_CODE.FAIR_USE_LIMIT_REACHED) {
    return { kind: 'fairUseLimitReached', message: error.message };
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

// Pratikte mesaj gönderme hataları. Hepsinde raunt ilerlemez; kullanıcı mesajı
// input'a geri konur. Backend mesajları Türkçe ve kullanıcıya yönelik — güvendiğimiz
// kodlarda olduğu gibi gösterilir.
export function toSendMessageErrorMessage(error: unknown): string {
  if (!(error instanceof ApiError)) {
    return 'Mesaj gönderilemedi, lütfen tekrar dene';
  }
  if (error.isNetworkError) {
    return NETWORK;
  }
  if (error.status === 401) {
    return SESSION;
  }
  const userFacing: readonly string[] = [
    API_ERROR_CODE.AI_UNAVAILABLE,
    API_ERROR_CODE.AI_RESPONSE_REFUSED,
    API_ERROR_CODE.SESSION_NOT_ACTIVE,
    API_ERROR_CODE.SESSION_TOKEN_LIMIT_REACHED,
    API_ERROR_CODE.SESSION_BUSY,
  ];
  if (error.code && userFacing.includes(error.code)) {
    return error.message;
  }
  if (error.status === 400 && error.errors.length > 0) {
    return error.errors.join('\n');
  }
  if (error.status === 429) {
    return TOO_FAST;
  }
  return 'Mesaj gönderilemedi, lütfen tekrar dene';
}

// Sonuç ekranının değerlendirme hataları.
export function toEvaluateErrorMessage(error: unknown): string {
  if (!(error instanceof ApiError)) {
    return 'Değerlendirme yüklenemedi';
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
  if (
    error.code === API_ERROR_CODE.AI_UNAVAILABLE ||
    error.code === API_ERROR_CODE.SESSION_NOT_COMPLETED ||
    error.code === API_ERROR_CODE.SESSION_BUSY
  ) {
    return error.message;
  }
  if (error.status === 429) {
    return TOO_FAST;
  }
  return 'Değerlendirme yüklenemedi';
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
