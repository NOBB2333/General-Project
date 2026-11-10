<script setup lang="ts">
import { Circle, Coffee, Heart, Plane, Sun } from 'lucide-vue-next';
import type { Component } from 'vue';
import type { TeamCalendarCell, TeamCalendarEvent, TeamCalendarEventType } from '../../model/team-calendar.types';

defineProps<{
  cells: TeamCalendarCell[];
}>();

const emit = defineEmits<{
  selectDay: [cell: TeamCalendarCell];
  selectEvent: [event: TeamCalendarEvent, iso: string];
}>();

const weekdays = [
  { label: '一', weekend: false },
  { label: '二', weekend: false },
  { label: '三', weekend: false },
  { label: '四', weekend: false },
  { label: '五', weekend: false },
  { label: '六', weekend: true },
  { label: '日', weekend: true },
];

const eventIcons: Record<TeamCalendarEventType, Component> = {
  年假: Sun,
  病假: Heart,
  事假: Circle,
  婚假: Heart,
  产假: Circle,
  调休: Coffee,
  丧假: Circle,
  出差: Plane,
};

function iconFor(type: TeamCalendarEventType) {
  return eventIcons[type];
}

function openCell(cell: TeamCalendarCell) {
  emit('selectDay', cell);
}
</script>

<template>
  <section
    :class="$style.calendarPanel"
    data-module-month-calendar
    data-schedule-mode="month-leave"
  >
    <div :class="$style.weekdayRow">
      <span
        v-for="weekday in weekdays"
        :key="weekday.label"
        :class="[$style.weekday, weekday.weekend && $style.weekend]"
      >
        {{ weekday.label }}
      </span>
    </div>

    <div :class="$style.calendarGrid">
      <div
        v-for="cell in cells"
        :key="cell.key"
        :class="[$style.dayCell, !cell.inMonth && $style.outsideMonth]"
        :data-day-iso="cell.iso"
        :data-in-month="cell.inMonth ? '1' : '0'"
        :data-event-count="cell.events.length"
        @click="openCell(cell)"
      >
        <button
          type="button"
          :class="[$style.dayNumber, cell.isToday && $style.today]"
          :aria-label="`${cell.iso}，${cell.events.length} 项安排`"
          :tabindex="cell.events.length ? 0 : -1"
          @click.stop="openCell(cell)"
        >
          {{ cell.day }}
        </button>
        <div :class="$style.eventStack">
          <button
            v-for="event in cell.events.slice(0, 3)"
            :key="event.id"
            type="button"
            :class="$style.eventChip"
            :data-event-type="event.type"
            :title="`${event.owner} - ${event.type}`"
            @click.stop="emit('selectEvent', event, cell.iso)"
          >
            <component
              :is="iconFor(event.type)"
              :size="10"
              :stroke-width="1.8"
              aria-hidden="true"
            />
            <span>{{ event.owner }}</span>
          </button>
          <span
            v-if="cell.events.length > 3"
            :class="$style.moreEvents"
          >
            +{{ cell.events.length - 3 }}...
          </span>
        </div>
      </div>
    </div>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.calendarPanel {
  @apply flex min-h-0 flex-1 flex-col overflow-hidden rounded-lg border bg-white;
  border-color: var(--ui-border);
}

.weekdayRow {
  @apply grid h-[36.5px] shrink-0 grid-cols-7 border-b;
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
  border-color: var(--ui-border);
}

.weekday {
  @apply flex items-center justify-center text-[11px] font-semibold;
}

.weekend {
  color: #ff5630;
}

.calendarGrid {
  @apply grid min-h-0 flex-1 grid-cols-7 overflow-y-auto;
  grid-template-rows: repeat(6, minmax(0, 1fr));
}

.dayCell {
  @apply cursor-default border-b border-r p-1 outline-none transition-colors;
  color: var(--ui-text);
  border-color: var(--ui-surface-hover);
  background: white;
}

.dayCell:has(.dayNumber[tabindex='0']) {
  @apply cursor-pointer;
}

.dayCell:hover,
.dayCell:focus-visible {
  background: var(--ui-surface-hover);
}

.dayCell:focus-visible {
  box-shadow: inset 0 0 0 2px var(--ui-focus);
}

.outsideMonth {
  color: #c1c7d0;
  background: #fcfdfd;
}

.dayNumber {
  @apply flex items-center justify-center border-0 bg-transparent p-0 text-[11px] font-medium;
  width: 21px;
  height: 21px;
  color: inherit;
}

.today {
  @apply rounded-full font-semibold text-white;
  background: var(--app-primary);
}

.eventStack {
  @apply mt-0.5 max-h-[52px] space-y-0.5 overflow-hidden;
}

.eventChip {
  @apply flex h-[17px] w-full items-center gap-0.5 truncate rounded border-0 px-1 py-0.5 text-left text-[9px] font-medium transition-opacity;
  border-left: 2px solid currentColor;
}

.eventChip:hover,
.eventChip:focus-visible {
  opacity: 0.8;
}

.eventChip:focus-visible {
  outline: 1px solid currentColor;
  outline-offset: 1px;
}

.eventChip span {
  @apply truncate;
}

.eventChip[data-event-type='年假'] {
  color: #ff8b00;
  background: #fff0b3;
}

.eventChip[data-event-type='病假'] {
  color: #ff5630;
  background: #ffebe6;
}

.eventChip[data-event-type='事假'] {
  color: #6554c0;
  background: #eae6ff;
}

.eventChip[data-event-type='婚假'] {
  color: #ff7452;
  background: #ffebe6;
}

.eventChip[data-event-type='产假'] {
  color: #00a3bf;
  background: #e6fcff;
}

.eventChip[data-event-type='调休'] {
  color: #00875a;
  background: #e3fcef;
}

.eventChip[data-event-type='丧假'] {
  color: #42526e;
  background: #ebecf0;
}

.eventChip[data-event-type='出差'] {
  color: #0052cc;
  background: #deebff;
}

.outsideMonth .eventChip {
  opacity: 0.55;
}

.moreEvents {
  @apply block px-1 text-[9px] font-medium;
  color: var(--ui-text-subtle);
}

@media (prefers-reduced-motion: reduce) {
  .dayCell,
  .eventChip {
    transition: none;
  }
}
</style>
