// Şifremi unuttum — 1. adım: e-posta al, 6 haneli kod iste.
// Backend kayıtlı olsun olmasın her zaman 200 döner (hesap varlığını
// sızdırmaz); bu yüzden başarıda her durumda kod ekranına geçilir.
// Login ile aynı tasarım dili: kare geri butonu, büyük başlık, ikonlu input.

import { zodResolver } from '@hookform/resolvers/zod';
import { router, useLocalSearchParams } from 'expo-router';
import { ArrowLeft, EnvelopeSimple } from 'phosphor-react-native';
import { useForm } from 'react-hook-form';
import {
  ScrollView,
  StyleSheet,
  View,
} from 'react-native';
import { KeyboardAvoidingView } from 'react-native-keyboard-controller';
import { SafeAreaView } from 'react-native-safe-area-context';
import { z } from 'zod';

import {
  Button,
  FormError,
  FormField,
  IconButton,
  Text,
} from '@/components/ui';
import { Colors, FontSize, FontWeight, Spacing } from '@/constants/theme';
import { usePasswordReset } from '@/hooks/usePasswordReset';

const forgotSchema = z.object({
  email: z.string().trim().email('Geçerli bir e-posta girin'),
});

type ForgotForm = z.infer<typeof forgotSchema>;

export default function ForgotPasswordScreen() {
  // Login ekranında yazılmış e-posta varsa taşınır, kullanıcı tekrar yazmaz.
  const { email: initialEmail } = useLocalSearchParams<{ email?: string }>();
  const { sendCode, isSendingCode, sendCodeError } = usePasswordReset();

  const {
    control,
    handleSubmit,
    formState: { isValid },
  } = useForm<ForgotForm>({
    resolver: zodResolver(forgotSchema),
    mode: 'onChange',
    defaultValues: { email: initialEmail ?? '' },
  });

  const onSubmit = async ({ email }: ForgotForm) => {
    try {
      await sendCode(email);
      router.push({ pathname: '/(auth)/reset-password', params: { email } });
    } catch {
      // Hata mesajı sendCodeError üzerinden formun altında gösteriliyor.
    }
  };

  return (
    <SafeAreaView style={styles.safeArea}>
      <KeyboardAvoidingView
        style={styles.flex}
        behavior="padding"
      >
        <ScrollView
          contentContainerStyle={styles.scrollContent}
          keyboardShouldPersistTaps="handled"
          showsVerticalScrollIndicator={false}
        >
          <View style={styles.topBar}>
            <IconButton
              icon={ArrowLeft}
              bordered
              onPress={() => router.back()}
              accessibilityLabel="Geri"
            />
          </View>

          <View style={styles.header}>
            <Text style={styles.title}>Şifreni mi unuttun?</Text>
            <Text style={styles.subtitle}>
              E-posta adresini yaz, sana 6 haneli bir doğrulama kodu
              gönderelim.
            </Text>
          </View>

          <View style={styles.form}>
            <FormField<ForgotForm>
              name="email"
              control={control}
              label="E-posta"
              placeholder="ornek@email.com"
              keyboardType="email-address"
              autoCapitalize="none"
              autoComplete="email"
              leftIcon={<EnvelopeSimple size={19} color={Colors.textMuted} />}
            />
          </View>

          <FormError message={sendCodeError} />

          <Button
            variant="primary"
            size="lg"
            fullWidth
            disabled={!isValid}
            loading={isSendingCode}
            onPress={handleSubmit(onSubmit)}
          >
            Kod gönder
          </Button>
        </ScrollView>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safeArea: {
    flex: 1,
    backgroundColor: Colors.background,
  },
  flex: {
    flex: 1,
  },
  scrollContent: {
    flexGrow: 1,
    paddingHorizontal: 28,
    paddingTop: Spacing.md,
    paddingBottom: Spacing.xl,
  },
  topBar: {
    flexDirection: 'row',
    alignItems: 'center',
  },
  header: {
    marginTop: 26,
    marginBottom: 30,
  },
  title: {
    fontSize: 30,
    fontWeight: FontWeight.black,
    letterSpacing: -0.9,
    color: Colors.text,
  },
  subtitle: {
    marginTop: Spacing.sm,
    fontSize: FontSize.base,
    color: Colors.textSecondary,
  },
  form: {
    gap: 18,
    marginBottom: Spacing.lg,
  },
});
