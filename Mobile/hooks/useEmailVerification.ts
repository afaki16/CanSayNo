// Kayıt doğrulama: e-postaya gelen 6 haneli kodu gir → hesap etkinleşir.
//
// Kullanıcı şifresini az önce kayıt (ya da giriş) formuna yazdığı için,
// doğrulamadan sonra tekrar sorulmaz: şifre YALNIZCA bellekte, bu modülde
// tutulur (diske/secure store'a yazılmaz) ve ilk kullanımda silinir. Uygulama
// kapanırsa kaybolur; o zaman kullanıcı login ekranına yönlendirilir.

import { useMutation } from '@tanstack/react-query';

import {
  authService,
  toForgotPasswordErrorMessage,
  toResetPasswordErrorMessage,
} from '@/lib/api';

// Mobil kullanıcılar tenant'sız; 0 = "belirtilmedi".
const NO_TENANT = 0;

let pending: { email: string; password: string } | null = null;

const normalize = (email: string) => email.trim().toLowerCase();

/** Kayıt/giriş formundaki şifreyi doğrulama sonrası otomatik giriş için hatırla. */
export function rememberPendingCredentials(email: string, password: string) {
  pending = { email: normalize(email), password };
}

/** Bu e-posta için hatırlanan şifreyi döndürür ve unutur. */
export function takePendingPassword(email: string): string | null {
  const match = pending && pending.email === normalize(email) ? pending.password : null;
  pending = null;
  return match;
}

export function useEmailVerification() {
  const verifyMutation = useMutation({
    mutationFn: (data: { email: string; code: string }) =>
      authService.verifyEmailWithCode({
        email: data.email.trim(),
        code: data.code.trim(),
      }),
  });

  const resendMutation = useMutation({
    mutationFn: (email: string) =>
      authService.resendVerification({ email: email.trim(), tenantId: NO_TENANT }),
  });

  return {
    verify: verifyMutation.mutateAsync,
    isVerifying: verifyMutation.isPending,
    // 400'ler (kod hatalı/süresi dolmuş/çok deneme) backend'in yerelleştirilmiş metni
    verifyError: verifyMutation.error
      ? toResetPasswordErrorMessage(verifyMutation.error)
      : null,

    resend: resendMutation.mutateAsync,
    isResending: resendMutation.isPending,
    resendError: resendMutation.error
      ? toForgotPasswordErrorMessage(resendMutation.error)
      : null,
  };
}
