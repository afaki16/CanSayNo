// Senaryo detayı — pratiğe başlamadan önceki brifing. Hero (bağlam chip + başlık + meta),
// sahne / karakter / hedef / ipucu kartları, geçmiş istatistikler ve sticky "Pratiğe Başla".

import { LinearGradient } from 'expo-linear-gradient';
import { router, useLocalSearchParams } from 'expo-router';
import { ArrowLeft, Clock, Play, User } from 'phosphor-react-native';
import { ScrollView, StyleSheet, Text, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';

import { Button, StarRating } from '@/components/ui';
import {
  Colors,
  FontSize,
  FontWeight,
  Radius,
  Shadow,
  Spacing,
} from '@/constants/theme';
import { getContextIcon } from '@/lib/icons';
import { getScenarioById, mockContexts } from '@/lib/mockData';

// Skora göre chip rengi (Home/ScenarioCard ile tutarlı).
function scoreColors(score: number) {
  if (score >= 80) return { bg: Colors.successSoft, text: Colors.success };
  if (score >= 60) return { bg: Colors.warningSoft, text: Colors.warning };
  return { bg: Colors.errorSoft, text: Colors.error };
}

export default function ScenarioDetailScreen() {
  const { scenarioId } = useLocalSearchParams<{ scenarioId: string }>();
  const scenario = getScenarioById(scenarioId);
  const context = scenario
    ? mockContexts.find((c) => c.id === scenario.contextId)
    : undefined;

  if (!scenario || !context) {
    return (
      <SafeAreaView style={styles.safeArea}>
        <View style={styles.notFound}>
          <Text style={styles.notFoundText}>Senaryo bulunamadı.</Text>
          <Button variant="ghost" onPress={() => router.back()}>
            Geri
          </Button>
        </View>
      </SafeAreaView>
    );
  }

  const Icon = getContextIcon(context.iconName);
  const hasHistory = scenario.attemptsCount > 0;
  const completedScore =
    scenario.isCompleted && scenario.bestScore !== null
      ? scoreColors(scenario.bestScore)
      : null;

  return (
    <SafeAreaView style={styles.safeArea} edges={['top']}>
      {/* Üst bar — transparan, hero üzerinde */}
      <View style={styles.topBar}>
        <Button
          variant="ghost"
          size="sm"
          onPress={() => router.back()}
          leftIcon={<ArrowLeft size={18} color={Colors.primary} weight="bold" />}
        >
          Geri
        </Button>
        {completedScore ? (
          <View style={[styles.topScore, { backgroundColor: completedScore.bg }]}>
            <Text style={[styles.topScoreText, { color: completedScore.text }]}>
              En iyi: {scenario.bestScore}
            </Text>
          </View>
        ) : null}
      </View>

      <ScrollView
        contentContainerStyle={styles.scrollContent}
        showsVerticalScrollIndicator={false}
      >
        {/* Hero */}
        <LinearGradient
          colors={[context.softColor, Colors.background]}
          start={{ x: 0, y: 0 }}
          end={{ x: 0, y: 1 }}
          style={styles.hero}
        >
          <View style={[styles.contextChip, { backgroundColor: context.color }]}>
            <Icon size={16} color={Colors.surface} weight="fill" />
            <Text style={styles.contextChipText}>{context.name}</Text>
          </View>
          <Text style={styles.heroTitle}>{scenario.title}</Text>
          <Text style={styles.heroSubtitle}>{scenario.shortDescription}</Text>
          <View style={styles.metaRow}>
            <StarRating count={scenario.difficulty} size={16} />
            <Text style={styles.metaSep}>·</Text>
            <Clock size={16} color={Colors.textSecondary} weight="bold" />
            <Text style={styles.metaText}>
              {scenario.estimatedDurationMinutes} dk
            </Text>
          </View>
        </LinearGradient>

        <View style={styles.body}>
          {/* Sahne */}
          <View style={styles.card}>
            <Text style={styles.cardLabel}>📍 SAHNE</Text>
            <Text style={styles.settingText}>{scenario.scene.setting}</Text>
            <Text style={styles.situationText}>{scenario.scene.situation}</Text>
          </View>

          {/* Karakter */}
          <View style={styles.card}>
            <Text style={styles.cardLabel}>👤 KARŞINDA</Text>
            <View style={styles.characterRow}>
              <View style={styles.avatar}>
                <User size={28} color={Colors.primary} weight="fill" />
              </View>
              <View style={styles.characterInfo}>
                <Text style={styles.characterName}>{scenario.character.name}</Text>
                <Text style={styles.characterRole}>
                  {scenario.character.role}, {scenario.character.age}
                </Text>
                <Text style={styles.characterRelationship}>
                  {scenario.character.relationship}
                </Text>
              </View>
            </View>
          </View>

          {/* Hedef */}
          <View style={styles.goalCard}>
            <Text style={styles.goalLabel}>🎯 SENİN HEDEFİN</Text>
            <Text style={styles.goalText}>{scenario.scene.userGoal}</Text>
          </View>

          {/* İpucu (varsa) */}
          {scenario.scene.hint ? (
            <View style={styles.hintCard}>
              <Text style={styles.hintText}>💡 {scenario.scene.hint}</Text>
            </View>
          ) : null}

          {/* Geçmiş istatistikleri (varsa) */}
          {hasHistory ? (
            <View style={styles.statsRow}>
              <View style={styles.statBox}>
                <Text style={styles.statValue}>{scenario.bestScore ?? '-'}</Text>
                <Text style={styles.statLabel}>En iyi skorum</Text>
              </View>
              <View style={styles.statBox}>
                <Text style={[styles.statValue, { color: Colors.secondary }]}>
                  {scenario.attemptsCount}
                </Text>
                <Text style={styles.statLabel}>Deneme sayım</Text>
              </View>
            </View>
          ) : null}
        </View>
      </ScrollView>

      {/* Sticky CTA */}
      <View style={styles.ctaBar}>
        <Button
          variant="primary"
          size="lg"
          fullWidth
          onPress={() =>
            router.push({
              pathname: '/(app)/scenarios/practice/[scenarioId]',
              params: { scenarioId: scenario.id },
            })
          }
          leftIcon={<Play size={20} color={Colors.surface} weight="fill" />}
        >
          Pratiğe Başla
        </Button>
      </View>
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
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingHorizontal: Spacing.md,
    paddingVertical: Spacing.xs,
  },
  topScore: {
    paddingHorizontal: Spacing.sm,
    paddingVertical: 4,
    borderRadius: Radius.full,
  },
  topScoreText: {
    fontSize: FontSize.xs,
    fontWeight: FontWeight.bold,
  },
  scrollContent: {
    paddingBottom: 100,
  },

  // Hero
  hero: {
    paddingHorizontal: Spacing.lg,
    paddingTop: Spacing.sm,
    paddingBottom: Spacing.lg,
    gap: Spacing.sm,
  },
  contextChip: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: Spacing.xs,
    alignSelf: 'flex-start',
    paddingHorizontal: Spacing.sm,
    paddingVertical: 4,
    borderRadius: Radius.full,
  },
  contextChipText: {
    fontSize: FontSize.xs,
    fontWeight: FontWeight.semibold,
    color: Colors.surface,
  },
  heroTitle: {
    fontSize: FontSize.hero,
    fontWeight: FontWeight.bold,
    color: Colors.text,
  },
  heroSubtitle: {
    fontSize: FontSize.base,
    color: Colors.textSecondary,
  },
  metaRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: Spacing.xs,
    marginTop: Spacing.xs,
  },
  metaSep: {
    fontSize: FontSize.base,
    color: Colors.textMuted,
  },
  metaText: {
    fontSize: FontSize.sm,
    color: Colors.textSecondary,
    fontWeight: FontWeight.medium,
  },

  // Body / cards
  body: {
    paddingHorizontal: Spacing.md,
    gap: Spacing.md,
  },
  card: {
    backgroundColor: Colors.surface,
    borderWidth: 1,
    borderColor: Colors.border,
    borderRadius: Radius.lg,
    padding: Spacing.lg,
    gap: Spacing.sm,
  },
  cardLabel: {
    fontSize: FontSize.xs,
    fontWeight: FontWeight.bold,
    color: Colors.textSecondary,
    letterSpacing: 1,
  },
  settingText: {
    fontSize: FontSize.base,
    fontWeight: FontWeight.medium,
    color: Colors.text,
  },
  situationText: {
    fontSize: FontSize.base,
    fontStyle: 'italic',
    color: Colors.textSecondary,
    lineHeight: FontSize.base * 1.5,
  },

  // Karakter
  characterRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: Spacing.md,
    marginTop: Spacing.xs,
  },
  avatar: {
    width: 48,
    height: 48,
    borderRadius: Radius.full,
    backgroundColor: Colors.primarySoft,
    alignItems: 'center',
    justifyContent: 'center',
  },
  characterInfo: {
    flex: 1,
    gap: 2,
  },
  characterName: {
    fontSize: FontSize.lg,
    fontWeight: FontWeight.bold,
    color: Colors.text,
  },
  characterRole: {
    fontSize: FontSize.sm,
    color: Colors.textSecondary,
  },
  characterRelationship: {
    fontSize: FontSize.xs,
    color: Colors.textMuted,
    fontStyle: 'italic',
  },

  // Hedef
  goalCard: {
    backgroundColor: Colors.primarySoft,
    borderRadius: Radius.lg,
    padding: Spacing.lg,
    gap: Spacing.sm,
  },
  goalLabel: {
    fontSize: FontSize.xs,
    fontWeight: FontWeight.bold,
    color: Colors.primary,
    letterSpacing: 1,
  },
  goalText: {
    fontSize: FontSize.base,
    fontWeight: FontWeight.semibold,
    color: Colors.text,
  },

  // İpucu — sol kenarda vurgu çubuğu
  hintCard: {
    backgroundColor: Colors.secondarySoft,
    borderLeftWidth: 4,
    borderLeftColor: Colors.secondary,
    borderRadius: Radius.md,
    padding: Spacing.md,
  },
  hintText: {
    fontSize: FontSize.sm,
    color: Colors.text,
    lineHeight: FontSize.sm * 1.5,
  },

  // İstatistikler
  statsRow: {
    flexDirection: 'row',
    gap: Spacing.md,
  },
  statBox: {
    flex: 1,
    backgroundColor: Colors.surface,
    borderWidth: 1,
    borderColor: Colors.border,
    borderRadius: Radius.lg,
    padding: Spacing.md,
    alignItems: 'center',
    gap: Spacing.xs,
  },
  statValue: {
    fontSize: FontSize.xxl,
    fontWeight: FontWeight.bold,
    color: Colors.primary,
  },
  statLabel: {
    fontSize: FontSize.xs,
    color: Colors.textSecondary,
  },

  // Sticky CTA
  ctaBar: {
    padding: Spacing.md,
    backgroundColor: Colors.background,
    borderTopWidth: 1,
    borderTopColor: Colors.divider,
    ...Shadow.lg,
    shadowOffset: { width: 0, height: -4 },
  },

  // Not found
  notFound: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
    gap: Spacing.md,
  },
  notFoundText: {
    fontSize: FontSize.base,
    color: Colors.textSecondary,
  },
});
