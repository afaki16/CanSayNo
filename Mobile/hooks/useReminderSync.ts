// Hatırlatma planını güncel tutar ((app) layout'ta bir kez). Plan şu durumlarda
// baştan kurulur: tercih değişince, istatistik değişince (pratik bitince
// practicedToday/currentStreak güncellenir) ve uygulama öne gelince (gün
// değişmiş olabilir). (app) grubundan çıkınca (çıkış) hepsi iptal edilir.

import { useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { AppState } from 'react-native';

import { useMyStats } from '@/hooks/queries/useStats';
import { cancelReminders, syncReminders } from '@/lib/notifications/reminders';
import { useReminderStore } from '@/stores/reminderStore';

export function useReminderSync() {
  const prefs = useReminderStore((state) => state.prefs);
  const isLoaded = useReminderStore((state) => state.isLoaded);
  const load = useReminderStore((state) => state.load);
  const { data: stats } = useMyStats();
  // Bildirim metinleri planlama anındaki dilde; dil değişince plan yeni dilde kurulur.
  const { i18n } = useTranslation();
  const language = i18n.language;

  const currentStreak = stats?.currentStreak;
  const practicedToday = stats?.practicedToday;

  useEffect(() => {
    void load();
  }, [load]);

  useEffect(() => {
    if (!isLoaded) {
      return;
    }
    const input =
      currentStreak === undefined || practicedToday === undefined
        ? null
        : { currentStreak, practicedToday };
    void syncReminders(prefs, input);

    const subscription = AppState.addEventListener('change', (state) => {
      if (state === 'active') {
        void syncReminders(prefs, input);
      }
    });
    return () => subscription.remove();
    // language: effect içinde okunmuyor ama dil değişince plan yeni dilde kurulmalı.
  }, [isLoaded, prefs, currentStreak, practicedToday, language]);

  // Çıkışta (app) grubu kalkar: önceki hesabın hatırlatmaları kalmasın.
  useEffect(() => {
    return () => {
      void cancelReminders();
    };
  }, []);
}
