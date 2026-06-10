// Paywall — full modal. Crown hero, özellik listesi, fiyat kartı ve (şimdilik disabled)
// yükselt butonu. RevenueCat entegrasyonu sonraki sprint.

import { router } from 'expo-router';
import { Check, Crown, X } from 'phosphor-react-native';
import { Pressable, ScrollView, StyleSheet, Text, View } from 'react-native';
import { LinearGradient } from 'expo-linear-gradient';
import { SafeAreaView } from 'react-native-safe-area-context';

import { Button } from '@/components/ui';
import {
  Colors,
  FontSize,
  FontWeight,
  Radius,
  Spacing,
} from '@/constants/theme';

const FEATURES = [
  '5 bağlamın tamamına erişim',
  '50+ senaryo (genişleyen kütüphane)',
  'Sınırsız AI pratik',
  'Detaylı performans analizi',
  'Sesli pratik (yakında)',
];

export default function PaywallScreen() {
  return (
    <SafeAreaView style={styles.safeArea} edges={['top']}>
      {/* Üst bar — kapat */}
      <View style={styles.topBar}>
        <Pressable
          onPress={() => router.back()}
          style={styles.closeButton}
          accessibilityRole="button"
          accessibilityLabel="Kapat"
        >
          <X size={24} color={Colors.textSecondary} weight="bold" />
        </Pressable>
      </View>

      <ScrollView
        contentContainerStyle={styles.scrollContent}
        showsVerticalScrollIndicator={false}
      >
        {/* Hero */}
        <LinearGradient
          colors={[Colors.primarySoft, Colors.background]}
          start={{ x: 0, y: 0 }}
          end={{ x: 0, y: 1 }}
          style={styles.hero}
        >
          <View style={styles.crownCircle}>
            <Crown size={64} color={Colors.secondary} weight="fill" />
          </View>
          <Text style={styles.title}>Tüm Bağlamlara Erişim</Text>
          <Text style={styles.subtitle}>
            Aile, arkadaş, romantik ve satıcı bağlamlarındaki 27 senaryoya sınırsız
            erişim
          </Text>
        </LinearGradient>

        {/* Özellikler */}
        <View style={styles.features}>
          {FEATURES.map((feature) => (
            <View key={feature} style={styles.featureRow}>
              <Check size={20} color={Colors.success} weight="bold" />
              <Text style={styles.featureText}>{feature}</Text>
            </View>
          ))}
        </View>

        {/* Fiyat kartı */}
        <View style={styles.priceCard}>
          <Text style={styles.priceLabel}>AYLIK</Text>
          <View style={styles.priceRow}>
            <Text style={styles.price}>₺49.99</Text>
            <Text style={styles.priceSuffix}>/ay</Text>
          </View>
          <Text style={styles.priceNote}>İstediğin zaman iptal et</Text>
        </View>

        {/* Aksiyonlar */}
        <View style={styles.actions}>
          <Button variant="primary" size="lg" fullWidth disabled onPress={() => {}}>
            Premium&apos;a Yükselt
          </Button>
          <Text style={styles.soon}>yakında</Text>
          <Button variant="ghost" size="sm" onPress={() => {}}>
            Şartlar ve koşullar
          </Button>
        </View>
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safeArea: {
    flex: 1,
    backgroundColor: Colors.background,
  },
  topBar: {
    flexDirection: 'row',
    justifyContent: 'flex-end',
    paddingHorizontal: Spacing.md,
    paddingTop: Spacing.xs,
  },
  closeButton: {
    padding: Spacing.xs,
  },
  scrollContent: {
    paddingBottom: 100,
    gap: Spacing.lg,
  },

  // Hero
  hero: {
    alignItems: 'center',
    paddingHorizontal: Spacing.xl,
    paddingTop: Spacing.md,
    paddingBottom: Spacing.xl,
    gap: Spacing.md,
  },
  crownCircle: {
    width: 110,
    height: 110,
    borderRadius: Radius.full,
    backgroundColor: Colors.secondarySoft,
    alignItems: 'center',
    justifyContent: 'center',
  },
  title: {
    fontSize: FontSize.hero,
    fontWeight: FontWeight.bold,
    color: Colors.text,
    textAlign: 'center',
  },
  subtitle: {
    fontSize: FontSize.base,
    color: Colors.textSecondary,
    textAlign: 'center',
  },

  // Özellikler
  features: {
    paddingHorizontal: Spacing.lg,
    gap: Spacing.md,
  },
  featureRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: Spacing.md,
  },
  featureText: {
    flex: 1,
    fontSize: FontSize.base,
    color: Colors.text,
  },

  // Fiyat kartı
  priceCard: {
    marginHorizontal: Spacing.lg,
    backgroundColor: Colors.primarySoft,
    borderWidth: 2,
    borderColor: Colors.primary,
    borderRadius: Radius.xl,
    padding: Spacing.lg,
    alignItems: 'center',
    gap: Spacing.xs,
  },
  priceLabel: {
    fontSize: FontSize.xs,
    fontWeight: FontWeight.bold,
    color: Colors.textSecondary,
    letterSpacing: 1,
  },
  priceRow: {
    flexDirection: 'row',
    alignItems: 'baseline',
    gap: Spacing.xs,
  },
  price: {
    fontSize: FontSize.display,
    fontWeight: FontWeight.bold,
    color: Colors.primary,
  },
  priceSuffix: {
    fontSize: FontSize.base,
    color: Colors.textSecondary,
  },
  priceNote: {
    fontSize: FontSize.sm,
    color: Colors.textSecondary,
  },

  // Aksiyonlar
  actions: {
    paddingHorizontal: Spacing.lg,
    alignItems: 'center',
    gap: Spacing.xs,
  },
  soon: {
    fontSize: FontSize.xs,
    color: Colors.textMuted,
    fontStyle: 'italic',
  },
});
