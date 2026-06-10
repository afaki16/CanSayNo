import { defineStore } from "pinia";
import type { User, LoginResponse, AuthState } from "~/types";
import { jwtDecode } from "jwt-decode";

/**
 * In-flight silentRefresh promise — parallel calls (e.g. auth.global.ts +
 * guest.ts middleware fire simultaneously) should share a single HTTP request.
 * Reset to null when the request settles.
 */
let inFlightSilentRefresh: Promise<boolean> | null = null;

/**
 * Auth store — güvenlik notları:
 *
 * 1) Access token SADECE memory'de (bu store'da) tutulur.
 *    Cookie veya localStorage'a YAZILMAZ — XSS ile çalınmasını önler.
 *
 * 2) Refresh token backend tarafında HttpOnly cookie olarak set edilir
 *    (AuthController.WriteRefreshTokenCookie). JS hiçbir zaman okuyamaz.
 *
 * 3) Sayfa refresh'inden sonra access token kaybolur. initializeAuth()
 *    çağrıldığında refresh-token endpoint'i çağrılıp yeni access token alınır
 *    (HttpOnly cookie tarayıcı tarafından otomatik gönderilir).
 *
 * 4) User data da memory'de tutulur. Sayfa refresh'inde /api/auth/me ile
 *    backend'den yeniden çekilir (single source of truth).
 *
 * 5) deviceId XSS açısından hassas değildir (anonim cihaz tanımlayıcı),
 *    cookie'de tutulur.
 */

interface JwtPayload {
  exp: number;
  iat?: number;
  sub?: string;
  [key: string]: unknown;
}

export const useAuthStore = defineStore("auth", {
  state: (): AuthState => ({
    user: null,
    accessToken: null,
    refreshToken: null, // Artık kullanılmıyor — HttpOnly cookie'de saklanıyor
    isAuthenticated: false,
    isLoading: false,
    permissions: [],
    roles: [],
  }),

  getters: {
    isLoggedIn: (state) => state.isAuthenticated && !!state.accessToken,

    userFullName: (state) =>
      state.user ? `${state.user.firstName} ${state.user.lastName}` : "",

    userInitials: (state) => {
      if (!state.user) return "";
      return `${state.user.firstName.charAt(0)}${state.user.lastName.charAt(0)}`.toUpperCase();
    },

    hasPermission: (state) => (permission: string) => {
      return state.permissions.includes(permission);
    },

    hasRole: (state) => (role: string) => {
      return state.roles.includes(role);
    },

    hasAnyPermission: (state) => (permissions: string[]) => {
      return permissions.some((permission) =>
        state.permissions.includes(permission),
      );
    },

    hasAllPermissions: (state) => (permissions: string[]) => {
      return permissions.every((permission) =>
        state.permissions.includes(permission),
      );
    },

    isTokenExpired: (state) => {
      if (!state.accessToken) return true;
      try {
        const decoded = jwtDecode<JwtPayload>(state.accessToken);
        const currentTime = Date.now() / 1000;
        return decoded.exp < currentTime;
      } catch {
        return true;
      }
    },
  },

  actions: {
    /**
     * Login / refresh sonrası çağrılır.
     * Access token MEMORY'DE tutulur — cookie/localStorage'a YAZILMAZ.
     */
    async setAuth(authData: LoginResponse, deviceId?: string) {
      this.user = authData.user;
      this.accessToken = authData.accessToken;
      this.isAuthenticated = true;

      this.permissions = (authData.user?.permissions ?? [])
        .map(
          (p: { fullPermission?: string; name?: string }) =>
            p?.fullPermission ?? p?.name,
        )
        .filter(Boolean) as string[];

      this.roles = authData.user?.roles?.map((r) => r.name) ?? [];

      // deviceId hassas değil — cookie'de tutulması OK (cihazı tanımak için).
      // localStorage YOK artık.
      if (import.meta.client && deviceId !== undefined) {
        const cookieMaxAge = 60 * 60 * 24 * 30; // 30 gün
        const deviceIdCookie = useCookie("device_id", {
          maxAge: cookieMaxAge,
          secure: true,
          sameSite: "strict",
        });
        deviceIdCookie.value = deviceId;
      }
    },

    setUser(user: User) {
      this.user = user;
      this.permissions = (user?.permissions ?? [])
        .map(
          (p: { fullPermission?: string; name?: string }) =>
            p?.fullPermission ?? p?.name,
        )
        .filter(Boolean) as string[];
      this.roles = user?.roles?.map((r) => r.name) ?? [];
    },

    clearAuth() {
      this.user = null;
      this.accessToken = null;
      this.refreshToken = null;
      this.isAuthenticated = false;
      this.permissions = [];
      this.roles = [];

      // deviceId cookie'sini de temizle (logout = cihaz unutma)
      if (import.meta.client) {
        const deviceIdCookie = useCookie("device_id");
        deviceIdCookie.value = null;

        // Eski versiyonlardan kalmış olabilecek hassas verileri temizle
        this.cleanupLegacyStorage();
      }
    },

    /**
     * Önceki sürümlerde access_token cookie'de ve user localStorage'da
     * tutuluyordu. Yeni sürüme güncelleme yapan kullanıcılarda bu eski
     * veriler kalmış olabilir — temizleyelim.
     */
    cleanupLegacyStorage() {
      if (!import.meta.client) return;
      try {
        localStorage.removeItem("user");
        localStorage.removeItem("deviceId"); // deviceId cookie'ye taşındı
        const oldAccessTokenCookie = useCookie("access_token");
        if (oldAccessTokenCookie.value) {
          oldAccessTokenCookie.value = null;
        }
      } catch (error) {
        console.warn("Legacy storage cleanup failed:", error);
      }
    },

    setLoading(loading: boolean) {
      this.isLoading = loading;
    },

    /**
     * Uygulama başlangıcında / sayfa refresh sonrası çağrılır.
     *
     * Akış:
     *   1. Eski versiyondan kalan veriyi temizle
     *   2. Zaten authenticated isek atla
     *   3. Refresh-token endpoint'ini çağır (HttpOnly cookie otomatik gider)
     *   4. Başarılıysa yeni access token + user data state'e yazılır
     *   5. Başarısızsa kullanıcı logged-out kabul edilir (sessizce)
     *
     * Bu fonksiyon HİÇBİR ZAMAN exception throw ETMEZ.
     */
    async initializeAuth() {
      if (!import.meta.client) return;

      // Eski sürümden kalan veriyi temizle (idempotent)
      this.cleanupLegacyStorage();

      // Zaten authenticated isek tekrar refresh atmaya gerek yok
      if (this.isAuthenticated && this.accessToken && !this.isTokenExpired) {
        return;
      }

      // device_id cookie'si bu cihazda önceden login olunduğunu işaret eder.
      // Yoksa: ya hiç login olmamış ya da logout edilmiş — refresh anlamsız.
      // Varsa: refresh_token HttpOnly cookie de büyük ihtimal mevcut (remember me).
      // Bu kontrol login sayfasındaki boş 400 isteklerini önler.
      const deviceIdCookie = useCookie("device_id");
      if (!deviceIdCookie.value) {
        this.isAuthenticated = false;
        this.accessToken = null;
        return;
      }

      try {
        await this.silentRefresh();
      } catch {
        // Refresh başarısız — kullanıcı authenticate olmadı,
        // ama bu normal bir durum (refresh_token expired, revoked vb.)
        this.isAuthenticated = false;
        this.accessToken = null;
      }
    },

    /**
     * Refresh token HttpOnly cookie'sini kullanarak yeni access token alır.
     */
    async silentRefresh(): Promise<boolean> {
      // Deduplication: zaten devam eden bir silentRefresh varsa onun sonucunu paylaş,
      // yeni HTTP isteği atma. auth.global.ts + guest.ts middleware'leri paralel
      // çağırdığında ikinci çağrı ilki bittiğinde aynı sonucu alır.
      if (inFlightSilentRefresh) {
        return inFlightSilentRefresh;
      }

      const refreshPromise = this._doSilentRefresh();
      inFlightSilentRefresh = refreshPromise;
      try {
        return await refreshPromise;
      } finally {
        inFlightSilentRefresh = null;
      }
    },

    /**
     * Internal — actual HTTP call. silentRefresh wraps this with deduplication.
     * Don't call this directly; use silentRefresh().
     */
    async _doSilentRefresh(): Promise<boolean> {
      const { $api } = useNuxtApp();
      try {
        // Refresh token HttpOnly cookie'de — withCredentials ile otomatik gider.
        // Body BOŞ — backend cookie'den okur.
        const response = await ($api as any).post(
          "/api/auth/refresh-token",
          {},
          { _skipAuthRefresh: true },
        );

        const data =
          response?.data?.data ?? response?.data?.value ?? response?.data;

        if (!data?.accessToken) {
          return false;
        }

        this.accessToken = data.accessToken;
        this.isAuthenticated = true;

        // User data backend'den geldiyse kullan, yoksa ayrıca /me çağır
        if (data.user) {
          this.setUser(data.user);
        } else {
          await this.fetchUserFromApi(false);
        }

        return true;
      } catch {
        return false;
      }
    },

    async fetchUserFromApi(clearOnError = true) {
      const { $api } = useNuxtApp();
      try {
        const response = await $api.get("/api/auth/me");
        const user =
          response.data?.data ?? response.data?.value ?? response.data;
        if (user) {
          this.setUser(user);
        } else if (clearOnError) {
          this.clearAuth();
        }
      } catch (error) {
        if (clearOnError) this.clearAuth();
      }
    },
  },
});
