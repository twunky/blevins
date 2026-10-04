// Display formatting for API calendar dates (YYYY-MM-DD) and local times (HH:mm[:ss]).
// Dates are read as calendar dates in UTC so a viewer's time zone never shifts the day.

function calendarDate(value: string | null | undefined): Date | null {
  const [year, month, day] = (value ?? '').split('-').map(Number);
  return year && month && day ? new Date(Date.UTC(year, month - 1, day)) : null;
}

function format(date: Date, options: Intl.DateTimeFormatOptions): string {
  return date.toLocaleDateString('en-US', { ...options, timeZone: 'UTC' });
}

/** Month abbreviation and day number for a date tile. */
export function dateTile(value: string | null | undefined): { month: string; day: string } {
  const date = calendarDate(value);
  return date ? { month: format(date, { month: 'short' }), day: String(date.getUTCDate()) } : { month: '', day: '' };
}

/** "Sat, Oct 5, 2026" */
export function displayDate(value: string | null | undefined): string {
  const date = calendarDate(value);
  return date ? format(date, { weekday: 'short', month: 'short', day: 'numeric', year: 'numeric' }) : (value ?? '');
}

/** "12:00 PM" from "12:00" or "12:00:00". */
export function displayTime(value: string | null | undefined): string {
  const [hours, minutes] = (value ?? '').split(':').map(Number);
  if (Number.isNaN(hours) || Number.isNaN(minutes)) return value ?? '';
  return `${hours % 12 || 12}:${String(minutes).padStart(2, '0')} ${hours < 12 ? 'AM' : 'PM'}`;
}

/** "Sat, Oct 5, 2026 · 12:00 PM" */
export function displayDateTime(date: string | null | undefined, time: string | null | undefined): string {
  return `${displayDate(date)} · ${displayTime(time)}`;
}

/** "Oct 31 – Nov 6, 2026" */
export function displayRange(start: string | null | undefined, end: string | null | undefined): string {
  const from = calendarDate(start);
  const to = calendarDate(end);
  if (!from || !to) return `${start ?? ''} to ${end ?? ''}`;
  const sameYear = from.getUTCFullYear() === to.getUTCFullYear();
  return `${format(from, sameYear ? { month: 'short', day: 'numeric' } : { month: 'short', day: 'numeric', year: 'numeric' })} – ${format(to, { month: 'short', day: 'numeric', year: 'numeric' })}`;
}
