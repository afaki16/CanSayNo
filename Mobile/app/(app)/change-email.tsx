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
import { Fragment, useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { Alert, Pressable, ScrollView, StyleSheet, View } from 'react-native';
import { KeyboardAvoidingView } from 'react-native-keyboard-controller';
import { SafeAreaView } from 'react-native-safe-area-context';
import { z } from 'zod';

import { Button, FormError, FormField, IconButton, Text } from '@/components/ui';
import { Colors, FontSize, FontWeight, Spacing } from '@/constants/theme';
import { useMe } from '@/hooks/queries/useProfile';
import { EMAIL_CHANGE_RESEND_SECONDS, useEmailChange } from '@/hooks/useEmailChange';

const requestSchema = z.object({
  newEmail: z.string().trim().min(1, 'E-posta gerekli').email('Geçerli bir e-posta gir'),
  currentPassword: z.string().min(1, 'Şifren gerekli'),
});

const codeSchema = z.object({
  code: z.string().regex(/^\d{6}$/, 'Kod 6 haneli olmalı'),
});

type RequestForm = z.infer<typeof requestSchema>;
type CodeForm = z.infer<typeof codeSchema>;

export default function ChangeEmailScreen() {
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
      Alert.alert('Kod gönderilemedi', 'Lütfen biraz sonra tekrar dene.');
    }
  };

  const onConfirm = async ({ code }: CodeForm) => {
    try {
      await confirm(code);
      // Yerel oturum temizlendi; route guard giriş ekranına götürür. Native Alert yönlendirmeden etkilenmez.
      Alert.alert(
        'E-postan güncellendi',
        `Güvenliğin için tüm cihazlardan çıkış yapıldı. ${pending?.newEmail ?? 'Yeni adresin'} ile tekrar giriş yap.`,
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
              accessibilityLabel="Geri"
            />
          </View>

          {!pending ? (
            // Ayrı key: iki adımın FormField'ları aynı konumda; key olmadan React ilk adımın
            // Controller'ını yeniden kullanır ve kod alanı eski forma bağlı kalır (yazılamaz).
            <Fragment key="request">
              <View style={styles.header}>
                <Text style={styles.title}>E-postanı değiştir</Text>
                <Text style={styles.subtitle}>
                  {me?.email ? (
                    <>
                      Şu anki adresin <Text style={styles.email}>{me.email}</Text>. Yeni adresine
                      6 haneli bir doğrulama kodu göndereceğiz.
                    </>
                  ) : (
                    'Yeni adresine 6 haneli bir doğrulama kodu göndereceğiz.'
                  )}
                </Text>
              </View>

              <View style={styles.form}>
                <FormField<RequestForm>
                  name="newEmail"
                  control={requestForm.control}
                  label="Yeni e-posta"
                  placeholder="ornek@eposta.com"
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
                  label="Mevcut şifre"
                  placeholder="Şifren"
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
                      accessibilityLabel={showPassword ? 'Şifreyi gizle' : 'Şifreyi göster'}
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
                Kod gönder
              </Button>
            </Fragment>
          ) : (
            <Fragment key="code">
              <View style={styles.header}>
                <Text style={styles.title}>Kodu gir</Text>
                <Text style={styles.subtitle}>
                  <Text style={styles.email}>{pending.newEmail}</Text> adresine 6 haneli bir kod
                  gönderdik. Kod 10 dakika geçerli.
                </Text>
              </View>

              <View style={styles.form}>
                <FormField<CodeForm>
                  name="code"
                  control={codeForm.control}
                  label="Doğrulama kodu"
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
                E-postayı güncelle
              </Button>

              <Text style={styles.note}>
                Güncellemeden sonra tüm cihazlardan çıkış yapılır; yeni adresinle tekrar giriş
                yaparsın.
              </Text>

              <Pressable
                onPress={onResend}
                disabled={cooldown > 0 || isSendingCode}
                style={styles.bottomLink}
                accessibilityRole="button"
              >
                <Text style={styles.bottomLinkText}>
                  Kod gelmedi mi?{' '}
                  <Text
                    style={
                      cooldown > 0 || isSendingCode
                        ? styles.bottomLinkMuted
                        : styles.bottomLinkAccent
                    }
                  >
                    {cooldown > 0 ? `Tekrar gönder (${cooldown} sn)` : 'Tekrar gönder'}
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
