// Senaryo domain tipleri. Bağlamlar (Context) ve ileride senaryo (Scenario)
// tipleri burada toplanır. Şimdilik mock data bu tipleri kullanır; API bağlandığında
// genişletilebilir.

// Bağlam kimlikleri — uygulamadaki sabit pratik kategorileri.
export type ContextId = 'is' | 'aile' | 'arkadas' | 'romantik' | 'satici';

export interface Context {
  id: ContextId;
  name: string;
  description: string;
  iconName: string; // Phosphor icon adı (string olarak; runtime'da component'e map'lenir)
  color: string;
  softColor: string;
  isFree: boolean;
  scenariosCount: number;
  completedCount: number;
}

// Zorluk: yıldız (1-5) ham veride tutulur; UI gruplaması için kolay/orta/zor'a indirgenir.
export type DifficultyLevel = 'kolay' | 'orta' | 'zor';
export type DifficultyStars = 1 | 2 | 3 | 4 | 5;

export interface ScenarioScene {
  setting: string; // "Cuma akşam 18:00, hafta sonu planın var"
  situation: string; // "Telefonun titriyor. Patronundan WhatsApp mesajı geldi."
  userGoal: string; // "Hafta sonu planını korumak. Köprüleri yakmadan."
  hint?: string; // "Net olmak, kabalık değildir."
}

export interface ScenarioCharacter {
  name: string;
  age: number;
  role: string;
  relationship: string;
  archetype: string; // örn: "babacan_manipulator"
}

export interface Scenario {
  id: string;
  contextId: ContextId;
  title: string;
  shortDescription: string;
  difficulty: DifficultyStars;
  estimatedDurationMinutes: number;
  scene: ScenarioScene;
  character: ScenarioCharacter;

  // User-spesifik (gelecekte API'den gelecek):
  isCompleted: boolean;
  bestScore: number | null;
  attemptsCount: number;

  // Tag'ler (filtreleme için, ileride):
  tags: string[];
}

// Helper: yıldız sayısından kolay/orta/zor'a:
export function getDifficultyLevel(stars: DifficultyStars): DifficultyLevel {
  if (stars <= 2) return 'kolay';
  if (stars === 3) return 'orta';
  return 'zor';
}

export function getDifficultyLabel(level: DifficultyLevel): string {
  return level === 'kolay' ? 'Kolay' : level === 'orta' ? 'Orta' : 'Zor';
}

// Tema renk anahtarı döner ('success' | 'warning' | 'error'); component'te Colors ile birleştirilir.
export function getDifficultyColor(
  level: DifficultyLevel,
): 'success' | 'warning' | 'error' {
  return level === 'kolay' ? 'success' : level === 'orta' ? 'warning' : 'error';
}
