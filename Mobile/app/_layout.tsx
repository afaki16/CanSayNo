// Kök layout: tüm route gruplarını saran Stack navigator.
// SafeAreaProvider + StatusBar global ayarları + React Query Provider burada.
// App açılışta auth state secure storage'dan rehydrate edilir ve Satoshi
// fontları yüklenir; ikisi bitene kadar LoadingScreen gösterilir.
//
// Route guard (Stack.Protected): giriş yapmış kullanıcı karşılama/auth ekranlarını,
// yapmamış kullanıcı (app) grubunu göremez. Guard değişince (login, logout, refresh
// başarısız) router erişilebilir ilk ekrana kendisi yönlendirir. Saklı oturum
// açılışta arka planda sunucuya doğrulatılır (useSessionValidation).

import { QueryClientProvider } from '@tanstack/react-query';
import { useFonts } from 'expo-font';
import { Stack } from 'expo-router';
import { StatusBar } from 'expo-status-bar';
import { useEffect, useRef, useState } from 'react';
import { KeyboardProvider } from 'react-native-keyboard-controller';
import { SafeAreaProvider } from 'react-native-safe-area-context';
import 'react-native-reanimated';

import { LoadingScreen } from '@/components/ui';
import { Fonts } from '@/constants/theme';
// Barrel import: queryClient + response interceptor (401 → refresh) side-effect olarak kayıt olur.
import { usePurchasesIdentity } from '@/hooks/usePurchasesIdentity';
import { useSessionValidation } from '@/hooks/useSessionValidation';
import { queryClient } from '@/lib/api';
import i18n, { loadStoredLanguage } from '@/lib/i18n';
import { useAuthStore } from '@/stores/authStore';

export default function RootLayout() {
  const isRehydrating = useAuthStore((state) => state.isRehydrating);
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);
  const rehydrate = useAuthStore((state) => state.rehydrate);

  useSessionValidation();
  // RevenueCat appUserID = backend User.Id; giriş/çıkışta eşitlenir.
  usePurchasesIdentity();

  // Oturum değişince (giriş, çıkış, refresh başarısız) sorguları sıfırla:
  // önceki hesabın verisi (ilerleme, istatistik) yeni hesaba görünmesin ve
  // önceki oturumdan kalan 401 hataları yeni girişte tekrar kullanılmasın.
  // clear() değil resetQueries(): ekrandaki sorgular silinmez, sıfırlanıp yeniden çekilir.
  const previousAuth = useRef(isAuthenticated);
  useEffect(() => {
    if (previousAuth.current !== isAuthenticated) {
      queryClient.resetQueries();
      previousAuth.current = isAuthenticated;
    }
  }, [isAuthenticated]);

  // Font yüklenemezse (fontError) sistem fontuyla devam et — açılışı bloklama.
  const [fontsLoaded, fontError] = useFonts({
    [Fonts.regular]: require('../assets/fonts/Satoshi-Regular.ttf'),
    [Fonts.medium]: require('../assets/fonts/Satoshi-Medium.ttf'),
    [Fonts.bold]: require('../assets/fonts/Satoshi-Bold.ttf'),
    [Fonts.black]: require('../assets/fonts/Satoshi-Black.ttf'),
  });

  useEffect(() => {
    rehydrate();
  }, [rehydrate]);

  // Kayıtlı dil tercihi ilk ekrandan önce uygulanır (yoksa cihaz dili; lib/i18n).
  const [languageReady, setLanguageReady] = useState(false);
  useEffect(() => {
    loadStoredLanguage().finally(() => setLanguageReady(true));
  }, []);

  // Dil değişince sunucudan gelen her şey (senaryo listesi, hata metinleri) o dilde yeniden çekilir.
  useEffect(() => {
    const onLanguageChanged = () => {
      queryClient.invalidateQueries();
    };
    i18n.on('languageChanged', onLanguageChanged);
    return () => i18n.off('languageChanged', onLanguageChanged);
  }, []);

  if (isRehydrating || !languageReady || (!fontsLoaded && !fontError)) {
    return <LoadingScreen />;
  }

  return (
    <QueryClientProvider client={queryClient}>
      <SafeAreaProvider>
        {/* RN'nin KeyboardAvoidingView'i Android edge-to-edge'de klavye kapanınca boşluk bırakıyordu;
            ekranlar react-native-keyboard-controller'ın sürümünü kullanır. */}
        <KeyboardProvider>
          <Stack screenOptions={{ headerShown: false }}>
            <Stack.Protected guard={!isAuthenticated}>
              <Stack.Screen name="index" />
              <Stack.Screen name="(auth)" />
            </Stack.Protected>
            <Stack.Protected guard={isAuthenticated}>
              <Stack.Screen name="(app)" />
            </Stack.Protected>
          </Stack>
        </KeyboardProvider>
        <StatusBar style="dark" />
      </SafeAreaProvider>
    </QueryClientProvider>
  );
}
