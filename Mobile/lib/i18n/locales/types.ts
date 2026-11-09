// İngilizce çeviri dosyaları Türkçe kaynakla AYNI anahtarları taşımalı:
//   export const auth: Translation<typeof trAuth> = { ... }
// Eksik ya da fazla anahtar tsc hatası verir. Değerler düz string (i18next {{değişken}} desteklenir).

export type Translation<T> = {
  [K in keyof T]: T[K] extends string ? string : Translation<T[K]>;
};
