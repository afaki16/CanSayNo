// "Pratiğe Başla" akışı: backend'de oturum açar, başarıda pratik ekranına gider.
// Premium senaryoda paywall açılır; günlük limit dolduysa premium teklifli uyarı çıkar.
// Senaryo detayı ve Sonuç ekranındaki "Tekrar dene" aynı akışı kullanır.

import { router } from 'expo-router';
import { useCallback } from 'react';
import { Alert } from 'react-native';

import { useStartSession } from '@/hooks/queries/useSessions';
import { toStartSessionError } from '@/lib/api';

interface StartPracticeOptions {
  // Sonuç ekranından tekrar denerken geçmişe yeni ekran yığmamak için.
  replace?: boolean;
}

export function useStartPractice() {
  const { mutateAsync, isPending } = useStartSession();

  const startPractice = useCallback(
    async (scenarioId: string, options: StartPracticeOptions = {}) => {
      try {
        const session = await mutateAsync(scenarioId);
        const target = {
          pathname: '/(app)/scenarios/practice/[scenarioId]' as const,
          params: { scenarioId, sessionId: session.id },
        };
        if (options.replace) {
          router.replace(target);
        } else {
          router.push(target);
        }
      } catch (error) {
        const { kind, message } = toStartSessionError(error);

        if (kind === 'premiumRequired') {
          router.push('/(app)/paywall');
          return;
        }

        if (kind === 'dailyLimitReached') {
          Alert.alert('Bugünlük bu kadar 🌙', message, [
            { text: 'Tamam', style: 'cancel' },
            {
              text: "Premium'a geç",
              onPress: () => router.push('/(app)/paywall'),
            },
          ]);
          return;
        }

        Alert.alert('Pratik başlatılamadı', message);
      }
    },
    [mutateAsync],
  );

  return { startPractice, isStarting: isPending };
}
