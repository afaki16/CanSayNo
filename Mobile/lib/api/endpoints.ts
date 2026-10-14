// Backend endpoint path'leri — tek kaynak. Hiçbir yerde çıplak path string'i
// yazma; buradan import et.
//
// Path'ler backend'deki controller route'una birebir yazıldı (`/api/Auth/...`).
// ASP.NET routing case-insensitive'dir, ama karşılaştırma yaparken (ör.
// interceptor'da) küçük harfe indirerek kıyasla — bkz. isPath().

export const AUTH_ENDPOINTS = {
  LOGIN: '/api/Auth/login',
  REGISTER: '/api/Auth/register',
  REFRESH_TOKEN: '/api/Auth/refresh-token',
  LOGOUT: '/api/Auth/logout',
  ME: '/api/Auth/me',
} as const;


// Senaryo uçları. LIST query param almaz — parametreler axios `params` ile
// geçilir (bkz. scenarioService), elle string birleştirme yok.
export const SCENARIO_ENDPOINTS = {
  LIST: '/api/Scenarios',
  DETAIL: (id: string) => `/api/Scenarios/${id}`,
  BY_CONTEXT: (contextId: string) => `/api/Scenarios/by-context/${contextId}`,
} as const;

// Pratik oturumu uçları. MY sayfalama parametrelerini axios `params` ile alır.
export const SESSION_ENDPOINTS = {
  START: '/api/Sessions',
  DETAIL: (id: string) => `/api/Sessions/${id}`,
  MY: '/api/Sessions/my',
} as const;

// 401 dönseler bile token refresh denenmemesi gereken endpoint'ler:
// bunlar zaten oturum açma yolları, refresh etmek anlamsız (ve yanlış hata
// mesajı üretir — "şifre hatalı" yerine "oturum süresi doldu" gibi).
export const NO_REFRESH_ENDPOINTS: readonly string[] = [
  AUTH_ENDPOINTS.LOGIN,
  AUTH_ENDPOINTS.REGISTER,
];

// Bir istek URL'inin verilen endpoint'e ait olup olmadığını case-insensitive
// kontrol eder. axios config.url baseURL'siz relative path'tir, ama absolute
// gelme ihtimaline karşı includes ile bakıyoruz.
export function isPath(url: string | undefined, endpoint: string): boolean {
  if (!url) {
    return false;
  }
  return url.toLowerCase().includes(endpoint.toLowerCase());
}
