// Pratik oturumu endpoint'lerinin transport katmanı. Sadece istek atar ve zarfı
// açar — cache yönetimi hooks/queries/useSessions'ta, hata çevirisi ./sessionErrors'da.

import { apiGet, apiPost } from '../hooks';
import { SESSION_ENDPOINTS } from '../endpoints';

import type {
  ApiPagedData,
  ApiSession,
  ApiSessionListItem,
  ApiStartSessionBody,
} from '@/types/api/session';

export const sessionService = {
  // Premium senaryo → 403 premiumRequired, günlük limit → 429 dailyLimitReached.
  start: (scenarioId: string) =>
    apiPost<ApiSession, ApiStartSessionBody>(SESSION_ENDPOINTS.START, { scenarioId }),

  detail: (id: string) => apiGet<ApiSession>(SESSION_ENDPOINTS.DETAIL(id)),

  my: (page = 1, pageSize = 20) =>
    apiGet<ApiPagedData<ApiSessionListItem>>(SESSION_ENDPOINTS.MY, { page, pageSize }),
};
