// Kullanıcı istatistikleri mock'u. Streak, XP/seviye, haftalık hedef ve rozetler.
// Backend bağlandığında bu yapı /me/stats endpoint'inden gelecek.

import type { ContextId } from '@/types/scenario';

export interface UserStats {
  currentStreak: number;
  longestStreak: number;
  totalSessions: number;
  todaySessionsCount: number;
  weeklyGoal: number;
  weeklyProgress: number;
  level: number;
  xp: number;
  xpToNextLevel: number;
  favoriteContextId: ContextId;
  badges: string[];
  lastSessionAt: string | null;
}

export const mockUserStats: UserStats = {
  currentStreak: 3,
  longestStreak: 7,
  totalSessions: 12,
  todaySessionsCount: 1,
  weeklyGoal: 5,
  weeklyProgress: 3,
  level: 2,
  xp: 240,
  xpToNextLevel: 500,
  favoriteContextId: 'is',
  badges: ['first_no', 'three_day_streak'],
  lastSessionAt: '2024-01-15T18:30:00Z',
};
