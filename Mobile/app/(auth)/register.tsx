// Register ekranı: react-hook-form + zod, gerçek backend'e (POST /api/Auth/register) bağlı.
// Kayıt sonrası otomatik login YOK — hesap e-posta doğrulanana kadar giriş yapamaz.
// Tasarım login ile tutarlı. Tek şifre alanı + göz ikonu; backend confirmPassword
// istediği için aynı şifre iki alana da gönderilir.

import { zodResolver } from '@hookform/resolvers/zod';
import { router } from 'expo-router';
import {
  ArrowLeft,
  EnvelopeSimple,
  Eye,
  EyeSlash,
  LockSimple,
  User,
} from 'phosphor-react-native';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
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
import { openLegalPage } from '@/lib/utils/openLegalPage';
import { newPasswordSchema } from '@/lib/validation/password';

const registerSchema = z.object({
  firstName: z.string().min(2, 'Ad en az 2 karakter olmalı'),
  lastName: z.string().min(2, 'Soyad en az 2 karakter olmalı'),
  email: z.string().email('Geçerli bir e-posta girin'),
  password: newPasswordSchema,
});

type RegisterForm = z.infer<typeof registerSchema>;

export default function RegisterScreen() {
  const { register, isRegistering, registerError } = useAuth();
  const [showPassword, setShowPassword] = useState(false);

  const {
    control,
    handleSubmit,
    formState: { isValid },
  } = useForm<RegisterForm>({
    resolver: zodResolver(registerSchema),
    mode: 'onChange',
    defaultValues: {
      firstName: '',
      lastName: '',
      email: '',
      password: '',
    },
  });

  const onSubmit = async (data: RegisterForm) => {
    try {
      const result = await register({
        firstName: data.firstName,
        lastName: data.lastName,
        email: data.email,
        password: data.password,
        confirmPassword: data.password,
      });
      // Otomatik login yok: hesap doğrulanmadan backend girişi reddediyor.
      router.replace({
        pathname: '/(auth)/verify-email',
        params: { email: result.email },
      });
    } catch {
      // Hata mesajı registerError üzerinden formun altında gösteriliyor.
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
              accessibilityLabel="Geri"
            />
          </View>

          {/* Başlık */}
          <View style={styles.header}>
            <Text style={styles.title}>Hesabını oluştur</Text>
            <Text style={styles.subtitle}>Bugün ilk “hayır”ını söyle.</Text>
          </View>

          {/* Form */}
          <View style={styles.form}>
            <FormField<RegisterForm>
              name="firstName"
              control={control}
              label="Ad"
              placeholder="Adın"
              autoCapitalize="words"
              leftIcon={<User size={19} color={Colors.textMuted} />}
            />
            <FormField<RegisterForm>
              name="lastName"
              control={control}
              label="Soyad"
              placeholder="Soyadın"
              autoCapitalize="words"
              leftIcon={<User size={19} color={Colors.textMuted} />}
            />

            <FormField<RegisterForm>
              name="email"
              control={control}
              label="E-posta"
              placeholder="ornek@email.com"
              keyboardType="email-address"
              autoCapitalize="none"
              autoComplete="email"
              leftIcon={<EnvelopeSimple size={19} color={Colors.textMuted} />}
            />

            <FormField<RegisterForm>
              name="password"
              control={control}
              label="Şifre"
              placeholder="En az 8 karakter"
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

          </View>

          {/* Aksiyon */}
          <FormError message={registerError} />

          <Button
            variant="primary"
            size="lg"
            fullWidth
            disabled={!isValid}
            loading={isRegistering}
            onPress={handleSubmit(onSubmit)}
          >
            Hesap oluştur
          </Button>

          <Text style={styles.terms}>
            Devam ederek{' '}
            <Text
              style={styles.termsAccent}
              onPress={() => openLegalPage('terms')}
              accessibilityRole="link"
            >
              Kullanım Şartları
            </Text>{' '}
            ve{' '}
            <Text
              style={styles.termsAccent}
              onPress={() => openLegalPage('privacy')}
              accessibilityRole="link"
            >
              Gizlilik Politikası
            </Text>
            &apos;nı
            kabul edersin.
          </Text>

          {/* Alt link */}
          <Pressable
            onPress={() => router.replace('/login')}
            style={styles.bottomLink}
            accessibilityRole="link"
          >
            <Text style={styles.bottomLinkText}>
              Zaten hesabın var mı?{' '}
              <Text style={styles.bottomLinkAccent}>Giriş yap</Text>
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
    marginBottom: 28,
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
    gap: Spacing.md,
    marginBottom: 22,
  },
  terms: {
    marginTop: 18,
    fontSize: FontSize.xs,
    lineHeight: 18,
    color: Colors.textMuted,
    textAlign: 'center',
  },
  termsAccent: {
    color: Colors.textSecondary,
    fontWeight: FontWeight.semibold,
    textDecorationLine: 'underline',
  },
  bottomLink: {
    marginTop: Spacing.md,
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
