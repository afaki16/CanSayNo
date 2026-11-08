// Paywall — koyu tam ekran modal. Taç hero, fayda listesi, yıllık/aylık plan
// seçimi ve CTA. Planlar ve fiyatlar RevenueCat "current" offering'inden gelir
// (mağazanın yerel fiyat metni); satın alma/geri yükleme usePremiumPurchase'ta.
// Premium, mağaza işleminden sonra backend webhook'u görünce açılır.

import { LinearGradient } from 'expo-linear-gradient';
import { router } from 'expo-router';
import { StatusBar } from 'expo-status-bar';
import {
  ChartLineUp,
  Crown,
  type Icon,
  InfinityIcon,
  LockKeyOpen,
  Microphone,
  X,
} from 'phosphor-react-native';
import { useState } from 'react';
import { ActivityIndicator, Alert, Pressable, ScrollView, StyleSheet, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';

import { Button, LegalLinks, Text } from '@/components/ui';
import { Colors, FontSize, FontWeight, Radius, Spacing } from '@/constants/theme';
import { useMySubscription } from '@/hooks/queries/useSubscription';
import { usePremiumPurchase } from '@/hooks/usePremiumPurchase';
import { toPurchaseErrorMessage, type PurchasesPackage } from '@/lib/purchases/purchases';

interface Benefit {
  icon: Icon;
  color: string;
  bg: string;
  title: string;
  description: string;
  soon?: boolean;
}

const BENEFITS: Benefit[] = [
  {
    icon: LockKeyOpen,
    color: Colors.primaryLight,
    bg: 'rgba(45,127,249,0.18)',
    title: '8 bağlamın tamamı',
    description: 'Aile, ilişki, okul, komşu, dijital ve daha fazlası',
  },
  {
    icon: InfinityIcon,
    color: '#3DD68C',
    bg: 'rgba(31,169,113,0.18)',
    title: 'Her gün daha çok pratik',
    description: 'Günde 30 pratiğe kadar, dilediğin senaryoda',
  },
  {
    icon: ChartLineUp,
    color: '#FF8A5B',
    bg: 'rgba(255,112,67,0.18)',
    title: 'Detaylı geri bildirim',
    description: 'Beceri kırılımı + kişisel koç notları',
  },
  {
    icon: Microphone,
    color: '#FFD166',
    bg: 'rgba(247,181,0,0.18)',
    title: 'Sesli pratik',
    description: 'Gerçek konuşma temposunda alıştırma',
    soon: true,
  },
];

type PlanId = 'yearly' | 'monthly';

const WHITE_55 = 'rgba(255,255,255,0.55)';

// Yıllığın aylık eşdeğerine göre indirim yüzdesi (ör. 48). Hesaplanamazsa null.
function yearlyDiscountPercent(
  yearly: PurchasesPackage | null,
  monthly: PurchasesPackage | null,
): number | null {
  if (!yearly || !monthly || monthly.product.price <= 0) {
    return null;
  }
  const percent = Math.round((1 - yearly.product.price / (monthly.product.price * 12)) * 100);
  return percent > 0 ? percent : null;
}

// Aylığın 12 katı, mağazanın para biriminde ("₺948"). Biçimlenemezse null.
function twelveMonthsPrice(monthly: PurchasesPackage | null): string | null {
  if (!monthly) {
    return null;
  }
  try {
    return new Intl.NumberFormat('tr-TR', {
      style: 'currency',
      currency: monthly.product.currencyCode,
      maximumFractionDigits: 0,
    }).format(monthly.product.price * 12);
  } catch {
    return null;
  }
}

export default function PaywallScreen() {
  const [plan, setPlan] = useState<PlanId>('yearly');
  const { data: subscription } = useMySubscription();
  const {
    enabled,
    yearly,
    monthly,
    isLoadingOffering,
    offeringError,
    refetchOffering,
    purchase,
    isPurchasing,
    restore,
    isRestoring,
  } = usePremiumPurchase();

  const isPremium = subscription?.isPremium ?? false;
  const hasPlans = yearly !== null || monthly !== null;
  // Seçili plan teklifte yoksa diğerine düş (ör. yalnızca aylık tanımlıysa).
  const selectedPlan: PlanId =
    plan === 'yearly' ? (yearly ? 'yearly' : 'monthly') : monthly ? 'monthly' : 'yearly';
  const selectedPackage = selectedPlan === 'yearly' ? yearly : monthly;
  const discount = yearlyDiscountPercent(yearly, monthly);
  const oldYearlyPrice = discount !== null ? twelveMonthsPrice(monthly) : null;

  const onPurchase = async () => {
    if (!selectedPackage) {
      return;
    }
    try {
      const outcome = await purchase(selectedPackage);
      if (outcome === 'premium') {
        Alert.alert('Premium aktif', 'Tüm bağlamların kilidi açıldı.', [
          { text: 'Harika', onPress: () => router.back() },
        ]);
      } else if (outcome === 'pending') {
        Alert.alert('Satın alma alındı', 'Premium birkaç dakika içinde hesabında aktif olacak.', [
          { text: 'Tamam', onPress: () => router.back() },
        ]);
      }
      // 'cancelled': kullanıcı vazgeçti, ekranda kal.
    } catch (e) {
      Alert.alert('Satın alma tamamlanamadı', toPurchaseErrorMessage(e));
    }
  };

  const onRestore = async () => {
    try {
      const outcome = await restore();
      if (outcome === 'premium') {
        Alert.alert('Geri yüklendi', 'Premium aboneliğin bu hesapta aktif.', [
          { text: 'Tamam', onPress: () => router.back() },
        ]);
      } else {
        Alert.alert(
          'Aktif abonelik bulunamadı',
          'Bu mağaza hesabında aktif bir CanSayNo aboneliği görünmüyor.',
        );
      }
    } catch (e) {
      Alert.alert('Geri yüklenemedi', toPurchaseErrorMessage(e));
    }
  };

  return (
    <SafeAreaView style={styles.safeArea} edges={['top', 'bottom']}>
      <StatusBar style="light" />
      <ScrollView
        contentContainerStyle={styles.scrollContent}
        showsVerticalScrollIndicator={false}
      >
        {/* Kapat */}
        <View style={styles.topBar}>
          <Pressable
            onPress={() => router.back()}
            style={styles.closeButton}
            hitSlop={8}
            accessibilityRole="button"
            accessibilityLabel="Kapat"
          >
            <X size={16} color="rgba(255,255,255,0.7)" weight="bold" />
          </Pressable>
        </View>

        {/* Hero */}
        <View style={styles.hero}>
          <LinearGradient
            colors={['#FFD166', Colors.amber]}
            start={{ x: 0, y: 0 }}
            end={{ x: 1, y: 1 }}
            style={styles.crownBox}
          >
            <Crown size={40} color={Colors.ink} weight="fill" />
          </LinearGradient>
          <Text style={styles.title}>CanSayNo Premium</Text>
          <Text style={styles.subtitle}>
            Her ilişkide sınırını koymayı pratik et. Tek seferlik değil, ömür boyu
            beceri.
          </Text>
        </View>

        {/* Faydalar */}
        <View style={styles.benefits}>
          {BENEFITS.map((benefit) => {
            const BenefitIcon = benefit.icon;
            return (
              <View key={benefit.title} style={styles.benefitRow}>
                <View style={[styles.benefitIcon, { backgroundColor: benefit.bg }]}>
                  <BenefitIcon size={20} color={benefit.color} weight="fill" />
                </View>
                <View style={styles.flex}>
                  <View style={styles.benefitTitleRow}>
                    <Text style={styles.benefitTitle}>{benefit.title}</Text>
                    {benefit.soon ? (
                      <View style={styles.soonTag}>
                        <Text style={styles.soonTagText}>YAKINDA</Text>
                      </View>
                    ) : null}
                  </View>
                  <Text style={styles.benefitDescription}>{benefit.description}</Text>
                </View>
              </View>
            );
          })}
        </View>

        {isPremium ? (
          <View style={styles.premiumBox}>
            <Text style={styles.premiumTitle}>Premium zaten aktif</Text>
            <Text style={styles.premiumText}>
              Tüm bağlamlar hesabında açık.
            </Text>
            <Button variant="primary" size="lg" fullWidth onPress={() => router.back()}>
              Pratiğe dön
            </Button>
          </View>
        ) : (
          <>
            {/* Planlar */}
            <View style={styles.plans}>
              {isLoadingOffering ? (
                <View style={styles.plansState}>
                  <ActivityIndicator color="#FFFFFF" />
                </View>
              ) : !enabled ? (
                <View style={styles.plansState}>
                  <Text style={styles.stateText}>Satın alma bu sürümde kapalı.</Text>
                </View>
              ) : offeringError || !hasPlans ? (
                <View style={styles.plansState}>
                  <Text style={styles.stateText}>Planlar yüklenemedi.</Text>
                  <Pressable
                    onPress={() => refetchOffering()}
                    hitSlop={8}
                    accessibilityRole="button"
                  >
                    <Text style={styles.retryText}>Tekrar dene</Text>
                  </Pressable>
                </View>
              ) : (
                <>
                  {yearly ? (
                    <Pressable
                      onPress={() => setPlan('yearly')}
                      accessibilityRole="radio"
                      accessibilityState={{ selected: selectedPlan === 'yearly' }}
                      style={[styles.planCard, selectedPlan === 'yearly' && styles.planCardSelected]}
                    >
                      {discount !== null ? (
                        <View style={styles.planBadge}>
                          <Text style={styles.planBadgeText}>
                            EN POPÜLER · %{discount} İNDİRİM
                          </Text>
                        </View>
                      ) : null}
                      <View style={styles.planRow}>
                        <View>
                          <Text style={styles.planName}>Yıllık</Text>
                          {yearly.product.pricePerMonthString ? (
                            <Text style={styles.planNote}>
                              {yearly.product.pricePerMonthString} / ay olarak faturalanır
                            </Text>
                          ) : null}
                        </View>
                        <View style={styles.planPriceCol}>
                          <Text style={styles.planPrice}>{yearly.product.priceString}</Text>
                          {oldYearlyPrice ? (
                            <Text style={styles.planOldPrice}>{oldYearlyPrice}</Text>
                          ) : null}
                        </View>
                      </View>
                    </Pressable>
                  ) : null}

                  {monthly ? (
                    <Pressable
                      onPress={() => setPlan('monthly')}
                      accessibilityRole="radio"
                      accessibilityState={{ selected: selectedPlan === 'monthly' }}
                      style={[
                        styles.planCard,
                        selectedPlan === 'monthly' && styles.planCardSelected,
                      ]}
                    >
                      <View style={styles.planRow}>
                        <View>
                          <Text style={styles.planName}>Aylık</Text>
                          <Text style={styles.planNote}>İstediğin zaman iptal et</Text>
                        </View>
                        <Text style={styles.planPrice}>{monthly.product.priceString}</Text>
                      </View>
                    </Pressable>
                  ) : null}
                </>
              )}
            </View>

            {/* CTA */}
            <View style={styles.cta}>
              <Button
                variant="primary"
                size="lg"
                fullWidth
                disabled={!selectedPackage || isRestoring}
                loading={isPurchasing}
                onPress={onPurchase}
              >
                Premium&apos;a geç
              </Button>
              {isPurchasing ? (
                <Text style={styles.soonNote}>Satın alma doğrulanıyor…</Text>
              ) : null}
              <Text style={styles.footer}>
                Abonelik, iptal edilmediği sürece dönem sonunda otomatik yenilenir. İptal,
                mağaza hesabının abonelik ayarlarından yapılır.
              </Text>
              {enabled ? (
                <Pressable
                  onPress={onRestore}
                  disabled={isPurchasing || isRestoring}
                  hitSlop={8}
                  accessibilityRole="button"
                  style={styles.restoreButton}
                >
                  <Text style={styles.restoreText}>
                    {isRestoring ? 'Geri yükleniyor…' : 'Satın alımı geri yükle'}
                  </Text>
                </Pressable>
              ) : null}
              {/* Apple abonelik kuralı: satın alma ekranında şartlar + gizlilik linki */}
              <LegalLinks onDark />
            </View>
          </>
        )}
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safeArea: {
    flex: 1,
    backgroundColor: Colors.ink,
  },
  flex: {
    flex: 1,
  },
  scrollContent: {
    paddingHorizontal: Spacing.lg,
    paddingBottom: Spacing.lg,
  },
  topBar: {
    flexDirection: 'row',
    justifyContent: 'flex-end',
    paddingTop: Spacing.sm,
  },
  closeButton: {
    width: 36,
    height: 36,
    borderRadius: 18,
    backgroundColor: 'rgba(255,255,255,0.1)',
    alignItems: 'center',
    justifyContent: 'center',
  },

  // Hero
  hero: {
    alignItems: 'center',
    marginTop: 6,
    marginBottom: 22,
  },
  crownBox: {
    width: 76,
    height: 76,
    borderRadius: 22,
    alignItems: 'center',
    justifyContent: 'center',
    shadowColor: Colors.amber,
    shadowOpacity: 0.35,
    shadowRadius: 15,
    shadowOffset: { width: 0, height: 14 },
    elevation: 8,
  },
  title: {
    marginTop: 18,
    fontSize: FontSize.xxl,
    fontWeight: FontWeight.black,
    letterSpacing: -0.85,
    color: '#FFFFFF',
  },
  subtitle: {
    marginTop: 6,
    maxWidth: 280,
    fontSize: 15,
    lineHeight: 22,
    color: 'rgba(255,255,255,0.6)',
    textAlign: 'center',
  },

  // Faydalar
  benefits: {
    gap: Spacing.md,
    paddingTop: Spacing.sm,
  },
  benefitRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 13,
  },
  benefitIcon: {
    width: 38,
    height: 38,
    borderRadius: 11,
    alignItems: 'center',
    justifyContent: 'center',
  },
  benefitTitleRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 6,
  },
  benefitTitle: {
    fontSize: 15,
    fontWeight: FontWeight.bold,
    color: '#FFFFFF',
  },
  benefitDescription: {
    fontSize: 13,
    color: WHITE_55,
  },
  soonTag: {
    paddingHorizontal: 6,
    paddingVertical: 2,
    borderRadius: 6,
    backgroundColor: 'rgba(255,255,255,0.15)',
  },
  soonTagText: {
    fontSize: 11,
    fontWeight: FontWeight.bold,
    color: '#FFFFFF',
  },

  // Planlar
  plans: {
    gap: 12,
    paddingVertical: Spacing.lg,
  },
  plansState: {
    minHeight: 120,
    alignItems: 'center',
    justifyContent: 'center',
    gap: Spacing.sm,
  },
  stateText: {
    fontSize: FontSize.sm,
    color: WHITE_55,
    textAlign: 'center',
  },
  retryText: {
    fontSize: FontSize.sm,
    fontWeight: FontWeight.bold,
    color: Colors.primaryLight,
  },
  planCard: {
    padding: Spacing.md,
    borderRadius: 18,
    borderWidth: 1,
    borderColor: 'rgba(255,255,255,0.12)',
    backgroundColor: '#23262F',
  },
  planCardSelected: {
    borderWidth: 2,
    borderColor: Colors.primary,
    padding: Spacing.md - 1,
  },
  planBadge: {
    position: 'absolute',
    top: -11,
    left: Spacing.md,
    paddingHorizontal: 11,
    paddingVertical: 4,
    borderRadius: Radius.full,
    backgroundColor: Colors.primary,
  },
  planBadgeText: {
    fontSize: 10,
    fontWeight: FontWeight.black,
    letterSpacing: 0.4,
    color: '#FFFFFF',
  },
  planRow: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    marginTop: 4,
  },
  planName: {
    fontSize: FontSize.base,
    fontWeight: FontWeight.extrabold,
    color: '#FFFFFF',
  },
  planNote: {
    fontSize: FontSize.xs,
    color: WHITE_55,
  },
  planPriceCol: {
    alignItems: 'flex-end',
  },
  planPrice: {
    fontSize: 20,
    fontWeight: FontWeight.black,
    color: '#FFFFFF',
  },
  planOldPrice: {
    fontSize: 11,
    color: 'rgba(255,255,255,0.4)',
    textDecorationLine: 'line-through',
  },

  // Premium zaten aktif
  premiumBox: {
    gap: Spacing.sm,
    paddingVertical: Spacing.lg,
    alignItems: 'center',
  },
  premiumTitle: {
    fontSize: FontSize.lg,
    fontWeight: FontWeight.black,
    color: '#FFFFFF',
  },
  premiumText: {
    fontSize: FontSize.sm,
    color: WHITE_55,
    textAlign: 'center',
    marginBottom: Spacing.sm,
  },

  // CTA
  cta: {
    gap: Spacing.sm,
  },
  soonNote: {
    fontSize: FontSize.xs,
    color: WHITE_55,
    textAlign: 'center',
  },
  footer: {
    marginTop: 6,
    fontSize: FontSize.xs,
    color: 'rgba(255,255,255,0.4)',
    textAlign: 'center',
  },
  restoreButton: {
    alignSelf: 'center',
    paddingVertical: Spacing.xs,
  },
  restoreText: {
    fontSize: FontSize.sm,
    fontWeight: FontWeight.bold,
    color: 'rgba(255,255,255,0.75)',
  },
});
