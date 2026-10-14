// Auth domain tipleri — uygulama içinde taşınan şekiller.
// Wire format (backend'in döndüğü ham gövde) için: @/types/api/auth.

// Secure storage'a yazılan kullanıcı özeti. Backend'in ApiUser'ından
// türetilir (lib/api/mappers/userMapper), ama roles/permissions taşımaz:
// kullanılmıyorlar ve şifreli depoda yer kaplıyorlar.
export interface StoredUser {
  id: number;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  emailConfirmed: boolean;
  profileImageUrl: string | null;
}

export interface AuthTokens {
  accessToken: string;
  refreshToken: string;
  // Backend'in döndüğü ISO-8601 son kullanma tarihi (oturumun/refresh
  // token'ın geçerlilik sınırı). Şimdilik sadece bilgi amaçlı.
  expiresAt?: string;
}
