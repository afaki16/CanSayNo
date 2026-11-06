// Senaryolar tab — bağlam seçimi. Bağlam kartları 2-sütun grid'de; sayı tekse son
// kart satırı doldurur ("öne çıkan" hissi). Ücretsiz bağlam senaryo listesine,
// premium bağlam paywall'a yönlendirir.
//
// Bağlam metadata'sı (isim/renk/ikon) client'ta sabit; kartlardaki senaryo sayıları
// API'den gelen listeden hesaplanır.

import { router } from 'expo-router';
import { ScrollView, StyleSheet, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';

import { ContextCard, ErrorState, Skeleton, Text } from '@/components/ui';
import { CONTEXT_CARD_MIN_HEIGHT } from '@/components/ui/ContextCard';
import { Colors, FontSize, FontWeight, Radius, Spacing } from '@/constants/theme';
import { useScenarios } from '@/hooks/queries/useScenarios';
import { toScenarioErrorMessage } from '@/lib/api';
import { contexts } from '@/lib/mockData';
import { buildContextProgress, progressFor } from '@/lib/scenarios/contextProgress';
import type { Context } from '@/types/scenario';

export default function ScenariosScreen() {
  const { data, isPending, isError, error, refetch, isFetching } = useScenarios();

  const progress = buildContextProgress(data);

  const handlePress = (context: Context) => {
    if (context.isFree) {
      router.push({
        pathname: '/(app)/scenarios/[id]',
        params: { id: context.id },
      });
    } else {
      router.push('/(app)/paywall');
    }
  };

  return (
    <SafeAreaView style={styles.safeArea} edges={['top']}>
      <ScrollView
        contentContainerStyle={styles.scrollContent}
        showsVerticalScrollIndicator={false}
      >
        <View style={styles.header}>
          <Text style={styles.title}>Senaryolar</Text>
          <Text style={styles.subtitle}>Pratik yapmak istediğin alanı seç</Text>
        </View>

        {isPending ? (
          <GridSkeleton />
        ) : isError ? (
          <ErrorState
            message={toScenarioErrorMessage(error)}
            onRetry={() => refetch()}
            isRetrying={isFetching}
          />
        ) : (
          <View style={styles.grid}>
            {contexts.map((context) => (
              <View key={context.id} style={styles.gridItem}>
                <ContextCard
                  context={context}
                  progress={progressFor(progress, context.id)}
                  onPress={() => handlePress(context)}
                />
              </View>
            ))}
          </View>
        )}
      </ScrollView>
    </SafeAreaView>
  );
}

// Kartlarla aynı grid düzeni ve aynı minHeight — veri gelince layout kaymaz.
function GridSkeleton() {
  return (
    <View style={styles.grid}>
      {contexts.map((context) => (
        <View key={context.id} style={styles.gridItem}>
          <Skeleton height={CONTEXT_CARD_MIN_HEIGHT} borderRadius={Radius.xl} />
        </View>
      ))}
    </View>
  );
}

const styles = StyleSheet.create({
  safeArea: {
    flex: 1,
    backgroundColor: Colors.background,
  },
  scrollContent: {
    paddingHorizontal: 20,
    paddingTop: Spacing.md,
    paddingBottom: Spacing.lg,
    gap: 22,
  },
  header: {
    gap: 3,
  },
  title: {
    fontSize: FontSize.xxl,
    fontWeight: FontWeight.black,
    letterSpacing: -0.85,
    color: Colors.text,
  },
  subtitle: {
    fontSize: 15,
    color: Colors.textSecondary,
  },
  grid: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: 14,
  },
  gridItem: {
    // 2 sütun; tek kalan son kart satırı doldurur (flexGrow).
    flexBasis: '47%',
    flexGrow: 1,
  },
});
