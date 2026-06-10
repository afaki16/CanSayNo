import type {
  LoginRequest,
  RegisterRequest,
  LoginResponse,
  User,
  Session,
} from "~/types";
import { API_ENDPOINTS } from "~/utils/apiEndpoints";
import { useAuthStore } from "~/stores/auth";
import { useApi } from "./useApi";
import { useToast } from "./useToast";

/**
 * Login response normalizer.
 * Backend tek formatta dönmeli; bu fonksiyon defensive katmandır.
 * Geçici — backend response shape'i tek formata sabitlendiğinde basitleştirilecek.
 */
function extractLoginData(response: any): LoginResponse | null {
  if (!response) return null;

  // Backend BaseController formatı: { success: true, data: {...} }
  if (response.success && response.data?.accessToken) return response.data;

  // Result<T> formatı: { isSuccess: true, value: {...} }
  if (response.isSuccess && response.value?.accessToken) return response.value;

  // Düz format (eski clientlar)
  if (response.accessToken) return response;

  return null;
}

export const useAuth = () => {
  const api = useApi();
  const authStore = useAuthStore();
  const router = useRouter();
  const toast = useToast();

  /**
   * Device ID — cihazı tanımak için (refresh token rotation kontrolünde
   * backend tarafından kullanılır). Hassas değil; XSS endişesi yok.
   * Cookie'de tutulur (clearAuth'da temizlenir).
   */
  const generateDeviceId = (): string => {
    if (!import.meta.client) {
      return `device_${Date.now()}_${Math.random().toString(36).slice(2, 11)}`;
    }

    const deviceIdCookie = useCookie("device_id", {
      maxAge: 60 * 60 * 24 * 30, // 30 gün
      secure: true,
      sameSite: "strict",
    });

    if (!deviceIdCookie.value) {
      deviceIdCookie.value = `device_${Date.now()}_${Math.random().toString(36).slice(2, 11)}`;
    }
    return deviceIdCookie.value;
  };

  const getDeviceName = (): string => {
    if (!import.meta.client) return "Unknown Device";

    const ua = navigator.userAgent;
    if (ua.includes("Windows")) return "Windows Device";
    if (ua.includes("Mac")) return "Mac Device";
    if (ua.includes("Linux")) return "Linux Device";
    if (ua.includes("Android")) return "Android Device";
    if (ua.includes("iOS") || /iPhone|iPad|iPod/.test(ua)) return "iOS Device";
    return "Unknown Device";
  };

  const login = async (credentials: LoginRequest) => {
    try {
      authStore.setLoading(true);

      const requestData = {
        ...credentials,
        deviceId: credentials.deviceId || generateDeviceId(),
        deviceName: credentials.deviceName || getDeviceName(),
      };

      const response = await api.post<LoginResponse>(
        API_ENDPOINTS.AUTH.LOGIN,
        requestData,
        { silent: true },
      );

      const loginData = extractLoginData(response);

      if (!loginData?.accessToken) {
        throw new Error("Invalid login response format");
      }

      const deviceId = credentials.deviceId || generateDeviceId();
      await authStore.setAuth(loginData, deviceId);

      await router.push("/dashboard");
      return loginData;
    } catch (error) {
      throw error;
    } finally {
      authStore.setLoading(false);
    }
  };

  const register = async (userData: RegisterRequest) => {
    try {
      authStore.setLoading(true);
      const response = await api.post<User>(
        API_ENDPOINTS.AUTH.REGISTER,
        userData,
      );

      if (response.success) {
        await router.push(
          `/auth/check-email?email=${encodeURIComponent(userData.email)}`,
        );
        return response.data;
      }
    } catch (error) {
      throw error;
    } finally {
      authStore.setLoading(false);
    }
  };

  const verifyEmail = async (email: string, token: string) => {
    return await api.post(
      API_ENDPOINTS.AUTH.VERIFY_EMAIL,
      { email, token },
      { silent: true },
    );
  };

  const resendVerification = async (email: string, tenantId: number = 0) => {
    return await api.post(
      API_ENDPOINTS.AUTH.RESEND_VERIFICATION,
      { email, tenantId },
      { silent: true },
    );
  };

  const logout = async () => {
    try {
      // Refresh token HttpOnly cookie'de — backend okuyup revoke eder
      await api.post(API_ENDPOINTS.AUTH.LOGOUT, {});
    } catch {
      // Logout her zaman client tarafında temizlik yapılmalı
    } finally {
      authStore.clearAuth();
      await router.push("/");
    }
  };

  /**
   * Manuel refresh — çoğu zaman gerek yok (axios interceptor otomatik yapıyor).
   * Sadece özel durumlar için (örn: kullanıcı "session'ı yenile" butonuna basarsa).
   */
  const refreshToken = async () => {
    const success = await authStore.silentRefresh();
    if (!success) {
      throw new Error("Token refresh failed");
    }
  };

  const getCurrentUser = async () => {
    try {
      const response = await api.get<User>(API_ENDPOINTS.AUTH.ME);
      if (response.success && response.data) {
        authStore.setUser(response.data);
        return response.data;
      }
    } catch (error) {
      throw error;
    }
  };

  const hasPermission = (permission: string): boolean => {
    return authStore.permissions.includes(permission);
  };

  const hasRole = (role: string): boolean => {
    return authStore.roles.includes(role);
  };

  const hasAnyPermission = (permissions: string[]): boolean => {
    return permissions.some((p) => hasPermission(p));
  };

  const hasAllPermissions = (permissions: string[]): boolean => {
    return permissions.every((p) => hasPermission(p));
  };

  const hasSystemRole = (): boolean => {
    return authStore.user?.roles?.some((r) => r.isSystemRole) ?? false;
  };

  const getUserSessions = async (): Promise<Session[]> => {
    try {
      const response = await api.get<Session[]>(API_ENDPOINTS.AUTH.SESSIONS);
      const list =
        (response as any)?.data ?? (response as any)?.value ?? response ?? [];
      return Array.isArray(list) ? list : [];
    } catch {
      return [];
    }
  };

  const logoutDevice = async (deviceId: string): Promise<void> => {
    await api.post(API_ENDPOINTS.AUTH.LOGOUT_DEVICE(deviceId), {});
  };

  const revokeSessionById = async (sessionId: number): Promise<void> => {
    await api.post(API_ENDPOINTS.AUTH.REVOKE_SESSION_BY_ID(sessionId), {});
  };

  const changePassword = async (
    currentPassword: string,
    newPassword: string,
  ): Promise<void> => {
    await api.post(API_ENDPOINTS.AUTH.CHANGE_PASSWORD, {
      currentPassword,
      newPassword,
    });
    toast.success("Şifreniz başarıyla güncellendi");
  };

  const logoutAllDevices = async (): Promise<void> => {
    try {
      await api.post(API_ENDPOINTS.AUTH.LOGOUT_ALL, {});
    } finally {
      authStore.clearAuth();
      await router.push("/");
    }
  };

  return {
    login,
    register,
    logout,
    refreshToken,
    getCurrentUser,
    verifyEmail,
    resendVerification,
    hasPermission,
    hasRole,
    hasAnyPermission,
    hasAllPermissions,
    hasSystemRole,
    getUserSessions,
    logoutDevice,
    logoutAllDevices,
    revokeSessionById,
    changePassword,
  };
};
