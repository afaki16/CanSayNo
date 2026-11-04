// Hesap uçlarının transport katmanı: KVKK veri dışa aktarma ve kalıcı silme.

import { apiDeleteVoid, apiGet } from '../hooks';
import { ACCOUNT_ENDPOINTS } from '../endpoints';

export const accountService = {
  // Profil, istatistikler, tüm pratikler (mesaj + değerlendirme), günlük kullanım,
  // abonelik özeti. Şekli olduğu gibi dosyaya yazılır; uygulama içinde yorumlanmaz.
  exportData: () => apiGet<Record<string, unknown>>(ACCOUNT_ENDPOINTS.EXPORT),

  // Yanlış şifre → 400 (Türkçe mesaj), son SuperAdmin → 403.
  deleteAccount: (password: string) =>
    apiDeleteVoid<{ password: string }>(ACCOUNT_ENDPOINTS.DELETE, { password }),
};
