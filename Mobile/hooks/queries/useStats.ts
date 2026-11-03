// Kullanıcı istatistikleri (Anasayfa, Profil). Değerlendirme seri/XP/rozetleri,
// oturum başlatma toplam pratik sayısını değiştirdiği için ikisinden sonra
// statsKeys.all invalidate edilir (bkz. useSessions).

import { useQuery } from '@tanstack/react-query';

import { statsService } from '@/lib/api';
import type { ApiError } from '@/lib/api';
import type { ApiUserStats } from '@/types/api/stats';

export const statsKeys = {
  all: ['stats'] as const,
  me: () => [...statsKeys.all, 'me'] as const,
};

export function useMyStats() {
  return useQuery<ApiUserStats, ApiError>({
    queryKey: statsKeys.me(),
    queryFn: () => statsService.me(),
  });
}

// Yüklenirken / hata durumunda kartların çizilebilmesi için boş başlangıç değeri.
export const EMPTY_STATS: ApiUserStats = {
  currentStreak: 0,
  longestStreak: 0,
  practicedToday: false,
  totalSessions: 0,
  totalCompletedSessions: 0,
  weekActivity: [false, false, false, false, false, false, false],
  // Veri gelene kadar cihazın günü; gelince backend'in TR günü kullanılır.
  todayIndex: (new Date().getDay() + 6) % 7,
  weeklyProgress: 0,
  weeklyGoal: 5,
  level: 1,
  totalPoints: 0,
  xpInLevel: 0,
  xpPerLevel: 500,
  unlockedBadges: [],
};
