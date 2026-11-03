// Yeni şifre kuralı — kayıt ve şifre sıfırlama aynı kuralı kullanır.
// Backend'in şifre gücü kontrolüyle uyumlu tutulmalı; asıl doğrulama backend'de.

import { z } from 'zod';

export const newPasswordSchema = z
  .string()
  .min(8, 'Şifre en az 8 karakter olmalı')
  .regex(/[A-Z]/, 'En az bir büyük harf içermeli')
  .regex(/[0-9]/, 'En az bir rakam içermeli');
