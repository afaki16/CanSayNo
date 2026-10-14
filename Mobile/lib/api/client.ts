// Ana axios instance + ortak tipler. Request interceptor burada (Bearer token).
// Response interceptor (401 → token refresh) ayrı dosyada: ./refreshInterceptor.
// Tüketim için barrel kullan: import { apiClient } from '@/lib/api'.

import axios from 'axios';
import type {
  AxiosError,
  AxiosInstance,
  InternalAxiosRequestConfig,
} from 'axios';

import { env } from '@/lib/config/env';
import { useAuthStore } from '@/stores/authStore';

// Backend'in başarılı Result zarfı.
// - Veri dönen uçlar:  { success: true, data: T }
// - Veri dönmeyen uçlar: { success: true, message: "..." }   (ör. logout)
export interface ApiResponse<T = unknown> {
  success: boolean;
  data?: T;
  message?: string;
  error?: ApiErrorPayload;
}

// Backend'in hata zarfı — gerçek gövde şu şekilde:
//   { "success": false,
//     "error": { "status": 401, "code": "unauthorized",
//                "errors": { "error": ["Email veya şifre hatalı"] } } }
//
// `code` çoğunlukla camelCase string ("unauthorized", "emailNotConfirmed"),
// ama bazı yollarda (FluentValidation handler, JWT challenge) sayısal enum
// değeri olarak geliyor (1001, 1012). İkisini de karşılamak zorundayız.
export interface ApiErrorPayload {
  status?: number;
  code?: string | number;
  message?: string;
  errors?: Record<string, string[]>;
}

// Backend'in ErrorCode enum'u. Sayısal karşılıkları
// CanSayNo.Domain/Common/Enums/ErrorCode.cs ile eşleşir.
export const API_ERROR_CODE = {
  INVALID_REQUEST: 'invalidRequest',
  VALIDATION_FAILED: 'validationFailed',
  UNAUTHORIZED: 'unauthorized',
  NOT_FOUND: 'notFound',
  EMAIL_NOT_CONFIRMED: 'emailNotConfirmed',
} as const;

export type ApiErrorCode = (typeof API_ERROR_CODE)[keyof typeof API_ERROR_CODE];

const NUMERIC_ERROR_CODES: Record<number, string> = {
  1000: API_ERROR_CODE.INVALID_REQUEST,
  1001: API_ERROR_CODE.VALIDATION_FAILED,
  1002: API_ERROR_CODE.NOT_FOUND,
  1012: API_ERROR_CODE.UNAUTHORIZED,
  1013: API_ERROR_CODE.EMAIL_NOT_CONFIRMED,
};

// Uygulama içi standart hata tipi. Tüm interceptor/hook hataları buna dönüşür.
// status 0 → istek sunucuya hiç ulaşamadı (ağ hatası / timeout).
export class ApiError extends Error {
  constructor(
    message: string,
    public status: number,
    public errors: string[] = [],
    public code?: string,
    public originalError?: AxiosError,
  ) {
    super(message);
    this.name = 'ApiError';
  }

  get isNetworkError(): boolean {
    return this.status === 0;
  }
}

function normalizeCode(code: string | number | undefined): string | undefined {
  if (typeof code === 'number') {
    return NUMERIC_ERROR_CODES[code] ?? String(code);
  }
  return code;
}

// Backend hatalarını tek tip hale getirir. Üç farklı gövde şekli görebiliyoruz:
//  1. Result zarfı:   { success: false, error: { code, errors: { alan: [...] } } }
//  2. ProblemDetails: { title, status, errors: { alan: [...] } }  (model binding)
//  3. Hiçbiri (HTML hata sayfası, boş gövde) → fallback mesaj.
export function parseApiErrorBody(
  body: unknown,
  fallbackMessage: string,
): { message: string; errors: string[]; code?: string } {
  const envelope = (body ?? {}) as ApiResponse & {
    title?: string;
    errors?: Record<string, string[]>;
  };
  const payload = envelope.error;

  // Alan bazlı hata sözlüğünü düz bir listeye indir.
  const errorDict = payload?.errors ?? envelope.errors;
  const errors = errorDict
    ? Object.values(errorDict).flat().filter(Boolean)
    : [];

  const message =
    payload?.message ??
    errors[0] ??
    envelope.message ??
    envelope.title ??
    fallbackMessage;

  return { message, errors, code: normalizeCode(payload?.code) };
}

export const apiClient: AxiosInstance = axios.create({
  baseURL: env.apiBaseUrl,
  timeout: 15000, // 15 saniye
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor: her isteğe geçerli access token'ı Bearer olarak ekle.
apiClient.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const accessToken = useAuthStore.getState().accessToken;
    if (accessToken) {
      config.headers.Authorization = `Bearer ${accessToken}`;
    }

    if (env.isDev) {
      console.log(`[API] ${config.method?.toUpperCase()} ${config.url}`);
    }

    return config;
  },
  (error) => Promise.reject(error),
);

// NOT: Response interceptor (401 refresh) ./refreshInterceptor'da kayıt edilir.
// Barrel (./index) o dosyayı side-effect olarak import eder.
