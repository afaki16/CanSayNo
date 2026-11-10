// Kayıt sonrası doğrulama: e-postaya gelen 6 haneli kod girilir. Kod 10 dk
// geçerli, 5 hatalı denemede yanar; "Tekrar gönder" backend'in 60 sn
// beklemesiyle aynı süre kapalı kalır. Başarıda, şifre bellekte duruyorsa
// (kayıt/giriş formundan) otomatik giriş yapılır; yoksa login ekranına dönülür.
// Giriş ekranı doğrulanmamış hesapta buraya yeni kodla yönlendirir.

import { zodResolver } from '@hookform/resolvers/zod';
import { router, useLocalSearchParams } from 'expo-router';
import { ArrowLeft, EnvelopeSimple, ShieldCheck } from 'phosphor-react-native';
import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { Trans, useTranslation } from 'react-i18next';
import { Alert, Pressable, ScrollView, StyleSheet, View } from 'react-native';
import { KeyboardAvoidingView } from 'react-native-keyboard-controller';
import { SafeAreaView } from 'react-native-safe-area-context';
import { z } from 'zod';

import { Button, FormError, FormField, IconButton, Text } from '@/components/ui';
import { Colors, FontSize, FontWeight, Radius, Spacing } from '@/constants/theme';
import { useAuth } from '@/hooks/useAuth';
import { takePendingPassword, useEmailVerification } from '@/hooks/useEmailVerification';
import { RESEND_COOLDOWN_SECONDS } from '@/hooks/usePasswordReset';
import { t as translate } from '@/lib/i18n';

const verifySchema = z.object({
  // Mesaj fonksiyon: doğrulama anındaki dil kullanılır.
  code: z
    .string()
    .regex(/^\d{6}$/, { error: () => translate('auth.validation.codeLength') }),
});

type VerifyForm = z.infer<typeof verifySchema>;

export default function VerifyEmailScreen() {
  const { t } = useTranslation();
  const { email = '' } = useLocalSearchParams<{ email?: string }>();
  const { verify, isVerifying, verifyError, resend, isResending } = useEmailVerification();
  const { login } = useAuth();
  const [isSigningIn, setIsSigningIn] = useState(false);
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
  } = useForm<VerifyForm>({
    resolver: zodResolver(verifySchema),
    mode: 'onChange',
    defaultValues: { code: '' },
  });

  const goToLogin = () => router.replace('/(auth)/login');

  const onResend = async () => {
    try {
      await resend(email);
      setCooldown(RESEND_COOLDOWN_SECONDS);
    } catch {
      Alert.alert(
        t('auth.verifyEmail.resendFailedTitle'),
        t('auth.verifyEmail.resendFailedMessage'),
      );
    }
  };

  const onSubmit = async (data: VerifyForm) => {
    try {
      await verify({ email, code: data.code });
    } catch {
      // Hata mesajı verifyError üzerinden formun altında gösteriliyor.
      return;
    }

    const password = takePendingPassword(email);
    if (password) {
      setIsSigningIn(true);
      try {
        await login(email, password);
        router.replace('/(app)/(tabs)/home');
        return;
      } catch {
        // Otomatik giriş olmadıysa (ağ vb.) kullanıcı kendisi girer.
      } finally {
        setIsSigningIn(false);
      }
    }

    Alert.alert(
      t('auth.verifyEmail.successTitle'),
      t('auth.verifyEmail.successMessage'),
      [{ text: t('auth.verifyEmail.successAction'), onPress: goToLogin }],
      { cancelable: false },
    );
  };

  return (
    <SafeAreaView style={styles.safeArea}>
      <KeyboardAvoidingView style={styles.flex} behavior="padding">
        <ScrollView
          contentContainerStyle={styles.scrollContent}
          keyboardShouldPersistTaps="handled"
          showsVerticalScrollIndicator={false}
        >
          <View style={styles.topBar}>
            <IconButton
              icon={ArrowLeft}
              bordered
              onPress={goToLogin}
              accessibilityLabel={t('common.back')}
            />
          </View>

          <View style={styles.iconCircle}>
            <EnvelopeSimple size={44} color={Colors.primary} weight="fill" />
          </View>

          <View style={styles.header}>
            <Text style={styles.title}>{t('auth.verifyEmail.title')}</Text>
            <Text style={styles.subtitle}>
              {email ? (
                <Trans
                  i18nKey="auth.verifyEmail.subtitleWithEmail"
                  values={{ email }}
                  components={{ bold: <Text style={styles.email} /> }}
                />
              ) : (
                t('auth.verifyEmail.subtitle')
              )}
            </Text>
          </View>

          <FormField<VerifyForm>
            name="code"
            control={control}
            label={t('auth.fields.code')}
            placeholder="123456"
            keyboardType="number-pad"
            maxLength={6}
            autoComplete="one-time-code"
            textContentType="oneTimeCode"
            leftIcon={<ShieldCheck size={19} color={Colors.textMuted} />}
          />

          <Text style={styles.hint}>{t('auth.verifyEmail.hint')}</Text>

          <FormError message={verifyError} />

          <Button
            variant="primary"
            size="lg"
            fullWidth
            disabled={!isValid || !email}
            loading={isVerifying || isSigningIn}
            onPress={handleSubmit(onSubmit)}
          >
            {t('auth.verifyEmail.submit')}
          </Button>

          <Pressable
            onPress={onResend}
            disabled={cooldown > 0 || isResending || !email}
            style={styles.bottomLink}
            accessibilityRole="button"
          >
            <Text style={styles.bottomLinkText}>
              {t('auth.verifyEmail.noCode')}{' '}
              <Text
                style={
                  cooldown > 0 || isResending
                    ? styles.bottomLinkMuted
                    : styles.bottomLinkAccent
                }
              >
                {cooldown > 0
                  ? t('auth.verifyEmail.resendCountdown', { seconds: cooldown })
                  : t('auth.verifyEmail.resend')}
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
  iconCircle: {
    marginTop: 26,
    width: 84,
    height: 84,
    borderRadius: Radius.full,
    backgroundColor: Colors.primarySoft,
    alignItems: 'center',
    justifyContent: 'center',
  },
  header: {
    marginTop: 22,
    marginBottom: 26,
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
    lineHeight: FontSize.base * 1.45,
  },
  email: {
    fontWeight: FontWeight.bold,
    color: Colors.text,
  },
  hint: {
    marginTop: Spacing.sm,
    marginBottom: Spacing.lg,
    fontSize: FontSize.sm,
    color: Colors.textMuted,
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
