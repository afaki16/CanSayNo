// Login ekranı: react-hook-form + zod, gerçek backend'e (POST /api/Auth/login) bağlı.
// Tasarım: kare geri butonu, büyük başlık, ikonlu input'lar, tek net aksiyon.

import { zodResolver } from '@hookform/resolvers/zod';
import { router } from 'expo-router';
import { StatusBar } from 'expo-status-bar';
import { LinearGradient } from 'expo-linear-gradient';
import {
  ArrowLeft,
  EnvelopeSimple,
  Eye,
  EyeSlash,
  HandPalm,
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
import Animated, { FadeInDown } from 'react-native-reanimated';
import { SafeAreaView, useSafeAreaInsets } from 'react-native-safe-area-context';
import { z } from 'zod';

import {
  BrandMark,
  Button,
  FormError,
  FormField,
  Text,
} from '@/components/ui';
import { Colors, FontSize, FontWeight, Gradients, Shadow, Spacing } from '@/constants/theme';
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
  const insets = useSafeAreaInsets();

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
    <SafeAreaView style={styles.safeArea} edges={['bottom']}>
      <StatusBar style="light" />
      <KeyboardAvoidingView
        style={styles.flex}
        behavior="padding"
      >
        <ScrollView
          contentContainerStyle={styles.scrollContent}
          keyboardShouldPersistTaps="handled"
          showsVerticalScrollIndicator={false}
        >
          {/* Marka başlığı: geçişli zemin + ürünün özü (ısrar → kibar bir "hayır") */}
          <LinearGradient
            colors={Gradients.brand.colors}
            start={Gradients.brand.start}
            end={Gradients.brand.end}
            style={[styles.hero, { paddingTop: insets.top + Spacing.md }]}
          >
            <View style={styles.topBar}>
              <Pressable
                onPress={() => router.back()}
                style={styles.backButton}
                hitSlop={8}
                accessibilityRole="button"
                accessibilityLabel={t('common.back')}
              >
                <ArrowLeft size={20} color="#FFFFFF" weight="bold" />
              </Pressable>
              <View style={styles.logoChip}>
                <BrandMark size={26} />
              </View>
            </View>

            <View
              style={styles.preview}
              accessibilityElementsHidden
              importantForAccessibility="no-hide-descendants"
            >
              <Animated.View
                entering={FadeInDown.delay(150).duration(450)}
                style={[styles.previewBubble, styles.previewAsk]}
              >
                <Text style={styles.previewAskText}>{t('auth.login.previewAsk')}</Text>
              </Animated.View>
              <Animated.View
                entering={FadeInDown.delay(750).duration(450)}
                style={[styles.previewBubble, styles.previewReply]}
              >
                <HandPalm size={15} color={Colors.secondary} weight="fill" />
                <Text style={styles.previewReplyText}>{t('auth.login.previewReply')}</Text>
              </Animated.View>
            </View>

            <Text style={styles.title}>{t('auth.login.title')}</Text>
            <Text style={styles.subtitle}>{t('auth.login.subtitle')}</Text>
          </LinearGradient>

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
          <View style={styles.actions}>
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
          </View>
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
    paddingBottom: Spacing.xl,
  },
  hero: {
    paddingHorizontal: 28,
    paddingBottom: 30,
    borderBottomLeftRadius: 32,
    borderBottomRightRadius: 32,
  },
  topBar: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
  },
  backButton: {
    width: 44,
    height: 44,
    borderRadius: 14,
    backgroundColor: 'rgba(255, 255, 255, 0.18)',
    borderWidth: 1,
    borderColor: 'rgba(255, 255, 255, 0.28)',
    alignItems: 'center',
    justifyContent: 'center',
  },
  logoChip: {
    padding: 3,
    borderRadius: 11,
    backgroundColor: 'rgba(255, 255, 255, 0.9)',
  },
  preview: {
    marginTop: 22,
    marginBottom: 22,
    gap: 10,
  },
  previewBubble: {
    maxWidth: '82%',
    paddingHorizontal: 14,
    paddingVertical: 10,
    borderRadius: 18,
  },
  previewAsk: {
    alignSelf: 'flex-start',
    borderBottomLeftRadius: 5,
    backgroundColor: 'rgba(255, 255, 255, 0.16)',
    borderWidth: 1,
    borderColor: 'rgba(255, 255, 255, 0.22)',
  },
  previewAskText: {
    fontSize: FontSize.sm,
    color: 'rgba(255, 255, 255, 0.92)',
  },
  previewReply: {
    alignSelf: 'flex-end',
    flexDirection: 'row',
    alignItems: 'center',
    gap: 7,
    borderBottomRightRadius: 5,
    backgroundColor: Colors.surface,
    ...Shadow.lg,
  },
  previewReplyText: {
    flexShrink: 1,
    fontSize: FontSize.sm,
    fontWeight: FontWeight.bold,
    color: Colors.text,
  },
  title: {
    fontSize: 30,
    fontWeight: FontWeight.black,
    letterSpacing: -0.9,
    color: '#FFFFFF',
  },
  subtitle: {
    marginTop: 6,
    fontSize: FontSize.base,
    color: 'rgba(255, 255, 255, 0.82)',
  },
  form: {
    gap: 18,
    marginTop: 28,
    marginBottom: Spacing.lg,
    paddingHorizontal: 28,
  },
  actions: {
    paddingHorizontal: 28,
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
