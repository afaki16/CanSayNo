// Zustand auth store: uygulamadaki tek auth source of truth.
// Token'lar secure storage'da kalıcı saklanır; state ile storage senkron tutulur.
//
// persist middleware KULLANILMIYOR — rehydrate logic'ini kendimiz yönetiyoruz
// (çift persist + secure store uyumsuzluğu yaşamamak için).

import { create } from 'zustand';

import { secureStorage, STORAGE_KEYS } from '@/lib/storage/secureStorage';
import type { AuthTokens, User } from '@/types/auth';

export interface AuthState {
  user: User | null;
  accessToken: string | null;
  refreshToken: string | null;
  isAuthenticated: boolean; // user !== null && accessToken !== null
  isRehydrating: boolean; // app açılışta token yükleniyor mu
}

export interface AuthActions {
  // Login başarılı olunca çağrılır.
  setAuth: (user: User, tokens: AuthTokens) => Promise<void>;

  // Logout / token expire olunca.
  clearAuth: () => Promise<void>;

  // Sadece access token güncelle (refresh token rotation sonrası).
  updateAccessToken: (accessToken: string) => Promise<void>;

  // App açılışta secure storage'dan oku, state'i doldur.
  rehydrate: () => Promise<void>;

  // User bilgisi güncelle (profil edit sonrası).
  updateUser: (user: Partial<User>) => Promise<void>;
}

const initialState: AuthState = {
  user: null,
  accessToken: null,
  refreshToken: null,
  isAuthenticated: false,
  isRehydrating: true,
};

export const useAuthStore = create<AuthState & AuthActions>()((set, get) => ({
  ...initialState,

  setAuth: async (user, tokens) => {
    try {
      await Promise.all([
        secureStorage.set(STORAGE_KEYS.ACCESS_TOKEN, tokens.accessToken),
        secureStorage.set(STORAGE_KEYS.REFRESH_TOKEN, tokens.refreshToken),
        secureStorage.setJSON(STORAGE_KEYS.USER, user),
      ]);
      set({
        user,
        accessToken: tokens.accessToken,
        refreshToken: tokens.refreshToken,
        isAuthenticated: true,
      });
    } catch (error) {
      console.error('[authStore] setAuth başarısız:', error);
    }
  },

  clearAuth: async () => {
    try {
      await Promise.all([
        secureStorage.delete(STORAGE_KEYS.ACCESS_TOKEN),
        secureStorage.delete(STORAGE_KEYS.REFRESH_TOKEN),
        secureStorage.delete(STORAGE_KEYS.USER),
      ]);
    } catch (error) {
      console.error('[authStore] clearAuth storage temizliği başarısız:', error);
    } finally {
      // Storage temizlenemese bile state mutlaka sıfırlanır.
      set({
        user: null,
        accessToken: null,
        refreshToken: null,
        isAuthenticated: false,
      });
    }
  },

  updateAccessToken: async (accessToken) => {
    try {
      await secureStorage.set(STORAGE_KEYS.ACCESS_TOKEN, accessToken);
      // Refresh token'a dokunma; sadece access token'ı güncelle.
      set((state) => ({
        accessToken,
        isAuthenticated: state.user !== null && accessToken !== null,
      }));
    } catch (error) {
      console.error('[authStore] updateAccessToken başarısız:', error);
    }
  },

  rehydrate: async () => {
    try {
      const [accessToken, refreshToken, user] = await Promise.all([
        secureStorage.get(STORAGE_KEYS.ACCESS_TOKEN),
        secureStorage.get(STORAGE_KEYS.REFRESH_TOKEN),
        secureStorage.getJSON<User>(STORAGE_KEYS.USER),
      ]);

      if (user && accessToken) {
        set({
          user,
          accessToken,
          refreshToken,
          isAuthenticated: true,
        });
      }
      // Eksik veri varsa state default'larda kalır.
      // NOT: token validity gerçek API ile DOĞRULANMIYOR (sonraki adımda eklenecek).
    } catch (error) {
      console.error('[authStore] rehydrate başarısız:', error);
    } finally {
      set({ isRehydrating: false });
    }
  },

  updateUser: async (partial) => {
    try {
      const current = get().user;
      if (!current) {
        return;
      }
      const updated: User = { ...current, ...partial };
      await secureStorage.setJSON(STORAGE_KEYS.USER, updated);
      set({ user: updated });
    } catch (error) {
      console.error('[authStore] updateUser başarısız:', error);
    }
  },
}));
