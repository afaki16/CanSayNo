// Backend Sessions endpoint'lerinin wire tipleri.
// Şekiller çalışan backend'e atılan isteklerden doğrulandı
// (POST /api/Sessions, GET /api/Sessions/{id}, GET /api/Sessions/my).

import type { ApiContextId } from './scenario';

export type ApiSessionStatus = 'active' | 'completed' | 'abandoned';

export type ApiEvaluationAxis =
  | 'netlik'
  | 'nezaket'
  | 'dayaniklilik'
  | 'alternatifSunma'
  | 'ozurDengesi';

export interface ApiSessionMessage {
  id: string;
  role: 'ai' | 'user';
  content: string;
  timestamp: string;
}

export interface ApiSessionEvaluation {
  scores: Partial<Record<ApiEvaluationAxis, number>>; // 0-100
  strengths: string[];
  improvements: string[];
  coachNote: string;
  idealResponseExample: string | null;
  totalScore: number; // 0-100
  xpEarned: number; // bu pratikten kazanılan XP
  streakAfter: number; // değerlendirme anındaki seri (gün)
}

// POST /api/Sessions ve GET /api/Sessions/{id}
export interface ApiSession {
  id: string;
  scenarioId: string;
  scenarioTitle: string;
  scenarioContextId: ApiContextId;
  status: ApiSessionStatus;
  currentRound: number;
  maxRounds: number;
  messages: ApiSessionMessage[];
  evaluation: ApiSessionEvaluation | null;
  totalScore: number | null;
  startedAt: string;
  completedAt: string | null;
}

// GET /api/Sessions/my öğesi — mesaj ve değerlendirme detayı yok.
export interface ApiSessionListItem {
  id: string;
  scenarioId: string;
  scenarioTitle: string;
  scenarioContextId: ApiContextId;
  status: ApiSessionStatus;
  totalScore: number | null;
  startedAt: string;
  completedAt: string | null;
}

// Kickstart sayfalı zarfının data kısmı.
export interface ApiPagedData<T> {
  items: T[];
  totalCount: number;
  totalPages: number;
  pageNumber: number;
}

export interface ApiStartSessionBody {
  scenarioId: string;
}

// POST /api/Sessions/{id}/messages — yanıt güncellenmiş ApiSession.
export interface ApiSendMessageBody {
  content: string;
}
