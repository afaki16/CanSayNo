// GET /api/Stats/me wire tipi.

export type ApiBadge =
  | 'firstNo'
  | 'threeDayStreak'
  | 'sevenDayStreak'
  | 'thirtyDayStreak'
  | 'workMaster'
  | 'perfectScore'
  | 'nightOwl'
  | 'earlyBird'
  | 'persistent'
  | 'contextExplorer';

export interface ApiUserStats {
  currentStreak: number; // son pratik bugün/dün değilse 0
  longestStreak: number;
  practicedToday: boolean;
  totalSessions: number; // başlatılan
  totalCompletedSessions: number; // tamamlanıp değerlendirilen
  weekActivity: boolean[]; // bu hafta Pzt..Paz (TR), 7 eleman
  todayIndex: number; // bugünün weekActivity indeksi (TR; Pazartesi = 0)
  weeklyProgress: number;
  weeklyGoal: number;
  level: number;
  totalPoints: number;
  xpInLevel: number;
  xpPerLevel: number;
  unlockedBadges: ApiBadge[];
}
