// Karşılama ekranı: uygulamanın entry point'i.
// Friendly playful tasarım — hero illustration + sıcak başlık + tek net aksiyon.

import { router } from 'expo-router';
import { Pressable, StyleSheet, Text, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';

import { Button, IllustrationPlaceholder } from '@/components/ui';
import {
  Colors,
  FontSize,
  FontWeight,
  Spacing,
} from '@/constants/theme';

export default function WelcomeScreen() {
  return (
    <SafeAreaView style={styles.safeArea}>
      <View style={styles.container}>
        {/* Hero — ekranın üst yarısı, ortalanmış illustration */}
        <View style={styles.hero}>
          <IllustrationPlaceholder size={240} />
        </View>

        {/* Başlık ve alt yazı */}
        <View style={styles.textBlock}>
          <Text style={styles.title}>Hayır demeyi öğren</Text>
          <Text style={styles.subtitle}>
            Hayatı dolu dolu yaşamak için sınırlarını çiz
          </Text>
        </View>

        {/* Alt aksiyon alanı */}
        <View style={styles.actions}>
          <Button
            variant="primary"
            size="lg"
            fullWidth
            onPress={() => router.push('/register')}
          >
            Hemen Başla
          </Button>

          <Pressable
            onPress={() => router.push('/login')}
            style={styles.loginLinkWrapper}
            accessibilityRole="link"
          >
            <Text style={styles.loginLinkText}>
              Zaten hesabın var mı?{' '}
              <Text style={styles.loginLinkAccent}>Giriş Yap</Text>
            </Text>
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
    paddingHorizontal: Spacing.lg,
    paddingBottom: Spacing.lg,
  },
  hero: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
  },
  textBlock: {
    alignItems: 'center',
    paddingHorizontal: Spacing.sm,
    marginBottom: Spacing.xl,
  },
  title: {
    fontSize: FontSize.hero,
    fontWeight: FontWeight.bold,
    color: Colors.primary,
    textAlign: 'center',
    letterSpacing: -0.5,
  },
  subtitle: {
    marginTop: Spacing.md,
    fontSize: FontSize.base,
    color: Colors.textSecondary,
    textAlign: 'center',
    lineHeight: FontSize.base * 1.5,
  },
  actions: {
    gap: Spacing.md,
  },
  loginLinkWrapper: {
    alignItems: 'center',
    paddingVertical: Spacing.sm,
  },
  loginLinkText: {
    fontSize: FontSize.sm,
    color: Colors.textSecondary,
  },
  loginLinkAccent: {
    color: Colors.primary,
    fontWeight: FontWeight.semibold,
    textDecorationLine: 'underline',
  },
});
