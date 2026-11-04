// RevenueCat ile tek temas noktası. Uygulamanın geri kalanı react-native-purchases'ı
// doğrudan import etmez.
//
// Kimlik: RevenueCat appUserID = backend User.Id (string). Webhook kullanıcıyı bu
// id'den bulur (anonim $RCAnonymousID olayları backend'de yok sayılır), bu yüzden
// SDK yalnızca giriş yapmış kullanıcıyla yapılandırılır; girişsiz satın alma yok.
//
// Premium kararı backend'de (GET /api/Subscriptions/me). Buradaki CustomerInfo
// arayüz kararları için kullanılmaz — webhook gelene kadar backend'i bekleriz.
//
// Expo Go'da SDK "browser mode"a geçer (native modül yok); gerçek satın alma için
// development build gerekir.

import Purchases, {
  LOG_LEVEL,
  PURCHASES_ERROR_CODE,
  type PurchasesError,
  type PurchasesOffering,
  type PurchasesPackage,
} from 'react-native-purchases';

import { env } from '@/lib/config/env';

export type { PurchasesOffering, PurchasesPackage };

let configured = false;
let currentUserId: string | null = null;
// syncPurchasesUser çağrıları sıralı çalışır (hızlı giriş/çıkışta logIn/logOut yarışmasın).
let queue: Promise<void> = Promise.resolve();

export function isPurchasesEnabled(): boolean {
  return env.revenueCatApiKey !== null;
}

/**
 * SDK kimliğini oturumla eşitler. Girişte (userId) yapılandırır ya da logIn yapar,
 * çıkışta (null) logOut yapar. Hata fırlatmaz — satın alma kapalı kalır, uygulama çalışır.
 */
export function syncPurchasesUser(userId: number | null): Promise<void> {
  queue = queue.then(() => doSync(userId)).catch((e) => {
    console.warn('[purchases] kimlik eşitlenemedi', e);
  });
  return queue;
}

async function doSync(userId: number | null): Promise<void> {
  const apiKey = env.revenueCatApiKey;
  if (!apiKey) {
    return;
  }
  const appUserID = userId === null ? null : String(userId);
  if (appUserID === currentUserId) {
    return;
  }

  if (!configured) {
    if (appUserID === null) {
      return; // Girişsiz kullanıcı için yapılandırma yok (anonim kullanıcı açılmasın).
    }
    if (env.isDev) {
      await Purchases.setLogLevel(LOG_LEVEL.DEBUG);
    }
    Purchases.configure({ apiKey, appUserID });
    configured = true;
  } else if (appUserID !== null) {
    await Purchases.logIn(appUserID);
  } else {
    // logOut önceki kullanıcının önbelleğini temizler; SDK anonim kullanıcıya geçer.
    // Sonraki girişte logIn ile tekrar gerçek id'ye döner.
    await Purchases.logOut();
  }
  currentUserId = appUserID;
}

/** Mevcut teklif (RevenueCat panelindeki "current" offering). Yoksa null. */
export async function getCurrentOffering(): Promise<PurchasesOffering | null> {
  await queue;
  if (!configured || currentUserId === null) {
    return null;
  }
  const offerings = await Purchases.getOfferings();
  return offerings.current;
}

export async function purchasePackage(pkg: PurchasesPackage): Promise<void> {
  await queue;
  await Purchases.purchasePackage(pkg);
}

export async function restorePurchases(): Promise<void> {
  await queue;
  await Purchases.restorePurchases();
}

function purchasesErrorCode(error: unknown): PURCHASES_ERROR_CODE | null {
  if (typeof error === 'object' && error !== null && 'code' in error) {
    return (error as PurchasesError).code;
  }
  return null;
}

export function isPurchaseCancelled(error: unknown): boolean {
  return purchasesErrorCode(error) === PURCHASES_ERROR_CODE.PURCHASE_CANCELLED_ERROR;
}

export function toPurchaseErrorMessage(error: unknown): string {
  switch (purchasesErrorCode(error)) {
    case PURCHASES_ERROR_CODE.NETWORK_ERROR:
      return 'İnternet bağlantını kontrol edip tekrar dene.';
    case PURCHASES_ERROR_CODE.PAYMENT_PENDING_ERROR:
      return 'Ödemen onay bekliyor. Onaylanınca Premium otomatik açılacak.';
    case PURCHASES_ERROR_CODE.PRODUCT_ALREADY_PURCHASED_ERROR:
      return 'Bu abonelik zaten hesabına tanımlı. "Satın alımı geri yükle"yi dene.';
    case PURCHASES_ERROR_CODE.RECEIPT_ALREADY_IN_USE_ERROR:
      return 'Bu mağaza aboneliği başka bir CanSayNo hesabına bağlı.';
    case PURCHASES_ERROR_CODE.PURCHASE_NOT_ALLOWED_ERROR:
      return 'Bu cihazda satın alma yapılamıyor.';
    case PURCHASES_ERROR_CODE.STORE_PROBLEM_ERROR:
      return 'Mağazaya ulaşılamadı. Biraz sonra tekrar dene.';
    default:
      return 'Satın alma tamamlanamadı. Lütfen tekrar dene.';
  }
}
