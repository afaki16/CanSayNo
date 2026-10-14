// ApiUser (wire format) → StoredUser (uygulama içi + secure storage) dönüşümü.
//
// roles/permissions BİLEREK atılıyor: admin kullanıcısında bu iki dizi ~40 kayıt
// (≈10 KB JSON) tutuyor ve şu an hiçbir ekran kullanmıyor. Secure storage'a
// yazılan her byte'ın bedeli var; yetkilendirme gerektiğinde ayrı bir
// permissions store'u eklenecek.

import type { ApiUser } from '@/types/api/auth';
import type { StoredUser } from '@/types/auth';

export function toStoredUser(user: ApiUser): StoredUser {
  return {
    id: user.id,
    firstName: user.firstName,
    lastName: user.lastName,
    fullName: user.fullName,
    email: user.email,
    emailConfirmed: user.emailConfirmed,
    profileImageUrl: user.profileImageUrl,
  };
}
