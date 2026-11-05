// Günlük pratik hatırlatmasının PLANI (saf fonksiyon, yan etkisiz).
// Zamanlamayı lib/notifications/reminders.ts yapar.
//
// Kural: önümüzdeki REMINDER_DAYS gün için günde bir hatırlatma. Bugün pratik
// yapıldıysa ya da saat geçtiyse bugünkü atlanır. Uygulama açıldıkça plan
// yenilenir; kullanıcı uygulamayı bir hafta açmazsa hatırlatmalar kendiliğinden
// susar (ısrarcı olmasın diye bilerek).

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
// açılmadıysa veri yok), bu yüzden sayı içermezler.
const LATER_MESSAGES: { title: string; body: string }[] = [
  { title: 'Bugünkü pratiğin hazır', body: '5 dakikanı ayır, bir sınır daha koy. 💪' },
  { title: 'Hayır demek bir kas', body: 'Kısa bir senaryoyla bugün de çalıştır.' },
  { title: 'Seri zamanı 🔥', body: 'Bugünkü pratiğini yaptın mı? Seni bir senaryo bekliyor.' },
  { title: 'Bir dakikan var mı?', body: 'Zor bir isteğe nazikçe "hayır" demeyi dene.' },
];

function firstMessage(stats: ReminderStatsInput | null): { title: string; body: string } {
  const streak = stats?.currentStreak ?? 0;
  if (streak >= 2) {
    return {
      title: `Serin ${streak} gün 🔥`,
      body: 'Bugünkü pratiğini yapıp serini koru.',
    };
  }
  if (streak === 1) {
    return { title: 'Dün harikaydın 🔥', body: 'Bugün de yaparsan serin başlıyor.' };
  }
  return LATER_MESSAGES[0];
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
      plan.length === 0 ? firstMessage(stats) : LATER_MESSAGES[(offset + 1) % LATER_MESSAGES.length];
    plan.push({ date, ...message });
  }
  return plan;
}

export function formatReminderTime(prefs: Pick<ReminderPrefs, 'hour' | 'minute'>): string {
  return `${String(prefs.hour).padStart(2, '0')}:${String(prefs.minute).padStart(2, '0')}`;
}
