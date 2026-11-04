// GET /api/Subscriptions/me wire tipi. Premium kararı backend'de; bu yalnızca gösterim için.

export type ApiSubscriptionStatus = 'active' | 'cancelled' | 'gracePeriod' | 'expired' | 'pending';

export interface ApiMySubscription {
  isPremium: boolean;
  productId: string | null;
  status: ApiSubscriptionStatus | null; // kayıt yoksa null
  expiresAt: string | null; // ISO-8601 UTC; süresizse null
  willRenew: boolean;
}
