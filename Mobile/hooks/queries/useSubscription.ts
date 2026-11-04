// Premium durumu (Profil bandı, Bugünkü görev). Erişim kontrolü backend'de yapılır;
// bu sorgu yalnızca arayüzü uyarlamak için.

import { useQuery } from '@tanstack/react-query';

import { subscriptionService } from '@/lib/api';
import type { ApiError } from '@/lib/api';
import type { ApiMySubscription } from '@/types/api/subscription';

export const subscriptionKeys = {
  all: ['subscription'] as const,
  me: () => [...subscriptionKeys.all, 'me'] as const,
};

export function useMySubscription() {
  return useQuery<ApiMySubscription, ApiError>({
    queryKey: subscriptionKeys.me(),
    queryFn: () => subscriptionService.me(),
  });
}
