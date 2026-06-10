export function dateFormat(date: string | Date) {
  if (!date) return '';
  const d = new Date(date);
  return d.toLocaleDateString('tr-TR');
}

export function dateTimeFormat(date: string | Date) {
  if (!date) return '';
  const d = new Date(date);
  return d.toLocaleString('tr-TR', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  });
}
export function dateTimeFormatLong(date: string | Date) {
  if (!date) return '';
  const d = new Date(date);
  return d.toLocaleString('tr-TR', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit'
  });
}
