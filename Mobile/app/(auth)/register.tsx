// Register ekranı: gerçek form (react-hook-form + zod), mock backend ile bağlı.
// friendly playful tasarım — login ile tutarlı, ad/soyad yan yana.

import { zodResolver } from '@hookform/resolvers/zod';
import { router } from 'expo-router';
import {
  ArrowLeft,
  Envelope,
  Eye,
  EyeSlash,
  Lock,
  User,
} from 'phosphor-react-native';
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

const registerSchema = z
  .object({
    firstName: z.string().min(2, 'Ad en az 2 karakter olmalı'),
    lastName: z.string().min(2, 'Soyad en az 2 karakter olmalı'),
    email: z.string().email('Geçerli bir e-posta girin'),
    password: z
      .string()
      .min(8, 'Şifre en az 8 karakter olmalı')
      .regex(/[A-Z]/, 'En az bir büyük harf içermeli')
      .regex(/[0-9]/, 'En az bir rakam içermeli'),
    confirmPassword: z.string(),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: 'Şifreler eşleşmiyor',
    path: ['confirmPassword'],
  });

type RegisterForm = z.infer<typeof registerSchema>;

export default function RegisterScreen() {
  const { register } = useAuth();
  const [isSubmitting, setIsSubmitting] = useState(false);
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
      confirmPassword: '',
    },
  });

  const onSubmit = async (data: RegisterForm) => {
    setIsSubmitting(true);
    try {
      await register({
        firstName: data.firstName,
        lastName: data.lastName,
        email: data.email,
        password: data.password,
      });
      router.replace('/(app)/(tabs)/home');
    } catch (error) {
      const message =
        error instanceof Error ? error.message : 'Bir şeyler ters gitti';
      Alert.alert('Kayıt oluşturulamadı', message);
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
            <Text style={styles.title}>Hadi tanışalım</Text>
            <Text style={styles.subtitle}>
              Hesap oluştur, pratik yapmaya başla
            </Text>
          </View>

          {/* Form */}
          <View style={styles.form}>
            <View style={styles.row}>
              <FormField<RegisterForm>
                name="firstName"
                control={control}
                containerStyle={styles.rowItem}
                label="Ad"
                placeholder="Adın"
                autoCapitalize="words"
                leftIcon={<User size={20} color={Colors.textMuted} />}
              />
              <FormField<RegisterForm>
                name="lastName"
                control={control}
                containerStyle={styles.rowItem}
                label="Soyad"
                placeholder="Soyadın"
                autoCapitalize="words"
                leftIcon={<User size={20} color={Colors.textMuted} />}
              />
            </View>

            <FormField<RegisterForm>
              name="email"
              control={control}
              label="E-posta"
              placeholder="ornek@email.com"
              keyboardType="email-address"
              autoCapitalize="none"
              autoComplete="email"
              leftIcon={<Envelope size={20} color={Colors.textMuted} />}
            />

            <FormField<RegisterForm>
              name="password"
              control={control}
              label="Şifre"
              placeholder="En az 8 karakter"
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

            <FormField<RegisterForm>
              name="confirmPassword"
              control={control}
              label="Şifre Tekrar"
              placeholder="Şifreni tekrar gir"
              secureTextEntry={!showPassword}
              autoCapitalize="none"
              leftIcon={<Lock size={20} color={Colors.textMuted} />}
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
            Kayıt Ol
          </Button>

          {/* Alt link */}
          <Pressable
            onPress={() => router.replace('/login')}
            style={styles.bottomLink}
            accessibilityRole="link"
          >
            <Text style={styles.bottomLinkText}>
              Zaten hesabın var mı?{' '}
              <Text style={styles.bottomLinkAccent}>Giriş Yap</Text>
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
  row: {
    flexDirection: 'row',
    gap: Spacing.md,
  },
  rowItem: {
    flex: 1,
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
