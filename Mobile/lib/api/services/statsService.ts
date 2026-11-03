// İstatistik endpoint'inin transport katmanı.

import { apiGet } from '../hooks';
import { STATS_ENDPOINTS } from '../endpoints';

import type { ApiUserStats } from '@/types/api/stats';

export const statsService = {
  me: () => apiGet<ApiUserStats>(STATS_ENDPOINTS.ME),
};
