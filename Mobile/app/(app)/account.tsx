// Hesap bilgileri — Profil'deki ayarlar kartından açılır.
// Ad/soyad düzenlenir (PUT /api/Auth/profile). E-posta salt okunur: değişiklik
// akışı backend'de var ama onay linkini açacak mobil ekran henüz yok.
// Telefon formda yok; /me'den okunan değer olduğu gibi geri gönderilir (silinmesin).
//
// Altında KVKK / App Store bölümleri: "Verilerimi indir" (JSON, paylaşım menüsü) ve
// "Hesabımı sil" (iki adımlı: önce uyarı, sonra mevcut şifre; kalıcı).

import { zodResolver } from '@hookform/resolvers/zod';
import { router } from 'expo-router';
import {
  ArrowLeft,
  DownloadSimple,
  EnvelopeSimple,
  LockSimple,
  Trash,
  User,
} from 'phosphor-react-native';
import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import {
  Alert,
  ScrollView,
  StyleSheet,
  View,
} from 'react-native';
import { KeyboardAvoidingView } from 'react-native-keyboard-controller';
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
import {
  SharingUnavailableError,
  useDeleteAccount,
  useExportData,
} from '@/hooks/useAccountData';
import { toAccountErrorMessage, toProfileErrorMessage } from '@/lib/api';

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
  const exportData = useExportData();
  const deleteAccount = useDeleteAccount();
  const [deleteOpen, setDeleteOpen] = useState(false);
  const [deletePassword, setDeletePassword] = useState('');

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

  const onExport = async () => {
    try {
      await exportData.mutateAsync();
    } catch (e) {
      Alert.alert(
        'Veriler indirilemedi',
        e instanceof SharingUnavailableError
          ? 'Bu cihazda dosya paylaşımı kullanılamıyor.'
          : toAccountErrorMessage(e),
      );
    }
  };

  const onDelete = async () => {
    try {
      await deleteAccount.mutateAsync(deletePassword);
      // Oturum temizlendi; route guard karşılama ekranına götürüyor.
      Alert.alert('Hesabın silindi', 'Hesabın ve tüm verilerin kalıcı olarak silindi.');
    } catch {
      // Hata mesajı şifre alanının altında gösteriliyor.
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
        behavior="padding"
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

          {/* Verilerin (KVKK) */}
          <View style={styles.section}>
            <Text style={styles.sectionTitle}>Verilerin</Text>
            <Text style={styles.sectionText}>
              Pratiklerin, değerlendirmelerin, istatistiklerin ve profil bilgilerin tek bir
              JSON dosyası olarak.
            </Text>
            <Button
              variant="outline"
              size="md"
              fullWidth
              loading={exportData.isPending}
              onPress={onExport}
              leftIcon={<DownloadSimple size={18} color={Colors.text} />}
            >
              Verilerimi indir
            </Button>
          </View>

          {/* Hesabı sil — iki adımlı */}
          <View style={styles.section}>
            <Text style={[styles.sectionTitle, styles.dangerTitle]}>Hesabı sil</Text>
            {!deleteOpen ? (
              <Button
                variant="ghost"
                size="md"
                fullWidth
                onPress={() => setDeleteOpen(true)}
                leftIcon={<Trash size={18} color={Colors.error} />}
              >
                <Text style={styles.dangerLink}>Hesabımı sil</Text>
              </Button>
            ) : (
              <View style={styles.dangerBox}>
                <Text style={styles.dangerText}>
                  Hesabın ve tüm verilerin (pratikler, değerlendirmeler, istatistikler)
                  kalıcı olarak silinir. Bu işlem geri alınamaz.
                </Text>
                <Text style={styles.dangerText}>
                  Aktif bir aboneliğin varsa App Store ya da Google Play üzerinden ayrıca
                  iptal etmelisin; hesabı silmek aboneliği iptal etmez.
                </Text>
                <Input
                  label="Onaylamak için şifreni gir"
                  value={deletePassword}
                  onChangeText={(text) => {
                    setDeletePassword(text);
                    if (deleteAccount.isError) deleteAccount.reset();
                  }}
                  placeholder="Şifren"
                  secureTextEntry
                  autoCapitalize="none"
                  leftIcon={<LockSimple size={19} color={Colors.textMuted} />}
                />
                <FormError
                  message={deleteAccount.error ? toAccountErrorMessage(deleteAccount.error) : null}
                />
                <Button
                  variant="danger"
                  size="md"
                  fullWidth
                  disabled={deletePassword.length === 0}
                  loading={deleteAccount.isPending}
                  onPress={onDelete}
                >
                  Hesabımı kalıcı olarak sil
                </Button>
                <Button
                  variant="ghost"
                  size="md"
                  fullWidth
                  onPress={() => {
                    setDeleteOpen(false);
                    setDeletePassword('');
                    deleteAccount.reset();
                  }}
                >
                  Vazgeç
                </Button>
              </View>
            )}
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
  section: {
    marginTop: Spacing.xl,
    gap: Spacing.sm,
  },
  sectionTitle: {
    fontSize: FontSize.lg,
    fontWeight: FontWeight.bold,
    color: Colors.text,
  },
  sectionText: {
    fontSize: FontSize.sm,
    color: Colors.textSecondary,
    marginBottom: Spacing.xs,
  },
  dangerTitle: {
    color: Colors.error,
  },
  dangerLink: {
    color: Colors.error,
    fontWeight: FontWeight.bold,
  },
  dangerBox: {
    gap: Spacing.sm,
    padding: Spacing.md,
    borderRadius: Radius.lg,
    borderWidth: 1,
    borderColor: Colors.errorSoft,
    backgroundColor: Colors.surface,
  },
  dangerText: {
    fontSize: FontSize.sm,
    color: Colors.textSecondary,
  },
  hint: {
    marginTop: 6,
    fontSize: FontSize.xs,
    color: Colors.textMuted,
  },
});
