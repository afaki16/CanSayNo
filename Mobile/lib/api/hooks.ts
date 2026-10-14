// TanStack Query yardımcıları. apiGet/apiPost, backend Result zarfını açar ve
// success=false durumunda ApiError fırlatır — böylece useQuery/useMutation
// hataları tek tip (ApiError) görür.

import type { UseMutationOptions, UseQueryOptions } from '@tanstack/react-query';

import { apiClient, ApiError, parseApiErrorBody } from './client';
import type { ApiResponse } from './client';

// success=false ile 2xx dönen (Result pattern) yanıtları da hataya çevirir.
function assertSuccess(response: {
  status: number;
  data: ApiResponse<unknown>;
}): void {
  if (response.data.success) {
    return;
  }
  const { message, errors, code } = parseApiErrorBody(
    response.data,
    'İstek başarısız oldu',
  );
  throw new ApiError(message, response.status, errors, code);
}

// API'den gelen ApiResponse'u unwrap eder, data'yı döner; aksi halde throw.
// Query string'i params ile geçir; undefined değerleri axios zaten atlar.
export async function apiGet<T>(
  url: string,
  params?: Record<string, string | number | boolean | undefined>,
): Promise<T> {
  const response = await apiClient.get<ApiResponse<T>>(url, { params });
  assertSuccess(response);
  if (response.data.data === undefined) {
    throw new ApiError('Sunucu beklenen veriyi döndürmedi', response.status);
  }
  return response.data.data;
}

export async function apiPost<TResponse, TBody = unknown>(
  url: string,
  body: TBody,
): Promise<TResponse> {
  const response = await apiClient.post<ApiResponse<TResponse>>(url, body);
  assertSuccess(response);
  if (response.data.data === undefined) {
    throw new ApiError('Sunucu beklenen veriyi döndürmedi', response.status);
  }
  return response.data.data;
}

// data DÖNMEYEN uçlar için. Backend bazı komutlarda sadece
// { success: true, message: "..." } dönüyor (ör. /api/Auth/logout);
// apiPost bunu "veri yok" diye hataya çevirirdi.
// Sadece success bayrağını doğrular, gövdedeki message'ı geri verir.
export async function apiPostVoid<TBody = unknown>(
  url: string,
  body: TBody,
): Promise<string | undefined> {
  const response = await apiClient.post<ApiResponse<never>>(url, body);
  assertSuccess(response);
  return response.data.message;
}

// Hook tipleri: queryKey/queryFn (veya mutationFn) hariç tüm seçenekler.
export type ApiQueryOptions<T> = Omit<
  UseQueryOptions<T, ApiError>,
  'queryKey' | 'queryFn'
>;
export type ApiMutationOptions<T, V> = Omit<
  UseMutationOptions<T, ApiError, V>,
  'mutationFn'
>;
