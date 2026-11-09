// t() anahtarlarının tip kontrolü: yanlış/olmayan anahtar tsc hatası verir.
import 'i18next';

import type { TranslationResources } from '@/lib/i18n/resources';

declare module 'i18next' {
  interface CustomTypeOptions {
    defaultNS: 'translation';
    resources: { translation: TranslationResources };
    returnNull: false;
  }
}
