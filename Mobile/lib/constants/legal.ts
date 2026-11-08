// Yasal sayfaların adresleri — web sitesi Web/ (cansayno.com), metinler docs/store/*.md.
// Apple abonelik kuralı: paywall'da ve kayıtta Kullanım Şartları + Gizlilik Politikası linki olmalı.

export const LEGAL_URLS = {
  privacy: 'https://cansayno.com/gizlilik/',
  terms: 'https://cansayno.com/kullanim-sartlari/',
  accountDeletion: 'https://cansayno.com/hesap-silme/',
} as const;

export type LegalPage = keyof typeof LEGAL_URLS;
