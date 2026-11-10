import { computed, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import type { TeamCalendarCell, TeamCalendarEvent } from '../../model/team-calendar.types';

const REFERENCE_YEAR = 2026;
const REFERENCE_MONTH = 2;
const REFERENCE_TODAY = '2026-03-10';

function formatIso(date: Date) {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

function monthBounds(year: number, month: number) {
  return {
    start: formatIso(new Date(year, month, 1)),
    end: formatIso(new Date(year, month + 1, 0)),
  };
}

function coversDate(event: TeamCalendarEvent, iso: string) {
  return event.date <= iso && iso <= event.endDate;
}

export function useTeamCalendar(referenceEvents: readonly TeamCalendarEvent[]) {
  const events = ref<TeamCalendarEvent[]>(cloneReferenceRows(referenceEvents));
  const visibleMonth = ref(new Date(REFERENCE_YEAR, REFERENCE_MONTH, 1));
  const department = ref('全部部门');
  const eventType = ref('全部类型');
  const detailOpen = ref(false);
  const selectedDate = ref('');
  const selectedEvents = ref<TeamCalendarEvent[]>([]);

  const departmentOptions = ['全部部门', '产品部', '技术部', '市场部', '销售部', '财务部', '人事部'].map((label) => ({
    value: label,
    label,
  }));
  const typeOptions = ['全部类型', '年假', '病假', '事假', '婚假', '产假', '调休', '出差'].map((label) => ({
    value: label,
    label,
  }));

  const monthLabel = computed(() => `${visibleMonth.value.getFullYear()}年${visibleMonth.value.getMonth() + 1}月`);

  const filteredEvents = computed(() =>
    events.value.filter(
      (event) =>
        (department.value === '全部部门' || event.dept === department.value) &&
        (eventType.value === '全部类型' || event.type === eventType.value),
    ),
  );

  const monthEvents = computed(() => {
    const { start, end } = monthBounds(visibleMonth.value.getFullYear(), visibleMonth.value.getMonth());
    return filteredEvents.value.filter((event) => event.date <= end && event.endDate >= start);
  });

  const monthCells = computed<TeamCalendarCell[]>(() => {
    const year = visibleMonth.value.getFullYear();
    const month = visibleMonth.value.getMonth();
    const first = new Date(year, month, 1);
    const mondayOffset = (first.getDay() + 6) % 7;
    const gridStart = new Date(year, month, 1 - mondayOffset);

    return Array.from({ length: 42 }, (_, index) => {
      const date = new Date(gridStart);
      date.setDate(gridStart.getDate() + index);
      const iso = formatIso(date);
      return {
        key: iso,
        iso,
        day: date.getDate(),
        inMonth: date.getMonth() === month,
        isToday: iso === REFERENCE_TODAY,
        events: filteredEvents.value.filter((event) => coversDate(event, iso)),
      };
    });
  });

  const leaveCount = computed(() => monthEvents.value.filter((event) => event.type !== '出差').length);
  const travelCount = computed(() => monthEvents.value.filter((event) => event.type === '出差').length);
  const conflictDates = computed(() => {
    const { start, end } = monthBounds(visibleMonth.value.getFullYear(), visibleMonth.value.getMonth());
    const cursor = new Date(`${start}T00:00:00`);
    const finalDay = new Date(`${end}T00:00:00`);
    let conflicts = 0;
    while (cursor <= finalDay) {
      const iso = formatIso(cursor);
      const people = new Set(monthEvents.value.filter((event) => coversDate(event, iso)).map((event) => event.owner));
      if (people.size >= 3) conflicts += 1;
      cursor.setDate(cursor.getDate() + 1);
    }
    return conflicts;
  });

  const detailTitle = computed(() => {
    if (!selectedEvents.value.length) return selectedDate.value;
    if (selectedEvents.value.length === 1) return selectedEvents.value[0]?.name ?? selectedDate.value;
    return `${selectedDate.value} · ${selectedEvents.value.length} 项安排`;
  });

  function moveMonth(offset: number) {
    const next = new Date(visibleMonth.value);
    next.setMonth(next.getMonth() + offset, 1);
    visibleMonth.value = next;
  }

  function returnToReferenceMonth() {
    visibleMonth.value = new Date(REFERENCE_YEAR, REFERENCE_MONTH, 1);
  }

  function openDay(cell: TeamCalendarCell) {
    if (!cell.events.length) return;
    selectedDate.value = cell.iso;
    selectedEvents.value = [...cell.events];
    detailOpen.value = true;
  }

  function openEvent(event: TeamCalendarEvent, iso: string) {
    selectedDate.value = iso;
    selectedEvents.value = [event];
    detailOpen.value = true;
  }

  return {
    visibleMonth,
    department,
    eventType,
    detailOpen,
    selectedDate,
    selectedEvents,
    departmentOptions,
    typeOptions,
    monthLabel,
    monthCells,
    leaveCount,
    travelCount,
    conflictDates,
    monthEventCount: computed(() => monthEvents.value.length),
    detailTitle,
    moveMonth,
    returnToReferenceMonth,
    openDay,
    openEvent,
  };
}
