// KVKK / App Store: kullanıcının verisini dışa aktarma ve hesabı kalıcı silme.
//
// Dışa aktarma: JSON önbellek klasörüne dosya olarak yazılır ve sistemin paylaşım
// menüsü açılır (kaydet, e-postayla gönder vb.).
// Silme: başarıda yerel oturum temizlenir; kök layout'taki route guard kullanıcıyı
// karşılama ekranına götürür ve sorgu önbelleği sıfırlanır. Backend'e logout
// çağrılmaz — kullanıcı ve token'ları zaten silindi.

import { useMutation } from '@tanstack/react-query';
import { File, Paths } from 'expo-file-system';
import * as Sharing from 'expo-sharing';

import { accountService } from '@/lib/api';
import type { ApiError } from '@/lib/api';
import { useAuthStore } from '@/stores/authStore';

export class SharingUnavailableError extends Error {
  constructor() {
    super('Bu cihazda paylaşım kullanılamıyor');
    this.name = 'SharingUnavailableError';
  }
}

export function useExportData() {
  return useMutation<void, ApiError | Error>({
    mutationFn: async () => {
      const data = await accountService.exportData();

      const date = new Date().toISOString().slice(0, 10);
      const file = new File(Paths.cache, `cansayno-verilerim-${date}.json`);
      file.create({ overwrite: true });
      file.write(JSON.stringify(data, null, 2));

      if (!(await Sharing.isAvailableAsync())) {
        throw new SharingUnavailableError();
      }
      await Sharing.shareAsync(file.uri, {
        mimeType: 'application/json',
        UTI: 'public.json',
        dialogTitle: 'Verilerini kaydet ya da paylaş',
      });
    },
  });
}

export function useDeleteAccount() {
  const clearAuth = useAuthStore((state) => state.clearAuth);

  return useMutation<void, ApiError, string>({
    mutationFn: async (password) => {
      await accountService.deleteAccount(password);
    },
    onSuccess: async () => {
      await clearAuth();
    },
  });
}
