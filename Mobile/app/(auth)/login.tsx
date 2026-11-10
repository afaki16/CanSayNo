// Login ekranı: react-hook-form + zod, gerçek backend'e (POST /api/Auth/login) bağlı.
// Tasarım: kare geri butonu, büyük başlık, ikonlu input'lar, tek net aksiyon.

import { zodResolver } from '@hookform/resolvers/zod';
import { router } from 'expo-router';
import {
  ArrowLeft,
  EnvelopeSimple,
  Eye,
  EyeSlash,
  LockSimple,
} from 'phosphor-react-native';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { useTranslation } from 'react-i18next';
import {
  Pressable,
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
import { useAuth } from '@/hooks/useAuth';
import { rememberPendingCredentials } from '@/hooks/useEmailVerification';
import { API_ERROR_CODE, ApiError, authService } from '@/lib/api';
import { t as translate } from '@/lib/i18n';

// Mesajlar fonksiyon: doğrulama anındaki dil kullanılır.
const loginSchema = z.object({
  email: z.string().email({ error: () => translate('auth.validation.email') }),
  password: z
    .string()
    .min(6, { error: () => translate('auth.validation.passwordMin6') }),
});

type LoginForm = z.infer<typeof loginSchema>;

export default function LoginScreen() {
  const { t } = useTranslation();
  const { login, isLoggingIn, loginError } = useAuth();
  const [showPassword, setShowPassword] = useState(false);

  const {
    control,
    handleSubmit,
    getValues,
    formState: { isValid },
  } = useForm<LoginForm>({
    resolver: zodResolver(loginSchema),
    mode: 'onChange',
    defaultValues: { email: '', password: '' },
  });

  const onSubmit = async (data: LoginForm) => {
    try {
      await login(data.email, data.password);
      router.replace('/(app)/(tabs)/home');
    } catch (error) {
      // Doğrulanmamış hesap: yeni kod gönder ve kod ekranına geç (şifre
      // bellekte kalır, doğrulayınca otomatik giriş). Diğer hatalar formun
      // altında loginError ile gösteriliyor.
      if (error instanceof ApiError && error.code === API_ERROR_CODE.EMAIL_NOT_CONFIRMED) {
        rememberPendingCredentials(data.email, data.password);
        authService.resendVerification({ email: data.email.trim(), tenantId: 0 }).catch(() => {});
        router.push({ pathname: '/(auth)/verify-email', params: { email: data.email.trim() } });
      }
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
          {/* Üst bar */}
          <View style={styles.topBar}>
            <IconButton
              icon={ArrowLeft}
              bordered
              onPress={() => router.back()}
              accessibilityLabel={t('common.back')}
            />
          </View>

          {/* Başlık */}
          <View style={styles.header}>
            <Text style={styles.title}>{t('auth.login.title')}</Text>
            <Text style={styles.subtitle}>{t('auth.login.subtitle')}</Text>
          </View>

          {/* Form */}
          <View style={styles.form}>
            <FormField<LoginForm>
              name="email"
              control={control}
              label={t('auth.fields.email')}
              placeholder={t('auth.fields.emailPlaceholder')}
              keyboardType="email-address"
              autoCapitalize="none"
              autoComplete="email"
              leftIcon={<EnvelopeSimple size={19} color={Colors.textMuted} />}
            />

            <FormField<LoginForm>
              name="password"
              control={control}
              label={t('auth.fields.password')}
              placeholder={t('auth.fields.passwordPlaceholder')}
              secureTextEntry={!showPassword}
              autoCapitalize="none"
              leftIcon={<LockSimple size={19} color={Colors.textMuted} />}
              rightIcon={
                <Pressable
                  onPress={() => setShowPassword((prev) => !prev)}
                  hitSlop={8}
                  accessibilityRole="button"
                  accessibilityLabel={
                    showPassword
                      ? t('auth.fields.hidePassword')
                      : t('auth.fields.showPassword')
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

            <Pressable
              onPress={() =>
                router.push({
                  pathname: '/(auth)/forgot-password',
                  params: { email: getValues('email').trim() },
                })
              }
              hitSlop={8}
              style={styles.forgotLink}
              accessibilityRole="link"
            >
              <Text style={styles.forgotLinkText}>{t('auth.login.forgotPassword')}</Text>
            </Pressable>
          </View>

          {/* Aksiyon */}
          <FormError message={loginError} />

          <Button
            variant="primary"
            size="lg"
            fullWidth
            disabled={!isValid}
            loading={isLoggingIn}
            onPress={handleSubmit(onSubmit)}
          >
            {t('auth.login.submit')}
          </Button>

          {/* Alt link */}
          <Pressable
            onPress={() => router.replace('/register')}
            style={styles.bottomLink}
            accessibilityRole="link"
          >
            <Text style={styles.bottomLinkText}>
              {t('auth.login.noAccount')}{' '}
              <Text style={styles.bottomLinkAccent}>
                {t('auth.login.signUp')}
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
  form: {
    gap: 18,
    marginBottom: Spacing.lg,
  },
  forgotLink: {
    alignSelf: 'flex-end',
  },
  forgotLinkText: {
    fontSize: FontSize.sm,
    fontWeight: FontWeight.semibold,
    color: Colors.primary,
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
});
