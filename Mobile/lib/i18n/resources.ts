// Tüm çeviri dosyaları tek "translation" namespace'inde, alan adına göre iç içe:
//   t('common.ok'), t('auth.login.title') ...
// Yeni alan eklerken: locales/tr/<alan>.ts + locales/en/<alan>.ts yaz, ikisini de buraya ekle.

import { account as enAccount } from './locales/en/account';
import { auth as enAuth } from './locales/en/auth';
import { common as enCommon } from './locales/en/common';
import { contexts as enContexts } from './locales/en/contexts';
import { practice as enPractice } from './locales/en/practice';
import { account as trAccount } from './locales/tr/account';
import { auth as trAuth } from './locales/tr/auth';
import { common as trCommon } from './locales/tr/common';
import { contexts as trContexts } from './locales/tr/contexts';
import { practice as trPractice } from './locales/tr/practice';

const tr = {
  common: trCommon,
  auth: trAuth,
  practice: trPractice,
  contexts: trContexts,
  account: trAccount,
};

const en: typeof tr = {
  common: enCommon,
  auth: enAuth,
  practice: enPractice,
  contexts: enContexts,
  account: enAccount,
};

export const resources = {
  tr: { translation: tr },
  en: { translation: en },
} as const;

export type TranslationResources = typeof tr;
