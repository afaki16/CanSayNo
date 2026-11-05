// Bildirimler — günlük pratik hatırlatması (cihazda zamanlanır, sunucu push'u yok).
// Aç/kapat + saat. Açarken sistem izni istenir; reddedildiyse ayarlara yönlendirilir.
// Plan useReminderSync tarafından kurulur (tercih değişince kendiliğinden).

import { router } from 'expo-router';
import { ArrowLeft, BellRinging } from 'phosphor-react-native';
import { useCallback, useEffect, useState } from 'react';
import { AppState, Pressable, ScrollView, StyleSheet, Switch, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';

import { IconButton, Text } from '@/components/ui';
import { Colors, FontSize, FontWeight, Radius, Spacing } from '@/constants/theme';
import { formatReminderTime } from '@/lib/notifications/reminderPlan';
import {
  getReminderPermission,
  openNotificationSettings,
  requestReminderPermission,
  type ReminderPermission,
} from '@/lib/notifications/reminders';
import { useReminderStore } from '@/stores/reminderStore';

const TIME_OPTIONS = [
  { hour: 9, minute: 0 },
  { hour: 12, minute: 30 },
  { hour: 18, minute: 0 },
  { hour: 20, minute: 0 },
  { hour: 21, minute: 30 },
  { hour: 23, minute: 0 },
];

export default function NotificationsScreen() {
  const prefs = useReminderStore((state) => state.prefs);
  const setPrefs = useReminderStore((state) => state.setPrefs);
  const [permission, setPermission] = useState<ReminderPermission | null>(null);

  const refreshPermission = useCallback(() => {
    void getReminderPermission().then(setPermission);
  }, []);

  // Kullanıcı ayarlardan izni açıp geri dönebilir.
  useEffect(() => {
    refreshPermission();
    const subscription = AppState.addEventListener('change', (state) => {
      if (state === 'active') refreshPermission();
    });
    return () => subscription.remove();
  }, [refreshPermission]);

  const onToggle = async (enabled: boolean) => {
    if (!enabled) {
      await setPrefs({ ...prefs, enabled: false });
      return;
    }
    const result = permission === 'granted' ? 'granted' : await requestReminderPermission();
    setPermission(result);
    if (result === 'granted') {
      await setPrefs({ ...prefs, enabled: true });
    }
  };

  const blocked = permission === 'blocked';

  return (
    <SafeAreaView style={styles.safeArea}>
      <ScrollView contentContainerStyle={styles.scrollContent} showsVerticalScrollIndicator={false}>
        <View style={styles.topBar}>
          <IconButton
            icon={ArrowLeft}
            bordered
            onPress={() => router.back()}
            accessibilityLabel="Geri"
          />
        </View>

        <View style={styles.header}>
          <Text style={styles.title}>Bildirimler</Text>
          <Text style={styles.subtitle}>
            Her gün seçtiğin saatte kısa bir hatırlatma. O gün pratik yaptıysan susar.
          </Text>
        </View>

        <View style={styles.card}>
          <View style={styles.row}>
            <View style={styles.rowIcon}>
              <BellRinging size={20} color={Colors.primary} weight="fill" />
            </View>
            <View style={styles.flex}>
              <Text style={styles.rowTitle}>Günlük pratik hatırlatması</Text>
              <Text style={styles.rowText}>
                {prefs.enabled && !blocked ? `Her gün ${formatReminderTime(prefs)}` : 'Kapalı'}
              </Text>
            </View>
            <Switch
              value={prefs.enabled && !blocked}
              onValueChange={onToggle}
              disabled={blocked}
              trackColor={{ true: Colors.primary, false: Colors.chip }}
              thumbColor="#FFFFFF"
              accessibilityLabel="Günlük pratik hatırlatması"
            />
          </View>
        </View>

        {blocked ? (
          <View style={styles.blockedBox}>
            <Text style={styles.blockedText}>
              Bildirim izni kapalı. Hatırlatma alabilmek için telefonunun ayarlarından CanSayNo
              bildirimlerine izin ver.
            </Text>
            <Pressable onPress={openNotificationSettings} hitSlop={8} accessibilityRole="button">
              <Text style={styles.blockedLink}>Ayarları aç</Text>
            </Pressable>
          </View>
        ) : null}

        {prefs.enabled && !blocked ? (
          <View style={styles.section}>
            <Text style={styles.sectionTitle}>Saat</Text>
            <View style={styles.chips}>
              {TIME_OPTIONS.map((option) => {
                const selected = option.hour === prefs.hour && option.minute === prefs.minute;
                return (
                  <Pressable
                    key={formatReminderTime(option)}
                    onPress={() => setPrefs({ ...prefs, ...option })}
                    accessibilityRole="radio"
                    accessibilityState={{ selected }}
                    style={[styles.chip, selected && styles.chipSelected]}
                  >
                    <Text style={[styles.chipText, selected && styles.chipTextSelected]}>
                      {formatReminderTime(option)}
                    </Text>
                  </Pressable>
                );
              })}
            </View>
          </View>
        ) : null}
      </ScrollView>
    </SafeAreaView>
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
    flexGrow: 1,
    paddingHorizontal: 28,
    paddingTop: Spacing.md,
    paddingBottom: Spacing.xl,
  },
  topBar: {
    flexDirection: 'row',
    alignItems: 'center',
  },
  header: {
    marginTop: 26,
    marginBottom: 30,
  },
  title: {
    fontSize: 30,
    fontWeight: FontWeight.black,
    letterSpacing: -0.9,
    color: Colors.text,
  },
  subtitle: {
    marginTop: Spacing.sm,
    fontSize: FontSize.base,
    color: Colors.textSecondary,
  },
  card: {
    padding: Spacing.md,
    borderRadius: Radius.lg,
    backgroundColor: Colors.surface,
    borderWidth: 1,
    borderColor: Colors.border,
  },
  row: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 12,
  },
  rowIcon: {
    width: 38,
    height: 38,
    borderRadius: 11,
    alignItems: 'center',
    justifyContent: 'center',
    backgroundColor: Colors.primarySoft,
  },
  rowTitle: {
    fontSize: FontSize.base,
    fontWeight: FontWeight.bold,
    color: Colors.text,
  },
  rowText: {
    fontSize: FontSize.sm,
    color: Colors.textSecondary,
  },
  blockedBox: {
    marginTop: Spacing.md,
    gap: Spacing.sm,
    padding: Spacing.md,
    borderRadius: Radius.lg,
    backgroundColor: Colors.surface,
    borderWidth: 1,
    borderColor: Colors.errorSoft,
  },
  blockedText: {
    fontSize: FontSize.sm,
    color: Colors.textSecondary,
  },
  blockedLink: {
    fontSize: FontSize.sm,
    fontWeight: FontWeight.bold,
    color: Colors.primary,
  },
  section: {
    marginTop: Spacing.xl,
    gap: Spacing.sm,
  },
  sectionTitle: {
    fontSize: FontSize.lg,
    fontWeight: FontWeight.bold,
    color: Colors.text,
  },
  chips: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: Spacing.sm,
  },
  chip: {
    paddingHorizontal: 16,
    paddingVertical: 10,
    borderRadius: Radius.full,
    backgroundColor: Colors.surface,
    borderWidth: 1,
    borderColor: Colors.border,
  },
  chipSelected: {
    backgroundColor: Colors.primary,
    borderColor: Colors.primary,
  },
  chipText: {
    fontSize: FontSize.sm,
    fontWeight: FontWeight.bold,
    color: Colors.text,
  },
  chipTextSelected: {
    color: '#FFFFFF',
  },
});
