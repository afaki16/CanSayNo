// Şifremi unuttum — 2. adım: e-postaya gelen 6 haneli kod + yeni şifre.
// Kod 10 dk geçerli, 5 hatalı denemede yanar (backend mesajı "yeni kod iste"
// der). "Tekrar gönder" backend'in 60 sn beklemesiyle aynı süre kapalı kalır.
// Başarıda tüm oturumlar kapanır; kullanıcı login ekranına yönlendirilir.

import { zodResolver } from '@hookform/resolvers/zod';
import { router, useLocalSearchParams } from 'expo-router';
import {
  ArrowLeft,
  Eye,
  EyeSlash,
  LockSimple,
  ShieldCheck,
} from 'phosphor-react-native';
import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import {
  Alert,
  KeyboardAvoidingView,
  Platform,
  Pressable,
  ScrollView,
  StyleSheet,
  View,
} from 'react-native';
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
import {
  RESEND_COOLDOWN_SECONDS,
  usePasswordReset,
} from '@/hooks/usePasswordReset';
import { newPasswordSchema } from '@/lib/validation/password';

const resetSchema = z.object({
  code: z.string().regex(/^\d{6}$/, 'Kod 6 haneli olmalı'),
  newPassword: newPasswordSchema,
});

type ResetForm = z.infer<typeof resetSchema>;

export default function ResetPasswordScreen() {
  const { email = '' } = useLocalSearchParams<{ email?: string }>();
  const {
    sendCode,
    isSendingCode,
    resetPassword,
    isResetting,
    resetError,
  } = usePasswordReset();
  const [showPassword, setShowPassword] = useState(false);
  // Bu ekrana kod az önce gönderilmiş olarak gelinir; sayaç dolu başlar.
  const [cooldown, setCooldown] = useState(RESEND_COOLDOWN_SECONDS);

  useEffect(() => {
    if (cooldown <= 0) {
      return;
    }
    const timer = setTimeout(() => setCooldown((s) => s - 1), 1000);
    return () => clearTimeout(timer);
  }, [cooldown]);

  const {
    control,
    handleSubmit,
    formState: { isValid },
  } = useForm<ResetForm>({
    resolver: zodResolver(resetSchema),
    mode: 'onChange',
    defaultValues: { code: '', newPassword: '' },
  });

  const onResend = async () => {
    try {
      await sendCode(email);
      setCooldown(RESEND_COOLDOWN_SECONDS);
    } catch {
      Alert.alert('Kod gönderilemedi', 'Lütfen biraz sonra tekrar dene.');
    }
  };

  const onSubmit = async (data: ResetForm) => {
    try {
      await resetPassword({ email, ...data });
      Alert.alert(
        'Şifren güncellendi',
        'Yeni şifrenle giriş yapabilirsin.',
        [{ text: 'Giriş yap', onPress: () => router.dismissTo('/(auth)/login') }],
        { cancelable: false },
      );
    } catch {
      // Hata mesajı resetError üzerinden formun altında gösteriliyor.
    }
  };

  return (
    <SafeAreaView style={styles.safeArea}>
      <KeyboardAvoidingView
        style={styles.flex}
        behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
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
            <Text style={styles.title}>Kodu gir</Text>
            <Text style={styles.subtitle}>
              {email ? (
                <>
                  <Text style={styles.email}>{email}</Text> adresi kayıtlıysa
                  6 haneli bir kod gönderdik. Kod 10 dakika geçerli.
                </>
              ) : (
                'E-posta adresine 6 haneli bir kod gönderdik. Kod 10 dakika geçerli.'
              )}
            </Text>
          </View>

          <View style={styles.form}>
            <FormField<ResetForm>
              name="code"
              control={control}
              label="Doğrulama kodu"
              placeholder="123456"
              keyboardType="number-pad"
              maxLength={6}
              autoComplete="one-time-code"
              textContentType="oneTimeCode"
              leftIcon={<ShieldCheck size={19} color={Colors.textMuted} />}
            />

            <FormField<ResetForm>
              name="newPassword"
              control={control}
              label="Yeni şifre"
              placeholder="En az 8 karakter"
              secureTextEntry={!showPassword}
              autoCapitalize="none"
              autoComplete="new-password"
              textContentType="newPassword"
              leftIcon={<LockSimple size={19} color={Colors.textMuted} />}
              rightIcon={
                <Pressable
                  onPress={() => setShowPassword((prev) => !prev)}
                  hitSlop={8}
                  accessibilityRole="button"
                  accessibilityLabel={
                    showPassword ? 'Şifreyi gizle' : 'Şifreyi göster'
                  }
                >
                  {showPassword ? (
                    <EyeSlash size={19} color={Colors.textMuted} />
                  ) : (
                    <Eye size={19} color={Colors.textMuted} />
                  )}
                </Pressable>
              }
            />
          </View>

          <FormError message={resetError} />

          <Button
            variant="primary"
            size="lg"
            fullWidth
            disabled={!isValid}
            loading={isResetting}
            onPress={handleSubmit(onSubmit)}
          >
            Şifreyi güncelle
          </Button>

          <Pressable
            onPress={onResend}
            disabled={cooldown > 0 || isSendingCode || !email}
            style={styles.bottomLink}
            accessibilityRole="button"
          >
            <Text style={styles.bottomLinkText}>
              Kod gelmedi mi?{' '}
              <Text
                style={
                  cooldown > 0 || isSendingCode
                    ? styles.bottomLinkMuted
                    : styles.bottomLinkAccent
                }
              >
                {cooldown > 0
                  ? `Tekrar gönder (${cooldown} sn)`
                  : 'Tekrar gönder'}
              </Text>
            </Text>
          </Pressable>
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
  email: {
    fontWeight: FontWeight.bold,
    color: Colors.text,
  },
  form: {
    gap: 18,
    marginBottom: Spacing.lg,
  },
  bottomLink: {
    marginTop: Spacing.lg,
    alignItems: 'center',
    paddingVertical: Spacing.sm,
  },
  bottomLinkText: {
    fontSize: FontSize.sm,
    color: Colors.textSecondary,
  },
  bottomLinkAccent: {
    color: Colors.primary,
    fontWeight: FontWeight.bold,
  },
  bottomLinkMuted: {
    color: Colors.textMuted,
    fontWeight: FontWeight.bold,
  },
});
