// Senaryo domain tipleri.
//
// Senaryo verisi artık backend'den geliyor; wire şekilleri @/types/api/scenario'da.
// Alan adları birebir örtüştüğü için burada no-op bir mapper tutmuyoruz — domain
// tipleri wire tiplerinin alias'ı. Şekiller ayrışırsa dönüşüm bu dosyaya girer.
//
// Bu dosyanın kendine ait olan kısmı: Context (UI metadata) ve zorluk yardımcıları.

import type {
  ApiCharacterArchetype,
  ApiContextId,
  ApiDifficulty,
  ApiScenarioCharacter,
  ApiScenarioDetail,
  ApiScenarioListItem,
  ApiScenarioScene,
} from '@/types/api/scenario';

export type ContextId = ApiContextId;
export type CharacterArchetype = ApiCharacterArchetype;
export type DifficultyStars = ApiDifficulty;

// Listelerde ve kartlarda taşınan senaryo (sahne/karakter içermez).
export type Scenario = ApiScenarioListItem;

// Detay ekranının senaryosu — sahne, karakter ve son deneme dahil.
export type ScenarioDetail = ApiScenarioDetail;
export type ScenarioScene = ApiScenarioScene;
export type ScenarioCharacter = ApiScenarioCharacter;

// Bağlam UI metadata'sı. Backend'de context endpoint'i YOK; isim/renk/ikon
// tamamen client'a ait (bkz. lib/mockData/contexts.ts).
// Senaryo sayıları burada tutulmaz — gerçek veriden hesaplanır.
export interface Context {
  id: ContextId;
  name: string;
  description: string;
  iconName: string; // Phosphor icon adı (runtime'da getContextIcon ile map'lenir)
  color: string;
  softColor: string;
  isFree: boolean;
}

// Bir bağlamın senaryo listesinden türetilen ilerleme sayıları.
export interface ContextProgress {
  scenariosCount: number;
  completedCount: number;
}

// Zorluk: yıldız (1-5) ham veride tutulur; UI gruplaması için kolay/orta/zor'a
// indirgenir. Backend difficultyLevel göndermez, client türetir.
export type DifficultyLevel = 'kolay' | 'orta' | 'zor';

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
