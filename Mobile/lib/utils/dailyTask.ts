// Anasayfa "Bugünkü görev" önerisi. Saf fonksiyon: senaryo listesinden (backend
// sırası korunur) kullanıcının ilerlemesine göre bir senaryo seçer.
//
// Öncelik: hiç denenmemiş → başlanıp tamamlanmamış → en düşük skorlu tamamlanmış.
// Premium olmayan kullanıcıya yalnızca ücretsiz senaryolar önerilir (paywall'a
// göndermemek için); premium kullanıcıya hepsi.

import { t } from '@/lib/i18n';
import type { ApiScenarioListItem } from '@/types/api/scenario';

export type DailyTaskReason = 'new' | 'unfinished' | 'improve';

export interface DailyTask {
  scenarioId: string;
  title: string;
  subtitle: string;
  reason: DailyTaskReason;
}

// Bağlama göre görev başlığı (tasarımdaki "İş baskısıyla pratik yap" kalıbı) çeviride:
// practice.dailyTask.titles.<bağlam>. Metinler çağrı anındaki dilde.

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
  const duration = t('practice.minutes', { count: scenario.estimatedDurationMinutes });
  const subtitle = t('practice.dailyTask.subtitle', { title: scenario.title, duration });

  if (reason === 'improve') {
    return {
      scenarioId: scenario.id,
      title: t('practice.dailyTask.improve'),
      subtitle:
        scenario.bestScore !== null
          ? t('practice.dailyTask.subtitleBest', {
              title: scenario.title,
              score: scenario.bestScore,
            })
          : subtitle,
      reason,
    };
  }

  return {
    scenarioId: scenario.id,
    title:
      reason === 'unfinished'
        ? t('practice.dailyTask.unfinished')
        : t(`practice.dailyTask.titles.${scenario.contextId}`),
    subtitle,
    reason,
  };
}
