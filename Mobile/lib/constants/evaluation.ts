// Değerlendirme eksenlerinin UI metadata'sı. Sıra, sonuç ekranındaki gösterim sırasıdır;
// anahtarlar backend'in SessionEvaluationDto.scores anahtarlarıyla birebir aynı.

import type { ApiEvaluationAxis } from '@/types/api/session';

export const EVALUATION_AXES: readonly ApiEvaluationAxis[] = [
  'netlik',
  'nezaket',
  'dayaniklilik',
  'alternatifSunma',
  'ozurDengesi',
];

export const EVALUATION_AXIS_LABELS: Record<ApiEvaluationAxis, string> = {
  netlik: 'Netlik',
  nezaket: 'Nezaket',
  dayaniklilik: 'Dayanıklılık',
  alternatifSunma: 'Alternatif sunma',
  ozurDengesi: 'Özür dengesi',
};
