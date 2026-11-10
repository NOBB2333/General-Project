export interface ProjectCalendarDateInput {
  endDate: null | string;
  startDate: null | string;
}

export type ProjectCalendarSpan =
  { endDate: string; kind: 'dated'; startDate: string } | { kind: 'invalid' } | { kind: 'undated' };

export interface ProjectCalendarGridCell {
  dateKey: string;
  day: number;
  inCurrentMonth: boolean;
}

function isValidDatePart(value: string): boolean {
  const match = /^(\d{4})-(\d{2})-(\d{2})$/.exec(value);
  if (!match) return false;

  const year = Number(match[1]);
  const month = Number(match[2]);
  const day = Number(match[3]);
  const date = new Date(Date.UTC(year, month - 1, day));

  return date.getUTCFullYear() === year && date.getUTCMonth() === month - 1 && date.getUTCDate() === day;
}

export function projectCalendarDatePart(value: null | string): null | string {
  if (!value) return null;

  const datePart = value.slice(0, 10);
  return isValidDatePart(datePart) ? datePart : null;
}

export function getProjectCalendarSpan(item: ProjectCalendarDateInput): ProjectCalendarSpan {
  const hasStart = item.startDate !== null && item.startDate !== undefined;
  const hasEnd = item.endDate !== null && item.endDate !== undefined;
  const startDate = projectCalendarDatePart(item.startDate);
  const endDate = projectCalendarDatePart(item.endDate);

  if ((hasStart && !startDate) || (hasEnd && !endDate)) return { kind: 'invalid' };
  if (!startDate && !endDate) return { kind: 'undated' };

  const normalizedStart = startDate || endDate;
  const normalizedEnd = endDate || startDate;
  if (!normalizedStart || !normalizedEnd || normalizedEnd < normalizedStart) return { kind: 'invalid' };

  return { endDate: normalizedEnd, kind: 'dated', startDate: normalizedStart };
}

export function projectCalendarLocalDateKey(date: Date): string {
  const month = `${date.getMonth() + 1}`.padStart(2, '0');
  const day = `${date.getDate()}`.padStart(2, '0');
  return `${date.getFullYear()}-${month}-${day}`;
}

export function buildProjectCalendarMonthGrid(year: number, monthIndex: number): ProjectCalendarGridCell[] {
  const firstOfMonth = new Date(year, monthIndex, 1);
  const mondayOffset = (firstOfMonth.getDay() + 6) % 7;
  const gridStart = new Date(year, monthIndex, 1 - mondayOffset);

  return Array.from({ length: 42 }, (_, index) => {
    const date = new Date(gridStart);
    date.setDate(gridStart.getDate() + index);
    const dateKey = projectCalendarLocalDateKey(date);

    return {
      dateKey,
      day: date.getDate(),
      inCurrentMonth: date.getMonth() === monthIndex && date.getFullYear() === year,
    };
  });
}

export function isProjectCalendarItemVisibleInMonth(
  item: ProjectCalendarDateInput,
  year: number,
  monthIndex: number,
): boolean {
  const span = getProjectCalendarSpan(item);
  if (span.kind !== 'dated') return false;

  const firstOfMonth = projectCalendarLocalDateKey(new Date(year, monthIndex, 1));
  const lastOfMonth = projectCalendarLocalDateKey(new Date(year, monthIndex + 1, 0));
  return span.startDate <= lastOfMonth && span.endDate >= firstOfMonth;
}

export function isProjectCalendarItemOnDate(item: ProjectCalendarDateInput, dateKey: string): boolean {
  const span = getProjectCalendarSpan(item);
  return span.kind === 'dated' && span.startDate <= dateKey && dateKey <= span.endDate;
}
