// Protected route grubu layout. Tab navigator'ı saran Stack.
// Paywall, tab bar'ın üstünde modal olarak açılır. Auth guard ileride eklenecek.

import { Stack } from 'expo-router';

export default function AppLayout() {
  return (
    <Stack screenOptions={{ headerShown: false }}>
      <Stack.Screen name="(tabs)" />
      <Stack.Screen name="paywall" options={{ presentation: 'modal' }} />
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
