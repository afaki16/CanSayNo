// Bağlam kartı — Senaryolar tab'ındaki renkli grid'in tek hücresi.
// Dolgun bağlam rengi zemin, beyaz içerik. Premium bağlamlarda kilit + PREMIUM chip,
// ücretsizlerde "tamamlanan/toplam" mini ilerleme göstergesi.

import { Lock } from 'phosphor-react-native';
import { Pressable, StyleSheet, Text, View } from 'react-native';

import {
  Colors,
  FontSize,
  FontWeight,
  Radius,
  Shadow,
  Spacing,
} from '@/constants/theme';
import { getContextIcon } from '@/lib/icons';
import type { Context } from '@/types/scenario';

export interface ContextCardProps {
  context: Context;
  onPress: () => void;
}

export function ContextCard({ context, onPress }: ContextCardProps) {
  const Icon = getContextIcon(context.iconName);
  const isLocked = !context.isFree;

  return (
    <Pressable
      onPress={onPress}
      accessibilityRole="button"
      accessibilityLabel={context.name}
      style={({ pressed }) => [
        styles.card,
        { backgroundColor: context.color, opacity: pressed ? 0.85 : 1 },
      ]}
    >
      {/* Üst satır: ikon + (kilit) */}
      <View style={styles.topRow}>
        <Icon size={32} color={Colors.surface} weight="fill" />
        {isLocked ? (
          <Lock size={20} color={Colors.surface} weight="fill" style={styles.lock} />
        ) : null}
      </View>

      {/* Alt blok: başlık + açıklama + ilerleme/etiket */}
      <View style={styles.bottom}>
        <Text style={styles.title} numberOfLines={1}>
          {context.name}
        </Text>
        <Text style={styles.description} numberOfLines={2}>
          {context.description}
        </Text>

        {isLocked ? (
          <View style={styles.premiumChip}>
            <Text style={[styles.premiumChipText, { color: context.color }]}>
              PREMIUM
            </Text>
          </View>
        ) : (
          <Text style={styles.progress}>
            {context.completedCount}/{context.scenariosCount} tamamlandı
          </Text>
        )}
      </View>
    </Pressable>
  );
}

const styles = StyleSheet.create({
  card: {
    flex: 1,
    minHeight: 140,
    padding: Spacing.lg,
    borderRadius: Radius.xl,
    justifyContent: 'space-between',
    ...Shadow.md,
  },
  topRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'flex-start',
  },
  lock: {
    opacity: 0.8,
  },
  bottom: {
    marginTop: Spacing.md,
    gap: 2,
  },
  title: {
    fontSize: FontSize.lg,
    fontWeight: FontWeight.bold,
    color: Colors.surface,
  },
  description: {
    fontSize: FontSize.xs,
    color: Colors.surface,
    opacity: 0.85,
  },
  progress: {
    marginTop: Spacing.sm,
    fontSize: FontSize.xs,
    color: Colors.surface,
    opacity: 0.9,
    fontWeight: FontWeight.semibold,
  },
  premiumChip: {
    marginTop: Spacing.sm,
    alignSelf: 'flex-start',
    backgroundColor: Colors.surface,
    paddingHorizontal: Spacing.sm,
    paddingVertical: 2,
    borderRadius: Radius.full,
  },
  premiumChipText: {
    fontSize: FontSize.xs,
    fontWeight: FontWeight.bold,
    letterSpacing: 0.5,
  },
});
