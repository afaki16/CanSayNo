// Bağlam UI metadata'sının (isim, renk, ikon) import noktası. Backend'de karşılığı
// yok, kalıcı — mock değil; ileride lib/constants/'a taşınabilir. Senaryolar,
// oturumlar, chat, değerlendirme ve istatistikler API'den geliyor.

export {
  getContextDescription,
  getContextName,
  getContexts,
  useContexts,
} from './contexts';
