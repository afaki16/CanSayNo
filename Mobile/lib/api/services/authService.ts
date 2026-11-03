// Auth endpoint'lerinin transport katmanı. Sadece istek atar ve zarfı açar —
// hata mesajı çevirisi ./authErrors'da, state yönetimi hooks/useAuth'ta.

import { apiPost, apiPostVoid } from '../hooks';
import { AUTH_ENDPOINTS } from '../endpoints';

import type {
  AuthResponse,
  ForgotPasswordCodeRequest,
  LoginRequest,
  LogoutRequest,
  RegisterRequest,
  RegisterResponse,
  ResetPasswordCodeRequest,
} from '@/types/api/auth';

export const authService = {
  login: (body: LoginRequest) =>
    apiPost<AuthResponse, LoginRequest>(AUTH_ENDPOINTS.LOGIN, body),

  // 201 + oluşturulan kullanıcı döner; token YOK — e-posta doğrulanmadan
  // login reddedilir.
  register: (body: RegisterRequest) =>
    apiPost<RegisterResponse, RegisterRequest>(AUTH_ENDPOINTS.REGISTER, body),

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
};
