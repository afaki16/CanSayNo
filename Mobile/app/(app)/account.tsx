// Hesap bilgileri — Profil'deki ayarlar kartından açılır.
// Ad/soyad düzenlenir (PUT /api/Auth/profile). E-posta salt okunur: değişiklik
// akışı backend'de var ama onay linkini açacak mobil ekran henüz yok.
// Telefon formda yok; /me'den okunan değer olduğu gibi geri gönderilir (silinmesin).

import { zodResolver } from '@hookform/resolvers/zod';
import { router } from 'expo-router';
import { ArrowLeft, EnvelopeSimple, User } from 'phosphor-react-native';
import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import {
  Alert,
  KeyboardAvoidingView,
  Platform,
  ScrollView,
  StyleSheet,
  View,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { z } from 'zod';

import {
  Button,
  ErrorState,
  FormError,
  FormField,
  IconButton,
  Input,
  Skeleton,
  Text,
} from '@/components/ui';
import { Colors, FontSize, FontWeight, Radius, Spacing } from '@/constants/theme';
import { useMe, useUpdateProfile } from '@/hooks/queries/useProfile';
import { toProfileErrorMessage } from '@/lib/api';

// Backend: ad/soyad zorunlu, en fazla 50 karakter. Kayıttaki 2 karakter alt sınırıyla aynı.
const accountSchema = z.object({
  firstName: z
    .string()
    .trim()
    .min(2, 'Ad en az 2 karakter olmalı')
    .max(50, 'Ad en fazla 50 karakter olabilir'),
  lastName: z
    .string()
    .trim()
    .min(2, 'Soyad en az 2 karakter olmalı')
    .max(50, 'Soyad en fazla 50 karakter olabilir'),
});

type AccountForm = z.infer<typeof accountSchema>;

export default function AccountScreen() {
  const { data: me, isPending, isError, error, refetch, isFetching } = useMe();
  const updateProfile = useUpdateProfile();

  const {
    control,
    handleSubmit,
    reset,
    formState: { isValid, isDirty },
  } = useForm<AccountForm>({
    resolver: zodResolver(accountSchema),
    mode: 'onChange',
    defaultValues: { firstName: '', lastName: '' },
  });

  // Sunucudaki değerler gelince formu doldur (dirty sayılmaz).
  useEffect(() => {
    if (me) reset({ firstName: me.firstName, lastName: me.lastName });
  }, [me, reset]);

  const onSubmit = async (data: AccountForm) => {
    if (!me) return;
    try {
      await updateProfile.mutateAsync({
        firstName: data.firstName,
        lastName: data.lastName,
        phoneNumber: me.phoneNumber,
      });
      Alert.alert('Kaydedildi', 'Bilgilerin güncellendi.', [
        { text: 'Tamam', onPress: () => router.back() },
      ]);
    } catch {
      // Hata mesajı formun altında gösteriliyor.
    }
  };

  const header = (
    <View style={styles.topBar}>
      <IconButton
        icon={ArrowLeft}
        bordered
        onPress={() => router.back()}
        accessibilityLabel="Geri"
      />
    </View>
  );

  if (isError) {
    return (
      <SafeAreaView style={styles.safeArea}>
        <View style={styles.padded}>{header}</View>
        <ErrorState
          message={toProfileErrorMessage(error)}
          onRetry={() => refetch()}
          isRetrying={isFetching}
        />
      </SafeAreaView>
    );
  }

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
          {header}

          <View style={styles.header}>
            <Text style={styles.title}>Hesap bilgileri</Text>
            <Text style={styles.subtitle}>Adın ve soyadın uygulamada böyle görünür.</Text>
          </View>

          {isPending ? (
            <View style={styles.form}>
              <Skeleton height={76} borderRadius={Radius.lg} />
              <Skeleton height={76} borderRadius={Radius.lg} />
              <Skeleton height={76} borderRadius={Radius.lg} />
            </View>
          ) : (
            <View style={styles.form}>
              <FormField<AccountForm>
                name="firstName"
                control={control}
                label="Ad"
                placeholder="Adın"
                autoCapitalize="words"
                autoComplete="given-name"
                maxLength={50}
                leftIcon={<User size={19} color={Colors.textMuted} />}
              />
              <FormField<AccountForm>
                name="lastName"
                control={control}
                label="Soyad"
                placeholder="Soyadın"
                autoCapitalize="words"
                autoComplete="family-name"
                maxLength={50}
                leftIcon={<User size={19} color={Colors.textMuted} />}
              />
              <View>
                <Input
                  label="E-posta"
                  value={me?.email ?? ''}
                  editable={false}
                  leftIcon={<EnvelopeSimple size={19} color={Colors.textMuted} />}
                />
                <Text style={styles.hint}>E-posta değişikliği yakında eklenecek.</Text>
              </View>
            </View>
          )}

          <FormError
            message={updateProfile.error ? toProfileErrorMessage(updateProfile.error) : null}
          />

          <Button
            variant="primary"
            size="lg"
            fullWidth
            disabled={!isValid || !isDirty || isPending}
            loading={updateProfile.isPending}
            onPress={handleSubmit(onSubmit)}
          >
            Kaydet
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
  padded: {
    paddingHorizontal: 28,
    paddingTop: Spacing.md,
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
  hint: {
    marginTop: 6,
    fontSize: FontSize.xs,
    color: Colors.textMuted,
  },
});
