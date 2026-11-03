// Response interceptor: 401 yakalar, refresh token ile yeni access token alır,
// orijinal isteği retry eder. Paralel 401'ler TEK refresh ile çözülür (queue).
//
// Bu dosya yan etki (side effect) olarak çalışır: import edildiğinde
// apiClient'a response interceptor'ı kayıt eder. Barrel (./index) import eder.

import axios from 'axios';
import type {
  AxiosError,
  AxiosResponse,
  InternalAxiosRequestConfig,
} from 'axios';

import { env } from '@/lib/config/env';
import { useAuthStore } from '@/stores/authStore';
import type { AuthResponse, RefreshTokenRequest } from '@/types/api/auth';

import { apiClient, ApiError, parseApiErrorBody } from './client';
import type { ApiResponse } from './client';
import { AUTH_ENDPOINTS, isPath, NO_REFRESH_ENDPOINTS } from './endpoints';

// Refresh sırasında gelen eşzamanlı 401'leri bekleten kuyruk.
let isRefreshing = false;
let failedQueue: {
  resolve: (value: string) => void;
  reject: (reason: unknown) => void;
}[] = [];

function processQueue(error: unknown, token: string | null = null) {
  failedQueue.forEach(({ resolve, reject }) => {
    if (error) {
      reject(error);
    } else if (token) {
      resolve(token);
    }
  });
  failedQueue = [];
}

async function refreshAccessToken(): Promise<string> {
  // Backend refresh-token ucu İKİ token'ı da ister: süresi dolmuş access
  // token + refresh token. Sadece refreshToken gönderilirse
  // 400 "Geçersiz yenileme tokeni" döner.
  const { accessToken: currentAccessToken, refreshToken } =
    useAuthStore.getState();

  if (!refreshToken || !currentAccessToken) {
    throw new Error('No refresh token available');
  }

  const body: RefreshTokenRequest = {
    accessToken: currentAccessToken,
    refreshToken,
  };

  // Direkt axios çağrısı — apiClient kullanırsak interceptor tekrar tetiklenir
  // ve sonsuz döngü oluşur.
  const response = await axios.post<ApiResponse<AuthResponse>>(
    `${env.apiBaseUrl}${AUTH_ENDPOINTS.REFRESH_TOKEN}`,
    body,
    { headers: { 'Content-Type': 'application/json' } },
  );

  if (!response.data.success || !response.data.data) {
    throw new Error('Token refresh failed');
  }

  const { accessToken, refreshToken: newRefreshToken } = response.data.data;

  // Refresh token rotation: backend her refresh'te yeni bir refresh token
  // üretip eskisini iptal ediyor — ikisini de güncellemek zorunlu.
  await useAuthStore.getState().updateTokens(accessToken, newRefreshToken);

  return accessToken;
}

apiClient.interceptors.response.use(
  // Backend Result pattern'inde success=false durumunda da 200 dönebilir.
  // Bunu çağıran kod (apiGet/apiPost) kontrol eder; burada response'a dokunmayız.
  (response: AxiosResponse<ApiResponse>) => response,
  async (error: AxiosError<ApiResponse>) => {
    const originalRequest = error.config as
      | (InternalAxiosRequestConfig & { _retry?: boolean })
      | undefined;
    const url = originalRequest?.url;

    // Login/register 401'i "kimlik bilgisi hatalı" demektir — refresh denemek
    // hem anlamsız hem de kullanıcıya yanlış mesaj gösterir. Aşağıdaki
    // normalizasyona düşsün.
    const skipRefresh = NO_REFRESH_ENDPOINTS.some((endpoint) =>
      isPath(url, endpoint),
    );

    if (
      error.response?.status === 401 &&
      originalRequest &&
      !originalRequest._retry &&
      !skipRefresh
    ) {
      // Refresh endpoint'in kendisi 401 verdiyse: refresh token da expired.
      if (isPath(url, AUTH_ENDPOINTS.REFRESH_TOKEN)) {
        await useAuthStore.getState().clearAuth();
        return Promise.reject(
          new ApiError(
            'Oturumun sona erdi, lütfen tekrar giriş yap',
            401,
            [],
            'sessionExpired',
          ),
        );
      }

      // Halihazırda bir refresh sürüyorsa: kuyruğa al, bitince retry et.
      if (isRefreshing) {
        return new Promise<string>((resolve, reject) => {
          failedQueue.push({ resolve, reject });
        }).then((token) => {
          originalRequest.headers.Authorization = `Bearer ${token}`;
          return apiClient(originalRequest);
        });
      }

      originalRequest._retry = true;
      isRefreshing = true;

      try {
        const newToken = await refreshAccessToken();
        processQueue(null, newToken);
        originalRequest.headers.Authorization = `Bearer ${newToken}`;
        return apiClient(originalRequest);
      } catch (refreshError) {
        processQueue(refreshError, null);

        // Sunucuya hiç ulaşılamadıysa (çevrimdışı açılış, timeout) refresh token
        // geçersiz değildir — oturumu silme, bağlantı gelince tekrar denenir.
        if (axios.isAxiosError(refreshError) && !refreshError.response) {
          return Promise.reject(
            new ApiError('Bağlantı kurulamadı', 0, [], undefined, refreshError),
          );
        }

        await useAuthStore.getState().clearAuth();
        return Promise.reject(
          new ApiError(
            'Oturumun sona erdi, lütfen tekrar giriş yap',
            401,
            [],
            'sessionExpired',
          ),
        );
      } finally {
        isRefreshing = false;
      }
    }

    // Kalan tüm hatalar: ApiError'a normalize et.
    // status 0 → sunucuya hiç ulaşılamadı (ağ hatası / timeout).
    const status = error.response?.status ?? 0;
    const { message, errors, code } = parseApiErrorBody(
      error.response?.data,
      error.message || 'Network error',
    );

    return Promise.reject(new ApiError(message, status, errors, code, error));
  },
);
