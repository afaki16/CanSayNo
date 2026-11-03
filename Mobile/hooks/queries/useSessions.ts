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
import { statsKeys } from './useStats';

export const sessionKeys = {
  all: ['sessions'] as const,
  lists: () => [...sessionKeys.all, 'list'] as const,
  my: (pageSize: number) => [...sessionKeys.lists(), 'my', pageSize] as const,
  details: () => [...sessionKeys.all, 'detail'] as const,
  detail: (id: string) => [...sessionKeys.details(), id] as const,
  // Detay ağacının DIŞINDA: detay invalidate edilince LLM değerlendirmesi yeniden tetiklenmesin.
  evaluation: (id: string) => [...sessionKeys.all, 'evaluation', id] as const,
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

// Pratikte mesaj gönderir. Yanıt güncellenmiş oturum olduğu için detay cache'i
// doğrudan güncellenir; son raunt oturumu tamamlar, listeler tazelenir.
export function useSendMessage(sessionId: string | undefined) {
  const queryClient = useQueryClient();

  return useMutation<ApiSession, ApiError, string>({
    mutationFn: (content) => sessionService.sendMessage(sessionId as string, content),
    onSuccess: (session) => {
      queryClient.setQueryData(sessionKeys.detail(session.id), session);
      if (session.status === 'completed') {
        queryClient.invalidateQueries({ queryKey: sessionKeys.lists() });
      }
    },
  });
}

// Sonuç ekranı: değerlendirmeyi ister. Backend idempotent olduğu için query olarak
// modellenir (tekrar açılışta LLM'e gitmeden aynı sonuç gelir). Başarıda skor ve
// tamamlanma bilgisi değiştiği için liste ve senaryo agregaları tazelenir.
export function useEvaluateSession(sessionId: string | undefined) {
  const queryClient = useQueryClient();

  return useQuery<ApiSession, ApiError>({
    queryKey: sessionKeys.evaluation(sessionId ?? ''),
    queryFn: async () => {
      const session = await sessionService.evaluate(sessionId as string);
      queryClient.setQueryData(sessionKeys.detail(session.id), session);
      queryClient.invalidateQueries({ queryKey: sessionKeys.lists() });
      queryClient.invalidateQueries({ queryKey: scenarioKeys.all });
      queryClient.invalidateQueries({ queryKey: statsKeys.all });
      return session;
    },
    enabled: !!sessionId,
    // LLM çağrısı: hata ekranda "Tekrar dene" ile ele alınır, sessiz tekrar yok.
    retry: false,
    staleTime: Infinity,
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
      queryClient.invalidateQueries({ queryKey: statsKeys.all });
    },
  });
}
