// Hesap bilgileri — Profil'deki ayarlar kartından açılır.
// Ad/soyad düzenlenir (PUT /api/Auth/profile). E-posta burada salt okunur;
// değişiklik kodla ayrı ekranda yapılır ("E-postayı değiştir" → change-email).
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
import { useEffect, useMemo, useState } from 'react';
import { useForm } from 'react-hook-form';
import { useTranslation } from 'react-i18next';
import {
  Alert,
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
import { t } from '@/lib/i18n';

// Backend: ad/soyad zorunlu, en fazla 50 karakter. Kayıttaki 2 karakter alt sınırıyla aynı.
// Mesajlar şemanın kurulduğu andaki dilde (dil değişince yeniden kurulur).
const createAccountSchema = () =>
  z.object({
    firstName: z
      .string()
      .trim()
      .min(2, t('account.details.validation.firstNameMin'))
      .max(50, t('account.details.validation.firstNameMax')),
    lastName: z
      .string()
      .trim()
      .min(2, t('account.details.validation.lastNameMin'))
      .max(50, t('account.details.validation.lastNameMax')),
  });

type AccountForm = z.infer<ReturnType<typeof createAccountSchema>>;

export default function AccountScreen() {
  const { t, i18n } = useTranslation();
  // Dil değişince doğrulama mesajları yeni dilde kurulsun.
  // eslint-disable-next-line react-hooks/exhaustive-deps
  const accountSchema = useMemo(() => createAccountSchema(), [i18n.language]);
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
      Alert.alert(t('account.details.savedTitle'), t('account.details.savedBody'), [
        { text: t('common.ok'), onPress: () => router.back() },
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
        t('account.details.exportFailedTitle'),
        e instanceof SharingUnavailableError
          ? t('account.details.sharingUnavailable')
          : toAccountErrorMessage(e),
      );
    }
  };

  const onDelete = async () => {
    try {
      await deleteAccount.mutateAsync(deletePassword);
      // Oturum temizlendi; route guard karşılama ekranına götürüyor.
      Alert.alert(t('account.details.deletedTitle'), t('account.details.deletedBody'));
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
        accessibilityLabel={t('common.back')}
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
            <Text style={styles.title}>{t('account.details.title')}</Text>
            <Text style={styles.subtitle}>{t('account.details.subtitle')}</Text>
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
                label={t('account.details.firstNameLabel')}
                placeholder={t('account.details.firstNamePlaceholder')}
                autoCapitalize="words"
                autoComplete="given-name"
                maxLength={50}
                leftIcon={<User size={19} color={Colors.textMuted} />}
              />
              <FormField<AccountForm>
                name="lastName"
                control={control}
                label={t('account.details.lastNameLabel')}
                placeholder={t('account.details.lastNamePlaceholder')}
                autoCapitalize="words"
                autoComplete="family-name"
                maxLength={50}
                leftIcon={<User size={19} color={Colors.textMuted} />}
              />
              <View>
                <Input
                  label={t('account.details.emailLabel')}
                  value={me?.email ?? ''}
                  editable={false}
                  leftIcon={<EnvelopeSimple size={19} color={Colors.textMuted} />}
                />
                <Pressable
                  onPress={() => router.push('/(app)/change-email')}
                  hitSlop={8}
                  accessibilityRole="button"
                  style={styles.hintLink}
                >
                  <Text style={styles.hintLinkText}>{t('account.details.changeEmail')}</Text>
                </Pressable>
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
            {t('common.save')}
          </Button>

          {/* Verilerin (KVKK) */}
          <View style={styles.section}>
            <Text style={styles.sectionTitle}>{t('account.details.dataTitle')}</Text>
            <Text style={styles.sectionText}>
              {t('account.details.dataText')}
            </Text>
            <Button
              variant="outline"
              size="md"
              fullWidth
              loading={exportData.isPending}
              onPress={onExport}
              leftIcon={<DownloadSimple size={18} color={Colors.text} />}
            >
              {t('account.details.exportButton')}
            </Button>
          </View>

          {/* Hesabı sil — iki adımlı */}
          <View style={styles.section}>
            <Text style={[styles.sectionTitle, styles.dangerTitle]}>{t('account.details.deleteTitle')}</Text>
            {!deleteOpen ? (
              <Button
                variant="ghost"
                size="md"
                fullWidth
                onPress={() => setDeleteOpen(true)}
                leftIcon={<Trash size={18} color={Colors.error} />}
              >
                <Text style={styles.dangerLink}>{t('account.details.deleteButton')}</Text>
              </Button>
            ) : (
              <View style={styles.dangerBox}>
                <Text style={styles.dangerText}>
                  {t('account.details.deleteWarning')}
                </Text>
                <Text style={styles.dangerText}>
                  {t('account.details.deleteSubscriptionNote')}
                </Text>
                <Input
                  label={t('account.details.deletePasswordLabel')}
                  value={deletePassword}
                  onChangeText={(text) => {
                    setDeletePassword(text);
                    if (deleteAccount.isError) deleteAccount.reset();
                  }}
                  placeholder={t('account.details.deletePasswordPlaceholder')}
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
                  {t('account.details.deleteConfirm')}
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
                  {t('common.cancel')}
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
  hintLink: {
    alignSelf: 'flex-start',
    marginTop: 8,
  },
  hintLinkText: {
    fontSize: FontSize.sm,
    fontWeight: FontWeight.bold,
    color: Colors.primary,
  },
});
