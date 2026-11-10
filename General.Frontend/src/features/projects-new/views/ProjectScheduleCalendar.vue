<script setup lang="ts">
import { computed, ref } from 'vue';
import type { ProjectApi } from '@/api/core/project';
import { UiButton } from '@/components/ui';
import {
  buildProjectCalendarMonthGrid,
  getProjectCalendarSpan,
  isProjectCalendarItemOnDate,
  isProjectCalendarItemVisibleInMonth,
  projectCalendarLocalDateKey,
  type ProjectCalendarSpan,
} from '../model/project-calendar';

const props = defineProps<{ items: ProjectApi.ProjectCalendarItem[] }>();

const now = new Date();
const visibleMonth = ref(new Date(now.getFullYear(), now.getMonth(), 1));
const selectedDate = ref(projectCalendarLocalDateKey(visibleMonth.value));
const todayKey = projectCalendarLocalDateKey(now);
const weekdayLabels = ['周一', '周二', '周三', '周四', '周五', '周六', '周日'];

const year = computed(() => visibleMonth.value.getFullYear());
const monthIndex = computed(() => visibleMonth.value.getMonth());
const monthLabel = computed(() => `${year.value}年${monthIndex.value + 1}月`);
const currentMonthLabel = computed(() => {
  const current = new Date();
  return `${current.getFullYear()}年${current.getMonth() + 1}月`;
});
const isCurrentMonth = computed(() => monthLabel.value === currentMonthLabel.value);
const visibleItems = computed(() =>
  props.items.filter((item) => isProjectCalendarItemVisibleInMonth(item, year.value, monthIndex.value)),
);
const cells = computed(() =>
  buildProjectCalendarMonthGrid(year.value, monthIndex.value).map((cell) => ({
    ...cell,
    items: visibleItems.value.filter((item) => isProjectCalendarItemOnDate(item, cell.dateKey)),
  })),
);
const selectedItems = computed(() =>
  visibleItems.value.filter((item) => isProjectCalendarItemOnDate(item, selectedDate.value)),
);
const undatedItems = computed(() => props.items.filter((item) => getProjectCalendarSpan(item).kind === 'undated'));
const invalidItems = computed(() => props.items.filter((item) => getProjectCalendarSpan(item).kind === 'invalid'));
const selectedDateLabel = computed(() => {
  const [selectedYear, selectedMonth, selectedDay] = selectedDate.value.split('-').map(Number);
  return `${selectedYear}年${selectedMonth}月${selectedDay}日`;
});

function moveMonth(delta: -1 | 1) {
  visibleMonth.value = new Date(year.value, monthIndex.value + delta, 1);
  selectedDate.value = projectCalendarLocalDateKey(visibleMonth.value);
}

function showCurrentMonth() {
  const current = new Date();
  visibleMonth.value = new Date(current.getFullYear(), current.getMonth(), 1);
  selectedDate.value = projectCalendarLocalDateKey(visibleMonth.value);
}

function formatSpan(item: ProjectApi.ProjectCalendarItem): string {
  const span: ProjectCalendarSpan = getProjectCalendarSpan(item);
  if (span.kind === 'undated') return '未设置排期日期';
  if (span.kind === 'invalid') return '日期无效，请检查服务端排期数据';
  if (span.startDate === span.endDate) return span.startDate;
  return `${span.startDate} 至 ${span.endDate}`;
}

function selectDate(dateKey: string) {
  selectedDate.value = dateKey;
}
</script>

<template>
  <section
    :class="$style.panel"
    aria-labelledby="project-calendar-title"
  >
    <header :class="$style.header">
      <div>
        <h3
          id="project-calendar-title"
          :class="$style.heading"
        >
          项目排期日历
        </h3>
        <p :class="$style.description">
          事项来自当前用户可见的任务与Sprint；日历保留接口日期，不做浏览器本地时区换算。
        </p>
      </div>
      <nav
        :class="$style.controls"
        aria-label="日历月份"
      >
        <UiButton
          variant="secondary"
          size="sm"
          aria-label="上一个月"
          @click="moveMonth(-1)"
        >
          上月
        </UiButton>
        <strong
          :class="$style.monthLabel"
          aria-live="polite"
        >
          {{ monthLabel }}
        </strong>
        <UiButton
          variant="secondary"
          size="sm"
          aria-label="下一个月"
          @click="moveMonth(1)"
        >
          下月
        </UiButton>
        <UiButton
          variant="ghost"
          size="sm"
          :disabled="isCurrentMonth"
          @click="showCurrentMonth"
        >
          本月
        </UiButton>
      </nav>
    </header>

    <div
      :class="$style.calendarGrid"
      role="grid"
      aria-label="项目月历"
    >
      <div
        v-for="weekday in weekdayLabels"
        :key="weekday"
        :class="$style.weekday"
        role="columnheader"
      >
        {{ weekday }}
      </div>
      <div
        v-for="cell in cells"
        :key="cell.dateKey"
        :class="[
          $style.dayCell,
          !cell.inCurrentMonth && $style.dayCellOutside,
          cell.dateKey === todayKey && $style.dayCellToday,
          cell.dateKey === selectedDate && $style.dayCellSelected,
        ]"
        role="gridcell"
        :aria-selected="cell.dateKey === selectedDate"
      >
        <button
          v-if="cell.inCurrentMonth"
          :class="$style.dayButton"
          type="button"
          :aria-label="`选择${cell.dateKey}`"
          :aria-pressed="cell.dateKey === selectedDate"
          @click="selectDate(cell.dateKey)"
        >
          {{ cell.day }}
        </button>
        <span
          v-else
          :class="$style.dayNumber"
        >
          {{ cell.day }}
        </span>
        <ul
          v-if="cell.items.length"
          :class="$style.cellEvents"
          :aria-label="`${cell.dateKey}排期`"
        >
          <li
            v-for="item in cell.items.slice(0, 2)"
            :key="item.id"
            :class="$style.eventChip"
            :title="`${item.type}：${item.title}`"
          >
            {{ item.title }}
          </li>
          <li
            v-if="cell.items.length > 2"
            :class="$style.moreEvents"
          >
            另有 {{ cell.items.length - 2 }} 项
          </li>
        </ul>
      </div>
    </div>

    <section
      :class="$style.dayAgenda"
      :aria-label="`${selectedDateLabel}排期明细`"
      aria-live="polite"
    >
      <h4 :class="$style.subheading">{{ selectedDateLabel }}</h4>
      <ul
        v-if="selectedItems.length"
        :class="$style.agendaList"
      >
        <li
          v-for="item in selectedItems"
          :key="item.id"
          :class="$style.agendaItem"
        >
          <span :class="$style.typeTag">{{ item.type || '排期' }}</span>
          <div>
            <strong>{{ item.title || '未命名排期' }}</strong>
            <p>{{ formatSpan(item) }}</p>
          </div>
          <span>{{ item.ownerName || '未指定负责人' }}</span>
          <span>{{ item.status || '—' }}</span>
        </li>
      </ul>
      <p
        v-else
        :class="$style.empty"
      >
        当日没有排期事项。
      </p>
    </section>

    <section
      v-if="undatedItems.length || invalidItems.length"
      :class="$style.unscheduled"
      aria-label="未排期事项"
    >
      <h4 :class="$style.subheading">未排期或日期无效的事项</h4>
      <ul :class="$style.agendaList">
        <li
          v-for="item in [...undatedItems, ...invalidItems]"
          :key="item.id"
          :class="$style.agendaItem"
        >
          <span :class="$style.typeTag">{{ item.type || '排期' }}</span>
          <div>
            <strong>{{ item.title || '未命名排期' }}</strong>
            <p>{{ formatSpan(item) }}</p>
          </div>
          <span>{{ item.ownerName || '未指定负责人' }}</span>
          <span>{{ item.status || '—' }}</span>
        </li>
      </ul>
    </section>

    <p :class="$style.sourceNote">
      当前月显示 {{ visibleItems.length }} 条排期。日历数据来自资源分区完整结果，没有独立服务端日期分页接口。
    </p>
  </section>
</template>

<style module>
@reference "../../../styles/index.css";

.panel {
  @apply space-y-4 rounded-2xl border bg-white p-4;
  border-color: var(--ui-border);
}

.header {
  @apply flex flex-wrap items-start justify-between gap-3;
}

.heading {
  @apply text-base font-semibold;
  color: var(--ui-text);
}

.description,
.sourceNote {
  @apply mt-1 text-sm;
  color: var(--ui-text-muted);
}

.controls {
  @apply flex flex-wrap items-center gap-2;
}

.monthLabel {
  @apply min-w-24 text-center text-sm font-semibold;
  color: var(--ui-text);
}

.calendarGrid {
  @apply grid grid-cols-7 gap-1;
}

.weekday {
  @apply py-2 text-center text-xs font-medium;
  color: var(--ui-text-subtle);
}

.dayCell {
  @apply min-w-0 rounded-lg border bg-white p-1;
  border-color: var(--ui-border);
  min-height: 6.5rem;
}

.dayCellOutside {
  color: var(--ui-text-subtle);
  background: var(--ui-surface-subtle);
}

.dayCellToday {
  outline: 1px solid var(--app-primary);
  outline-offset: -1px;
}

.dayCellSelected {
  background: var(--app-primary-soft);
}

.dayButton,
.dayNumber {
  @apply inline-flex h-7 min-w-7 items-center justify-center rounded-full px-2 text-sm;
}

.dayButton {
  @apply font-semibold hover:bg-[#F4F5F7] focus-visible:outline-none;
  color: var(--ui-text);
}

.dayButton:focus-visible {
  outline: 2px solid var(--ui-focus);
  outline-offset: 2px;
}

.dayButton[aria-pressed='true'] {
  color: white;
  background: var(--app-primary);
}

.cellEvents {
  @apply mt-1 space-y-1;
}

.eventChip {
  @apply truncate rounded px-1 py-0.5 text-xs;
  color: var(--ui-text);
  background: var(--ui-surface-hover);
}

.moreEvents {
  @apply px-1 text-xs;
  color: var(--ui-text-subtle);
}

.dayAgenda,
.unscheduled {
  @apply border-t pt-3;
  border-color: var(--ui-border);
}

.subheading {
  @apply font-medium;
  color: var(--ui-text);
}

.agendaList {
  @apply mt-2 space-y-2;
}

.agendaItem {
  @apply flex flex-wrap items-center gap-3 rounded-lg border px-3 py-2 text-sm;
  border-color: var(--ui-border);
}

.agendaItem > div {
  @apply min-w-40 flex-1;
}

.agendaItem p {
  @apply text-xs;
  color: var(--ui-text-subtle);
}

.typeTag {
  @apply rounded-full border px-2 py-1 text-xs;
  border-color: var(--ui-border);
  color: var(--ui-text-muted);
}

.empty {
  @apply mt-2 rounded-lg border border-dashed px-3 py-4 text-center text-sm;
  border-color: var(--ui-border);
  color: var(--ui-text-muted);
}

.sourceNote {
  @apply border-t pt-3;
  border-color: var(--ui-border);
}
</style>
