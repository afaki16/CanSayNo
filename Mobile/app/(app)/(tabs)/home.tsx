// Anasayfa: oyunlaştırma ekranı. Seri (haftalık gün çizelgesiyle), bugünkü görev,
// istatistikler, XP ilerlemesi ve son pratikler. Seri/haftalık aktivite/XP
// GET /api/Stats/me'den, son pratikler GET /api/Sessions/my'den.

import { LinearGradient } from 'expo-linear-gradient';
import { router } from 'expo-router';
import {
  Check,
  Fire,
  Flame,
  Play,
  Star,
  Target,
  Trophy,
  User,
} from 'phosphor-react-native';
import { Trans, useTranslation } from 'react-i18next';
import { Pressable, ScrollView, StyleSheet, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';

import { Skeleton, Text } from '@/components/ui';
import {
  Colors,
  FontSize,
  FontWeight,
  Gradients,
  Radius,
  Shadow,
  Spacing,
} from '@/constants/theme';
import { useScenarios } from '@/hooks/queries/useScenarios';
import { useMySessions } from '@/hooks/queries/useSessions';
import { useMySubscription } from '@/hooks/queries/useSubscription';
import { EMPTY_STATS, useMyStats } from '@/hooks/queries/useStats';
import { useAuth } from '@/hooks/useAuth';
import { toSessionErrorMessage } from '@/lib/api';
import { getContextIcon } from '@/lib/icons';
import { useContexts } from '@/lib/mockData';
import { getDailyTask } from '@/lib/utils/dailyTask';
import { formatRelativeTime } from '@/lib/utils/datetime';

// Pzt..Paz — kısa adlar çeviride (practice.home.weekDays).
const WEEK_DAYS = ['mon', 'tue', 'wed', 'thu', 'fri', 'sat', 'sun'] as const;


// Skora göre badge renk eşlemesi (soft bg + text).
function scoreColors(score: number) {
  if (score >= 80) return { bg: Colors.successSoft, text: Colors.success };
  if (score >= 60) return { bg: Colors.warningSoft, text: Colors.warning };
  return { bg: Colors.errorSoft, text: Colors.error };
}

type DayState = 'done' | 'today' | 'future' | 'missed';

// Backend'in haftalık aktivitesinden (Pzt..Paz, o gün değerlendirilmiş pratik var mı)
// gün durumlarını türetir. Hafta ve "bugün" backend'de TR saatine göre hesaplanır
// (cihaz saat dilimi farklı olabilir).
function buildWeek(weekActivity: boolean[], todayIndex: number): DayState[] {
  return WEEK_DAYS.map((_, i) => {
    const practiced = weekActivity[i] ?? false;
    if (i > todayIndex) return 'future';
    if (i === todayIndex) return practiced ? 'done' : 'today';
    return practiced ? 'done' : 'missed';
  });
}

export default function HomeScreen() {
  const { t } = useTranslation();
  const { user } = useAuth();
  // Yüklenirken/hata durumunda sıfırlarla çizilir; kartlar kaybolmaz.
  const { data } = useMyStats();
  const stats = data ?? EMPTY_STATS;
  // Bugünkü görev: senaryo listesindeki ilerlemeye göre önerilir (lib/utils/dailyTask).
  const { data: scenarios } = useScenarios();
  const { data: subscription } = useMySubscription();
  const dailyTask = scenarios ? getDailyTask(scenarios, subscription?.isPremium ?? false) : null;

  const firstName = user?.firstName ?? t('practice.home.defaultName');
  const xpRatio = Math.min(stats.xpInLevel / stats.xpPerLevel, 1);
  const xpRemaining = Math.max(stats.xpPerLevel - stats.xpInLevel, 0);
  const week = buildWeek(stats.weekActivity, stats.todayIndex);
  const streakTitle =
    stats.currentStreak > 0
      ? t('practice.home.streakTitle', { count: stats.currentStreak })
      : t('practice.home.streakStart');
  const streakSub = stats.practicedToday
    ? t('practice.home.streakDoneToday')
    : stats.currentStreak > 0
      ? t('practice.home.streakKeepGoing')
      : t('practice.home.streakFirst');

  const goScenarios = () => router.push('/(app)/(tabs)/scenarios');
  const goProfile = () => router.push('/(app)/(tabs)/profile');
  const goScenarioDetail = (scenarioId: string) =>
    router.push({
      pathname: '/(app)/scenarios/detail/[scenarioId]',
      params: { scenarioId },
    });

  return (
    <SafeAreaView style={styles.safeArea} edges={['top']}>
      <ScrollView
        contentContainerStyle={styles.scrollContent}
        showsVerticalScrollIndicator={false}
      >
        {/* Başlık — selamlama + avatar */}
        <View style={styles.header}>
          <View style={styles.headerText}>
            <Text style={styles.greeting}>
              {t('practice.home.greeting', { name: firstName })}
            </Text>
            <Text style={styles.greetingSub}>{t('practice.home.greetingSub')}</Text>
          </View>
          <Pressable
            onPress={goProfile}
            style={styles.avatar}
            accessibilityRole="button"
            accessibilityLabel={t('practice.home.profileA11y')}
          >
            <User size={22} color={Colors.primary} weight="fill" />
          </Pressable>
        </View>

        {/* Seri kartı */}
        <Pressable onPress={goScenarios} accessibilityRole="button">
          <LinearGradient
            colors={['#FF8A5B', '#FF6B35']}
            start={{ x: 0, y: 0 }}
            end={{ x: 1, y: 1 }}
            style={styles.streakCard}
          >
            <View style={styles.streakTop}>
              <View style={styles.streakIcon}>
                <Fire size={30} color="#FFFFFF" weight="fill" />
              </View>
              <View style={styles.flex}>
                <Text style={styles.streakTitle}>{streakTitle}</Text>
                <Text style={styles.streakSub}>{streakSub}</Text>
              </View>
            </View>
            <View style={styles.weekRow}>
              {week.map((state, i) => (
                <View key={WEEK_DAYS[i]} style={styles.dayCol}>
                  <View style={[styles.dayDot, dayDotStyle[state]]}>
                    {state === 'done' ? (
                      <Check size={15} color="#FF6B35" weight="bold" />
                    ) : null}
                  </View>
                  <Text
                    style={[
                      styles.dayLabel,
                      state === 'today' && styles.dayLabelToday,
                      state === 'future' && styles.dayLabelFuture,
                    ]}
                  >
                    {state === 'today'
                      ? t('practice.home.today')
                      : t(`practice.home.weekDays.${WEEK_DAYS[i]}`)}
                  </Text>
                </View>
              ))}
            </View>
          </LinearGradient>
        </Pressable>

        {/* Bugünkü görev — senaryolar yüklenene kadar kart gösterilmez */}
        {dailyTask ? (
          <Pressable
            onPress={() => goScenarioDetail(dailyTask.scenarioId)}
            style={styles.taskCard}
            accessibilityRole="button"
          >
            <LinearGradient
              colors={Gradients.brand.colors}
              start={Gradients.brand.start}
              end={Gradients.brand.end}
              style={[StyleSheet.absoluteFill, { borderRadius: 22 }]}
              pointerEvents="none"
            />
            <View style={styles.flex}>
              <Text style={styles.taskLabel}>{t('practice.home.dailyTaskLabel')}</Text>
              <Text style={styles.taskTitle}>{dailyTask.title}</Text>
              <Text style={styles.taskSub} numberOfLines={1}>
                {dailyTask.subtitle}
              </Text>
            </View>
            <View style={styles.playButton}>
              <Play size={24} color={Colors.primary} weight="fill" />
            </View>
          </Pressable>
        ) : null}

        {/* İstatistikler — 2x2 grid */}
        <View style={styles.statsGrid}>
          <StatCard
            icon={<Trophy size={24} color={Colors.primary} weight="fill" />}
            value={`${stats.totalCompletedSessions}`}
            label={t('practice.home.stats.total')}
          />
          <StatCard
            icon={<Flame size={24} color={Colors.secondary} weight="fill" />}
            value={t('practice.home.stats.longestStreakValue', { count: stats.longestStreak })}
            label={t('practice.home.stats.longestStreak')}
          />
          <StatCard
            icon={<Target size={24} color={Colors.success} weight="fill" />}
            value={`${stats.weeklyProgress}/${stats.weeklyGoal}`}
            label={t('practice.home.stats.thisWeek')}
          />
          <StatCard
            icon={<Star size={24} color={Colors.amber} weight="fill" />}
            value={`${stats.level}`}
            label={t('practice.home.stats.level')}
          />
        </View>

        {/* XP ilerlemesi */}
        <View style={styles.card}>
          <View style={styles.xpHeader}>
            <Text style={styles.xpLevel}>
              {t('practice.home.levelTitle', { level: stats.level })}
            </Text>
            <Text style={styles.xpValue}>
              {stats.xpInLevel} / {stats.xpPerLevel} XP
            </Text>
          </View>
          <View style={styles.xpTrack}>
            <LinearGradient
              colors={Gradients.brand.colors}
              start={{ x: 0, y: 0 }}
              end={{ x: 1, y: 0 }}
              style={[styles.xpFill, { width: `${xpRatio * 100}%` }]}
            />
          </View>
          <Text style={styles.xpRemaining}>
            {/* Vurgulu kısmın cümledeki yeri dile göre değişir (<accent>). */}
            <Trans
              i18nKey="practice.home.xpToNext"
              values={{ xp: xpRemaining }}
              components={{ accent: <Text style={styles.xpRemainingAccent} /> }}
            />
          </Text>
        </View>

        {/* Son pratikler */}
        <View style={styles.activityHeader}>
          <Text style={styles.sectionTitle}>{t('practice.home.recentTitle')}</Text>
          <Pressable onPress={goScenarios} accessibilityRole="link">
            <Text style={styles.seeAll}>{t('practice.home.seeAll')}</Text>
          </Pressable>
        </View>

        <RecentSessions
          onOpenScenario={goScenarioDetail}
          onBrowse={goScenarios}
        />
      </ScrollView>
    </SafeAreaView>
  );
}

const RECENT_SESSIONS_COUNT = 3;

// Son pratikler — GET /api/Sessions/my. Skoru olmayan (henüz değerlendirilmemiş)
// oturumlar "Devam" rozetiyle gösterilir.
function RecentSessions({
  onOpenScenario,
  onBrowse,
}: {
  onOpenScenario: (scenarioId: string) => void;
  onBrowse: () => void;
}) {
  const { t } = useTranslation();
  const contexts = useContexts();
  const { data, isPending, isError, error } = useMySessions(RECENT_SESSIONS_COUNT);

  if (isPending) {
    return (
      <View style={styles.activityList}>
        {Array.from({ length: RECENT_SESSIONS_COUNT }, (_, i) => (
          <Skeleton key={i} height={68} borderRadius={Radius.lg} />
        ))}
      </View>
    );
  }

  if (isError) {
    return <Text style={styles.activityNote}>{toSessionErrorMessage(error)}</Text>;
  }

  if (data.items.length === 0) {
    return (
      <Pressable
        onPress={onBrowse}
        style={styles.emptyActivity}
        accessibilityRole="button"
      >
        <Text style={styles.emptyActivityTitle}>{t('practice.home.emptyTitle')}</Text>
        <Text style={styles.emptyActivityText}>{t('practice.home.emptyText')}</Text>
      </Pressable>
    );
  }

  return (
    <View style={styles.activityList}>
      {data.items.map((session) => {
        const context = contexts.find((c) => c.id === session.scenarioContextId);
        const ContextIcon = getContextIcon(context?.iconName ?? '');
        const score = session.totalScore;
        const badge =
          score !== null
            ? scoreColors(score)
            : { bg: Colors.primarySoft, text: Colors.primary };
        return (
          <Pressable
            key={session.id}
            onPress={() => onOpenScenario(session.scenarioId)}
            style={styles.activityItem}
            accessibilityRole="button"
          >
            <View
              style={[
                styles.activityIcon,
                { backgroundColor: context?.softColor ?? Colors.primarySoft },
              ]}
            >
              <ContextIcon
                size={21}
                color={context?.color ?? Colors.primary}
                weight="fill"
              />
            </View>
            <View style={styles.activityInfo}>
              <Text style={styles.activityTitle} numberOfLines={1}>
                {session.scenarioTitle}
              </Text>
              <Text style={styles.activityMeta} numberOfLines={1}>
                {context?.name ?? t('practice.scenarioFallback')} ·{' '}
                {formatRelativeTime(session.completedAt ?? session.startedAt)}
              </Text>
            </View>
            <View style={[styles.scoreBadge, { backgroundColor: badge.bg }]}>
              <Text style={[styles.scoreText, { color: badge.text }]}>
                {score ?? t('practice.home.inProgress')}
              </Text>
            </View>
          </Pressable>
        );
      })}
    </View>
  );
}

interface StatCardProps {
  icon: React.ReactNode;
  value: string;
  label: string;
}

function StatCard({ icon, value, label }: StatCardProps) {
  return (
    <View style={styles.statCard}>
      {icon}
      <Text style={styles.statValue}>{value}</Text>
      <Text style={styles.statLabel}>{label}</Text>
    </View>
  );
}

const WHITE_SOFT = 'rgba(255,255,255,0.28)';

const dayDotStyle = StyleSheet.create({
  done: { backgroundColor: '#FFFFFF' },
  today: { borderWidth: 2.5, borderColor: '#FFFFFF' },
  future: { backgroundColor: WHITE_SOFT },
  missed: { backgroundColor: WHITE_SOFT },
});

const styles = StyleSheet.create({
  safeArea: {
    flex: 1,
    backgroundColor: Colors.background,
  },
  flex: {
    flex: 1,
  },
  scrollContent: {
    paddingHorizontal: 20,
    paddingTop: Spacing.md,
    paddingBottom: Spacing.lg,
    gap: 18,
  },

  // Header
  header: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
  },
  headerText: {
    flex: 1,
    paddingRight: Spacing.md,
  },
  greeting: {
    fontSize: 26,
    fontWeight: FontWeight.black,
    letterSpacing: -0.8,
    color: Colors.text,
  },
  greetingSub: {
    marginTop: 3,
    fontSize: FontSize.sm,
    color: Colors.textSecondary,
  },
  avatar: {
    width: 46,
    height: 46,
    borderRadius: Radius.full,
    backgroundColor: Colors.primarySoft,
    alignItems: 'center',
    justifyContent: 'center',
  },

  // Seri
  streakCard: {
    padding: 20,
    borderRadius: 22,
    shadowColor: '#FF6B35',
    shadowOpacity: 0.3,
    shadowRadius: 14,
    shadowOffset: { width: 0, height: 12 },
    elevation: 6,
  },
  streakTop: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 14,
  },
  streakIcon: {
    width: 54,
    height: 54,
    borderRadius: 16,
    backgroundColor: 'rgba(255,255,255,0.2)',
    alignItems: 'center',
    justifyContent: 'center',
  },
  streakTitle: {
    fontSize: FontSize.xl,
    fontWeight: FontWeight.black,
    letterSpacing: -0.4,
    color: '#FFFFFF',
  },
  streakSub: {
    marginTop: 1,
    fontSize: 13,
    color: 'rgba(255,255,255,0.9)',
  },
  weekRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    marginTop: 18,
  },
  dayCol: {
    alignItems: 'center',
    gap: 6,
  },
  dayDot: {
    width: 30,
    height: 30,
    borderRadius: 15,
    alignItems: 'center',
    justifyContent: 'center',
  },
  dayLabel: {
    fontSize: 11,
    fontWeight: FontWeight.semibold,
    color: 'rgba(255,255,255,0.85)',
  },
  dayLabelToday: {
    color: '#FFFFFF',
    fontWeight: FontWeight.extrabold,
  },
  dayLabelFuture: {
    color: 'rgba(255,255,255,0.7)',
  },

  // Bugünkü görev
  taskCard: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 14,
    padding: 20,
    borderRadius: 22,
    backgroundColor: Colors.primary,
    ...Shadow.primary,
  },
  taskLabel: {
    fontSize: 11,
    fontWeight: FontWeight.extrabold,
    letterSpacing: 1.1,
    color: 'rgba(255,255,255,0.7)',
  },
  taskTitle: {
    marginTop: 5,
    fontSize: FontSize.lg,
    fontWeight: FontWeight.extrabold,
    letterSpacing: -0.36,
    color: '#FFFFFF',
  },
  taskSub: {
    marginTop: 2,
    fontSize: 13,
    color: 'rgba(255,255,255,0.85)',
  },
  playButton: {
    width: 52,
    height: 52,
    borderRadius: 26,
    backgroundColor: '#FFFFFF',
    alignItems: 'center',
    justifyContent: 'center',
  },

  // İstatistikler
  statsGrid: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: 12,
  },
  statCard: {
    flexGrow: 1,
    flexBasis: '46%',
    padding: Spacing.md,
    borderRadius: 18,
    borderWidth: 1,
    borderColor: Colors.divider,
    backgroundColor: Colors.surface,
  },
  statValue: {
    marginTop: Spacing.sm,
    fontSize: 26,
    fontWeight: FontWeight.black,
    letterSpacing: -0.8,
    color: Colors.text,
  },
  statLabel: {
    fontSize: 13,
    color: Colors.textSecondary,
  },

  // XP
  card: {
    padding: 18,
    borderRadius: 18,
    borderWidth: 1,
    borderColor: Colors.divider,
    backgroundColor: Colors.surface,
  },
  xpHeader: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    marginBottom: 12,
  },
  xpLevel: {
    fontSize: 15,
    fontWeight: FontWeight.extrabold,
    color: Colors.text,
  },
  xpValue: {
    fontSize: 13,
    fontWeight: FontWeight.semibold,
    color: Colors.textSecondary,
  },
  xpTrack: {
    height: 10,
    borderRadius: Radius.full,
    backgroundColor: Colors.track,
    overflow: 'hidden',
  },
  xpFill: {
    height: '100%',
    borderRadius: Radius.full,
  },
  xpRemaining: {
    marginTop: 10,
    fontSize: FontSize.xs,
    color: Colors.textMuted,
    textAlign: 'center',
  },
  xpRemainingAccent: {
    color: Colors.primary,
    fontWeight: FontWeight.bold,
  },

  // Son pratikler
  activityHeader: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    marginTop: 2,
  },
  sectionTitle: {
    fontSize: FontSize.lg,
    fontWeight: FontWeight.extrabold,
    letterSpacing: -0.36,
    color: Colors.text,
  },
  seeAll: {
    fontSize: FontSize.sm,
    fontWeight: FontWeight.bold,
    color: Colors.primary,
  },
  activityList: {
    gap: 10,
    marginTop: -8,
  },
  activityNote: {
    marginTop: -8,
    fontSize: 13,
    color: Colors.textMuted,
  },
  emptyActivity: {
    marginTop: -8,
    padding: 18,
    borderRadius: Radius.lg,
    borderWidth: 1,
    borderColor: Colors.divider,
    backgroundColor: Colors.surface,
    gap: 4,
  },
  emptyActivityTitle: {
    fontSize: 15,
    fontWeight: FontWeight.bold,
    color: Colors.text,
  },
  emptyActivityText: {
    fontSize: 13,
    fontWeight: FontWeight.semibold,
    color: Colors.primary,
  },
  activityItem: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 12,
    paddingVertical: 12,
    paddingHorizontal: 14,
    borderRadius: Radius.lg,
    borderWidth: 1,
    borderColor: Colors.divider,
    backgroundColor: Colors.surface,
  },
  activityIcon: {
    width: 42,
    height: 42,
    borderRadius: 13,
    alignItems: 'center',
    justifyContent: 'center',
  },
  activityInfo: {
    flex: 1,
    minWidth: 0,
  },
  activityTitle: {
    fontSize: 15,
    fontWeight: FontWeight.bold,
    letterSpacing: -0.15,
    color: Colors.text,
  },
  activityMeta: {
    marginTop: 1,
    fontSize: FontSize.xs,
    color: Colors.textMuted,
  },
  scoreBadge: {
    minWidth: 42,
    height: 30,
    paddingHorizontal: 10,
    borderRadius: Radius.full,
    alignItems: 'center',
    justifyContent: 'center',
  },
  scoreText: {
    fontSize: FontSize.sm,
    fontWeight: FontWeight.extrabold,
  },
});
