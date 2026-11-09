// E-posta değişikliği. 1. adım: yeni adres + mevcut şifre → yeni adrese kod.
// 2. adım: 6 haneli kod. Başarıda tüm oturumlar kapanır; kullanıcı yeni
// adresiyle giriş yapar (yönlendirmeyi kök layout'taki route guard yapar).

import { zodResolver } from '@hookform/resolvers/zod';
import { router } from 'expo-router';
import {
  ArrowLeft,
  EnvelopeSimple,
  Eye,
  EyeSlash,
  LockSimple,
  ShieldCheck,
} from 'phosphor-react-native';
import { Fragment, useEffect, useMemo, useState } from 'react';
import { useForm } from 'react-hook-form';
import { Trans, useTranslation } from 'react-i18next';
import { Alert, Pressable, ScrollView, StyleSheet, View } from 'react-native';
import { KeyboardAvoidingView } from 'react-native-keyboard-controller';
import { SafeAreaView } from 'react-native-safe-area-context';
import { z } from 'zod';

import { Button, FormError, FormField, IconButton, Text } from '@/components/ui';
import { Colors, FontSize, FontWeight, Spacing } from '@/constants/theme';
import { useMe } from '@/hooks/queries/useProfile';
import { EMAIL_CHANGE_RESEND_SECONDS, useEmailChange } from '@/hooks/useEmailChange';
import { t } from '@/lib/i18n';

// Mesajlar şemanın kurulduğu andaki dilde (dil değişince yeniden kurulur).
const createRequestSchema = () =>
  z.object({
    newEmail: z
      .string()
      .trim()
      .min(1, t('account.changeEmail.validation.emailRequired'))
      .email(t('account.changeEmail.validation.emailInvalid')),
    currentPassword: z.string().min(1, t('account.changeEmail.validation.passwordRequired')),
  });

const createCodeSchema = () =>
  z.object({
    code: z.string().regex(/^\d{6}$/, t('account.changeEmail.validation.codeLength')),
  });

type RequestForm = z.infer<ReturnType<typeof createRequestSchema>>;
type CodeForm = z.infer<ReturnType<typeof createCodeSchema>>;

export default function ChangeEmailScreen() {
  const { t, i18n } = useTranslation();
  // Dil değişince doğrulama mesajları yeni dilde kurulsun.
  /* eslint-disable react-hooks/exhaustive-deps */
  const requestSchema = useMemo(() => createRequestSchema(), [i18n.language]);
  const codeSchema = useMemo(() => createCodeSchema(), [i18n.language]);
  /* eslint-enable react-hooks/exhaustive-deps */
  const { data: me } = useMe();
  const { sendCode, isSendingCode, sendCodeError, confirm, isConfirming, confirmError } =
    useEmailChange();

  // Kod gönderildikten sonra 2. adıma geçilir; "tekrar gönder" için şifre bellekte tutulur.
  const [pending, setPending] = useState<RequestForm | null>(null);
  const [showPassword, setShowPassword] = useState(false);
  const [cooldown, setCooldown] = useState(0);

  useEffect(() => {
    if (cooldown <= 0) {
      return;
    }
    const timer = setTimeout(() => setCooldown((s) => s - 1), 1000);
    return () => clearTimeout(timer);
  }, [cooldown]);

  const requestForm = useForm<RequestForm>({
    resolver: zodResolver(requestSchema),
    mode: 'onChange',
    defaultValues: { newEmail: '', currentPassword: '' },
  });

  const codeForm = useForm<CodeForm>({
    resolver: zodResolver(codeSchema),
    mode: 'onChange',
    defaultValues: { code: '' },
  });

  const onRequest = async (data: RequestForm) => {
    try {
      await sendCode(data);
      setPending(data);
      setCooldown(EMAIL_CHANGE_RESEND_SECONDS);
    } catch {
      // Hata sendCodeError üzerinden formun altında gösteriliyor.
    }
  };

  const onResend = async () => {
    if (!pending) {
      return;
    }
    try {
      await sendCode(pending);
      setCooldown(EMAIL_CHANGE_RESEND_SECONDS);
    } catch {
      Alert.alert(
        t('account.changeEmail.resendFailedTitle'),
        t('account.changeEmail.resendFailedBody'),
      );
    }
  };

  const onConfirm = async ({ code }: CodeForm) => {
    try {
      await confirm(code);
      // Yerel oturum temizlendi; route guard giriş ekranına götürür. Native Alert yönlendirmeden etkilenmez.
      Alert.alert(
        t('account.changeEmail.updatedTitle'),
        pending?.newEmail
          ? t('account.changeEmail.updatedBody', { email: pending.newEmail })
          : t('account.changeEmail.updatedBodyFallback'),
      );
    } catch {
      // Hata confirmError üzerinden gösteriliyor.
    }
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
              onPress={() => (pending ? setPending(null) : router.back())}
              accessibilityLabel={t('common.back')}
            />
          </View>

          {!pending ? (
            // Ayrı key: iki adımın FormField'ları aynı konumda; key olmadan React ilk adımın
            // Controller'ını yeniden kullanır ve kod alanı eski forma bağlı kalır (yazılamaz).
            <Fragment key="request">
              <View style={styles.header}>
                <Text style={styles.title}>{t('account.changeEmail.title')}</Text>
                <Text style={styles.subtitle}>
                  {me?.email ? (
                    <Trans
                      i18nKey="account.changeEmail.subtitleWithEmail"
                      values={{ email: me.email }}
                      components={{ bold: <Text style={styles.email} /> }}
                    />
                  ) : (
                    t('account.changeEmail.subtitle')
                  )}
                </Text>
              </View>

              <View style={styles.form}>
                <FormField<RequestForm>
                  name="newEmail"
                  control={requestForm.control}
                  label={t('account.changeEmail.newEmailLabel')}
                  placeholder={t('account.changeEmail.emailPlaceholder')}
                  keyboardType="email-address"
                  autoCapitalize="none"
                  autoComplete="email"
                  textContentType="emailAddress"
                  maxLength={100}
                  leftIcon={<EnvelopeSimple size={19} color={Colors.textMuted} />}
                />
                <FormField<RequestForm>
                  name="currentPassword"
                  control={requestForm.control}
                  label={t('account.changeEmail.currentPasswordLabel')}
                  placeholder={t('account.changeEmail.passwordPlaceholder')}
                  secureTextEntry={!showPassword}
                  autoCapitalize="none"
                  autoComplete="current-password"
                  textContentType="password"
                  leftIcon={<LockSimple size={19} color={Colors.textMuted} />}
                  rightIcon={
                    <Pressable
                      onPress={() => setShowPassword((prev) => !prev)}
                      hitSlop={8}
                      accessibilityRole="button"
                      accessibilityLabel={
                        showPassword
                          ? t('account.changeEmail.hidePassword')
                          : t('account.changeEmail.showPassword')
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

              <FormError message={sendCodeError} />

              <Button
                variant="primary"
                size="lg"
                fullWidth
                disabled={!requestForm.formState.isValid}
                loading={isSendingCode}
                onPress={requestForm.handleSubmit(onRequest)}
              >
                {t('account.changeEmail.sendCode')}
              </Button>
            </Fragment>
          ) : (
            <Fragment key="code">
              <View style={styles.header}>
                <Text style={styles.title}>{t('account.changeEmail.codeTitle')}</Text>
                <Text style={styles.subtitle}>
                  <Trans
                    i18nKey="account.changeEmail.codeSubtitle"
                    values={{ email: pending.newEmail }}
                    components={{ bold: <Text style={styles.email} /> }}
                  />
                </Text>
              </View>

              <View style={styles.form}>
                <FormField<CodeForm>
                  name="code"
                  control={codeForm.control}
                  label={t('account.changeEmail.codeLabel')}
                  placeholder="123456"
                  keyboardType="number-pad"
                  maxLength={6}
                  autoComplete="one-time-code"
                  textContentType="oneTimeCode"
                  leftIcon={<ShieldCheck size={19} color={Colors.textMuted} />}
                />
              </View>

              <FormError message={confirmError} />

              <Button
                variant="primary"
                size="lg"
                fullWidth
                disabled={!codeForm.formState.isValid}
                loading={isConfirming}
                onPress={codeForm.handleSubmit(onConfirm)}
              >
                {t('account.changeEmail.confirm')}
              </Button>

              <Text style={styles.note}>
                {t('account.changeEmail.note')}
              </Text>

              <Pressable
                onPress={onResend}
                disabled={cooldown > 0 || isSendingCode}
                style={styles.bottomLink}
                accessibilityRole="button"
              >
                <Text style={styles.bottomLinkText}>
                  {t('account.changeEmail.noCode')}{' '}
                  <Text
                    style={
                      cooldown > 0 || isSendingCode
                        ? styles.bottomLinkMuted
                        : styles.bottomLinkAccent
                    }
                  >
                    {cooldown > 0
                      ? t('account.changeEmail.resendIn', { seconds: cooldown })
                      : t('account.changeEmail.resend')}
                  </Text>
                </Text>
              </Pressable>
            </Fragment>
          )}
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
  note: {
    marginTop: Spacing.md,
    fontSize: FontSize.xs,
    color: Colors.textMuted,
    textAlign: 'center',
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
