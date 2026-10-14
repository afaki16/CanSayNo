// Senaryo endpoint'lerinin transport katmanı. Sadece istek atar ve zarfı açar —
// cache/loading yönetimi hooks/queries/useScenarios'ta, hata mesajı çevirisi
// ./scenarioErrors'da.

import { apiGet } from '../hooks';
import { SCENARIO_ENDPOINTS } from '../endpoints';

import type {
  ApiContextId,
  ApiScenarioDetail,
  ApiScenarioListItem,
  ApiScenarioListParams,
} from '@/types/api/scenario';

export const scenarioService = {
  // params undefined alanları axios query string'e hiç koymaz.
  list: (params?: ApiScenarioListParams) =>
    apiGet<ApiScenarioListItem[]>(SCENARIO_ENDPOINTS.LIST, {
      contextId: params?.contextId,
      isFreeOnly: params?.isFreeOnly,
    }),

  // Bilinmeyen id'de backend 404 + code "notFound" döner.
  detail: (id: string) =>
    apiGet<ApiScenarioDetail>(SCENARIO_ENDPOINTS.DETAIL(id)),

  byContext: (contextId: ApiContextId) =>
    apiGet<ApiScenarioListItem[]>(SCENARIO_ENDPOINTS.BY_CONTEXT(contextId)),
};
