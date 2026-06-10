// Auth grubu layout: login/register ekranlarını saran Stack.

import { Stack } from 'expo-router';

export default function AuthLayout() {
  return <Stack screenOptions={{ headerShown: false }} />;
}
