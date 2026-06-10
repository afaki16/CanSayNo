// Tarih/zaman yardımcıları. UI'da Türkçe relative ("2 saat önce") gösterim için.

const MINUTE = 60 * 1000;
const HOUR = 60 * MINUTE;
const DAY = 24 * HOUR;
const WEEK = 7 * DAY;
const MONTH = 30 * DAY;
const YEAR = 365 * DAY;

// ISO tarih string'ini "şimdi"ye göre Türkçe relative metne çevirir.
export function formatRelativeTime(iso: string, now: Date = new Date()): string {
  const diff = now.getTime() - new Date(iso).getTime();
  if (diff < MINUTE) return 'az önce';
  if (diff < HOUR) return `${Math.floor(diff / MINUTE)} dakika önce`;
  if (diff < DAY) return `${Math.floor(diff / HOUR)} saat önce`;
  if (diff < WEEK) return `${Math.floor(diff / DAY)} gün önce`;
  if (diff < MONTH) return `${Math.floor(diff / WEEK)} hafta önce`;
  if (diff < YEAR) return `${Math.floor(diff / MONTH)} ay önce`;
  return `${Math.floor(diff / YEAR)} yıl önce`;
}
