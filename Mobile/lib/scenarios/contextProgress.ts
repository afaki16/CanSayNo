// Bağlam kartlarındaki "tamamlanan/toplam" sayıları. Sayılar statik metadata'da
// DEĞİL, API'den gelen senaryo listesinden türetilir.

import type { ContextId, ContextProgress, Scenario } from '@/types/scenario';

const EMPTY: ContextProgress = { scenariosCount: 0, completedCount: 0 };

// Senaryo listesini contextId'ye göre gruplayıp her bağlamın sayılarını çıkarır.
export function buildContextProgress(
  scenarios: Scenario[] | undefined,
): Record<ContextId, ContextProgress> {
  const byContext = {} as Record<ContextId, ContextProgress>;

  for (const scenario of scenarios ?? []) {
    const current = byContext[scenario.contextId] ?? EMPTY;
    byContext[scenario.contextId] = {
      scenariosCount: current.scenariosCount + 1,
      completedCount: current.completedCount + (scenario.isCompleted ? 1 : 0),
    };
  }

  return byContext;
}

// Hiç senaryosu olmayan bağlamlar için 0/0 döner.
export function progressFor(
  progress: Record<ContextId, ContextProgress>,
  contextId: ContextId,
): ContextProgress {
  return progress[contextId] ?? EMPTY;
}
