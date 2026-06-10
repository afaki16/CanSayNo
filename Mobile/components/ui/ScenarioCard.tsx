// Senaryo kartı — senaryo listesinde tek satır. Solda bağlam renginde yuvarlak ikon,
// ortada başlık/açıklama/meta, sağda tamamlanmışsa skor chip'i yoksa ileri ok.

import { ArrowRight } from 'phosphor-react-native';
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
import type { Context, Scenario } from '@/types/scenario';

import { StarRating } from './StarRating';

export interface ScenarioCardProps {
  scenario: Scenario;
  context: Context;
  onPress: () => void;
}

// Skora göre chip rengi (Home'daki eşleme ile tutarlı).
function scoreColors(score: number) {
  if (score >= 80) return { bg: Colors.successSoft, text: Colors.success };
  if (score >= 60) return { bg: Colors.warningSoft, text: Colors.warning };
  return { bg: Colors.errorSoft, text: Colors.error };
}

export function ScenarioCard({ scenario, context, onPress }: ScenarioCardProps) {
  const Icon = getContextIcon(context.iconName);
  const sc =
    scenario.isCompleted && scenario.bestScore !== null
      ? scoreColors(scenario.bestScore)
      : null;

  return (
    <Pressable
      onPress={onPress}
      accessibilityRole="button"
      accessibilityLabel={scenario.title}
      style={({ pressed }) => [styles.card, pressed && styles.pressed]}
    >
      <View style={[styles.iconCircle, { backgroundColor: context.softColor }]}>
        <Icon size={24} color={context.color} weight="fill" />
      </View>

      <View style={styles.info}>
        <Text style={styles.title} numberOfLines={1}>
          {scenario.title}
        </Text>
        <Text style={styles.description} numberOfLines={1}>
          {scenario.shortDescription}
        </Text>
        <View style={styles.metaRow}>
          <Text style={styles.meta}>
            {scenario.estimatedDurationMinutes} dk ·{' '}
          </Text>
          <StarRating count={scenario.difficulty} size={12} />
        </View>
      </View>

      {sc ? (
        <View style={[styles.scoreBadge, { backgroundColor: sc.bg }]}>
          <Text style={[styles.scoreText, { color: sc.text }]}>
            {scenario.bestScore}
          </Text>
        </View>
      ) : (
        <ArrowRight size={20} color={Colors.textMuted} weight="bold" />
      )}
    </Pressable>
  );
}

const styles = StyleSheet.create({
  card: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: Spacing.md,
    minHeight: 90,
    backgroundColor: Colors.surface,
    borderWidth: 1,
    borderColor: Colors.border,
    borderRadius: Radius.lg,
    padding: Spacing.md,
    ...Shadow.sm,
  },
  pressed: {
    opacity: 0.7,
  },
  iconCircle: {
    width: 48,
    height: 48,
    borderRadius: Radius.full,
    alignItems: 'center',
    justifyContent: 'center',
  },
  info: {
    flex: 1,
    gap: 2,
  },
  title: {
    fontSize: FontSize.base,
    fontWeight: FontWeight.semibold,
    color: Colors.text,
  },
  description: {
    fontSize: FontSize.xs,
    color: Colors.textMuted,
  },
  metaRow: {
    flexDirection: 'row',
    alignItems: 'center',
    marginTop: 2,
  },
  meta: {
    fontSize: FontSize.xs,
    color: Colors.textMuted,
  },
  scoreBadge: {
    minWidth: 40,
    height: 32,
    paddingHorizontal: Spacing.sm,
    borderRadius: Radius.full,
    alignItems: 'center',
    justifyContent: 'center',
  },
  scoreText: {
    fontSize: FontSize.sm,
    fontWeight: FontWeight.bold,
  },
});
