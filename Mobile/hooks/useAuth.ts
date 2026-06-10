// Auth store'a ergonomik erişim için custom hook.
// Ekranlar auth state'i ve login/logout aksiyonlarını buradan tüketir.

import { useCallback } from 'react';

import { useAuthStore } from '@/stores/authStore';
import type { LoginResponse } from '@/types/auth';

// TODO: Backend integration in Step 3 — bu mock gerçek API çağrısıyla değişecek.
const MOCK_EMAIL = 'test@cansayno.com';
const MOCK_PASSWORD = '123456';
const MOCK_LATENCY_MS = 800;

// Mock token üretici — login/register paylaşır.
function mockTokens() {
  const now = Date.now();
  return {
    accessToken: 'mock-access-token-' + now,
    refreshToken: 'mock-refresh-token-' + now,
    accessTokenExpiresAt: now + 15 * 60 * 1000, // 15 dakika
  };
}

async function mockLogin(
  email: string,
  password: string,
): Promise<LoginResponse> {
  // Gerçek API hissi için kısa bir gecikme.
  await new Promise((resolve) => setTimeout(resolve, MOCK_LATENCY_MS));

  if (email !== MOCK_EMAIL || password !== MOCK_PASSWORD) {
    throw new Error('E-posta veya şifre hatalı');
  }

  return {
    user: {
      id: 'mock-user-1',
      email: MOCK_EMAIL,
      firstName: 'Test',
      lastName: 'User',
      emailVerified: true,
    },
    tokens: mockTokens(),
  };
}

export interface RegisterData {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
}

async function mockRegister(data: RegisterData): Promise<LoginResponse> {
  // Gerçek API hissi için kısa bir gecikme.
  await new Promise((resolve) => setTimeout(resolve, MOCK_LATENCY_MS));

  return {
    user: {
      id: 'mock-user-' + Date.now(),
      email: data.email,
      firstName: data.firstName,
      lastName: data.lastName,
      // emailVerified: false → ileride e-posta doğrulama akışı eklenecek.
      emailVerified: false,
    },
    tokens: mockTokens(),
  };
}

export function useAuth() {
  const user = useAuthStore((state) => state.user);
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);
  const isRehydrating = useAuthStore((state) => state.isRehydrating);
  const setAuth = useAuthStore((state) => state.setAuth);
  const clearAuth = useAuthStore((state) => state.clearAuth);

  const login = useCallback(
    async (email: string, password: string) => {
      // TODO: Backend integration in Step 3 — mockLogin yerine gerçek API.
      const { user: loggedInUser, tokens } = await mockLogin(email, password);
      await setAuth(loggedInUser, tokens);
    },
    [setAuth],
  );

  const register = useCallback(
    async (data: RegisterData) => {
      // TODO: Backend integration in Step 3 — mockRegister yerine gerçek API.
      const { user: newUser, tokens } = await mockRegister(data);
      await setAuth(newUser, tokens);
    },
    [setAuth],
  );

  const logout = useCallback(async () => {
    await clearAuth();
  }, [clearAuth]);

  return {
    user,
    isAuthenticated,
    isRehydrating,
    login,
    register,
    logout,
  };
}
