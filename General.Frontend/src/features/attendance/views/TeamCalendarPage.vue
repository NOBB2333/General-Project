<script setup lang="ts">
import { ChevronLeft, ChevronRight } from 'lucide-vue-next';
import { UiButton, UiDialog, UiSelect } from '@/components/ui';
import { teamCalendarReferenceFixture } from '../model/team-calendar.data';
import TeamCalendarGrid from './team-calendar/TeamCalendarGrid.vue';
import TeamCalendarHero from './team-calendar/TeamCalendarHero.vue';
import TeamCalendarOverview from './team-calendar/TeamCalendarOverview.vue';
import { useTeamCalendar } from './team-calendar/useTeamCalendar';

const legendTypes = ['年假', '病假', '事假', '婚假', '产假', '调休', '丧假', '出差'];

const {
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
  monthEventCount,
  detailTitle,
  moveMonth,
  returnToReferenceMonth,
  openDay,
  openEvent,
} = useTeamCalendar(teamCalendarReferenceFixture.rows);
</script>

<template>
  <section
    :class="$style.page"
    data-page-family="calendar"
    data-page-route="/attendance/team-calendar"
  >
    <TeamCalendarHero />

    <div :class="$style.bodyGrid">
      <div
        :class="$style.calendarColumn"
        :data-month-label="monthLabel"
      >
        <header :class="$style.toolbar">
          <div :class="$style.monthNavigation">
            <UiButton
              variant="ghost"
              size="icon-sm"
              aria-label="上个月"
              title="上个月"
              @click="moveMonth(-1)"
            >
              <ChevronLeft
                :size="16"
                :stroke-width="1.8"
              />
            </UiButton>
            <h2>{{ monthLabel }}</h2>
            <UiButton
              variant="ghost"
              size="icon-sm"
              aria-label="下个月"
              title="下个月"
              @click="moveMonth(1)"
            >
              <ChevronRight
                :size="16"
                :stroke-width="1.8"
              />
            </UiButton>
            <UiButton
              :class="$style.todayButton"
              variant="secondary"
              size="xs"
              @click="returnToReferenceMonth"
            >
              今天
            </UiButton>
          </div>

          <div :class="$style.filters">
            <UiSelect
              v-model="department"
              :class="$style.filterSelect"
              :options="departmentOptions"
              label="筛选部门"
              size="md"
            />
            <UiSelect
              v-model="eventType"
              :class="$style.filterSelect"
              :options="typeOptions"
              label="筛选类型"
              size="md"
            />
          </div>
        </header>

        <TeamCalendarGrid
          :cells="monthCells"
          @select-day="openDay"
          @select-event="openEvent"
        />

        <footer
          :class="$style.legend"
          aria-label="日历图例"
        >
          <span
            v-for="type in legendTypes"
            :key="type"
            :class="$style.legendItem"
            :data-event-type="type"
          >
            <i aria-hidden="true" />
            {{ type }}
          </span>
          <span :class="[$style.legendItem, $style.conflictLegend]">
            <i aria-hidden="true">△</i>
            人力冲突(3人+)
          </span>
        </footer>
      </div>

      <TeamCalendarOverview
        :leave-count="leaveCount"
        :travel-count="travelCount"
        :conflict-dates="conflictDates"
        :total-count="monthEventCount"
      />
    </div>

    <UiDialog
      v-model:open="detailOpen"
      :title="detailTitle"
      :description="selectedDate ? `${selectedDate} 的团队安排` : '团队日历安排'"
      size="sm"
    >
      <div :class="$style.detailList">
        <article
          v-for="event in selectedEvents"
          :key="event.id"
          :class="$style.detailItem"
          :data-event-type="event.type"
        >
          <span
            :class="$style.detailTone"
            aria-hidden="true"
          />
          <div :class="$style.detailCopy">
            <strong>{{ event.owner }} · {{ event.type }}</strong>
            <p>{{ event.dept }} · {{ event.date }} 至 {{ event.endDate }}</p>
          </div>
        </article>
      </div>
      <template #footer>
        <UiButton
          variant="primary"
          size="sm"
          @click="detailOpen = false"
        >
          知道了
        </UiButton>
      </template>
    </UiDialog>
  </section>
</template>

<style module>
@reference "../../../styles/index.css";

.page {
  @apply flex h-full flex-col;
  color: var(--ui-text);
}

.bodyGrid {
  @apply grid min-h-0 flex-1 gap-4 overflow-hidden;
  grid-template-columns: minmax(0, 1fr) 280px;
}

.calendarColumn {
  @apply flex min-h-0 min-w-0 flex-col;
}

.toolbar {
  @apply mb-3 flex h-[38px] shrink-0 items-center justify-between gap-4;
}

.monthNavigation {
  @apply flex items-center gap-1;
}

.monthNavigation h2 {
  @apply min-w-[120px] text-center text-[16px] font-bold leading-6;
  color: var(--ui-text);
}

.todayButton {
  @apply ml-2;
  width: 45px;
  color: var(--app-primary);
  border-color: #b3d4ff;
}

.filters {
  @apply flex items-center gap-2;
}

.filterSelect {
  width: 84px;
  min-width: 84px;
  height: 38px;
}

.legend {
  @apply mt-2 flex min-h-[15px] shrink-0 flex-wrap items-center gap-x-3 gap-y-1 text-[10px];
  color: var(--ui-text-subtle);
}

.legendItem {
  @apply inline-flex items-center gap-1 whitespace-nowrap;
}

.legendItem i {
  @apply block h-2 w-2 shrink-0 not-italic;
  background: #6554c0;
}

.legendItem[data-event-type='年假'] i {
  background: #ff8b00;
}

.legendItem[data-event-type='病假'] i {
  background: #ff5630;
}

.legendItem[data-event-type='婚假'] i {
  background: #ff7452;
}

.legendItem[data-event-type='产假'] i {
  background: #00b8d9;
}

.legendItem[data-event-type='调休'] i {
  background: #00875a;
}

.legendItem[data-event-type='丧假'] i {
  background: #42526e;
}

.legendItem[data-event-type='出差'] i {
  background: #0052cc;
}

.conflictLegend {
  @apply font-semibold;
  color: #ff5630;
}

.conflictLegend i {
  @apply flex h-auto w-auto items-center justify-center text-[11px] font-bold;
  color: #ff5630;
  background: transparent;
}

.detailList {
  @apply flex flex-col gap-2 p-5;
}

.detailItem {
  @apply flex items-start gap-3 rounded border p-3;
  border-color: var(--ui-border-subtle);
  background: var(--ui-surface-subtle);
}

.detailTone {
  @apply mt-1 h-3 w-1 shrink-0 rounded-full;
  background: #6554c0;
}

.detailItem[data-event-type='年假'] .detailTone {
  background: #ff8b00;
}

.detailItem[data-event-type='病假'] .detailTone {
  background: #ff5630;
}

.detailItem[data-event-type='调休'] .detailTone {
  background: #00875a;
}

.detailItem[data-event-type='出差'] .detailTone {
  background: #0052cc;
}

.detailCopy {
  @apply min-w-0;
}

.detailCopy strong {
  @apply block text-[13px] font-semibold;
}

.detailCopy p {
  @apply mt-1 text-[11px];
  color: var(--ui-text-subtle);
}
</style>
