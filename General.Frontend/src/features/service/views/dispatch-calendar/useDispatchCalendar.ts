import { computed, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import type { UiSelectOption } from '@/components/ui';
import type {
  DispatchCalendarDay,
  DispatchCalendarMode,
  DispatchCalendarReferenceRow,
  DispatchEngineerRow,
  DispatchUnscheduledRow,
} from '../../model/dispatch-calendar.types';

const REFERENCE_PERIOD_START = new Date(2026, 6, 13);
const REFERENCE_TODAY = '2026-07-15';
const WEEKDAY_LABELS = ['周日', '周一', '周二', '周三', '周四', '周五', '周六'] as const;

function isEngineer(row: DispatchCalendarReferenceRow): row is DispatchEngineerRow {
  return row.role === 'engineer';
}

function isUnscheduled(row: DispatchCalendarReferenceRow): row is DispatchUnscheduledRow {
  return row.role === 'unscheduled';
}

function addDays(date: Date, days: number) {
  const next = new Date(date);
  next.setDate(next.getDate() + days);
  return next;
}

function formatIso(date: Date) {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

function formatShortDate(date: Date) {
  return `${date.getMonth() + 1}/${date.getDate()}`;
}

function toDay(date: Date): DispatchCalendarDay {
  const iso = formatIso(date);
  return {
    id: iso,
    iso,
    dateLabel: formatShortDate(date),
    weekdayLabel: WEEKDAY_LABELS[date.getDay()] ?? '',
    isToday: iso === REFERENCE_TODAY,
  };
}

export function useDispatchCalendar(referenceRows: readonly DispatchCalendarReferenceRow[]) {
  const rows = ref<DispatchCalendarReferenceRow[]>(cloneReferenceRows(referenceRows));
  const mode = ref<DispatchCalendarMode>('week');
  const region = ref('全部地区');
  const engineer = ref('全部工程师');
  const periodAnchor = ref(new Date(REFERENCE_PERIOD_START));
  const selectedOrder = ref<DispatchUnscheduledRow | null>(null);
  const detailOpen = ref(false);

  const engineers = computed(() => rows.value.filter(isEngineer));
  const unscheduledOrders = computed(() => rows.value.filter(isUnscheduled));

  const regionOptions: readonly UiSelectOption[] = ['全部地区', '上海', '北京', '广州', '远程', '其他'].map(
    (label) => ({ value: label, label }),
  );
  const engineerOptions = computed<UiSelectOption[]>(() => [
    { value: '全部工程师', label: '全部工程师' },
    ...engineers.value.map((item) => ({ value: item.name, label: item.name })),
  ]);

  const filteredEngineers = computed(() =>
    engineers.value.filter(
      (item) =>
        (region.value === '全部地区' || item.region === region.value) &&
        (engineer.value === '全部工程师' || item.name === engineer.value),
    ),
  );

  const filteredUnscheduledOrders = computed(() =>
    unscheduledOrders.value.filter(
      (item) =>
        (region.value === '全部地区' || item.region === region.value) &&
        (engineer.value === '全部工程师' || item.engineerName === engineer.value),
    ),
  );

  const visibleDays = computed<DispatchCalendarDay[]>(() => {
    const count = mode.value === 'month' ? 28 : mode.value === 'biweek' ? 14 : 7;
    return Array.from({ length: count }, (_, index) => toDay(addDays(periodAnchor.value, index)));
  });

  const periodLabel = computed(() => {
    const periodDays = mode.value === 'month' ? 28 : mode.value === 'biweek' ? 14 : 7;
    const end = addDays(periodAnchor.value, periodDays - 1);
    return `${formatShortDate(periodAnchor.value)} - ${formatShortDate(end)}`;
  });

  const totalScheduleCount = computed(() =>
    filteredEngineers.value.reduce((total, item) => total + item.scheduleCount, 0),
  );

  function movePeriod(direction: -1 | 1) {
    const next = new Date(periodAnchor.value);
    const periodDays = mode.value === 'month' ? 28 : mode.value === 'biweek' ? 14 : 7;
    next.setDate(next.getDate() + direction * periodDays);
    periodAnchor.value = next;
  }

  function returnToCurrentPeriod() {
    periodAnchor.value = new Date(REFERENCE_PERIOD_START);
  }

  function openOrder(order: DispatchUnscheduledRow) {
    selectedOrder.value = order;
    detailOpen.value = true;
  }

  return {
    mode,
    region,
    engineer,
    selectedOrder,
    detailOpen,
    regionOptions,
    engineerOptions,
    filteredEngineers,
    filteredUnscheduledOrders,
    visibleDays,
    periodLabel,
    totalScheduleCount,
    movePeriod,
    returnToCurrentPeriod,
    openOrder,
  };
}
