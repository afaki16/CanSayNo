// RevenueCat kimliğini oturumla eşitler (kök layout'ta bir kez). Giriş, çıkış,
// hesap değişimi ve refresh hatası hep authStore.user üzerinden geçtiği için
// tüm yollar burada yakalanır.

import { useEffect } from 'react';

import { syncPurchasesUser } from '@/lib/purchases/purchases';
import { useAuthStore } from '@/stores/authStore';

export function usePurchasesIdentity() {
  const userId = useAuthStore((state) => state.user?.id ?? null);
  const isRehydrating = useAuthStore((state) => state.isRehydrating);

  useEffect(() => {
    if (!isRehydrating) {
      void syncPurchasesUser(userId);
    }
  }, [userId, isRehydrating]);
}
