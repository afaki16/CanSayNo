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
import {
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
import { useAuth } from '@/hooks/useAuth';

const loginSchema = z.object({
  email: z.string().email('Geçerli bir e-posta girin'),
  password: z.string().min(6, 'Şifre en az 6 karakter olmalı'),
});

type LoginForm = z.infer<typeof loginSchema>;

export default function LoginScreen() {
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
    } catch {
      // Hata mesajı loginError üzerinden formun altında gösteriliyor.
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
          {/* Üst bar */}
          <View style={styles.topBar}>
            <IconButton
              icon={ArrowLeft}
              bordered
              onPress={() => router.back()}
              accessibilityLabel="Geri"
            />
          </View>

          {/* Başlık */}
          <View style={styles.header}>
            <Text style={styles.title}>Tekrar hoş geldin 👋</Text>
            <Text style={styles.subtitle}>Kaldığın yerden devam et.</Text>
          </View>

          {/* Form */}
          <View style={styles.form}>
            <FormField<LoginForm>
              name="email"
              control={control}
              label="E-posta"
              placeholder="ornek@email.com"
              keyboardType="email-address"
              autoCapitalize="none"
              autoComplete="email"
              leftIcon={<EnvelopeSimple size={19} color={Colors.textMuted} />}
            />

            <FormField<LoginForm>
              name="password"
              control={control}
              label="Şifre"
              placeholder="Şifreni gir"
              secureTextEntry={!showPassword}
              autoCapitalize="none"
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
              <Text style={styles.forgotLinkText}>Şifremi unuttum</Text>
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
            Giriş yap
          </Button>

          {/* Alt link */}
          <Pressable
            onPress={() => router.replace('/register')}
            style={styles.bottomLink}
            accessibilityRole="link"
          >
            <Text style={styles.bottomLinkText}>
              Hesabın yok mu?{' '}
              <Text style={styles.bottomLinkAccent}>Kayıt Ol</Text>
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
