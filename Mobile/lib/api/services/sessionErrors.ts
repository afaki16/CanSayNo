// Pratik oturumu isteklerinin kullanıcıya gösterilecek karşılıkları (çağrı anındaki dilde).
// Premium ve günlük limit hataları ayrı "kind" olarak döner — ekranlar bunlara
// özel aksiyon gösterir (paywall'a yönlendirme gibi).

import { API_ERROR_CODE, ApiError } from '../client';

import { t } from '@/lib/i18n';

// Kullanıcı başına dakikalık istek sınırı (backend "ai" rate limit politikası; gövdesi kodsuz)
// → common.errors.tooFast.

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
    return { kind: 'other', message: t('practice.errors.startFailed') };
  }
  if (error.code === API_ERROR_CODE.PREMIUM_REQUIRED || error.status === 403) {
    return { kind: 'premiumRequired', message: error.message };
  }
  // Premium adil kullanım sınırı da 429 — premium kullanıcıya paywall önerilmesin diye önce bakılır.
  if (error.code === API_ERROR_CODE.FAIR_USE_LIMIT_REACHED) {
    return { kind: 'fairUseLimitReached', message: error.message };
  }
  if (error.code === API_ERROR_CODE.DAILY_LIMIT_REACHED || error.status === 429) {
    return { kind: 'dailyLimitReached', message: t('practice.errors.dailyLimit') };
  }
  if (error.isNetworkError) {
    return { kind: 'other', message: t('common.errors.network') };
  }
  if (error.status === 401) {
    return { kind: 'other', message: t('common.errors.session') };
  }
  if (error.code === API_ERROR_CODE.NOT_FOUND || error.status === 404) {
    return { kind: 'other', message: t('practice.errors.scenarioNotFound') };
  }
  return { kind: 'other', message: t('practice.errors.startFailed') };
}

// Pratikte mesaj gönderme hataları. Hepsinde raunt ilerlemez; kullanıcı mesajı
// input'a geri konur. Backend mesajları kullanıcıya yönelik (Accept-Language'a göre) —
// güvendiğimiz kodlarda olduğu gibi gösterilir.
export function toSendMessageErrorMessage(error: unknown): string {
  if (!(error instanceof ApiError)) {
    return t('practice.errors.sendFailed');
  }
  if (error.isNetworkError) {
    return t('common.errors.network');
  }
  if (error.status === 401) {
    return t('common.errors.session');
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
    return t('common.errors.tooFast');
  }
  return t('practice.errors.sendFailed');
}

// Sonuç ekranının değerlendirme hataları.
export function toEvaluateErrorMessage(error: unknown): string {
  if (!(error instanceof ApiError)) {
    return t('practice.errors.evaluateFailed');
  }
  if (error.isNetworkError) {
    return t('common.errors.network');
  }
  if (error.status === 401) {
    return t('common.errors.session');
  }
  if (error.status === 404) {
    return t('practice.errors.sessionNotFound');
  }
  if (
    error.code === API_ERROR_CODE.AI_UNAVAILABLE ||
    error.code === API_ERROR_CODE.SESSION_NOT_COMPLETED ||
    error.code === API_ERROR_CODE.SESSION_BUSY
  ) {
    return error.message;
  }
  if (error.status === 429) {
    return t('common.errors.tooFast');
  }
  return t('practice.errors.evaluateFailed');
}

// Liste/detay okuma hataları (Son pratikler, oturum detayı).
export function toSessionErrorMessage(error: unknown): string {
  if (!(error instanceof ApiError)) {
    return t('practice.errors.sessionsFailed');
  }
  if (error.isNetworkError) {
    return t('common.errors.network');
  }
  if (error.status === 401) {
    return t('common.errors.session');
  }
  if (error.status === 404) {
    return t('practice.errors.sessionNotFound');
  }
  return t('practice.errors.sessionsFailed');
}
