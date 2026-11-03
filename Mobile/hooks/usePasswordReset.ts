// "Şifremi unuttum" akışı: e-postaya kod iste → kod + yeni şifreyle sıfırla.
// Oturum açmayı gerektirmez, auth store'a dokunmaz; başarılı sıfırlamadan
// sonra kullanıcı login ekranından yeni şifresiyle girer.

import { useMutation } from '@tanstack/react-query';

import {
  authService,
  toForgotPasswordErrorMessage,
  toResetPasswordErrorMessage,
} from '@/lib/api';

// Mobil kullanıcılar tenant'sız; 0 = "belirtilmedi" (backend e-posta tek
// hesaba aitse tenant'a bakmıyor).
const NO_TENANT = 0;

// Backend aynı adrese 60 sn içinde yeni kod göndermiyor; "Tekrar gönder"
// butonu bu süre boyunca kapalı tutulur.
export const RESEND_COOLDOWN_SECONDS = 60;

export function usePasswordReset() {
  const sendCodeMutation = useMutation({
    mutationFn: (email: string) =>
      authService.sendPasswordResetCode({
        email: email.trim(),
        tenantId: NO_TENANT,
      }),
  });

  const resetMutation = useMutation({
    mutationFn: (data: { email: string; code: string; newPassword: string }) =>
      authService.resetPasswordWithCode({
        email: data.email.trim(),
        tenantId: NO_TENANT,
        code: data.code.trim(),
        newPassword: data.newPassword,
      }),
  });

  return {
    sendCode: sendCodeMutation.mutateAsync,
    isSendingCode: sendCodeMutation.isPending,
    sendCodeError: sendCodeMutation.error
      ? toForgotPasswordErrorMessage(sendCodeMutation.error)
      : null,

    resetPassword: resetMutation.mutateAsync,
    isResetting: resetMutation.isPending,
    resetError: resetMutation.error
      ? toResetPasswordErrorMessage(resetMutation.error)
      : null,
  };
}
