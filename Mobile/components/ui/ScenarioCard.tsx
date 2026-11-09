// Senaryo kartı — senaryo listesinde tek satır. Solda başlık/açıklama ve
// zorluk + süre meta'sı; sağda tamamlanmışsa en iyi skor, değilse oynat butonu.

import { Clock, Play } from 'phosphor-react-native';
import { useTranslation } from 'react-i18next';
import { Pressable, StyleSheet, View } from 'react-native';

import { Text } from './Text';

import { Colors, FontSize, FontWeight, Radius } from '@/constants/theme';
import type { Context, Scenario } from '@/types/scenario';
import { getDifficultyColor, getDifficultyLevel } from '@/types/scenario';

export interface ScenarioCardProps {
  scenario: Scenario;
  context: Context;
  onPress: () => void;
}

// Senaryo listesindeki skeleton bu yüksekliği kullanır — layout kaymasın.
export const SCENARIO_CARD_MIN_HEIGHT = 104;

// Skora göre rozet rengi (Home'daki eşleme ile tutarlı).
function scoreColors(score: number) {
  if (score >= 80) return { bg: Colors.successSoft, text: Colors.success };
  if (score >= 60) return { bg: Colors.warningSoft, text: Colors.warning };
  return { bg: Colors.errorSoft, text: Colors.error };
}

export function ScenarioCard({ scenario, context, onPress }: ScenarioCardProps) {
  const { t } = useTranslation();
  const level = getDifficultyLevel(scenario.difficulty);
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
      <View style={styles.info}>
        <Text style={styles.title} numberOfLines={2}>
          {scenario.title}
        </Text>
        <Text style={styles.description} numberOfLines={2}>
          {scenario.shortDescription}
        </Text>
        <View style={styles.metaRow}>
          <View style={styles.levelChip}>
            <View
              style={[
                styles.levelDot,
                { backgroundColor: Colors[getDifficultyColor(level)] },
              ]}
            />
            <Text style={styles.levelText}>{t(`practice.difficulty.${level}`)}</Text>
          </View>
          <View style={styles.duration}>
            <Clock size={13} color={Colors.textMuted} />
            <Text style={styles.durationText}>
              {t('practice.minutes', { count: scenario.estimatedDurationMinutes })}
            </Text>
          </View>
        </View>
      </View>

      {sc ? (
        <View style={styles.scoreCol}>
          <View style={[styles.circle, { backgroundColor: sc.bg }]}>
            <Text style={[styles.scoreText, { color: sc.text }]}>
              {scenario.bestScore}
            </Text>
          </View>
          <Text style={styles.scoreLabel}>{t('practice.list.best')}</Text>
        </View>
      ) : (
        <View style={[styles.circle, { backgroundColor: context.softColor }]}>
          <Play size={18} color={context.color} weight="fill" />
        </View>
      )}
    </Pressable>
  );
}

const styles = StyleSheet.create({
  card: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 14,
    minHeight: SCENARIO_CARD_MIN_HEIGHT,
    padding: 15,
    borderRadius: 18,
    borderWidth: 1,
    borderColor: Colors.divider,
    backgroundColor: Colors.surface,
  },
  pressed: {
    opacity: 0.7,
  },
  info: {
    flex: 1,
    minWidth: 0,
  },
  title: {
    fontSize: FontSize.base,
    fontWeight: FontWeight.extrabold,
    letterSpacing: -0.32,
    lineHeight: 19,
    color: Colors.text,
  },
  description: {
    marginTop: 3,
    fontSize: 13,
    lineHeight: 17,
    color: Colors.textMuted,
  },
  metaRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
    marginTop: 10,
  },
  levelChip: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 4,
    paddingHorizontal: 9,
    paddingVertical: 4,
    borderRadius: Radius.full,
    backgroundColor: Colors.chip,
  },
  levelDot: {
    width: 6,
    height: 6,
    borderRadius: 3,
  },
  levelText: {
    fontSize: 11,
    fontWeight: FontWeight.bold,
    color: Colors.textSecondary,
  },
  duration: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 4,
  },
  durationText: {
    fontSize: 11,
    fontWeight: FontWeight.semibold,
    color: Colors.textMuted,
  },
  scoreCol: {
    alignItems: 'center',
    gap: 4,
  },
  circle: {
    width: 44,
    height: 44,
    borderRadius: 22,
    alignItems: 'center',
    justifyContent: 'center',
  },
  scoreText: {
    fontSize: 15,
    fontWeight: FontWeight.black,
  },
  scoreLabel: {
    fontSize: 10,
    fontWeight: FontWeight.semibold,
    color: Colors.textMuted,
  },
});
