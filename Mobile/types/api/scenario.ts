// Backend Scenario endpoint'lerinin wire tipleri.
// Şekiller çalışan backend'e atılan gerçek isteklerden doğrulandı
// (GET /api/Scenarios, /api/Scenarios/{id}, /api/Scenarios/by-context/{contextId}).
//
// Backend enum'ları camelCase STRING olarak serialize ediyor — number değil.

export type ApiContextId =
  | 'is'
  | 'aile'
  | 'arkadas'
  | 'romantik'
  | 'satici'
  | 'komsu'
  | 'okul'
  | 'dijital';

export type ApiCharacterArchetype =
  | 'babacanManipulator'
  | 'duygusalKurban'
  | 'agresifIsrarci'
  | 'tatliDilliSatici'
  | 'masumRicaci'
  | 'pasifAgresif'
  | 'otoriter';

// Zorluk backend'de 1–5 arası ham yıldız sayısı. Kolay/Orta/Zor gruplaması
// client tarafında türetilir (bkz. getDifficultyLevel) — backend göndermiyor.
export type ApiDifficulty = 1 | 2 | 3 | 4 | 5;

// GET /api/Scenarios ve /by-context/{id} bunun dizisini döner.
export interface ApiScenarioListItem {
  id: string;
  title: string;
  shortDescription: string;
  contextId: ApiContextId;
  difficulty: ApiDifficulty;
  estimatedDurationMinutes: number;
  isFree: boolean;
  tags: string[];

  // Kullanıcıya özel ilerleme. Session yazma akışı gelene kadar hepsi 0/false/null.
  isCompleted: boolean;
  bestScore: number | null;
  attemptsCount: number;
}

export interface ApiScenarioScene {
  setting: string;
  situation: string;
  userGoal: string;
  hint: string | null;
}

export interface ApiScenarioCharacter {
  name: string;
  age: number;
  role: string;
  relationship: string;
  archetype: ApiCharacterArchetype;
  avatarImageUrl: string | null;
}

// GET /api/Scenarios/{id} — liste alanlarının hepsi + sahne/karakter/son deneme.
// NOT: pressureTactics DTO'da bilinçli olarak yok, client'a gönderilmiyor.
export interface ApiScenarioDetail extends ApiScenarioListItem {
  scene: ApiScenarioScene;
  character: ApiScenarioCharacter;
  lastAttemptAt: string | null;
}

// GET /api/Scenarios query string'i. İkisi de opsiyonel.
export interface ApiScenarioListParams {
  contextId?: ApiContextId;
  isFreeOnly?: boolean;
}
