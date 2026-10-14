// Pratik oturumları için TanStack Query hook'ları.
//
// Oturum başlatmak senaryo agregalarını (attemptsCount vb.) da değiştirdiği için
// başarılı start sonrası hem sessionKeys.all hem scenarioKeys.all invalidate edilir.

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';

import { sessionService } from '@/lib/api';
import type { ApiError } from '@/lib/api';
import type {
  ApiPagedData,
  ApiSession,
  ApiSessionListItem,
} from '@/types/api/session';

import { scenarioKeys } from './useScenarios';

export const sessionKeys = {
  all: ['sessions'] as const,
  lists: () => [...sessionKeys.all, 'list'] as const,
  my: (pageSize: number) => [...sessionKeys.lists(), 'my', pageSize] as const,
  details: () => [...sessionKeys.all, 'detail'] as const,
  detail: (id: string) => [...sessionKeys.details(), id] as const,
};

// Kullanıcının son oturumları (ilk sayfa). Anasayfa ve Profil kullanır.
export function useMySessions(pageSize = 20) {
  return useQuery<ApiPagedData<ApiSessionListItem>, ApiError>({
    queryKey: sessionKeys.my(pageSize),
    queryFn: () => sessionService.my(1, pageSize),
  });
}

export function useSession(id: string | undefined) {
  return useQuery<ApiSession, ApiError>({
    queryKey: sessionKeys.detail(id ?? ''),
    queryFn: () => sessionService.detail(id as string),
    enabled: !!id,
  });
}

export function useStartSession() {
  const queryClient = useQueryClient();

  return useMutation<ApiSession, ApiError, string>({
    mutationFn: (scenarioId) => sessionService.start(scenarioId),
    onSuccess: (session) => {
      queryClient.setQueryData(sessionKeys.detail(session.id), session);
      queryClient.invalidateQueries({ queryKey: sessionKeys.lists() });
      queryClient.invalidateQueries({ queryKey: scenarioKeys.all });
    },
  });
}
