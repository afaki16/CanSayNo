// Anasayfa: oyunsu/canlı gamification ekranı. Streak, bugünkü görev, istatistikler,
// XP ilerleme çubuğu ve son aktiviteler. Tüm veriler şimdilik mock data'dan gelir.

import { LinearGradient } from 'expo-linear-gradient';
import { router } from 'expo-router';
import {
  ArrowRight,
  Fire,
  Flame,
  Play,
  Star,
  Target,
  Trophy,
  User,
} from 'phosphor-react-native';
import {
  Pressable,
  ScrollView,
  StyleSheet,
  Text,
  View,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';

import {
  Colors,
  FontSize,
  FontWeight,
  Radius,
  Shadow,
  Spacing,
} from '@/constants/theme';
import { useAuth } from '@/hooks/useAuth';
import { getContextIcon } from '@/lib/icons';
import {
  mockContexts,
  mockRecentActivities,
  mockUserStats,
} from '@/lib/mockData';
import { formatRelativeTime } from '@/lib/utils/datetime';

// Skora göre badge renk eşlemesi (soft bg + text).
function scoreColors(score: number) {
  if (score >= 80) return { bg: Colors.successSoft, text: Colors.success };
  if (score >= 60) return { bg: Colors.warningSoft, text: Colors.warning };
  return { bg: Colors.errorSoft, text: Colors.error };
}

export default function HomeScreen() {
  const { user } = useAuth();
  const stats = mockUserStats;

  const firstName = user?.firstName ?? 'dostum';
  const xpRatio = Math.min(stats.xp / stats.xpToNextLevel, 1);
  const xpRemaining = Math.max(stats.xpToNextLevel - stats.xp, 0);

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
        keyboardShouldPersistTaps="handled"
        showsVerticalScrollIndicator={false}
      >
        {/* A) Başlık — selamlama + avatar */}
        <View style={styles.header}>
          <View style={styles.headerText}>
            <Text style={styles.greeting}>Selam, {firstName}! 👋</Text>
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

        {/* B) Streak kartı — en prominent, warm gradient */}
        <Pressable onPress={goScenarios} accessibilityRole="button">
          <LinearGradient
            colors={['#FF6B35', '#F4A261']}
            start={{ x: 0, y: 0 }}
            end={{ x: 1, y: 1 }}
            style={styles.streakCard}
          >
            <Fire size={48} color="#FFFFFF" weight="fill" />
            <View style={styles.streakText}>
              <Text style={styles.streakTitle}>
                {stats.currentStreak} GÜNLÜK SERİ
              </Text>
              <Text style={styles.streakSub}>
                Bugün de pratik yap, seriyi kaybetme!
              </Text>
            </View>
            <ArrowRight size={20} color="#FFFFFF" weight="bold" />
          </LinearGradient>
        </Pressable>

        {/* C) Bugünkü görev — büyük CTA */}
        <Pressable
          onPress={() => goScenarioDetail('is_001')}
          style={styles.taskCard}
          accessibilityRole="button"
        >
          <View style={styles.taskTextBlock}>
            <Text style={styles.taskLabel}>BUGÜNKÜ GÖREV</Text>
            <Text style={styles.taskTitle}>İş baskısıyla pratik yap</Text>
            <Text style={styles.taskSub}>Cuma 18:00 mesajını ele al</Text>
          </View>
          <View style={styles.playButton}>
            <Play size={28} color={Colors.primary} weight="fill" />
          </View>
        </Pressable>

        {/* D) İstatistikler — 2x2 grid */}
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
            icon={<Star size={24} color={Colors.warning} weight="fill" />}
            value={`${stats.level}`}
            label="Seviye"
          />
        </View>

        {/* E) XP ilerleme çubuğu */}
        <View style={styles.xpCard}>
          <View style={styles.xpHeader}>
            <Text style={styles.xpLevel}>Seviye {stats.level}</Text>
            <Text style={styles.xpValue}>
              {stats.xp} / {stats.xpToNextLevel} XP
            </Text>
          </View>
          <View style={styles.xpTrack}>
            <View style={[styles.xpFill, { width: `${xpRatio * 100}%` }]} />
          </View>
          <Text style={styles.xpRemaining}>
            Sonraki seviyeye {xpRemaining} XP
          </Text>
        </View>

        {/* F) Son aktiviteler */}
        <View style={styles.activitySection}>
          <View style={styles.activityHeader}>
            <Text style={styles.sectionTitle}>Son Pratiklerin</Text>
            <Pressable onPress={goScenarios} accessibilityRole="link">
              <Text style={styles.seeAll}>Tümü →</Text>
            </Pressable>
          </View>

          <View style={styles.activityList}>
            {mockRecentActivities.map((activity) => {
              const context = mockContexts.find(
                (c) => c.id === activity.contextId,
              );
              const ContextIcon = getContextIcon(context?.iconName ?? '');
              const sc = scoreColors(activity.score);
              return (
                <Pressable
                  key={activity.id}
                  onPress={() => goScenarioDetail(activity.scenarioId)}
                  style={styles.activityItem}
                  accessibilityRole="button"
                >
                  <View
                    style={[
                      styles.activityIcon,
                      { backgroundColor: activity.contextColor },
                    ]}
                  >
                    <ContextIcon size={20} color="#FFFFFF" weight="fill" />
                  </View>
                  <View style={styles.activityInfo}>
                    <Text style={styles.activityTitle} numberOfLines={1}>
                      {activity.scenarioTitle}
                    </Text>
                    <Text style={styles.activityMeta} numberOfLines={1}>
                      {activity.contextName} ·{' '}
                      {formatRelativeTime(activity.completedAt)}
                    </Text>
                  </View>
                  <View style={[styles.scoreBadge, { backgroundColor: sc.bg }]}>
                    <Text style={[styles.scoreText, { color: sc.text }]}>
                      {activity.score}
                    </Text>
                  </View>
                </Pressable>
              );
            })}
          </View>
        </View>
      </ScrollView>
    </SafeAreaView>
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

const styles = StyleSheet.create({
  safeArea: {
    flex: 1,
    backgroundColor: Colors.background,
  },
  scrollContent: {
    padding: Spacing.md,
    paddingBottom: Spacing.xxl + 40,
    gap: Spacing.lg,
  },

  // A) Header
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
    fontSize: FontSize.xxl,
    fontWeight: FontWeight.bold,
    color: Colors.text,
  },
  greetingSub: {
    marginTop: Spacing.xs,
    fontSize: FontSize.sm,
    color: Colors.textSecondary,
    fontStyle: 'italic',
  },
  avatar: {
    width: 40,
    height: 40,
    borderRadius: Radius.full,
    backgroundColor: Colors.primarySoft,
    alignItems: 'center',
    justifyContent: 'center',
  },

  // B) Streak
  streakCard: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: Spacing.md,
    padding: Spacing.lg,
    borderRadius: Radius.xl,
    ...Shadow.md,
  },
  streakText: {
    flex: 1,
  },
  streakTitle: {
    fontSize: FontSize.xs,
    fontWeight: FontWeight.bold,
    color: '#FFFFFF',
    letterSpacing: 1,
  },
  streakSub: {
    marginTop: Spacing.xs,
    fontSize: FontSize.sm,
    color: '#FFFFFF',
    opacity: 0.9,
  },

  // C) Bugünkü görev
  taskCard: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    padding: Spacing.lg,
    borderRadius: Radius.xl,
    backgroundColor: Colors.primary,
    ...Shadow.md,
  },
  taskTextBlock: {
    flex: 1,
    paddingRight: Spacing.md,
  },
  taskLabel: {
    fontSize: FontSize.xs,
    fontWeight: FontWeight.bold,
    color: '#FFFFFF',
    opacity: 0.7,
    letterSpacing: 1,
  },
  taskTitle: {
    marginTop: Spacing.xs,
    fontSize: FontSize.lg,
    fontWeight: FontWeight.bold,
    color: '#FFFFFF',
  },
  taskSub: {
    marginTop: 2,
    fontSize: FontSize.sm,
    color: '#FFFFFF',
    opacity: 0.8,
  },
  playButton: {
    width: 48,
    height: 48,
    borderRadius: Radius.full,
    backgroundColor: Colors.primarySoft,
    alignItems: 'center',
    justifyContent: 'center',
  },

  // D) İstatistikler
  statsGrid: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: Spacing.md,
  },
  statCard: {
    // 2 sütun: (toplam genişlik - gap) / 2 → flexBasis ile yaklaşık %48
    flexBasis: '47%',
    flexGrow: 1,
    backgroundColor: Colors.surface,
    borderWidth: 1,
    borderColor: Colors.border,
    borderRadius: Radius.lg,
    padding: Spacing.md,
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

  // E) XP
  xpCard: {
    backgroundColor: Colors.surface,
    borderWidth: 1,
    borderColor: Colors.border,
    borderRadius: Radius.lg,
    padding: Spacing.md,
    gap: Spacing.sm,
  },
  xpHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
  },
  xpLevel: {
    fontSize: FontSize.sm,
    fontWeight: FontWeight.bold,
    color: Colors.text,
  },
  xpValue: {
    fontSize: FontSize.sm,
    color: Colors.textSecondary,
  },
  xpTrack: {
    height: 8,
    borderRadius: Radius.full,
    backgroundColor: Colors.border,
    overflow: 'hidden',
  },
  xpFill: {
    height: '100%',
    borderRadius: Radius.full,
    backgroundColor: Colors.primary,
  },
  xpRemaining: {
    fontSize: FontSize.xs,
    color: Colors.textMuted,
    textAlign: 'center',
  },

  // F) Son aktiviteler
  activitySection: {
    gap: Spacing.md,
  },
  activityHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
  },
  sectionTitle: {
    fontSize: FontSize.lg,
    fontWeight: FontWeight.bold,
    color: Colors.text,
  },
  seeAll: {
    fontSize: FontSize.sm,
    color: Colors.primary,
    fontWeight: FontWeight.semibold,
  },
  activityList: {
    gap: Spacing.sm,
  },
  activityItem: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: Spacing.md,
    backgroundColor: Colors.surface,
    borderWidth: 1,
    borderColor: Colors.border,
    borderRadius: Radius.lg,
    padding: Spacing.md,
  },
  activityIcon: {
    width: 40,
    height: 40,
    borderRadius: Radius.full,
    alignItems: 'center',
    justifyContent: 'center',
  },
  activityInfo: {
    flex: 1,
  },
  activityTitle: {
    fontSize: FontSize.base,
    fontWeight: FontWeight.medium,
    color: Colors.text,
  },
  activityMeta: {
    marginTop: 2,
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
