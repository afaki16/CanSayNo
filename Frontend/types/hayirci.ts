// Hayırcı admin paneli tipleri — backend /api/admin/* (HayirciAdminController) ile birebir.
// Enum'lar JSON'da camelCase string gelir (JsonStringEnumConverter).

export type ContextId =
  | 'is'
  | 'aile'
  | 'arkadas'
  | 'romantik'
  | 'satici'
  | 'komsu'
  | 'okul'
  | 'dijital'

/** Senaryonun içerik dili; liste mobilde kullanıcının diline göre filtrelenir. */
export type ScenarioLanguage = 'tr' | 'en'

export type CharacterArchetype =
  | 'babacanManipulator'
  | 'duygusalKurban'
  | 'agresifIsrarci'
  | 'tatliDilliSatici'
  | 'masumRicaci'
  | 'pasifAgresif'
  | 'otoriter'

export type PressureTactic =
  | 'ovguVeGuven'
  | 'suclulukYukleme'
  | 'acilDurumYaratma'
  | 'sosyalKanit'
  | 'otorite'
  | 'kiyaslama'
  | 'karsiTeklif'
  | 'pasifTehdit'
  | 'sahteAnlayis'
  | 'duyguSomurusu'
  | 'kisKisSessizlik'
  | 'asiriOzurAlan'
  | 'sahteZamanBaskisi'

export type SubscriptionStatus = 'active' | 'cancelled' | 'gracePeriod' | 'expired' | 'pending'
export type SessionStatus = 'active' | 'completed' | 'abandoned'

export interface AdminScenarioScene {
  setting: string
  situation: string
  userGoal: string
  hint: string | null
  openingMessage: string
}

export interface AdminScenarioCharacter {
  name: string
  age: number
  role: string
  relationship: string
  archetype: CharacterArchetype
}

export interface AdminScenarioTactics {
  roundOne: PressureTactic[]
  roundTwo: PressureTactic[]
  roundThree: PressureTactic[]
}

export interface AdminScenarioUsage {
  sessions: number
  evaluated: number
  averageScore: number | null
  lastPlayedAt: string | null
}

/** PUT gövdesi; POST'ta ayrıca `id` verilir (URL'de). */
export interface AdminScenarioUpsert {
  title: string
  shortDescription: string
  contextId: ContextId
  /** Oluşturulduktan sonra değiştirilemez (ID öneki ve geçmiş pratiklerin dili). */
  language: ScenarioLanguage
  difficulty: number
  estimatedDurationMinutes: number
  isFree: boolean
  isActive: boolean
  sortOrder: number
  tags: string[]
  scene: AdminScenarioScene
  character: AdminScenarioCharacter
  pressureTactics: AdminScenarioTactics
}

export interface AdminScenario extends AdminScenarioUpsert {
  id: string
  usage: AdminScenarioUsage
  createdDate: string
  updatedDate: string | null
}

export interface CreateAdminScenarioRequest extends AdminScenarioUpsert {
  id: string
}

export interface AdminUserSubscription {
  productId: string
  status: SubscriptionStatus
  expiresAt: string | null
  startedAt: string | null
  cancelledAt: string | null
}

export interface AdminUserSession {
  id: string
  scenarioId: string
  scenarioTitle: string
  contextId: ContextId
  status: SessionStatus
  totalScore: number | null
  tokensUsed: number
  startedAt: string
  completedAt: string | null
}

export interface AdminUserOverview {
  userId: number
  email: string
  fullName: string
  registeredAt: string
  lastLoginAt: string | null
  lastPracticeAt: string | null
  isPremium: boolean
  subscription: AdminUserSubscription | null
  level: number
  totalPoints: number
  currentStreak: number
  longestStreak: number
  badges: string[]
  totalSessions: number
  evaluatedSessions: number
  averageScore: number | null
  totalTokens: number
  recentSessions: AdminUserSession[]
}

export interface AdminDailyPoint {
  date: string // yyyy-MM-dd (TR günü)
  sessions: number
  evaluated: number
  activeUsers: number
  tokens: number
}

export interface AdminScenarioRank {
  id: string
  title: string
  contextId: ContextId
  sessions: number
  averageScore: number | null
}

export interface AdminDashboard {
  appUsers: number
  newUsersLast7Days: number
  premiumUsers: number
  premiumRatePercent: number
  activeUsersToday: number
  activeUsersLast7Days: number
  sessionsToday: number
  evaluatedToday: number
  tokensLast30Days: number
  estimatedCostUsdLast30Days: number
  usdPerMillionTokens: number
  daily: AdminDailyPoint[]
  topScenarios: AdminScenarioRank[]
  lowestScoringScenarios: AdminScenarioRank[]
}
