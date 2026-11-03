// Backend Auth endpoint'lerinin request/response tipleri.
// Şekiller Swagger'dan DEĞİL, çalışan backend'e atılan gerçek isteklerden
// doğrulandı (bkz. /api/Auth/login, /register, /refresh-token, /logout).
//
// Bu tipler wire format'ı temsil eder — uygulama içinde taşınan tip
// StoredUser'dır (@/types/auth). Dönüşüm: lib/api/mappers/userMapper.

export interface ApiPermission {
  id: number;
  name: string;
  description: string;
  resource: string;
  type: string;
  fullPermission: string;
  createdDate: string;
  individualPermissions: unknown[];
}

export interface ApiRole {
  id: number;
  name: string;
  description: string;
  isSystemRole: boolean;
  createdDate: string;
  permissions: ApiPermission[];
}

export interface ApiUser {
  id: number;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  phoneNumber: string | null;
  status: string; // "active" | "pendingVerification" | ... (backend enum'u camelCase string)
  lastLoginDate: string | null;
  emailConfirmed: boolean;
  phoneConfirmed: boolean;
  profileImageUrl: string | null;
  createdDate: string;
  // Register ile açılan hesaplarda henüz tenant atanmamış olabiliyor.
  tenantId: number | null;
  roles: ApiRole[];
  permissions: ApiPermission[];
}

// /api/Auth/login ve /api/Auth/refresh-token aynı gövdeyi döner.
export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string; // ISO-8601 UTC
  user: ApiUser;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  confirmPassword: string;
  // Opsiyonel — ama boş string GÖNDERME. Users.PhoneNumber üzerinde unique
  // index var; "" ikinci kayıtta 23505 çakışması yapıp 500'e düşüyor.
  // Değer yoksa null gönder (Postgres unique index NULL'ları çakıştırmaz).
  phoneNumber: string | null;
}

// DİKKAT: refresh-token İKİ token'ı da ister. Sadece refreshToken gönderilirse
// backend 400 "Geçersiz yenileme tokeni" döner.
export interface RefreshTokenRequest {
  accessToken: string;
  refreshToken: string;
}

export interface LogoutRequest {
  refreshToken: string;
  reason: string;
}

// PUT /api/Auth/profile. Telefon gönderilen değerle DEĞİŞTİRİLİR (null = sil);
// mevcut telefonu korumak için /me'den okunan değer geri gönderilmeli.
export interface UpdateProfileRequest {
  firstName: string;
  lastName: string;
  phoneNumber: string | null;
}

// Kodla şifre sıfırlama. tenantId: mobil kullanıcılar tenant'sız; backend
// e-posta tek hesaba aitse tenantId'ye bakmıyor, 0 "belirtilmedi" demek.
export interface ForgotPasswordCodeRequest {
  email: string;
  tenantId: number;
}

export interface ResetPasswordCodeRequest {
  email: string;
  tenantId: number;
  code: string;
  newPassword: string;
}

// /api/Auth/register 201 ile oluşturulan kullanıcıyı döner (token YOK —
// e-posta doğrulanana kadar login reddedilir).
export type RegisterResponse = ApiUser;
