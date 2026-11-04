// E-posta değişikliği (kodla): mevcut şifre + yeni adres → yeni adrese 6 haneli
// kod gider → kod girilince adres değişir. Başarıda backend TÜM oturumları
// kapatır (token'lar eski adresi taşıyor); yerel oturum da temizlenir ve kök
// layout kullanıcıyı giriş ekranına yönlendirir.

import { useMutation } from '@tanstack/react-query';

import { authService, toEmailChangeErrorMessage } from '@/lib/api';
import { useAuthStore } from '@/stores/authStore';

// Backend aynı amaçla 60 sn içinde yeni kod göndermiyor.
export const EMAIL_CHANGE_RESEND_SECONDS = 60;

export function useEmailChange() {
  const clearAuth = useAuthStore((state) => state.clearAuth);

  const sendCodeMutation = useMutation({
    mutationFn: (data: { newEmail: string; currentPassword: string }) =>
      authService.sendEmailChangeCode({
        newEmail: data.newEmail.trim(),
        currentPassword: data.currentPassword,
      }),
  });

  const confirmMutation = useMutation({
    mutationFn: (code: string) => authService.confirmEmailChange({ code: code.trim() }),
    onSuccess: async () => {
      await clearAuth();
    },
  });

  return {
    sendCode: sendCodeMutation.mutateAsync,
    isSendingCode: sendCodeMutation.isPending,
    sendCodeError: sendCodeMutation.error
      ? toEmailChangeErrorMessage(sendCodeMutation.error)
      : null,

    confirm: confirmMutation.mutateAsync,
    isConfirming: confirmMutation.isPending,
    confirmError: confirmMutation.error
      ? toEmailChangeErrorMessage(confirmMutation.error)
      : null,
  };
}
