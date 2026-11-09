// Yeni şifre kuralı — kayıt ve şifre sıfırlama aynı kuralı kullanır.
// Backend'in şifre gücü kontrolüyle uyumlu tutulmalı; asıl doğrulama backend'de.

import { z } from 'zod';

import { t } from '@/lib/i18n';

// Mesajlar fonksiyon: doğrulama anındaki dil kullanılır.
export const newPasswordSchema = z
  .string()
  .min(8, { error: () => t('auth.validation.passwordMin8') })
  .regex(/[A-Z]/, { error: () => t('auth.validation.passwordUppercase') })
  .regex(/[0-9]/, { error: () => t('auth.validation.passwordDigit') });
