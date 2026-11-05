// Yerel (cihazda zamanlanan) pratik hatırlatmaları. Sunucu push'u YOK — bu
// modül yalnızca expo-notifications'ın yerel zamanlayıcısını kullanır.
// Uygulamanın geri kalanı expo-notifications'ı doğrudan import etmez.

import * as Notifications from 'expo-notifications';
import { Linking, Platform } from 'react-native';

import {
  planReminders,
  type ReminderPrefs,
  type ReminderStatsInput,
} from '@/lib/notifications/reminderPlan';

const CHANNEL_ID = 'practice-reminders';

// Uygulama açıkken gelen hatırlatma gösterilmez: kullanıcı zaten uygulamada.
Notifications.setNotificationHandler({
  handleNotification: async () => ({
    shouldShowBanner: false,
    shouldShowList: false,
    shouldPlaySound: false,
    shouldSetBadge: false,
  }),
});

async function ensureChannel(): Promise<void> {
  if (Platform.OS !== 'android') {
    return;
  }
  await Notifications.setNotificationChannelAsync(CHANNEL_ID, {
    name: 'Pratik hatırlatmaları',
    description: 'Günlük pratik ve seri hatırlatmaları',
    importance: Notifications.AndroidImportance.DEFAULT,
  });
}

// 'blocked': sistem penceresi artık çıkmıyor, yalnızca ayarlardan açılabilir.
// Android 13+ hiç sorulmamış izni de 'denied' bildirir; farkı canAskAgain'dir.
export type ReminderPermission = 'granted' | 'blocked' | 'askable';

function toReminderPermission(result: Notifications.NotificationPermissionsStatus): ReminderPermission {
  if (result.status === 'granted') return 'granted';
  return result.canAskAgain ? 'askable' : 'blocked';
}

export async function getReminderPermission(): Promise<ReminderPermission> {
  return toReminderPermission(await Notifications.getPermissionsAsync());
}

/** İzin ister (Android 13+ ve iOS'ta sistem penceresi). Önceden reddedildiyse pencere çıkmaz. */
export async function requestReminderPermission(): Promise<ReminderPermission> {
  await ensureChannel(); // Android: kanal yoksa izin penceresi çıkmayabiliyor.
  return toReminderPermission(await Notifications.requestPermissionsAsync());
}

export function openNotificationSettings(): Promise<void> {
  return Linking.openSettings();
}

// Eşitlemeler SIRAYLA çalışır: iki çağrının iptal/zamanla adımları iç içe geçerse
// aynı güne iki hatırlatma kurulur (giriş sonrası tercih + istatistik art arda gelir).
let queue: Promise<void> = Promise.resolve();

/**
 * Planı baştan kurar: önce tüm zamanlanmışları iptal eder, sonra izin varsa
 * yenilerini zamanlar. Hata fırlatmaz (hatırlatma kritik değil).
 */
export function syncReminders(
  prefs: ReminderPrefs,
  stats: ReminderStatsInput | null,
): Promise<void> {
  queue = queue.then(() => runSync(prefs, stats));
  return queue;
}

/** Bekleyen eşitlemelerden sonra hepsini iptal eder (çıkış). */
export function cancelReminders(): Promise<void> {
  queue = queue.then(() => Notifications.cancelAllScheduledNotificationsAsync()).catch((e) => {
    console.warn('[reminders] iptal edilemedi', e);
  });
  return queue;
}

async function runSync(prefs: ReminderPrefs, stats: ReminderStatsInput | null): Promise<void> {
  try {
    await Notifications.cancelAllScheduledNotificationsAsync();
    if (!prefs.enabled || (await getReminderPermission()) !== 'granted') {
      return;
    }
    await ensureChannel();
    for (const reminder of planReminders(new Date(), prefs, stats)) {
      await Notifications.scheduleNotificationAsync({
        content: { title: reminder.title, body: reminder.body },
        trigger: {
          type: Notifications.SchedulableTriggerInputTypes.DATE,
          date: reminder.date,
          channelId: CHANNEL_ID,
        },
      });
    }
  } catch (e) {
    console.warn('[reminders] zamanlanamadı', e);
  }
}
