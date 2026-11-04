// Auth endpoint'lerinin transport katmanı. Sadece istek atar ve zarfı açar —
// hata mesajı çevirisi ./authErrors'da, state yönetimi hooks/useAuth'ta.

import { apiGet, apiPost, apiPostVoid, apiPutVoid } from '../hooks';
import { AUTH_ENDPOINTS } from '../endpoints';

import type {
  ApiUser,
  AuthResponse,
  ForgotPasswordCodeRequest,
  LoginRequest,
  LogoutRequest,
  RegisterRequest,
  RegisterResponse,
  ResetPasswordCodeRequest,
  EmailChangeCodeRequest,
  EmailChangeConfirmRequest,
  UpdateProfileRequest,
} from '@/types/api/auth';

export const authService = {
  // Mobilde her zaman "beni hatırla": refresh token yenilenirken bitiş tarihi
  // korunur (kayan süre yok), yani bu süre dolunca kullanıcı tekrar giriş yapar.
  login: (body: LoginRequest) =>
    apiPost<AuthResponse, LoginRequest & { rememberMe: boolean }>(AUTH_ENDPOINTS.LOGIN, {
      ...body,
      rememberMe: true,
    }),

  // 201 + oluşturulan kullanıcı döner; token YOK — e-posta doğrulanmadan
  // login reddedilir.
  register: (body: RegisterRequest) =>
    apiPost<RegisterResponse, RegisterRequest>(AUTH_ENDPOINTS.REGISTER, body),

  // Oturum doğrulama (açılışta). Access token süresi dolmuşsa interceptor refresh
  // eder; refresh token da geçersizse oturum temizlenir.
  me: () => apiGet<ApiUser>(AUTH_ENDPOINTS.ME),

  updateProfile: (body: UpdateProfileRequest) =>
    apiPutVoid<UpdateProfileRequest>(AUTH_ENDPOINTS.PROFILE, body),

  // { success: true, message: "..." } döner, data yok → apiPostVoid.
  logout: (body: LogoutRequest) =>
    apiPostVoid<LogoutRequest>(AUTH_ENDPOINTS.LOGOUT, body),

  // Her zaman 200 döner — e-postanın kayıtlı olup olmadığını sızdırmaz.
  // Aynı adrese 60 sn içinde ikinci istek de 200 döner ama yeni kod gitmez.
  sendPasswordResetCode: (body: ForgotPasswordCodeRequest) =>
    apiPostVoid<ForgotPasswordCodeRequest>(
      AUTH_ENDPOINTS.FORGOT_PASSWORD_CODE,
      body,
    ),

  // Başarıda tüm oturumlar kapanır, giriş kilidi temizlenir; kullanıcı yeni
  // şifreyle login olur.
  resetPasswordWithCode: (body: ResetPasswordCodeRequest) =>
    apiPostVoid<ResetPasswordCodeRequest>(
      AUTH_ENDPOINTS.RESET_PASSWORD_CODE,
      body,
    ),

  // Hatalar: 400 (yanlış şifre, aynı adres, 60 sn bekleme), 409 (adres kullanımda).
  sendEmailChangeCode: (body: EmailChangeCodeRequest) =>
    apiPostVoid<EmailChangeCodeRequest>(AUTH_ENDPOINTS.EMAIL_CHANGE_CODE, body),

  // Başarıda tüm oturumlar kapanır (refresh token'lar iptal); yeniden giriş gerekir.
  confirmEmailChange: (body: EmailChangeConfirmRequest) =>
    apiPostVoid<EmailChangeConfirmRequest>(AUTH_ENDPOINTS.EMAIL_CHANGE_CONFIRM, body),
};
