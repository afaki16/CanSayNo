// Abonelik durumu. RevenueCat webhook'larıyla backend'de güncellenir.

import { apiGet } from '../hooks';
import { SUBSCRIPTION_ENDPOINTS } from '../endpoints';

import type { ApiMySubscription } from '@/types/api/subscription';

export const subscriptionService = {
  me: () => apiGet<ApiMySubscription>(SUBSCRIPTION_ENDPOINTS.ME),
};
