// Senaryo listesi — bir bağlam scope'unda. [id] = bağlam kimliği (örn: 'is').
// Senaryolar API'den (GET /api/Scenarios/by-context/{contextId}) gelir.
// Üstte bağlam renginde header bandı + ilerleme çubuğu, altta düz senaryo listesi.
// Bağlamda hiç senaryo yoksa (premium) paywall yönlendiren boş state gösterilir.

import { router, useLocalSearchParams } from 'expo-router';
import { ArrowLeft, LockSimple } from 'phosphor-react-native';
import { ScrollView, StyleSheet, View } from 'react-native';
import { SafeAreaView, useSafeAreaInsets } from 'react-native-safe-area-context';

import {
  Button,
  ErrorState,
  IconButton,
  ScenarioCard,
  Skeleton,
  Text,
} from '@/components/ui';
import { SCENARIO_CARD_MIN_HEIGHT } from '@/components/ui/ScenarioCard';
import {
  Colors,
  FontSize,
  FontWeight,
  Radius,
  Spacing,
} from '@/constants/theme';
import { useScenariosByContext } from '@/hooks/queries/useScenarios';
import { toScenarioErrorMessage } from '@/lib/api';
import { getContextIcon } from '@/lib/icons';
import { contexts } from '@/lib/mockData';
import type { Context, ContextId, Scenario } from '@/types/scenario';

export default function ScenarioListScreen() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const context = contexts.find((c) => c.id === (id as ContextId));
  const insets = useSafeAreaInsets();

  // Hook koşulsuz çağrılır (React kuralı); bilinmeyen bağlamda enabled=false
  // olduğu için istek atılmaz.
  const { data, isPending, isError, error, refetch, isFetching } =
    useScenariosByContext(context?.id);

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

  const scenarios = data ?? [];
  const completedCount = scenarios.filter((s) => s.isCompleted).length;
  const progressRatio =
    scenarios.length > 0 ? completedCount / scenarios.length : 0;

  const Icon = getContextIcon(context.iconName);

  return (
    <View style={styles.safeArea}>
      <ScrollView showsVerticalScrollIndicator={false}>
        {/* Bağlam header bandı — status bar'ın arkasına kadar uzanır */}
        <View
          style={[
            styles.header,
            { backgroundColor: context.softColor, paddingTop: insets.top + 12 },
          ]}
        >
          <View style={styles.headerRow}>
            <IconButton
              icon={ArrowLeft}
              onPress={() => router.back()}
              accessibilityLabel="Geri"
            />
            <View style={[styles.headerIcon, { backgroundColor: context.color }]}>
              <Icon size={25} color={Colors.surface} weight="fill" />
            </View>
            <View style={styles.headerText}>
              <Text style={styles.headerTitle} numberOfLines={1}>
                {context.name}
              </Text>
              {isPending || isError ? null : (
                <Text style={[styles.headerProgress, { color: context.color }]}>
                  {completedCount} / {scenarios.length} tamamlandı
                </Text>
              )}
            </View>
          </View>
          <View style={styles.progressTrack}>
            <View
              style={[
                styles.progressFill,
                {
                  backgroundColor: context.color,
                  width: `${progressRatio * 100}%`,
                },
              ]}
            />
          </View>
        </View>

        <View style={[styles.content, { paddingBottom: insets.bottom + 24 }]}>
          {isPending ? (
            <ListSkeleton />
          ) : isError ? (
            <ErrorState
              message={toScenarioErrorMessage(error)}
              onRetry={() => refetch()}
              isRetrying={isFetching}
            />
          ) : scenarios.length === 0 ? (
            <EmptyState />
          ) : (
            scenarios.map((scenario) => (
              <ScenarioCardLink
                key={scenario.id}
                scenario={scenario}
                context={context}
              />
            ))
          )}
        </View>
      </ScrollView>
    </View>
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

const SKELETON_ROWS = 5;

// ScenarioCard ile aynı yükseklik ve aynı boşluk — veri gelince zıplamaz.
function ListSkeleton() {
  return (
    <>
      {Array.from({ length: SKELETON_ROWS }, (_, i) => (
        <Skeleton key={i} height={SCENARIO_CARD_MIN_HEIGHT} borderRadius={18} />
      ))}
    </>
  );
}

function EmptyState() {
  return (
    <View style={styles.empty}>
      <View style={styles.emptyIcon}>
        <LockSimple size={52} color={Colors.primary} weight="fill" />
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
        Premium&apos;a geç
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
    paddingHorizontal: 20,
    paddingBottom: 22,
  },
  headerRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 14,
  },
  headerIcon: {
    width: 48,
    height: 48,
    borderRadius: 15,
    alignItems: 'center',
    justifyContent: 'center',
  },
  headerText: {
    flex: 1,
  },
  headerTitle: {
    fontSize: 21,
    fontWeight: FontWeight.black,
    letterSpacing: -0.42,
    color: Colors.text,
  },
  headerProgress: {
    fontSize: 13,
    fontWeight: FontWeight.semibold,
  },
  progressTrack: {
    height: 8,
    marginTop: Spacing.md,
    borderRadius: Radius.full,
    backgroundColor: Colors.surface,
    overflow: 'hidden',
  },
  progressFill: {
    height: '100%',
    borderRadius: Radius.full,
  },
  content: {
    paddingHorizontal: 20,
    paddingTop: 18,
    gap: 12,
  },

  // Boş state
  empty: {
    alignItems: 'center',
    gap: Spacing.md,
    paddingVertical: Spacing.xxl,
  },
  emptyIcon: {
    width: 112,
    height: 112,
    borderRadius: Radius.full,
    backgroundColor: Colors.primarySoft,
    alignItems: 'center',
    justifyContent: 'center',
    marginBottom: Spacing.sm,
  },
  emptyTitle: {
    fontSize: FontSize.lg,
    fontWeight: FontWeight.extrabold,
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
