// Ana tab navigator: Anasayfa / Senaryolar / Profil.
// Active ikon Phosphor "fill" weight (dolu), inactive "regular" (ince çizgi) —
// oyunsu hissin önemli bir parçası. Beyaz tab bar, üstte ince kenar çizgisi.

import { Tabs } from 'expo-router';
import { CardsThree, House, User } from 'phosphor-react-native';
import { Platform } from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';

import { Colors, Fonts } from '@/constants/theme';

export default function TabsLayout() {
  const insets = useSafeAreaInsets();
  // Android'de tab bar daha kısa; iOS'ta home indicator için inset eklenir.
  const baseHeight = Platform.OS === 'android' ? 62 : 64;

  return (
    <Tabs
      screenOptions={{
        headerShown: false,
        tabBarActiveTintColor: Colors.primary,
        tabBarInactiveTintColor: Colors.textMuted,
        tabBarStyle: {
          backgroundColor: Colors.surface,
          height: baseHeight + insets.bottom,
          paddingTop: 10,
          paddingBottom: insets.bottom > 0 ? insets.bottom : 10,
          borderTopWidth: 1,
          borderTopColor: Colors.divider,
          elevation: 0,
        },
        tabBarLabelStyle: {
          fontSize: 11,
          fontFamily: Fonts.bold,
          marginTop: 2,
        },
      }}
    >
      <Tabs.Screen
        name="home"
        options={{
          title: 'Ana Sayfa',
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
            <CardsThree size={24} color={color} weight={focused ? 'fill' : 'regular'} />
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
