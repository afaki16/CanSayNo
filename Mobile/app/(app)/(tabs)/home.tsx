// Anasayfa: oyunlaştırma ekranı. Seri (haftalık gün çizelgesiyle), bugünkü görev,
// istatistikler, XP ilerlemesi ve son pratikler. Son pratikler gerçek veriden
// (GET /api/Sessions/my); seri/XP/istatistikler hâlâ mock userStats — UserStats
// endpoint'i gelince bağlanacak.

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
import { Pressable, ScrollView, StyleSheet, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';

import { Skeleton, Text } from '@/components/ui';
import {
  Colors,
  FontSize,
  FontWeight,
  Radius,
  Shadow,
  Spacing,
} from '@/constants/theme';
import { useMySessions } from '@/hooks/queries/useSessions';
import { useAuth } from '@/hooks/useAuth';
import { toSessionErrorMessage } from '@/lib/api';
import { getContextIcon } from '@/lib/icons';
import { contexts, mockUserStats } from '@/lib/mockData';
import { formatRelativeTime } from '@/lib/utils/datetime';

const WEEK_DAYS = ['Pzt', 'Sal', 'Çar', 'Per', 'Cum', 'Cmt', 'Paz'];

// Bugünkü görev şimdilik sabit; öneri endpoint'i gelince API'den seçilecek.
const TODAYS_TASK_SCENARIO_ID = 'is_001_cuma_mesaji';

// Skora göre badge renk eşlemesi (soft bg + text).
function scoreColors(score: number) {
  if (score >= 80) return { bg: Colors.successSoft, text: Colors.success };
  if (score >= 60) return { bg: Colors.warningSoft, text: Colors.warning };
  return { bg: Colors.errorSoft, text: Colors.error };
}

type DayState = 'done' | 'today' | 'future' | 'missed';

// Seri sayısından bu haftanın gün durumlarını türetir: bugünden geriye
// `streak` gün tamamlanmış sayılır. Bugün pratik yapıldıysa bugün de dahil.
function buildWeek(streak: number, practicedToday: boolean): DayState[] {
  const todayIndex = (new Date().getDay() + 6) % 7; // Pazartesi = 0
  const doneBeforeToday = practicedToday ? streak - 1 : streak;
  return WEEK_DAYS.map((_, i) => {
    if (i > todayIndex) return 'future';
    if (i === todayIndex) return practicedToday ? 'done' : 'today';
    return todayIndex - i <= doneBeforeToday ? 'done' : 'missed';
  });
}

export default function HomeScreen() {
  const { user } = useAuth();
  const stats = mockUserStats;

  const firstName = user?.firstName ?? 'dostum';
  const xpRatio = Math.min(stats.xp / stats.xpToNextLevel, 1);
  const xpRemaining = Math.max(stats.xpToNextLevel - stats.xp, 0);
  const week = buildWeek(stats.currentStreak, stats.todaySessionsCount > 0);

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
            <Text style={styles.greeting}>Selam, {firstName} 👋</Text>
            <Text style={styles.greetingSub}>
              Bugün hangi sınırını koyacaksın?
            </Text>
          </View>
          <Pressable
            onPress={goProfile}
            style={styles.avatar}
            accessibilityRole="button"
            accessibilityLabel="Profil"
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
                <Text style={styles.streakTitle}>
                  {stats.currentStreak} günlük seri 🔥
                </Text>
                <Text style={styles.streakSub}>
                  Bugün de pratik yap, kaybetme!
                </Text>
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
                    {state === 'today' ? 'Bugün' : WEEK_DAYS[i]}
                  </Text>
                </View>
              ))}
            </View>
          </LinearGradient>
        </Pressable>

        {/* Bugünkü görev */}
        <Pressable
          onPress={() => goScenarioDetail(TODAYS_TASK_SCENARIO_ID)}
          style={styles.taskCard}
          accessibilityRole="button"
        >
          <View style={styles.flex}>
            <Text style={styles.taskLabel}>BUGÜNKÜ GÖREV</Text>
            <Text style={styles.taskTitle}>İş baskısıyla pratik yap</Text>
            <Text style={styles.taskSub}>Cuma 18:00 mesajını ele al · 5 dk</Text>
          </View>
          <View style={styles.playButton}>
            <Play size={24} color={Colors.primary} weight="fill" />
          </View>
        </Pressable>

        {/* İstatistikler — 2x2 grid */}
        <View style={styles.statsGrid}>
          <StatCard
            icon={<Trophy size={24} color={Colors.primary} weight="fill" />}
            value={`${stats.totalSessions}`}
            label="Toplam Pratik"
          />
          <StatCard
            icon={<Flame size={24} color={Colors.secondary} weight="fill" />}
            value={`${stats.longestStreak} gün`}
            label="En Uzun Seri"
          />
          <StatCard
            icon={<Target size={24} color={Colors.success} weight="fill" />}
            value={`${stats.weeklyProgress}/${stats.weeklyGoal}`}
            label="Bu Hafta"
          />
          <StatCard
            icon={<Star size={24} color={Colors.amber} weight="fill" />}
            value={`${stats.level}`}
            label="Seviye"
          />
        </View>

        {/* XP ilerlemesi */}
        <View style={styles.card}>
          <View style={styles.xpHeader}>
            <Text style={styles.xpLevel}>Seviye {stats.level}</Text>
            <Text style={styles.xpValue}>
              {stats.xp} / {stats.xpToNextLevel} XP
            </Text>
          </View>
          <View style={styles.xpTrack}>
            <LinearGradient
              colors={[Colors.primary, Colors.primaryLight]}
              start={{ x: 0, y: 0 }}
              end={{ x: 1, y: 0 }}
              style={[styles.xpFill, { width: `${xpRatio * 100}%` }]}
            />
          </View>
          <Text style={styles.xpRemaining}>
            Sonraki seviyeye{' '}
            <Text style={styles.xpRemainingAccent}>{xpRemaining} XP</Text>
          </Text>
        </View>

        {/* Son pratikler */}
        <View style={styles.activityHeader}>
          <Text style={styles.sectionTitle}>Son Pratiklerin</Text>
          <Pressable onPress={goScenarios} accessibilityRole="link">
            <Text style={styles.seeAll}>Tümü →</Text>
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
        <Text style={styles.emptyActivityTitle}>Henüz pratik yapmadın</Text>
        <Text style={styles.emptyActivityText}>
          İlk senaryonu seç, ilk “hayır”ını söyle →
        </Text>
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
                {context?.name ?? 'Senaryo'} ·{' '}
                {formatRelativeTime(session.completedAt ?? session.startedAt)}
              </Text>
            </View>
            <View style={[styles.scoreBadge, { backgroundColor: badge.bg }]}>
              <Text style={[styles.scoreText, { color: badge.text }]}>
                {score ?? 'Devam'}
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
