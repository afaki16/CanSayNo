// "Pratiğe Başla" akışı: backend'de oturum açar, başarıda pratik ekranına gider.
// Premium senaryoda paywall açılır; günlük limit dolduysa premium teklifli uyarı çıkar.
// Senaryo detayı ve Sonuç ekranındaki "Tekrar dene" aynı akışı kullanır.

import { router } from 'expo-router';
import { useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Alert } from 'react-native';

import { useStartSession } from '@/hooks/queries/useSessions';
import { toStartSessionError } from '@/lib/api';

interface StartPracticeOptions {
  // Sonuç ekranından tekrar denerken geçmişe yeni ekran yığmamak için.
  replace?: boolean;
}

export function useStartPractice() {
  const { t } = useTranslation();
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
          Alert.alert(t('practice.startAlert.limitTitle'), message, [
            { text: t('common.ok'), style: 'cancel' },
            {
              text: t('practice.startAlert.goPremium'),
              onPress: () => router.push('/(app)/paywall'),
            },
          ]);
          return;
        }

        if (kind === 'fairUseLimitReached') {
          Alert.alert(t('practice.startAlert.limitTitle'), message);
          return;
        }

        Alert.alert(t('practice.startAlert.failedTitle'), message);
      }
    },
    [mutateAsync, t],
  );

  return { startPractice, isStarting: isPending };
}
