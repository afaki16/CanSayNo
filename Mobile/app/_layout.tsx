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
import { useEffect } from 'react';
import { SafeAreaProvider } from 'react-native-safe-area-context';
import 'react-native-reanimated';

import { LoadingScreen } from '@/components/ui';
import { Fonts } from '@/constants/theme';
// Barrel import: queryClient + response interceptor (401 → refresh) side-effect olarak kayıt olur.
import { useSessionValidation } from '@/hooks/useSessionValidation';
import { queryClient } from '@/lib/api';
import { useAuthStore } from '@/stores/authStore';

export default function RootLayout() {
  const isRehydrating = useAuthStore((state) => state.isRehydrating);
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);
  const rehydrate = useAuthStore((state) => state.rehydrate);

  useSessionValidation();

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

  if (isRehydrating || (!fontsLoaded && !fontError)) {
    return <LoadingScreen />;
  }

  return (
    <QueryClientProvider client={queryClient}>
      <SafeAreaProvider>
        <Stack screenOptions={{ headerShown: false }}>
          <Stack.Protected guard={!isAuthenticated}>
            <Stack.Screen name="index" />
            <Stack.Screen name="(auth)" />
          </Stack.Protected>
          <Stack.Protected guard={isAuthenticated}>
            <Stack.Screen name="(app)" />
          </Stack.Protected>
        </Stack>
        <StatusBar style="dark" />
      </SafeAreaProvider>
    </QueryClientProvider>
  );
}
