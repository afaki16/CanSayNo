// Hatırlatma tercihleri (cihaza özel; hesaba bağlı değil). Kök/(app) layout
// ve Bildirimler ekranı aynı durumu paylaşır.

import { create } from 'zustand';

import { DEFAULT_REMINDER_PREFS, type ReminderPrefs } from '@/lib/notifications/reminderPlan';
import { secureStorage, STORAGE_KEYS } from '@/lib/storage/secureStorage';

interface ReminderState {
  prefs: ReminderPrefs;
  isLoaded: boolean;
  load: () => Promise<void>;
  setPrefs: (prefs: ReminderPrefs) => Promise<void>;
}

function isValidPrefs(value: unknown): value is ReminderPrefs {
  const v = value as ReminderPrefs | null;
  return (
    !!v &&
    typeof v.enabled === 'boolean' &&
    Number.isInteger(v.hour) &&
    v.hour >= 0 &&
    v.hour <= 23 &&
    Number.isInteger(v.minute) &&
    v.minute >= 0 &&
    v.minute <= 59
  );
}

export const useReminderStore = create<ReminderState>()((set) => ({
  prefs: DEFAULT_REMINDER_PREFS,
  isLoaded: false,

  load: async () => {
    const stored = await secureStorage.getJSON<ReminderPrefs>(STORAGE_KEYS.REMINDER_PREFS);
    set({ prefs: isValidPrefs(stored) ? stored : DEFAULT_REMINDER_PREFS, isLoaded: true });
  },

  setPrefs: async (prefs) => {
    set({ prefs });
    await secureStorage.setJSON(STORAGE_KEYS.REMINDER_PREFS, prefs);
  },
}));
