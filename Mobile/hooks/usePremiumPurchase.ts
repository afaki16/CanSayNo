// Paywall satın alma akışı: teklif (fiyatlar), satın alma, geri yükleme.
//
// Mağaza işlemi bitince premium hemen açılmaz: backend'e RevenueCat webhook'u
// gelmesi gerekir (genellikle birkaç saniye). Bu yüzden işlem sonrası
// GET /api/Subscriptions/me birkaç kez yeniden çekilir; premium görünürse
// senaryo listeleri de yenilenir (premium senaryoların kilidi açılsın).

import { useMutation, useQuery } from '@tanstack/react-query';

import { scenarioKeys } from '@/hooks/queries/useScenarios';
import { subscriptionKeys } from '@/hooks/queries/useSubscription';
import { queryClient, subscriptionService } from '@/lib/api';
import {
  getCurrentOffering,
  isPurchaseCancelled,
  isPurchasesEnabled,
  purchasePackage,
  restorePurchases,
  type PurchasesPackage,
} from '@/lib/purchases/purchases';
import { useAuthStore } from '@/stores/authStore';

const PREMIUM_POLL_ATTEMPTS = 8;
const PREMIUM_POLL_INTERVAL_MS = 2000;

export type PurchaseOutcome =
  | 'premium' // backend premium'u gördü
  | 'pending' // mağaza işlemi tamam, webhook henüz gelmedi
  | 'cancelled'; // kullanıcı vazgeçti

export type RestoreOutcome = 'premium' | 'notFound';

export const offeringKeys = {
  current: (userId: number | null) => ['offering', 'current', userId] as const,
};

const sleep = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

async function waitForBackendPremium(): Promise<boolean> {
  for (let attempt = 0; attempt < PREMIUM_POLL_ATTEMPTS; attempt++) {
    try {
      const me = await subscriptionService.me();
      queryClient.setQueryData(subscriptionKeys.me(), me);
      if (me.isPremium) {
        await queryClient.invalidateQueries({ queryKey: scenarioKeys.all });
        return true;
      }
    } catch {
      // Geçici ağ hatası: bir sonraki denemede tekrar sor.
    }
    await sleep(PREMIUM_POLL_INTERVAL_MS);
  }
  return false;
}

export function usePremiumPurchase() {
  const userId = useAuthStore((state) => state.user?.id ?? null);
  const enabled = isPurchasesEnabled();

  const offeringQuery = useQuery({
    queryKey: offeringKeys.current(userId),
    queryFn: getCurrentOffering,
    enabled: enabled && userId !== null,
    staleTime: 10 * 60 * 1000,
  });

  const purchaseMutation = useMutation({
    mutationFn: async (pkg: PurchasesPackage): Promise<PurchaseOutcome> => {
      try {
        await purchasePackage(pkg);
      } catch (e) {
        if (isPurchaseCancelled(e)) {
          return 'cancelled';
        }
        throw e;
      }
      return (await waitForBackendPremium()) ? 'premium' : 'pending';
    },
  });

  const restoreMutation = useMutation({
    mutationFn: async (): Promise<RestoreOutcome> => {
      await restorePurchases();
      return (await waitForBackendPremium()) ? 'premium' : 'notFound';
    },
  });

  const offering = offeringQuery.data ?? null;

  return {
    enabled,
    offering,
    yearly: offering?.annual ?? null,
    monthly: offering?.monthly ?? null,
    isLoadingOffering: offeringQuery.isPending && offeringQuery.fetchStatus !== 'idle',
    offeringError: offeringQuery.error,
    refetchOffering: offeringQuery.refetch,

    purchase: purchaseMutation.mutateAsync,
    isPurchasing: purchaseMutation.isPending,
    restore: restoreMutation.mutateAsync,
    isRestoring: restoreMutation.isPending,
  };
}
