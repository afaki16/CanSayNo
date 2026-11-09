// Günlük pratik hatırlatmasının PLANI (saf fonksiyon, yan etkisiz).
// Zamanlamayı lib/notifications/reminders.ts yapar.
//
// Kural: önümüzdeki REMINDER_DAYS gün için günde bir hatırlatma. Bugün pratik
// yapıldıysa ya da saat geçtiyse bugünkü atlanır. Uygulama açıldıkça plan
// yenilenir; kullanıcı uygulamayı bir hafta açmazsa hatırlatmalar kendiliğinden
// susar (ısrarcı olmasın diye bilerek).

import { t } from '@/lib/i18n';

export const REMINDER_DAYS = 7;

export interface ReminderPrefs {
  enabled: boolean;
  hour: number; // 0-23, cihaz saati
  minute: number; // 0-59
}

export const DEFAULT_REMINDER_PREFS: ReminderPrefs = { enabled: false, hour: 20, minute: 0 };

export interface ReminderStatsInput {
  currentStreak: number;
  practicedToday: boolean;
}

export interface PlannedReminder {
  date: Date;
  title: string;
  body: string;
}

// Sonraki günler için dönen metinler. O günün serisini bilemeyiz (uygulama
// açılmadıysa veri yok), bu yüzden sayı içermezler. Metinler planlama anındaki
// dilde çözülür; dil değişince plan yeniden kurulur (useReminderSync).
const LATER_MESSAGES = ['ready', 'muscle', 'streakTime', 'minute'] as const;

type LaterMessageId = (typeof LATER_MESSAGES)[number];

function laterMessage(id: LaterMessageId): { title: string; body: string } {
  return {
    title: t(`account.reminders.later.${id}.title`),
    body: t(`account.reminders.later.${id}.body`),
  };
}

function firstMessage(stats: ReminderStatsInput | null): { title: string; body: string } {
  const streak = stats?.currentStreak ?? 0;
  if (streak >= 2) {
    return {
      title: t('account.reminders.streak.title', { count: streak }),
      body: t('account.reminders.streak.body'),
    };
  }
  if (streak === 1) {
    return {
      title: t('account.reminders.yesterday.title'),
      body: t('account.reminders.yesterday.body'),
    };
  }
  return laterMessage(LATER_MESSAGES[0]);
}

export function planReminders(
  now: Date,
  prefs: ReminderPrefs,
  stats: ReminderStatsInput | null,
): PlannedReminder[] {
  if (!prefs.enabled) {
    return [];
  }

  const todayAt = new Date(now);
  todayAt.setHours(prefs.hour, prefs.minute, 0, 0);
  const skipToday = (stats?.practicedToday ?? false) || todayAt.getTime() <= now.getTime();

  const plan: PlannedReminder[] = [];
  for (let offset = skipToday ? 1 : 0; plan.length < REMINDER_DAYS; offset++) {
    const date = new Date(todayAt);
    date.setDate(todayAt.getDate() + offset);
    const message =
      plan.length === 0
        ? firstMessage(stats)
        : laterMessage(LATER_MESSAGES[(offset + 1) % LATER_MESSAGES.length]);
    plan.push({ date, ...message });
  }
  return plan;
}

export function formatReminderTime(prefs: Pick<ReminderPrefs, 'hour' | 'minute'>): string {
  return `${String(prefs.hour).padStart(2, '0')}:${String(prefs.minute).padStart(2, '0')}`;
}
