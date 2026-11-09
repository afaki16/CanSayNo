// Senaryo isteklerinin kullanıcıya gösterilecek karşılıkları (çağrı anındaki dilde).
// authErrors ile aynı kural: teknik detay (status, stack, endpoint) sızmaz.

import { API_ERROR_CODE, ApiError } from '../client';

import { t } from '@/lib/i18n';

export function toScenarioErrorMessage(error: unknown): string {
  if (!(error instanceof ApiError)) {
    return t('practice.errors.scenariosFailed');
  }
  if (error.isNetworkError) {
    return t('common.errors.network');
  }
  if (error.status === 401) {
    return t('common.errors.session');
  }
  if (error.code === API_ERROR_CODE.NOT_FOUND || error.status === 404) {
    return t('practice.errors.scenarioNotFound');
  }
  return t('practice.errors.scenariosFailed');
}

// 404'ü ekranlar ayrı ele alıyor (detay sayfasında "Geri Dön"lü boş state).
export function isNotFoundError(error: unknown): boolean {
  return (
    error instanceof ApiError &&
    (error.status === 404 || error.code === API_ERROR_CODE.NOT_FOUND)
  );
}
