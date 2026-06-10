// Senaryo listesi — bir bağlam scope'unda. [id] = bağlam kimliği (örn: 'is').
// Senaryolar zorluğa göre 3 gruba ayrılır (Kolay/Orta/Zor); boş gruplar gizlenir.
// Bağlamda hiç senaryo yoksa (premium) paywall yönlendiren boş state gösterilir.

import { router, useLocalSearchParams } from 'expo-router';
import { ArrowLeft, Lock } from 'phosphor-react-native';
import { ScrollView, StyleSheet, Text, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';

import { Button, ScenarioCard, StarRating } from '@/components/ui';
import {
  Colors,
  FontSize,
  FontWeight,
  Radius,
  Spacing,
} from '@/constants/theme';
import { getContextIcon } from '@/lib/icons';
import { getScenariosByContext, mockContexts } from '@/lib/mockData';
import type {
  Context,
  ContextId,
  DifficultyLevel,
  Scenario,
} from '@/types/scenario';
import { getDifficultyLabel, getDifficultyLevel } from '@/types/scenario';

// Grup sırası ve her grubu temsil eden yıldız sayısı (section header göstergesi).
const GROUP_ORDER: { level: DifficultyLevel; stars: number }[] = [
  { level: 'kolay', stars: 2 },
  { level: 'orta', stars: 3 },
  { level: 'zor', stars: 5 },
];

export default function ScenarioListScreen() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const context = mockContexts.find((c) => c.id === (id as ContextId));

  if (!context) {
    return (
      <SafeAreaView style={styles.safeArea}>
        <View style={styles.notFound}>
          <Text style={styles.notFoundText}>Bağlam bulunamadı.</Text>
          <Button variant="ghost" onPress={() => router.back()}>
            Geri
          </Button>
        </View>
      </SafeAreaView>
    );
  }

  const scenarios = getScenariosByContext(context.id);
  const groups = GROUP_ORDER.map((g) => ({
    ...g,
    items: scenarios.filter((s) => getDifficultyLevel(s.difficulty) === g.level),
  })).filter((g) => g.items.length > 0);

  const Icon = getContextIcon(context.iconName);

  return (
    <SafeAreaView style={styles.safeArea} edges={['top']}>
      {/* Header */}
      <View style={[styles.header, { backgroundColor: context.softColor }]}>
        <Button
          variant="ghost"
          size="sm"
          onPress={() => router.back()}
          leftIcon={<ArrowLeft size={18} color={Colors.primary} weight="bold" />}
        >
          Geri
        </Button>
        <View style={styles.headerRow}>
          <Icon size={32} color={context.color} weight="fill" />
          <View style={styles.headerText}>
            <Text style={[styles.headerTitle, { color: context.color }]}>
              {context.name}
            </Text>
            <Text style={styles.headerSubtitle}>{context.description}</Text>
          </View>
        </View>
      </View>

      <ScrollView
        contentContainerStyle={styles.scrollContent}
        showsVerticalScrollIndicator={false}
      >
        {scenarios.length === 0 ? (
          <EmptyState />
        ) : (
          groups.map((group) => (
            <View key={group.level} style={styles.group}>
              <View style={styles.groupHeader}>
                <Text style={styles.groupTitle}>
                  {getDifficultyLabel(group.level)}
                </Text>
                <View style={styles.countChip}>
                  <Text style={styles.countChipText}>
                    {group.items.length} senaryo
                  </Text>
                </View>
                <View style={styles.groupStars}>
                  <StarRating count={group.stars} size={14} />
                </View>
              </View>

              <View style={styles.list}>
                {group.items.map((scenario) => (
                  <ScenarioCardLink
                    key={scenario.id}
                    scenario={scenario}
                    context={context}
                  />
                ))}
              </View>
            </View>
          ))
        )}
      </ScrollView>
    </SafeAreaView>
  );
}

function ScenarioCardLink({
  scenario,
  context,
}: {
  scenario: Scenario;
  context: Context;
}) {
  return (
    <ScenarioCard
      scenario={scenario}
      context={context}
      onPress={() =>
        router.push({
          pathname: '/(app)/scenarios/detail/[scenarioId]',
          params: { scenarioId: scenario.id },
        })
      }
    />
  );
}

function EmptyState() {
  return (
    <View style={styles.empty}>
      <View style={styles.emptyIcon}>
        <Lock size={56} color={Colors.primary} weight="fill" />
      </View>
      <Text style={styles.emptyTitle}>Premium üyelik ile erişebilirsin</Text>
      <Text style={styles.emptySubtitle}>
        Bu bağlamdaki senaryolar premium üyelere özel.
      </Text>
      <Button
        variant="primary"
        size="lg"
        onPress={() => router.push('/(app)/paywall')}
      >
        Premium&apos;a Yükselt
      </Button>
    </View>
  );
}

const styles = StyleSheet.create({
  safeArea: {
    flex: 1,
    backgroundColor: Colors.background,
  },
  header: {
    paddingHorizontal: Spacing.md,
    paddingBottom: Spacing.lg,
    gap: Spacing.sm,
  },
  headerRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: Spacing.md,
    paddingHorizontal: Spacing.xs,
  },
  headerText: {
    flex: 1,
  },
  headerTitle: {
    fontSize: FontSize.xl,
    fontWeight: FontWeight.bold,
  },
  headerSubtitle: {
    marginTop: 2,
    fontSize: FontSize.sm,
    color: Colors.textSecondary,
  },
  scrollContent: {
    padding: Spacing.md,
    paddingBottom: 100,
    gap: Spacing.lg,
  },
  group: {
    gap: Spacing.md,
  },
  groupHeader: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: Spacing.sm,
  },
  groupTitle: {
    fontSize: FontSize.lg,
    fontWeight: FontWeight.bold,
    color: Colors.text,
  },
  countChip: {
    backgroundColor: Colors.primarySoft,
    paddingHorizontal: Spacing.sm,
    paddingVertical: 2,
    borderRadius: Radius.full,
  },
  countChipText: {
    fontSize: FontSize.xs,
    color: Colors.primary,
    fontWeight: FontWeight.semibold,
  },
  groupStars: {
    marginLeft: 'auto',
  },
  list: {
    gap: Spacing.sm,
  },

  // Boş state
  empty: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
    gap: Spacing.md,
    paddingVertical: Spacing.xxl,
  },
  emptyIcon: {
    width: 120,
    height: 120,
    borderRadius: Radius.full,
    backgroundColor: Colors.primarySoft,
    alignItems: 'center',
    justifyContent: 'center',
    marginBottom: Spacing.sm,
  },
  emptyTitle: {
    fontSize: FontSize.lg,
    fontWeight: FontWeight.bold,
    color: Colors.text,
    textAlign: 'center',
  },
  emptySubtitle: {
    fontSize: FontSize.sm,
    color: Colors.textSecondary,
    textAlign: 'center',
    paddingHorizontal: Spacing.lg,
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
