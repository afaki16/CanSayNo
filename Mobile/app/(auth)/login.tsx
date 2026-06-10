// Login ekranı: gerçek form (react-hook-form + zod), mock backend ile bağlı.
// friendly playful tasarım — sıcak başlık, ikonlu input'lar, net tek aksiyon.

import { zodResolver } from '@hookform/resolvers/zod';
import { router } from 'expo-router';
import { ArrowLeft, Envelope, Eye, EyeSlash, Lock } from 'phosphor-react-native';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import {
  Alert,
  KeyboardAvoidingView,
  Platform,
  Pressable,
  ScrollView,
  StyleSheet,
  Text,
  View,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { z } from 'zod';

import { Button, FormField } from '@/components/ui';
import { Colors, FontSize, FontWeight, Spacing } from '@/constants/theme';
import { useAuth } from '@/hooks/useAuth';

const loginSchema = z.object({
  email: z.string().email('Geçerli bir e-posta girin'),
  password: z.string().min(6, 'Şifre en az 6 karakter olmalı'),
});

type LoginForm = z.infer<typeof loginSchema>;

export default function LoginScreen() {
  const { login } = useAuth();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [showPassword, setShowPassword] = useState(false);

  const {
    control,
    handleSubmit,
    formState: { isValid },
  } = useForm<LoginForm>({
    resolver: zodResolver(loginSchema),
    mode: 'onChange',
    defaultValues: { email: '', password: '' },
  });

  const onSubmit = async (data: LoginForm) => {
    setIsSubmitting(true);
    try {
      await login(data.email, data.password);
      router.replace('/(app)/(tabs)/home');
    } catch (error) {
      const message =
        error instanceof Error ? error.message : 'Bir şeyler ters gitti';
      Alert.alert('Giriş yapılamadı', message);
    } finally {
      setIsSubmitting(false);
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
            <Button
              variant="ghost"
              size="sm"
              onPress={() => router.back()}
              leftIcon={<ArrowLeft size={20} color={Colors.primary} />}
            >
              Geri
            </Button>
          </View>

          {/* Başlık */}
          <View style={styles.header}>
            <Text style={styles.title}>Tekrar hoş geldin</Text>
            <Text style={styles.subtitle}>Devam etmek için giriş yap</Text>
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
              leftIcon={<Envelope size={20} color={Colors.textMuted} />}
            />

            <FormField<LoginForm>
              name="password"
              control={control}
              label="Şifre"
              placeholder="Şifreni gir"
              secureTextEntry={!showPassword}
              autoCapitalize="none"
              leftIcon={<Lock size={20} color={Colors.textMuted} />}
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
                    <EyeSlash size={20} color={Colors.textMuted} />
                  ) : (
                    <Eye size={20} color={Colors.textMuted} />
                  )}
                </Pressable>
              }
            />
          </View>

          {/* Aksiyon */}
          <Button
            variant="primary"
            size="lg"
            fullWidth
            disabled={!isValid}
            loading={isSubmitting}
            onPress={handleSubmit(onSubmit)}
          >
            Giriş Yap
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
    paddingHorizontal: Spacing.lg,
    paddingBottom: Spacing.xl,
  },
  topBar: {
    flexDirection: 'row',
    alignItems: 'center',
    marginLeft: -Spacing.sm,
  },
  header: {
    marginTop: Spacing.md,
    marginBottom: Spacing.xl,
  },
  title: {
    fontSize: FontSize.xxl,
    fontWeight: FontWeight.bold,
    color: Colors.text,
  },
  subtitle: {
    marginTop: Spacing.sm,
    fontSize: FontSize.base,
    color: Colors.textSecondary,
  },
  form: {
    gap: Spacing.md,
    marginBottom: Spacing.xl,
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
    fontWeight: FontWeight.semibold,
    textDecorationLine: 'underline',
  },
});
