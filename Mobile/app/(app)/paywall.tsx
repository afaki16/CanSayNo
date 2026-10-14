// Paywall — koyu tam ekran modal. Taç hero, fayda listesi, yıllık/aylık plan
// seçimi ve CTA. Satın alma RevenueCat entegrasyonuna kadar devre dışı;
// fiyatlar şimdilik sabit (aylık ₺79, yıllık ₺499).

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
import { Pressable, ScrollView, StyleSheet, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';

import { Button, Text } from '@/components/ui';
import { Colors, FontSize, FontWeight, Radius, Spacing } from '@/constants/theme';

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
    title: '5 bağlamın tamamı',
    description: 'Aile, arkadaş, romantik, satıcı + iş',
  },
  {
    icon: InfinityIcon,
    color: '#3DD68C',
    bg: 'rgba(31,169,113,0.18)',
    title: 'Sınırsız pratik',
    description: 'Günlük limit yok, dilediğin kadar dene',
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

export default function PaywallScreen() {
  const [plan, setPlan] = useState<PlanId>('yearly');

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

        {/* Planlar */}
        <View style={styles.plans}>
          <Pressable
            onPress={() => setPlan('yearly')}
            accessibilityRole="radio"
            accessibilityState={{ selected: plan === 'yearly' }}
            style={[styles.planCard, plan === 'yearly' && styles.planCardSelected]}
          >
            <View style={styles.planBadge}>
              <Text style={styles.planBadgeText}>EN POPÜLER · %48 İNDİRİM</Text>
            </View>
            <View style={styles.planRow}>
              <View>
                <Text style={styles.planName}>Yıllık</Text>
                <Text style={styles.planNote}>₺41,58 / ay olarak faturalanır</Text>
              </View>
              <View style={styles.planPriceCol}>
                <Text style={styles.planPrice}>₺499</Text>
                <Text style={styles.planOldPrice}>₺948</Text>
              </View>
            </View>
          </Pressable>

          <Pressable
            onPress={() => setPlan('monthly')}
            accessibilityRole="radio"
            accessibilityState={{ selected: plan === 'monthly' }}
            style={[styles.planCard, plan === 'monthly' && styles.planCardSelected]}
          >
            <View style={styles.planRow}>
              <View>
                <Text style={styles.planName}>Aylık</Text>
                <Text style={styles.planNote}>İstediğin zaman iptal et</Text>
              </View>
              <Text style={styles.planPrice}>₺79</Text>
            </View>
          </Pressable>
        </View>

        {/* CTA — RevenueCat gelene kadar devre dışı */}
        <View style={styles.cta}>
          <Button variant="primary" size="lg" fullWidth disabled onPress={() => {}}>
            Premium&apos;a geç
          </Button>
          <Text style={styles.soonNote}>Satın alma yakında aktif olacak</Text>
          <Text style={styles.footer}>
            Satın alımı geri yükle · Koşullar · Gizlilik
          </Text>
        </View>
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
});
