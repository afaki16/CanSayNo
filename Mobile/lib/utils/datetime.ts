// Tarih/zaman yardımcıları. UI'da relative ("2 saat önce") gösterim için; metin şu anki dilde.

import { t } from '@/lib/i18n';

const MINUTE = 60 * 1000;
const HOUR = 60 * MINUTE;
const DAY = 24 * HOUR;
const WEEK = 7 * DAY;
const MONTH = 30 * DAY;
const YEAR = 365 * DAY;

// ISO tarih string'ini "şimdi"ye göre relative metne çevirir.
export function formatRelativeTime(iso: string, now: Date = new Date()): string {
  const diff = now.getTime() - new Date(iso).getTime();
  if (diff < MINUTE) return t('practice.time.justNow');
  if (diff < HOUR) return t('practice.time.minutesAgo', { count: Math.floor(diff / MINUTE) });
  if (diff < DAY) return t('practice.time.hoursAgo', { count: Math.floor(diff / HOUR) });
  if (diff < WEEK) return t('practice.time.daysAgo', { count: Math.floor(diff / DAY) });
  if (diff < MONTH) return t('practice.time.weeksAgo', { count: Math.floor(diff / WEEK) });
  if (diff < YEAR) return t('practice.time.monthsAgo', { count: Math.floor(diff / MONTH) });
  return t('practice.time.yearsAgo', { count: Math.floor(diff / YEAR) });
}
