// Protected route grubu layout. Tab navigator'ı saran Stack.
// Paywall, tab bar'ın üstünde modal olarak açılır. Auth guard kök layout'ta
// (Stack.Protected): giriş yapılmadan bu gruba erişilemez.

import { Stack } from 'expo-router';

import { useReminderSync } from '@/hooks/useReminderSync';

export default function AppLayout() {
  // Günlük pratik hatırlatmalarını güncel tutar; çıkışta iptal eder.
  useReminderSync();

  return (
    <Stack screenOptions={{ headerShown: false }}>
      <Stack.Screen name="(tabs)" />
      <Stack.Screen name="paywall" options={{ presentation: 'modal' }} />
      <Stack.Screen name="account" />
      <Stack.Screen name="change-email" />
      <Stack.Screen name="notifications" />
      <Stack.Screen name="scenarios/[id]" />
      <Stack.Screen name="scenarios/detail/[scenarioId]" />
      <Stack.Screen name="scenarios/practice/[scenarioId]" />
      <Stack.Screen
        name="scenarios/result/[scenarioId]"
        options={{ gestureEnabled: false }}
      />
    </Stack>
  );
}
