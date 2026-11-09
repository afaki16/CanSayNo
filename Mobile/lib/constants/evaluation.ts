// Değerlendirme eksenlerinin UI metadata'sı. Sıra, sonuç ekranındaki gösterim sırasıdır;
// anahtarlar backend'in SessionEvaluationDto.scores anahtarlarıyla birebir aynı.

import { t } from '@/lib/i18n';
import type { ApiEvaluationAxis } from '@/types/api/session';

export const EVALUATION_AXES: readonly ApiEvaluationAxis[] = [
  'netlik',
  'nezaket',
  'dayaniklilik',
  'alternatifSunma',
  'ozurDengesi',
];

// Eksen etiketi şu anki dilde (çeviri: practice.evaluation.<eksen>).
export function getEvaluationAxisLabel(axis: ApiEvaluationAxis): string {
  return t(`practice.evaluation.${axis}`);
}
