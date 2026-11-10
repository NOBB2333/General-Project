<script setup lang="ts">
import type { DispatchCalendarDay, DispatchEngineerRow } from '../../model/dispatch-calendar.types';

defineProps<{
  days: readonly DispatchCalendarDay[];
  engineers: readonly DispatchEngineerRow[];
}>();
</script>

<template>
  <section
    :class="$style.schedulePanel"
    :data-period-days="days.length"
    aria-label="工程师派工排程"
  >
    <div :class="$style.horizontalViewport">
      <div :class="$style.scheduleGrid">
        <header :class="$style.dateHeader">
          <div :class="$style.resourceHeading">工程师 | 日期</div>
          <div
            v-for="day in days"
            :key="day.id"
            :class="[$style.dayHeading, day.isToday && $style.todayHeading]"
            :data-day-iso="day.iso"
            :data-today="day.isToday || undefined"
          >
            <strong>{{ day.dateLabel }}</strong>
            <span>{{ day.weekdayLabel }}<template v-if="day.isToday"> · 今</template></span>
          </div>
        </header>

        <div
          v-if="!engineers.length"
          :class="$style.emptyEngineers"
        >
          当前筛选下没有工程师
        </div>

        <template v-else>
          <div
            v-for="item in engineers"
            :key="item.id"
            :class="$style.engineerRow"
            :data-engineer-id="item.id"
          >
            <div :class="$style.engineerCell">
              <span :class="$style.avatar">{{ item.name.slice(0, 1) }}</span>
              <div :class="$style.engineerIdentity">
                <strong>{{ item.name }}</strong>
                <span>{{ item.team }}</span>
              </div>
              <span
                :class="$style.scheduleBadge"
                :aria-label="`${item.name} ${item.scheduleCount} 项排程`"
              >
                {{ item.scheduleCount }}
              </span>
            </div>
            <div
              v-for="day in days"
              :key="`${item.id}-${day.id}`"
              :class="[$style.scheduleCell, day.isToday && $style.todayCell]"
              :data-schedule-cell="`${item.id}:${day.iso}`"
            />
          </div>
        </template>
      </div>
    </div>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.schedulePanel {
  @apply min-w-0 flex-1 overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
  border-radius: var(--app-control-radius);
}

.horizontalViewport {
  @apply h-full overflow-x-auto;
  scrollbar-width: thin;
  scrollbar-color: #c1c7d0 transparent;
}

.horizontalViewport::-webkit-scrollbar {
  width: 6px;
  height: 6px;
}

.horizontalViewport::-webkit-scrollbar-thumb {
  background: #c1c7d0;
  border-radius: 999px;
}

.scheduleGrid {
  @apply min-w-full;
  width: max-content;
}

.dateHeader,
.engineerRow {
  display: grid;
  grid-template-columns: 140px;
  grid-auto-flow: column;
  grid-auto-columns: 165px;
}

.schedulePanel[data-period-days='28'] .dateHeader,
.schedulePanel[data-period-days='28'] .engineerRow {
  grid-auto-columns: 60px;
}

.dateHeader {
  @apply sticky top-0 z-[1] h-12 border-b;
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}

.resourceHeading,
.dayHeading {
  @apply flex flex-col items-center justify-center border-r text-center;
  border-color: var(--ui-border);
}

.resourceHeading {
  @apply sticky left-0 z-[2] items-start px-3 text-[11px] font-semibold;
  color: var(--ui-text-subtle);
  background: var(--ui-surface-subtle);
}

.dayHeading strong {
  @apply text-[12px] font-semibold leading-5;
  color: var(--ui-text);
}

.dayHeading span {
  @apply text-[10px] leading-4;
  color: var(--ui-text-subtle);
}

.todayHeading {
  background: #deebff;
}

.engineerRow {
  @apply min-h-[88px] border-b;
  border-color: var(--ui-border-subtle);
}

.engineerRow:last-child {
  @apply border-b-0;
}

.engineerCell {
  @apply sticky left-0 z-[1] flex items-center border-r bg-white px-3;
  border-color: var(--ui-border);
}

.avatar {
  @apply flex h-7 w-7 shrink-0 items-center justify-center rounded-full text-[11px] font-semibold;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.engineerIdentity {
  @apply ml-2 min-w-0;
}

.engineerIdentity strong {
  @apply block truncate text-[12px] font-semibold;
  color: var(--ui-text);
}

.engineerIdentity span {
  @apply mt-0.5 block truncate text-[10px];
  color: var(--ui-text-subtle);
}

.scheduleBadge {
  @apply ml-auto flex h-5 min-w-5 items-center justify-center rounded px-1 text-[10px] font-semibold;
  color: #97a0af;
  background: var(--ui-surface-hover);
}

.scheduleCell {
  @apply border-r;
  border-color: var(--ui-border-subtle);
  background: white;
}

.todayCell {
  background: color-mix(in srgb, #deebff 43%, white);
}

.emptyEngineers {
  @apply flex h-[180px] min-w-[760px] items-center justify-center text-[12px];
  color: var(--ui-text-subtle);
}
</style>
