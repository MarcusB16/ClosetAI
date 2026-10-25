export function isoDay(d: Date = new Date()): string {
  const y = d.getFullYear();
  const m = String(d.getMonth() + 1).padStart(2, '0');
  const day = String(d.getDate()).padStart(2, '0');
  return `${y}-${m}-${day}`;
}

export function daysAgoIso(n: number, from: Date = new Date()): string {
  const d = new Date(from);
  d.setDate(d.getDate() - n);
  return isoDay(d);
}

export function parseDay(iso: string): Date {
  const [y, m, d] = iso.split('-').map(Number);
  return new Date(y, m - 1, d);
}

export function daysSince(iso: string | undefined, today: Date = new Date()): number {
  if (!iso) return Infinity;
  const t = parseDay(isoDay(today)).getTime();
  return Math.round((t - parseDay(iso).getTime()) / 86_400_000);
}

export const fmtDay = (iso: string) =>
  parseDay(iso).toLocaleDateString('en-US', { weekday: 'short', month: 'short', day: 'numeric' });
export const fmtMonthDay = (iso: string) =>
  parseDay(iso).toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
export const fmtMonth = (iso: string) => parseDay(iso).toLocaleDateString('en-US', { month: 'short' });
export const weekdayShort = (iso: string) => parseDay(iso).toLocaleDateString('en-US', { weekday: 'short' });
