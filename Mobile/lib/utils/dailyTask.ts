// Anasayfa "Bugünkü görev" önerisi. Saf fonksiyon: senaryo listesinden (backend
// sırası korunur) kullanıcının ilerlemesine göre bir senaryo seçer.
//
// Öncelik: hiç denenmemiş → başlanıp tamamlanmamış → en düşük skorlu tamamlanmış.
// Premium olmayan kullanıcıya yalnızca ücretsiz senaryolar önerilir (paywall'a
// göndermemek için); premium kullanıcıya hepsi.

import type { ApiContextId, ApiScenarioListItem } from '@/types/api/scenario';

export type DailyTaskReason = 'new' | 'unfinished' | 'improve';

export interface DailyTask {
  scenarioId: string;
  title: string;
  subtitle: string;
  reason: DailyTaskReason;
}

// Bağlama göre görev başlığı — tasarımdaki "İş baskısıyla pratik yap" kalıbı.
const CONTEXT_TASK_TITLES: Record<ApiContextId, string> = {
  is: 'İş baskısıyla pratik yap',
  aile: 'Aile baskısıyla pratik yap',
  arkadas: 'Arkadaş baskısıyla pratik yap',
  romantik: 'İlişki baskısıyla pratik yap',
  satici: 'Satıcı ısrarıyla pratik yap',
};

function pick(scenarios: ApiScenarioListItem[], isPremium: boolean): {
  scenario: ApiScenarioListItem;
  reason: DailyTaskReason;
} | null {
  const candidates = isPremium ? scenarios : scenarios.filter((s) => s.isFree);
  if (candidates.length === 0) return null;

  const fresh = candidates.find((s) => s.attemptsCount === 0);
  if (fresh) return { scenario: fresh, reason: 'new' };

  const unfinished = candidates.find((s) => !s.isCompleted);
  if (unfinished) return { scenario: unfinished, reason: 'unfinished' };

  // Hepsi tamamlandı: en düşük en-iyi-skorlu olanı geliştirmeye çağır.
  const weakest = [...candidates].sort(
    (a, b) => (a.bestScore ?? 0) - (b.bestScore ?? 0),
  )[0];
  return { scenario: weakest, reason: 'improve' };
}

export function getDailyTask(
  scenarios: ApiScenarioListItem[],
  isPremium = false,
): DailyTask | null {
  const choice = pick(scenarios, isPremium);
  if (!choice) return null;

  const { scenario, reason } = choice;
  const duration = `${scenario.estimatedDurationMinutes} dk`;

  if (reason === 'improve') {
    return {
      scenarioId: scenario.id,
      title: 'Skorunu yükselt',
      subtitle:
        scenario.bestScore !== null
          ? `${scenario.title} · en iyi: ${scenario.bestScore}`
          : `${scenario.title} · ${duration}`,
      reason,
    };
  }

  return {
    scenarioId: scenario.id,
    title:
      reason === 'unfinished'
        ? 'Yarım kalanı bitir'
        : CONTEXT_TASK_TITLES[scenario.contextId],
    subtitle: `${scenario.title} · ${duration}`,
    reason,
  };
}
