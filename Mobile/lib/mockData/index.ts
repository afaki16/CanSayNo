// Ekranların tek import noktası. Senaryo verisi ARTIK BURADA DEĞİL — API'den
// geliyor (hooks/queries/useScenarios). Burada kalanlar:
//   contexts        → bağlam UI metadata'sı (backend'de karşılığı yok)
//   aiResponses     → pratik sahnesinin mock AI script'i (LLM entegrasyonuna kadar)
//   userStats /
//   recentActivity  → hâlâ mock; session endpoint'leri gelince gerçekleşecek
//   evaluation      → sonuç ekranının mock değerlendirmesi (evaluate endpoint'ine kadar)

export { mockUserStats } from './userStats';
export type { UserStats } from './userStats';
export { contexts } from './contexts';
export { mockRecentActivities } from './recentActivity';
export type { RecentActivity } from './recentActivity';
export { getAIResponseScript, mockAIResponses } from './aiResponses';
export type { AIResponseScript } from './aiResponses';
export {
  EVALUATION_AXIS_LABELS,
  mockEvaluation,
} from './evaluation';
export type { EvaluationAxis, SessionEvaluation } from './evaluation';
