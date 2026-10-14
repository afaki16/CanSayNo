// Auth endpoint'lerinin transport katmanı. Sadece istek atar ve zarfı açar —
// hata mesajı çevirisi ./authErrors'da, state yönetimi hooks/useAuth'ta.

import { apiPost, apiPostVoid } from '../hooks';
import { AUTH_ENDPOINTS } from '../endpoints';

import type {
  AuthResponse,
  LoginRequest,
  LogoutRequest,
  RegisterRequest,
  RegisterResponse,
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
};
