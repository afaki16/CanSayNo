// Senaryolar tab — bağlam seçimi. 5 renkli bağlam kartı 2-sütun grid'de; son kart
// (tek kalan) satırı doldurur ("öne çıkan" hissi). Ücretsiz bağlam senaryo listesine,
// premium bağlam paywall'a yönlendirir.

import { router } from 'expo-router';
import { ScrollView, StyleSheet, Text, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';

import { ContextCard } from '@/components/ui';
import { Colors, FontSize, FontWeight, Spacing } from '@/constants/theme';
import { mockContexts } from '@/lib/mockData';
import type { Context } from '@/types/scenario';

export default function ScenariosScreen() {
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

        <View style={styles.grid}>
          {mockContexts.map((context) => (
            <View key={context.id} style={styles.gridItem}>
              <ContextCard context={context} onPress={() => handlePress(context)} />
            </View>
          ))}
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
  scrollContent: {
    paddingHorizontal: Spacing.md,
    paddingTop: Spacing.lg,
    paddingBottom: 100,
    gap: Spacing.lg,
  },
  header: {
    gap: Spacing.xs,
  },
  title: {
    fontSize: FontSize.xxl,
    fontWeight: FontWeight.bold,
    color: Colors.text,
  },
  subtitle: {
    fontSize: FontSize.sm,
    color: Colors.textSecondary,
  },
  grid: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: Spacing.md,
  },
  gridItem: {
    // 2 sütun; tek kalan son kart satırı doldurur (flexGrow).
    flexBasis: '47%',
    flexGrow: 1,
  },
});
