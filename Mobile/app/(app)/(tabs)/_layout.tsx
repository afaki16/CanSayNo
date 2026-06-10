// Ana tab navigator: Anasayfa / Senaryolar / Profil.
// Active ikon Phosphor "fill" weight (dolu), inactive "regular" (ince çizgi) —
// oyunsu hissin önemli bir parçası. Tab bar straight bottom, üstte hafif gölge.

import { Tabs } from 'expo-router';
import { ChatCircleText, House, User } from 'phosphor-react-native';
import { Platform } from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';

import { Colors, FontSize, Shadow } from '@/constants/theme';

export default function TabsLayout() {
  const insets = useSafeAreaInsets();
  // Android'de tab bar daha kısa; iOS'ta home indicator için inset eklenir.
  const baseHeight = Platform.OS === 'android' ? 56 : 60;

  return (
    <Tabs
      screenOptions={{
        headerShown: false,
        tabBarActiveTintColor: Colors.primary,
        tabBarInactiveTintColor: Colors.textMuted,
        tabBarStyle: {
          backgroundColor: Colors.surface,
          height: baseHeight + insets.bottom,
          paddingTop: 6,
          paddingBottom: insets.bottom > 0 ? insets.bottom : 8,
          borderTopWidth: 0,
          // Yukarı doğru hafif gölge (Shadow.md tabanlı, offset yukarı).
          ...Shadow.md,
          shadowOffset: { width: 0, height: -2 },
        },
        tabBarLabelStyle: {
          fontSize: FontSize.xs,
          marginTop: 2,
        },
      }}
    >
      <Tabs.Screen
        name="home"
        options={{
          title: 'Anasayfa',
          tabBarIcon: ({ color, focused }) => (
            <House size={24} color={color} weight={focused ? 'fill' : 'regular'} />
          ),
        }}
      />
      <Tabs.Screen
        name="scenarios"
        options={{
          title: 'Senaryolar',
          tabBarIcon: ({ color, focused }) => (
            <ChatCircleText
              size={24}
              color={color}
              weight={focused ? 'fill' : 'regular'}
            />
          ),
        }}
      />
      <Tabs.Screen
        name="profile"
        options={{
          title: 'Profil',
          tabBarIcon: ({ color, focused }) => (
            <User size={24} color={color} weight={focused ? 'fill' : 'regular'} />
          ),
        }}
      />
    </Tabs>
  );
}
