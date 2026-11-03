// Pratik oturumu endpoint'lerinin transport katmanı. Sadece istek atar ve zarfı
// açar — cache yönetimi hooks/queries/useSessions'ta, hata çevirisi ./sessionErrors'da.

import { apiGet, apiPost } from '../hooks';
import { SESSION_ENDPOINTS } from '../endpoints';

import type {
  ApiPagedData,
  ApiSendMessageBody,
  ApiSession,
  ApiSessionListItem,
  ApiStartSessionBody,
} from '@/types/api/session';

// LLM çağrıları varsayılan 15 sn'yi aşabilir (SDK tekrar denemeleri dahil).
// Değerlendirme daha uzun düşünür; karakter cevabı kısa.
const MESSAGE_TIMEOUT_MS = 45_000;
const EVALUATE_TIMEOUT_MS = 90_000;

export const sessionService = {
  // Premium senaryo → 403 premiumRequired, günlük limit → 429 dailyLimitReached.
  start: (scenarioId: string) =>
    apiPost<ApiSession, ApiStartSessionBody>(SESSION_ENDPOINTS.START, { scenarioId }),

  detail: (id: string) => apiGet<ApiSession>(SESSION_ENDPOINTS.DETAIL(id)),

  my: (page = 1, pageSize = 20) =>
    apiGet<ApiPagedData<ApiSessionListItem>>(SESSION_ENDPOINTS.MY, { page, pageSize }),

  // Hatalar (raunt ilerlemez): 409 sessionNotActive, 422 aiResponseRefused,
  // 429 sessionTokenLimitReached, 503 aiUnavailable, 400 doğrulama (≤500 karakter).
  sendMessage: (id: string, content: string) =>
    apiPost<ApiSession, ApiSendMessageBody>(
      SESSION_ENDPOINTS.MESSAGES(id),
      { content },
      { timeoutMs: MESSAGE_TIMEOUT_MS },
    ),

  // Değerlendirme zaten varsa LLM'e gitmeden aynısı döner.
  // Hatalar: 409 sessionNotCompleted, 503 aiUnavailable.
  evaluate: (id: string) =>
    apiPost<ApiSession, undefined>(SESSION_ENDPOINTS.EVALUATE(id), undefined, {
      timeoutMs: EVALUATE_TIMEOUT_MS,
    }),
};
