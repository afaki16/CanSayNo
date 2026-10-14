// Auth store + backend arasındaki köprü. Ekranlar auth state'i ve
// login/register/logout aksiyonlarını buradan tüketir.
//
// Aksiyonlar TanStack Query mutation'ı; loading/error state'leri hook'tan
// çıkar, ekranlar kendi useState'ini tutmaz. Hata mesajları ekrana hazır
// Türkçe metin olarak gelir (bkz. lib/api/services/authErrors).

import { useMutation } from '@tanstack/react-query';
import { useCallback } from 'react';

import {
  authService,
  toLoginErrorMessage,
  toRegisterErrorMessage,
  toStoredUser,
} from '@/lib/api';
import { useAuthStore } from '@/stores/authStore';
import type { LoginRequest, RegisterRequest } from '@/types/api/auth';

// Formda telefon alanı yok, backend de zorunlu tutmuyor.
//
// DİKKAT: boş string ("") GÖNDERİLMEZ. Users.PhoneNumber sütununda unique
// index var: ilk "" kaydı geçer, ikincisi 23505 duplicate key ile 500 döner.
// null'da bu sorun yok (Postgres unique index NULL'ları birbirinden ayrı
// sayar) ve backend validator da null'ı atlıyor. Canlı backend'de doğrulandı.
const UNSET_PHONE_NUMBER = null;

// Logout'ta backend'e iletilen sebep — oturumu kullanıcının kendisi kapattı.
const LOGOUT_REASON = 'user_initiated';

export interface RegisterData {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  confirmPassword: string;
}

// Register başarılı olduğunda otomatik login YAPILMAZ: hesap
// "pendingVerification" durumunda açılır ve login reddedilir. Ekran bu sonuca
// bakıp doğrulama ekranına yönlendirir.
export interface RegisterResult {
  requiresEmailVerification: true;
  email: string;
}

export function useAuth() {
  const user = useAuthStore((state) => state.user);
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);
  const isRehydrating = useAuthStore((state) => state.isRehydrating);
  const setAuth = useAuthStore((state) => state.setAuth);
  const clearAuth = useAuthStore((state) => state.clearAuth);

  const loginMutation = useMutation({
    mutationFn: async (credentials: LoginRequest) => {
      const result = await authService.login(credentials);
      await setAuth(toStoredUser(result.user), {
        accessToken: result.accessToken,
        refreshToken: result.refreshToken,
        expiresAt: result.expiresAt,
      });
    },
  });

  const registerMutation = useMutation({
    mutationFn: async (data: RegisterData): Promise<RegisterResult> => {
      const body: RegisterRequest = {
        firstName: data.firstName,
        lastName: data.lastName,
        email: data.email,
        password: data.password,
        confirmPassword: data.confirmPassword,
        phoneNumber: UNSET_PHONE_NUMBER,
      };
      const created = await authService.register(body);
      return { requiresEmailVerification: true, email: created.email };
    },
  });

  const logoutMutation = useMutation({
    // Bilerek asla reject etmez: backend'e ulaşılamasa veya token süresi
    // dolmuş olsa bile kullanıcı uygulamadan çıkabilmeli.
    mutationFn: async () => {
      const refreshToken = useAuthStore.getState().refreshToken;
      try {
        if (refreshToken) {
          await authService.logout({
            refreshToken,
            reason: LOGOUT_REASON,
          });
        }
      } catch (error) {
        // Sunucu tarafında token iptal edilemedi; local oturumu yine de kapat.
        console.warn('[useAuth] logout isteği başarısız, local oturum yine de kapatılıyor:', error);
      } finally {
        await clearAuth();
      }
    },
  });

  const login = useCallback(
    (email: string, password: string) =>
      loginMutation.mutateAsync({ email, password }),
    [loginMutation],
  );

  const register = useCallback(
    (data: RegisterData) => registerMutation.mutateAsync(data),
    [registerMutation],
  );

  const logout = useCallback(
    () => logoutMutation.mutateAsync(),
    [logoutMutation],
  );

  return {
    user,
    isAuthenticated,
    isRehydrating,

    login,
    register,
    logout,

    isLoggingIn: loginMutation.isPending,
    isRegistering: registerMutation.isPending,
    isLoggingOut: logoutMutation.isPending,
    // Herhangi bir auth işlemi sürüyor mu.
    isLoading:
      loginMutation.isPending ||
      registerMutation.isPending ||
      logoutMutation.isPending,

    // Ekrana basılmaya hazır Türkçe mesajlar; hata yoksa null.
    loginError: loginMutation.error
      ? toLoginErrorMessage(loginMutation.error)
      : null,
    registerError: registerMutation.error
      ? toRegisterErrorMessage(registerMutation.error)
      : null,
    error: loginMutation.error ?? registerMutation.error ?? null,

    resetLoginError: loginMutation.reset,
    resetRegisterError: registerMutation.reset,
  };
}
