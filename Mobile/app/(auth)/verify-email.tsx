// Kayıt sonrası ekranı: hesap açıldı ama e-posta doğrulanana kadar giriş
// yapılamaz (backend "pendingVerification" durumundaki hesapları reddediyor).
// Login/register ile aynı tasarım dili — aynı spacing, tipografi ve renkler.

import { router, useLocalSearchParams } from 'expo-router';
import { EnvelopeSimple } from 'phosphor-react-native';
import { StyleSheet, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';

import { Button, Text } from '@/components/ui';
import {
  Colors,
  FontSize,
  FontWeight,
  Radius,
  Spacing,
} from '@/constants/theme';

export default function VerifyEmailScreen() {
  const { email } = useLocalSearchParams<{ email?: string }>();

  return (
    <SafeAreaView style={styles.safeArea}>
      <View style={styles.container}>
        <View style={styles.content}>
          <View style={styles.iconCircle}>
            <EnvelopeSimple size={56} color={Colors.primary} weight="fill" />
          </View>

          <Text style={styles.title}>E-postanı kontrol et</Text>

          <Text style={styles.description}>
            {email ? (
              <>
                <Text style={styles.email}>{email}</Text> adresine bir doğrulama
                bağlantısı gönderdik. Bağlantıya tıkladıktan sonra giriş
                yapabilirsin.
              </>
            ) : (
              'E-posta adresine bir doğrulama bağlantısı gönderdik. Bağlantıya tıkladıktan sonra giriş yapabilirsin.'
            )}
          </Text>

          <Text style={styles.hint}>
            Gelen kutunda göremiyorsan spam klasörüne bakmayı unutma.
          </Text>
        </View>

        <Button
          variant="primary"
          size="lg"
          fullWidth
          onPress={() => router.replace('/(auth)/login')}
        >
          Giriş yap
        </Button>
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
    paddingBottom: Spacing.xl,
  },
  content: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
    paddingHorizontal: Spacing.sm,
  },
  iconCircle: {
    width: 112,
    height: 112,
    borderRadius: Radius.full,
    backgroundColor: Colors.primarySoft,
    alignItems: 'center',
    justifyContent: 'center',
    marginBottom: Spacing.xl,
  },
  title: {
    fontSize: FontSize.xxl,
    fontWeight: FontWeight.black,
    letterSpacing: -0.8,
    color: Colors.text,
    textAlign: 'center',
  },
  description: {
    marginTop: Spacing.md,
    fontSize: FontSize.base,
    color: Colors.textSecondary,
    textAlign: 'center',
    lineHeight: FontSize.base * 1.5,
  },
  email: {
    color: Colors.text,
    fontWeight: FontWeight.semibold,
  },
  hint: {
    marginTop: Spacing.lg,
    fontSize: FontSize.sm,
    color: Colors.textMuted,
    textAlign: 'center',
  },
});
