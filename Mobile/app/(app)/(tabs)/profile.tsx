// Profil tab — kimlik + seviye rozeti, istatistik üçlüsü, rozetler, premium
// banner'ı ve çıkış. Seri/pratik sayısı/seviye/rozetler GET /api/Stats/me'den,
// ortalama skor son oturumlardan (GET /api/Sessions/my).

import { LinearGradient } from 'expo-linear-gradient';
import { router } from 'expo-router';
import {
  ArrowRight,
  Bell,
  CaretRight,
  Crown,
  Fire,
  HandPalm,
  type Icon,
  LockSimple,
  Medal,
  SignOut,
  Star,
  Translate,
  UserCircle,
} from 'phosphor-react-native';
import { useTranslation } from 'react-i18next';
import { ActivityIndicator, Pressable, ScrollView, StyleSheet, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';

import { LegalLinks, Text } from '@/components/ui';
import {
  Colors,
  FontSize,
  FontWeight,
  Gradients,
  Radius,
  Shadow,
  Spacing,
} from '@/constants/theme';
import { useMySessions } from '@/hooks/queries/useSessions';
import { EMPTY_STATS, useMyStats } from '@/hooks/queries/useStats';
import { useMySubscription } from '@/hooks/queries/useSubscription';
import { useAuth } from '@/hooks/useAuth';
import { currentLocale, type AppLanguage } from '@/lib/i18n';
import { formatReminderTime } from '@/lib/notifications/reminderPlan';
import { getInitials } from '@/lib/utils/initials';
import { useReminderStore } from '@/stores/reminderStore';
import type { ApiBadge } from '@/types/api/stats';
import type { ApiMySubscription } from '@/types/api/subscription';

// Seviye unvanları (UI metadata; seviye backend'den gelir, 500 XP'de bir artar).
// Değerler çeviri anahtarı; metin render anında çözülür.
const LEVEL_TITLES = {
  1: 'account.profile.levelTitles.level1',
  2: 'account.profile.levelTitles.level2',
  3: 'account.profile.levelTitles.level3',
  4: 'account.profile.levelTitles.level4',
} as const;

function levelTitleKey(level: number) {
  return LEVEL_TITLES[level as keyof typeof LEVEL_TITLES] ?? LEVEL_TITLES[4];
}

const AVERAGE_SAMPLE_SIZE = 50;

interface BadgeDef {
  id: ApiBadge & keyof typeof BADGE_LABEL_KEYS;
  icon: Icon;
  color: string;
  softColor: string;
}

const BADGE_LABEL_KEYS = {
  firstNo: 'account.profile.badges.firstNo',
  threeDayStreak: 'account.profile.badges.threeDayStreak',
  sevenDayStreak: 'account.profile.badges.sevenDayStreak',
  persistent: 'account.profile.badges.persistent',
} as const;

// Profilde gösterilen rozetler; id'ler backend'in Badge enum'unun camelCase hali.
// "Usta" = 50 pratik tamamlama (persistent).
const BADGES: BadgeDef[] = [
  { id: 'firstNo', icon: HandPalm, color: Colors.primary, softColor: Colors.primarySoft },
  { id: 'threeDayStreak', icon: Fire, color: Colors.secondary, softColor: Colors.secondarySoft },
  { id: 'sevenDayStreak', icon: Fire, color: Colors.secondary, softColor: Colors.secondarySoft },
  { id: 'persistent', icon: Medal, color: Colors.amber, softColor: Colors.amberSoft },
];

// Premium bandının alt satırı: yenilenecekse/bitecekse tarih.
function premiumSubtitle(
  sub: ApiMySubscription | undefined,
  t: ReturnType<typeof useTranslation>['t'],
): string {
  if (!sub?.expiresAt) return t('account.profile.premium.allContexts');
  const date = new Date(sub.expiresAt).toLocaleDateString(currentLocale(), {
    day: 'numeric',
    month: 'long',
    year: 'numeric',
  });
  return sub.willRenew
    ? t('account.profile.premium.renews', { date })
    : t('account.profile.premium.ends', { date });
}

export default function ProfileScreen() {
  const { t, i18n } = useTranslation();
  const language: AppLanguage = i18n.language === 'en' ? 'en' : 'tr';
  const { user, logout, isLoggingOut } = useAuth();
  const { data } = useMyStats();
  const stats = data ?? EMPTY_STATS;
  const { data: subscription } = useMySubscription();
  const reminderPrefs = useReminderStore((state) => state.prefs);
  const isPremium = subscription?.isPremium ?? false;

  // logout() bilerek throw etmiyor: backend'e ulaşılamasa veya token süresi
  // dolmuş olsa bile local oturum kapanır ve kullanıcı Welcome'a döner.
  const handleLogout = async () => {
    await logout();
    router.replace('/');
  };

  const fullName =
    [user?.firstName, user?.lastName].filter(Boolean).join(' ') || t('account.profile.defaultName');
  // Son 50 oturumun skorlu olanlarının ortalaması; hiç skor yoksa "–".
  const { data: sessions } = useMySessions(AVERAGE_SAMPLE_SIZE);
  const scores = (sessions?.items ?? [])
    .map((s) => s.totalScore)
    .filter((score): score is number => score !== null);
  const averageScore = scores.length
    ? Math.round(scores.reduce((sum, s) => sum + s, 0) / scores.length)
    : '–';
  const levelTitle = t(levelTitleKey(stats.level));

  return (
    <SafeAreaView style={styles.safeArea} edges={['top']}>
      <ScrollView
        contentContainerStyle={styles.scrollContent}
        showsVerticalScrollIndicator={false}
      >
        {/* Kimlik */}
        <View style={styles.identity}>
          <LinearGradient
            colors={Gradients.brand.colors}
            start={Gradients.brand.start}
            end={Gradients.brand.end}
            style={styles.avatar}
          >
            <Text style={styles.avatarText}>{getInitials(fullName)}</Text>
          </LinearGradient>
          <View style={styles.identityText}>
            <Text style={styles.name}>{fullName}</Text>
            <Text style={styles.email}>{user?.email ?? ''}</Text>
          </View>
          <View style={styles.levelPill}>
            <Star size={14} color={Colors.amber} weight="fill" />
            <Text style={styles.levelPillText}>
              {t('account.profile.levelPill', { level: stats.level, title: levelTitle })}
            </Text>
          </View>
        </View>

        {/* İstatistik üçlüsü */}
        <View style={styles.statsRow}>
          <StatBox value={stats.currentStreak} label={t('account.profile.stats.streak')} color={Colors.secondary} />
          <StatBox value={stats.totalCompletedSessions} label={t('account.profile.stats.practices')} color={Colors.primary} />
          <StatBox value={averageScore} label={t('account.profile.stats.averageScore')} color={Colors.success} />
        </View>

        {/* Rozetler */}
        <Text style={styles.sectionTitle}>{t('account.profile.badgesTitle')}</Text>
        <View style={styles.badgeRow}>
          {BADGES.map((badge) => {
            const earned = stats.unlockedBadges.includes(badge.id);
            const BadgeIcon = earned ? badge.icon : LockSimple;
            return (
              <View key={badge.id} style={styles.badge}>
                <View
                  style={[
                    styles.badgeCircle,
                    { backgroundColor: earned ? badge.softColor : '#F1F0EC' },
                  ]}
                >
                  <BadgeIcon
                    size={earned ? 28 : 24}
                    color={earned ? badge.color : Colors.textDisabled}
                    weight="fill"
                  />
                </View>
                <Text
                  style={[styles.badgeLabel, !earned && styles.badgeLabelLocked]}
                  numberOfLines={2}
                >
                  {t(BADGE_LABEL_KEYS[badge.id])}
                </Text>
              </View>
            );
          })}
        </View>

        {/* Premium banner */}
        <Pressable
          onPress={isPremium ? undefined : () => router.push('/(app)/paywall')}
          disabled={isPremium}
          accessibilityRole={isPremium ? 'summary' : 'button'}
          accessibilityLabel={
            isPremium ? t('account.profile.premium.active') : t('account.profile.premium.upgrade')
          }
        >
          <LinearGradient
            colors={[Colors.ink, '#2B2F3A']}
            start={{ x: 0, y: 0.3 }}
            end={{ x: 1, y: 0.7 }}
            style={styles.premiumBanner}
          >
            <View style={styles.premiumIcon}>
              <Crown size={23} color={Colors.ink} weight="fill" />
            </View>
            <View style={styles.flex}>
              <Text style={styles.premiumTitle}>
                {isPremium
                  ? t('account.profile.premium.active')
                  : t('account.profile.premium.upgrade')}
              </Text>
              <Text style={styles.premiumSub}>
                {isPremium
                  ? premiumSubtitle(subscription, t)
                  : t('account.profile.premium.upgradeSubtitle')}
              </Text>
            </View>
            {!isPremium && <ArrowRight size={18} color="#FFFFFF" weight="bold" />}
          </LinearGradient>
        </Pressable>

        {/* Ayarlar */}
        <View style={styles.settings}>
          <Pressable
            onPress={() => router.push('/(app)/account')}
            accessibilityRole="button"
            style={({ pressed }) => [
              styles.settingRow,
              styles.settingRowDivider,
              pressed && styles.pressed,
            ]}
          >
            <UserCircle size={21} color={Colors.textSecondary} />
            <Text style={styles.settingText}>{t('account.profile.settings.account')}</Text>
            <CaretRight size={16} color={Colors.textDisabled} />
          </Pressable>

          {/* Yerel günlük hatırlatma (sunucu push'u yok). */}
          <Pressable
            onPress={() => router.push('/(app)/notifications')}
            accessibilityRole="button"
            style={({ pressed }) => [
              styles.settingRow,
              styles.settingRowDivider,
              pressed && styles.pressed,
            ]}
          >
            <Bell size={21} color={Colors.textSecondary} />
            <Text style={styles.settingText}>{t('account.profile.settings.notifications')}</Text>
            <Text style={styles.settingValue}>
              {reminderPrefs.enabled
                ? formatReminderTime(reminderPrefs)
                : t('account.profile.settings.off')}
            </Text>
            <CaretRight size={16} color={Colors.textDisabled} />
          </Pressable>

          <Pressable
            onPress={() => router.push('/(app)/language')}
            accessibilityRole="button"
            style={({ pressed }) => [
              styles.settingRow,
              styles.settingRowDivider,
              pressed && styles.pressed,
            ]}
          >
            <Translate size={21} color={Colors.textSecondary} />
            <Text style={styles.settingText}>{t('common.language.title')}</Text>
            <Text style={styles.settingValue}>{t(`common.language.${language}`)}</Text>
            <CaretRight size={16} color={Colors.textDisabled} />
          </Pressable>

          <Pressable
            onPress={handleLogout}
            disabled={isLoggingOut}
            accessibilityRole="button"
            style={({ pressed }) => [styles.settingRow, pressed && styles.pressed]}
          >
            {isLoggingOut ? (
              <ActivityIndicator size="small" color={Colors.error} />
            ) : (
              <SignOut size={21} color={Colors.error} />
            )}
            <Text style={[styles.settingText, { color: Colors.error }]}>
              {t('account.profile.settings.logout')}
            </Text>
          </Pressable>
        </View>

        <View style={styles.legal}>
          <LegalLinks />
        </View>
      </ScrollView>
    </SafeAreaView>
  );
}

function StatBox({
  value,
  label,
  color,
}: {
  value: number | string;
  label: string;
  color: string;
}) {
  return (
    <View style={styles.statBox}>
      <Text style={[styles.statValue, { color }]}>{value}</Text>
      <Text style={styles.statLabel}>{label}</Text>
    </View>
  );
}

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
  },

  // Kimlik
  identity: {
    alignItems: 'center',
    gap: 10,
  },
  avatar: {
    width: 84,
    height: 84,
    borderRadius: 42,
    alignItems: 'center',
    justifyContent: 'center',
    ...Shadow.primary,
  },
  avatarText: {
    fontSize: 30,
    fontWeight: FontWeight.black,
    color: '#FFFFFF',
  },
  identityText: {
    alignItems: 'center',
  },
  name: {
    fontSize: FontSize.xl,
    fontWeight: FontWeight.black,
    letterSpacing: -0.44,
    color: Colors.text,
  },
  email: {
    fontSize: FontSize.sm,
    color: Colors.textMuted,
  },
  levelPill: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 5,
    paddingHorizontal: 13,
    paddingVertical: 6,
    borderRadius: Radius.full,
    backgroundColor: Colors.amberSoft,
  },
  levelPillText: {
    fontSize: 13,
    fontWeight: FontWeight.extrabold,
    color: '#9A7400',
  },

  // İstatistik
  statsRow: {
    flexDirection: 'row',
    gap: 10,
    marginTop: 22,
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

  // Rozetler
  sectionTitle: {
    marginTop: Spacing.lg,
    marginBottom: 12,
    fontSize: FontSize.base,
    fontWeight: FontWeight.extrabold,
    letterSpacing: -0.16,
    color: Colors.text,
  },
  badgeRow: {
    flexDirection: 'row',
    gap: 12,
  },
  badge: {
    flex: 1,
    alignItems: 'center',
    gap: Spacing.sm,
  },
  badgeCircle: {
    width: 60,
    height: 60,
    borderRadius: 30,
    alignItems: 'center',
    justifyContent: 'center',
  },
  badgeLabel: {
    fontSize: 11,
    lineHeight: 13,
    fontWeight: FontWeight.bold,
    color: Colors.textSecondary,
    textAlign: 'center',
  },
  badgeLabelLocked: {
    color: Colors.textDisabled,
  },

  // Premium
  premiumBanner: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 13,
    marginTop: 22,
    padding: Spacing.md,
    borderRadius: 18,
  },
  premiumIcon: {
    width: 42,
    height: 42,
    borderRadius: 12,
    backgroundColor: Colors.amber,
    alignItems: 'center',
    justifyContent: 'center',
  },
  premiumTitle: {
    fontSize: 15,
    fontWeight: FontWeight.extrabold,
    color: '#FFFFFF',
  },
  premiumSub: {
    fontSize: FontSize.xs,
    color: 'rgba(255,255,255,0.7)',
  },

  // Ayarlar
  settings: {
    marginTop: 14,
    borderRadius: 18,
    borderWidth: 1,
    borderColor: Colors.divider,
    backgroundColor: Colors.surface,
    overflow: 'hidden',
  },
  settingRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 13,
    paddingVertical: 15,
    paddingHorizontal: Spacing.md,
  },
  settingRowDivider: {
    borderBottomWidth: 1,
    borderBottomColor: Colors.chip,
  },
  legal: {
    marginTop: Spacing.lg,
  },
  settingText: {
    flex: 1,
    fontSize: 15,
    fontWeight: FontWeight.semibold,
  },
  settingValue: {
    fontSize: FontSize.sm,
    color: Colors.textMuted,
  },
  pressed: {
    backgroundColor: Colors.chip,
  },
});
