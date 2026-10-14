// Karşılama ekranı: uygulamanın entry point'i.
// Üstte marka, ortada "NO" balonu + başlık, altta tek net aksiyon.

import { router } from 'expo-router';
import { Pressable, StyleSheet, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';

import { BrandMark, Button, NoBubble, Text } from '@/components/ui';
import { Colors, FontSize, FontWeight, Spacing } from '@/constants/theme';

export default function WelcomeScreen() {
  return (
    <SafeAreaView style={styles.safeArea}>
      <View style={styles.container}>
        <View style={styles.brand}>
          <BrandMark withName />
        </View>

        <View style={styles.hero}>
          <NoBubble />
          <View style={styles.textBlock}>
            <Text style={styles.title}>Hayır demek{'\n'}bir beceridir.</Text>
            <Text style={styles.subtitle}>
              Güvenli senaryolarda pratik yap, gerçek hayatta sınırını rahatça
              koru.
            </Text>
          </View>
        </View>

        <View style={styles.actions}>
          <Button
            variant="primary"
            size="lg"
            fullWidth
            onPress={() => router.push('/register')}
          >
            Hadi başlayalım
          </Button>

          <Pressable
            onPress={() => router.push('/login')}
            style={styles.loginLink}
            accessibilityRole="link"
          >
            <Text style={styles.loginLinkText}>Zaten hesabım var</Text>
          </Pressable>
        </View>
      </View>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safeArea: {
    flex: 1,
    backgroundColor: Colors.background,
  },
  container: {
    flex: 1,
    justifyContent: 'space-between',
    paddingHorizontal: 30,
    paddingTop: Spacing.lg,
    paddingBottom: Spacing.lg,
  },
  brand: {
    alignItems: 'center',
  },
  hero: {
    alignItems: 'center',
    gap: 30,
  },
  textBlock: {
    alignItems: 'center',
  },
  title: {
    fontSize: FontSize.hero,
    fontWeight: FontWeight.black,
    color: Colors.text,
    textAlign: 'center',
    letterSpacing: -1,
    lineHeight: 35,
    marginBottom: 12,
  },
  subtitle: {
    fontSize: FontSize.base,
    color: Colors.textSecondary,
    textAlign: 'center',
    lineHeight: 24,
    paddingHorizontal: 6,
  },
  actions: {
    gap: 11,
  },
  loginLink: {
    alignItems: 'center',
    paddingVertical: 13,
  },
  loginLinkText: {
    fontSize: 15,
    fontWeight: FontWeight.semibold,
    color: Colors.textSecondary,
  },
});
