// Kök layout: tüm route gruplarını saran Stack navigator.
// SafeAreaProvider + StatusBar global ayarları + React Query Provider burada.
// App açılışta auth state secure storage'dan rehydrate edilir.

import { QueryClientProvider } from '@tanstack/react-query';
import { Stack } from 'expo-router';
import { StatusBar } from 'expo-status-bar';
import { useEffect } from 'react';
import { SafeAreaProvider } from 'react-native-safe-area-context';
import 'react-native-reanimated';

import { LoadingScreen } from '@/components/ui';
import { queryClient } from '@/lib/api/queryClient';
import { useAuthStore } from '@/stores/authStore';

export default function RootLayout() {
  const isRehydrating = useAuthStore((state) => state.isRehydrating);
  const rehydrate = useAuthStore((state) => state.rehydrate);

  useEffect(() => {
    rehydrate();
  }, [rehydrate]);

  if (isRehydrating) {
    return <LoadingScreen />;
  }

  return (
    <QueryClientProvider client={queryClient}>
      <SafeAreaProvider>
        <Stack screenOptions={{ headerShown: false }}>
          <Stack.Screen name="index" />
          <Stack.Screen name="(auth)" />
          <Stack.Screen name="(app)" />
        </Stack>
        <StatusBar style="dark" />
      </SafeAreaProvider>
    </QueryClientProvider>
  );
}
