// Açılışta secure storage'dan gelen oturumu sunucuya doğrulatır (GET /api/Auth/me).
//
// Açılışı BLOKLAMAZ: kullanıcı hemen içeri girer, doğrulama arka planda koşar.
//  - Access token süresi dolmuşsa refresh interceptor yeniler, istek tekrarlanır.
//  - Refresh token da geçersizse interceptor oturumu temizler → route guard'ı
//    kullanıcıyı karşılama ekranına götürür.
//  - Ağ hatasında hiçbir şey olmaz (çevrimdışı açılış oturumu düşürmez).
// Başarıda saklı kullanıcı bilgisi (ad, e-posta doğrulama vb.) tazelenir.

import { useEffect, useRef } from 'react';

import { authService, toStoredUser } from '@/lib/api';
import { useAuthStore } from '@/stores/authStore';

export function useSessionValidation() {
  const isRehydrating = useAuthStore((state) => state.isRehydrating);
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);
  const updateUser = useAuthStore((state) => state.updateUser);
  const validated = useRef(false);

  useEffect(() => {
    if (isRehydrating || !isAuthenticated || validated.current) return;
    validated.current = true;

    authService
      .me()
      .then((user) => updateUser(toStoredUser(user)))
      .catch(() => {
        // Oturum hatası interceptor'da ele alınıyor; diğer hatalar açılışı etkilemez.
      });
  }, [isRehydrating, isAuthenticated, updateUser]);
}
