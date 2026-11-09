// Auth hatalarının kullanıcıya gösterilecek Türkçe karşılıkları.
//
// Kural: teknik detay (status kodu, stack, endpoint, backend exception mesajı)
// ASLA ekrana sızmaz. Backend'in kendi mesajları zaten Türkçe geliyor
// (varsayılan kültür "tr"), ama sadece güvendiğimiz hata kodlarında
// pass-through yapıyoruz — beklenmeyen 500'lerin iç detayını göstermeyiz.

import { t } from '@/lib/i18n';

import { API_ERROR_CODE, ApiError } from '../client';

// Çeviriler çağrı anında çözülür (dil sonradan değişebilir).
const NETWORK = () => t('common.errors.network');
const GENERIC = () => t('common.errors.generic');
const EMAIL_NOT_CONFIRMED = () => t('auth.errors.emailNotConfirmed');
const TOO_MANY_ATTEMPTS = () => t('auth.errors.tooManyAttempts');

// Mesajı olduğu gibi kullanıcıya gösterebileceğimiz hata kodları:
// bunlar backend'in localize edilmiş, kullanıcıya yönelik metinleri.
const USER_FACING_CODES: readonly string[] = [
  API_ERROR_CODE.VALIDATION_FAILED,
  API_ERROR_CODE.INVALID_REQUEST,
];

function isApiError(error: unknown): error is ApiError {
  return error instanceof ApiError;
}

export function toLoginErrorMessage(error: unknown): string {
  if (!isApiError(error)) {
    return GENERIC();
  }
  if (error.isNetworkError) {
    return NETWORK();
  }
  if (error.code === API_ERROR_CODE.EMAIL_NOT_CONFIRMED) {
    return EMAIL_NOT_CONFIRMED();
  }
  if (error.status === 401) {
    // Backend hesap durumu sızdırmamak için hatalı şifre, bulunamayan hesap,
    // pasif hesap ve doğrulanmamış e-postanın hepsine aynı 401'i döner.
    return t('auth.errors.invalidCredentials');
  }
  if (error.status === 429) {
    return TOO_MANY_ATTEMPTS();
  }
  if (error.code && USER_FACING_CODES.includes(error.code)) {
    return error.message;
  }
  return GENERIC();
}

// Kod isteme ucu kayıtlı/kayıtsız ayrımı yapmadan 200 döner; burada sadece
// ağ, rate limit ve beklenmeyen hatalar kalır.
export function toForgotPasswordErrorMessage(error: unknown): string {
  if (!isApiError(error)) {
    return GENERIC();
  }
  if (error.isNetworkError) {
    return NETWORK();
  }
  if (error.status === 429) {
    return TOO_MANY_ATTEMPTS();
  }
  if (error.status === 400 && error.errors.length > 0) {
    return error.errors.join('\n');
  }
  return GENERIC();
}

// 400'ler kullanıcıya yönelik, backend'de Türkçe yerelleştirilmiş: kod
// hatalı/süresi dolmuş, çok fazla deneme (yeni kod iste), şifre kuralları.
export function toResetPasswordErrorMessage(error: unknown): string {
  if (!isApiError(error)) {
    return GENERIC();
  }
  if (error.isNetworkError) {
    return NETWORK();
  }
  if (error.status === 429) {
    return TOO_MANY_ATTEMPTS();
  }
  if (error.status === 400 && error.errors.length > 0) {
    return error.errors.join('\n');
  }
  if (error.code && USER_FACING_CODES.includes(error.code)) {
    return error.message;
  }
  return GENERIC();
}

export function toEmailChangeErrorMessage(error: unknown): string {
  if (!isApiError(error)) {
    return GENERIC();
  }
  if (error.isNetworkError) {
    return NETWORK();
  }
  if (error.status === 409) {
    return t('auth.errors.emailInUse');
  }
  if (error.status === 429) {
    return TOO_MANY_ATTEMPTS();
  }
  if (error.status === 400 && error.errors.length > 0) {
    return error.errors.join('\n');
  }
  if (error.code && USER_FACING_CODES.includes(error.code)) {
    return error.message;
  }
  return GENERIC();
}

export function toProfileErrorMessage(error: unknown): string {
  if (!isApiError(error)) {
    return GENERIC();
  }
  if (error.isNetworkError) {
    return NETWORK();
  }
  if (error.status === 400 && error.errors.length > 0) {
    return error.errors.join('\n');
  }
  if (error.code && USER_FACING_CODES.includes(error.code)) {
    return error.message;
  }
  return t('auth.errors.profileSaveFailed');
}

// Hesap silme / veri dışa aktarma. Yanlış şifre (400) ve son SuperAdmin (403)
// mesajları backend'de Türkçe ve kullanıcıya yönelik.
export function toAccountErrorMessage(error: unknown): string {
  if (!isApiError(error)) {
    return GENERIC();
  }
  if (error.isNetworkError) {
    return NETWORK();
  }
  if (error.status === 429) {
    return TOO_MANY_ATTEMPTS();
  }
  if ((error.status === 400 || error.status === 403) && error.errors.length > 0) {
    return error.errors.join('\n');
  }
  if (error.code && USER_FACING_CODES.includes(error.code)) {
    return error.message;
  }
  return GENERIC();
}

export function toRegisterErrorMessage(error: unknown): string {
  if (!isApiError(error)) {
    return GENERIC();
  }
  if (error.isNetworkError) {
    return NETWORK();
  }
  if (error.status === 409) {
    return t('auth.errors.emailAlreadyRegistered');
  }
  if (error.status === 429) {
    return TOO_MANY_ATTEMPTS();
  }
  // 400 doğrulama hatalarında backend alan bazlı Türkçe mesaj veriyor
  // ("Geçerli bir e-posta adresi girin" gibi) — doğrudan göstermek faydalı.
  if (error.status === 400 && error.errors.length > 0) {
    return error.errors.join('\n');
  }
  if (error.code && USER_FACING_CODES.includes(error.code)) {
    return error.message;
  }
  return GENERIC();
}
