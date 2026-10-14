// Senaryo detayı — pratiğe başlamadan önceki brifing. Hero (bağlam chip + başlık + meta),
// sahne / karakter / hedef / ipucu kartları, geçmiş istatistikler ve sticky "Pratiğe Başla".
//
// Veri API'den (GET /api/Scenarios/{id}). bestScore/attemptsCount gerçek veriden gelir.
// "Pratiğe Başla" backend'de oturum açar (useStartPractice): premium → paywall,
// günlük limit → uyarı.

import { LinearGradient } from 'expo-linear-gradient';
import { router, useLocalSearchParams } from 'expo-router';
import {
  ArrowLeft,
  Clock,
  Lightbulb,
  MapPin,
  Play,
  Target,
  Trophy,
  User,
} from 'phosphor-react-native';
import { ScrollView, StyleSheet, View } from 'react-native';
import { SafeAreaView, useSafeAreaInsets } from 'react-native-safe-area-context';

import {
  Button,
  ErrorState,
  IconButton,
  Skeleton,
  StarRating,
  Text,
} from '@/components/ui';
import {
  Colors,
  FontSize,
  FontWeight,
  Radius,
  Spacing,
} from '@/constants/theme';
import { useScenarioDetail } from '@/hooks/queries/useScenarios';
import { useStartPractice } from '@/hooks/useStartPractice';
import { isNotFoundError, toScenarioErrorMessage } from '@/lib/api';
import { getContextIcon } from '@/lib/icons';
import { contexts } from '@/lib/mockData';
import { getInitials } from '@/lib/utils/initials';
import { getDifficultyLabel, getDifficultyLevel } from '@/types/scenario';

// contextId metadata'da yoksa (backend yeni bir bağlam eklerse) ekran yine çizilsin.
const FALLBACK_CONTEXT = {
  name: 'Senaryo',
  iconName: '',
  color: Colors.primary,
  softColor: Colors.primarySoft,
};

export default function ScenarioDetailScreen() {
  const { scenarioId } = useLocalSearchParams<{ scenarioId: string }>();
  const insets = useSafeAreaInsets();
  const { startPractice, isStarting } = useStartPractice();
  const { data: scenario, isPending, isError, error, refetch, isFetching } =
    useScenarioDetail(scenarioId);

  const context = scenario
    ? contexts.find((c) => c.id === scenario.contextId)
    : undefined;

  if (isPending) {
    return <DetailSkeleton />;
  }

  // 404 ayrı ele alınır: hata değil, "böyle bir senaryo yok".
  if (isNotFoundError(error)) {
    return (
      <SafeAreaView style={styles.safeArea}>
        <View style={styles.notFound}>
          <Text style={styles.notFoundText}>Senaryo bulunamadı.</Text>
          <Button variant="primary" onPress={() => router.back()}>
            Geri dön
          </Button>
        </View>
      </SafeAreaView>
    );
  }

  if (isError) {
    return (
      <SafeAreaView style={styles.safeArea}>
        <View style={styles.stateWrapper}>
          <ErrorState
            message={toScenarioErrorMessage(error)}
            onRetry={() => refetch()}
            isRetrying={isFetching}
          />
        </View>
      </SafeAreaView>
    );
  }

  // Bağlam metadata'sı client'ta sabit; bilinmeyen contextId gelirse yine de
  // ekranı çizebilmek için nötr bir tema kullan.
  const theme = context ?? FALLBACK_CONTEXT;
  const Icon = getContextIcon(theme.iconName);
  const hasHistory = scenario.attemptsCount > 0;
  const level = getDifficultyLevel(scenario.difficulty);
  const { character } = scenario;

  return (
    <View style={styles.safeArea}>
      <ScrollView showsVerticalScrollIndicator={false}>
        {/* Hero — bağlam soft renginden zemine geçiş, status bar'ın arkasına uzanır */}
        <LinearGradient
          colors={[theme.softColor, Colors.background]}
          start={{ x: 0, y: 0 }}
          end={{ x: 0, y: 1 }}
          style={[styles.hero, { paddingTop: insets.top + 12 }]}
        >
          <View style={styles.topBar}>
            <IconButton
              icon={ArrowLeft}
              onPress={() => router.back()}
              accessibilityLabel="Geri"
            />
            {scenario.isCompleted && scenario.bestScore !== null ? (
              <View style={styles.bestPill}>
                <Trophy size={14} color={Colors.success} weight="fill" />
                <Text style={styles.bestPillText}>
                  En iyi: {scenario.bestScore}
                </Text>
              </View>
            ) : null}
          </View>

          <View style={[styles.contextChip, { backgroundColor: theme.color }]}>
            <Icon size={14} color={Colors.surface} weight="fill" />
            <Text style={styles.contextChipText}>{theme.name}</Text>
          </View>
          <Text style={styles.heroTitle}>{scenario.title}</Text>
          <Text style={styles.heroSubtitle}>{scenario.shortDescription}</Text>
          <View style={styles.metaRow}>
            <View style={styles.metaItem}>
              <StarRating count={scenario.difficulty} size={15} />
              <Text style={styles.metaLevel}>{getDifficultyLabel(level)}</Text>
            </View>
            <View style={styles.metaItem}>
              <Clock size={15} color={Colors.textSecondary} />
              <Text style={styles.metaText}>
                {scenario.estimatedDurationMinutes} dk
              </Text>
            </View>
          </View>
        </LinearGradient>

        <View style={styles.body}>
          {/* Sahne */}
          <View style={styles.card}>
            <CardLabel icon={<MapPin size={16} color={Colors.secondary} weight="fill" />}>
              SAHNE
            </CardLabel>
            <Text style={styles.settingText}>{scenario.scene.setting}</Text>
            <Text style={styles.situationText}>{scenario.scene.situation}</Text>
          </View>

          {/* Karakter */}
          <View style={styles.card}>
            <CardLabel icon={<User size={16} color={Colors.primary} weight="fill" />}>
              KARŞINDA
            </CardLabel>
            <View style={styles.characterRow}>
              <View style={styles.avatar}>
                <Text style={styles.avatarText}>{getInitials(character.name)}</Text>
              </View>
              <View style={styles.characterInfo}>
                <Text style={styles.characterName}>
                  {character.name}, {character.age}
                </Text>
                <Text style={styles.characterRole}>{character.role}</Text>
                <Text style={styles.characterRelationship}>
                  {character.relationship}
                </Text>
              </View>
            </View>
          </View>

          {/* Hedef */}
          <View style={styles.goalCard}>
            <CardLabel
              icon={<Target size={16} color={Colors.primary} weight="fill" />}
              color={Colors.primary}
            >
              SENİN HEDEFİN
            </CardLabel>
            <Text style={styles.goalText}>{scenario.scene.userGoal}</Text>
          </View>

          {/* İpucu (varsa) */}
          {scenario.scene.hint ? (
            <View style={styles.hintCard}>
              <Lightbulb size={16} color={Colors.secondary} weight="fill" />
              <Text style={styles.hintText}>{scenario.scene.hint}</Text>
            </View>
          ) : null}

          {/* Geçmiş istatistikleri (varsa) */}
          {hasHistory ? (
            <View style={styles.statsRow}>
              <View style={styles.statBox}>
                <Text style={[styles.statValue, { color: Colors.success }]}>
                  {scenario.bestScore ?? '-'}
                </Text>
                <Text style={styles.statLabel}>En iyi skorum</Text>
              </View>
              <View style={styles.statBox}>
                <Text style={[styles.statValue, { color: Colors.primary }]}>
                  {scenario.attemptsCount}
                </Text>
                <Text style={styles.statLabel}>Deneme sayım</Text>
              </View>
            </View>
          ) : null}
        </View>
      </ScrollView>

      {/* Sticky CTA */}
      <View style={[styles.ctaBar, { paddingBottom: insets.bottom + 16 }]}>
        <Button
          variant="primary"
          size="lg"
          fullWidth
          loading={isStarting}
          onPress={() => startPractice(scenario.id)}
          leftIcon={<Play size={19} color={Colors.surface} weight="fill" />}
        >
          Pratiğe Başla
        </Button>
      </View>
    </View>
  );
}

function CardLabel({
  icon,
  color = Colors.textMuted,
  children,
}: {
  icon: React.ReactNode;
  color?: string;
  children: string;
}) {
  return (
    <View style={styles.cardLabelRow}>
      {icon}
      <Text style={[styles.cardLabel, { color }]}>{children}</Text>
    </View>
  );
}

// Gerçek düzenin iskeleti: hero + üç kart. Ölçüler gerçek içerikle aynı
// hizada tutuldu ki veri gelince sayfa zıplamasın.
function DetailSkeleton() {
  return (
    <SafeAreaView style={styles.safeArea} edges={['top']}>
      <View style={styles.skeletonHero}>
        <Skeleton width={40} height={40} borderRadius={13} />
        <Skeleton width={110} height={26} borderRadius={Radius.full} />
        <Skeleton width="85%" height={34} />
        <Skeleton width="70%" height={20} />
        <Skeleton width={140} height={16} />
      </View>
      <View style={styles.body}>
        <Skeleton height={140} borderRadius={18} />
        <Skeleton height={120} borderRadius={18} />
        <Skeleton height={80} borderRadius={18} />
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
    marginBottom: 18,
  },
  bestPill: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 5,
    paddingHorizontal: 12,
    paddingVertical: 6,
    borderRadius: Radius.full,
    backgroundColor: Colors.successSoft,
  },
  bestPillText: {
    fontSize: FontSize.xs,
    fontWeight: FontWeight.bold,
    color: Colors.success,
  },

  // Hero
  hero: {
    paddingHorizontal: 22,
    paddingBottom: Spacing.lg,
  },
  contextChip: {
    flexDirection: 'row',
    alignItems: 'center',
    alignSelf: 'flex-start',
    gap: 6,
    paddingHorizontal: 13,
    paddingVertical: 6,
    borderRadius: Radius.full,
    marginBottom: 14,
  },
  contextChipText: {
    fontSize: FontSize.xs,
    fontWeight: FontWeight.bold,
    color: Colors.surface,
  },
  heroTitle: {
    fontSize: 30,
    fontWeight: FontWeight.black,
    letterSpacing: -0.9,
    lineHeight: 33,
    color: Colors.text,
    marginBottom: Spacing.sm,
  },
  heroSubtitle: {
    fontSize: 15,
    lineHeight: 21,
    color: Colors.textSecondary,
    marginBottom: 14,
  },
  metaRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 14,
  },
  metaItem: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 4,
  },
  metaLevel: {
    marginLeft: 3,
    fontSize: 13,
    fontWeight: FontWeight.bold,
    color: Colors.textSecondary,
  },
  metaText: {
    fontSize: 13,
    fontWeight: FontWeight.semibold,
    color: Colors.textSecondary,
  },

  // Gövde
  body: {
    paddingHorizontal: 20,
    paddingTop: 6,
    paddingBottom: Spacing.lg,
    gap: 14,
  },
  card: {
    padding: 18,
    borderRadius: 18,
    borderWidth: 1,
    borderColor: Colors.divider,
    backgroundColor: Colors.surface,
  },
  cardLabelRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 7,
    marginBottom: 10,
  },
  cardLabel: {
    fontSize: FontSize.xs,
    fontWeight: FontWeight.extrabold,
    letterSpacing: 0.72,
  },
  settingText: {
    fontSize: 15,
    fontWeight: FontWeight.bold,
    color: Colors.text,
    marginBottom: 6,
  },
  situationText: {
    fontSize: FontSize.sm,
    lineHeight: 21,
    fontStyle: 'italic',
    color: Colors.textSecondary,
  },
  characterRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 14,
  },
  avatar: {
    width: 50,
    height: 50,
    borderRadius: 25,
    backgroundColor: Colors.primarySoft,
    alignItems: 'center',
    justifyContent: 'center',
  },
  avatarText: {
    fontSize: FontSize.lg,
    fontWeight: FontWeight.black,
    color: Colors.primary,
  },
  characterInfo: {
    flex: 1,
  },
  characterName: {
    fontSize: 17,
    fontWeight: FontWeight.extrabold,
    color: Colors.text,
  },
  characterRole: {
    fontSize: 13,
    color: Colors.textSecondary,
  },
  characterRelationship: {
    marginTop: 2,
    fontSize: FontSize.xs,
    fontStyle: 'italic',
    color: Colors.textMuted,
  },
  goalCard: {
    padding: 18,
    borderRadius: 18,
    backgroundColor: Colors.primarySoft,
  },
  goalText: {
    fontSize: FontSize.base,
    fontWeight: FontWeight.extrabold,
    letterSpacing: -0.16,
    lineHeight: 22,
    color: Colors.text,
  },
  hintCard: {
    flexDirection: 'row',
    alignItems: 'flex-start',
    gap: 6,
    paddingVertical: 14,
    paddingHorizontal: Spacing.md,
    borderLeftWidth: 4,
    borderLeftColor: Colors.secondary,
    borderRadius: 12,
    backgroundColor: Colors.tipSoft,
  },
  hintText: {
    flex: 1,
    fontSize: 13,
    lineHeight: 20,
    color: '#7A5230',
  },
  statsRow: {
    flexDirection: 'row',
    gap: 10,
  },
  statBox: {
    flex: 1,
    alignItems: 'center',
    padding: 14,
    borderRadius: Radius.lg,
    borderWidth: 1,
    borderColor: Colors.divider,
    backgroundColor: Colors.surface,
  },
  statValue: {
    fontSize: 24,
    fontWeight: FontWeight.black,
  },
  statLabel: {
    fontSize: FontSize.xs,
    color: Colors.textSecondary,
  },

  // Sticky CTA
  ctaBar: {
    paddingHorizontal: 20,
    paddingTop: 14,
    backgroundColor: Colors.background,
    borderTopWidth: 1,
    borderTopColor: '#ECEAE3',
  },

  // Skeleton
  skeletonHero: {
    paddingHorizontal: 22,
    paddingTop: 12,
    paddingBottom: Spacing.lg,
    gap: 12,
  },

  // Durumlar
  stateWrapper: {
    flex: 1,
    justifyContent: 'center',
    padding: Spacing.lg,
  },
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
