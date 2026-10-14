// Senaryo verisi için TanStack Query hook'ları.
//
// Query key'ler hiyerarşik: scenarioKeys.all ile tüm senaryo cache'i,
// scenarioKeys.lists() ile sadece listeler invalidate edilebilir. Session yazma
// akışı geldiğinde (pratik bitince) invalidasyon buradan yürüyecek.

import { useQuery } from '@tanstack/react-query';

import { scenarioService } from '@/lib/api';
import type { ApiError } from '@/lib/api';
import type {
  ApiContextId,
  ApiScenarioDetail,
  ApiScenarioListItem,
  ApiScenarioListParams,
} from '@/types/api/scenario';

export const scenarioKeys = {
  all: ['scenarios'] as const,
  lists: () => [...scenarioKeys.all, 'list'] as const,
  list: (filters?: object) => [...scenarioKeys.lists(), filters ?? {}] as const,
  details: () => [...scenarioKeys.all, 'detail'] as const,
  detail: (id: string) => [...scenarioKeys.details(), id] as const,
};

export function useScenarios(params?: ApiScenarioListParams) {
  return useQuery<ApiScenarioListItem[], ApiError>({
    queryKey: scenarioKeys.list(params),
    queryFn: () => scenarioService.list(params),
  });
}

// contextId route param'ından gelir; gelmeden istek atmasın.
export function useScenariosByContext(contextId: ApiContextId | undefined) {
  return useQuery<ApiScenarioListItem[], ApiError>({
    // Ayrı bir uç olduğu için key de ayrı: /by-context ile ?contextId= aynı
    // sonucu dönse de cache girdilerini karıştırmayalım.
    queryKey: scenarioKeys.list({ byContext: contextId }),
    queryFn: () => scenarioService.byContext(contextId as ApiContextId),
    enabled: !!contextId,
  });
}

export function useScenarioDetail(id: string | undefined) {
  return useQuery<ApiScenarioDetail, ApiError>({
    queryKey: scenarioKeys.detail(id ?? ''),
    queryFn: () => scenarioService.detail(id as string),
    enabled: !!id,
  });
}
