<script setup lang="ts">
import { useRouter } from 'vue-router';
import { dispatchCalendarReferenceFixture } from '../model/dispatch-calendar.data';
import DispatchCalendarHero from './dispatch-calendar/DispatchCalendarHero.vue';
import DispatchCalendarMetrics from './dispatch-calendar/DispatchCalendarMetrics.vue';
import DispatchOrderDetailDialog from './dispatch-calendar/DispatchOrderDetailDialog.vue';
import DispatchCalendarToolbar from './dispatch-calendar/DispatchCalendarToolbar.vue';
import DispatchScheduleGrid from './dispatch-calendar/DispatchScheduleGrid.vue';
import DispatchUnscheduledPanel from './dispatch-calendar/DispatchUnscheduledPanel.vue';
import { useDispatchCalendar } from './dispatch-calendar/useDispatchCalendar';

const router = useRouter();
const {
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
} = useDispatchCalendar(dispatchCalendarReferenceFixture.rows);

function goToDispatchWorkbench() {
  detailOpen.value = false;
  void router.push('/service/to-dispatch');
}

function goToServiceTicket() {
  detailOpen.value = false;
  void router.push('/service/service-tickets');
}
</script>

<template>
  <section
    :class="$style.page"
    data-page-family="schedule"
    data-page-route="/service/dispatch-calendar"
    data-module-dispatch-calendar
    :data-calendar-mode="mode"
  >
    <DispatchCalendarHero @go-dispatch="goToDispatchWorkbench" />

    <DispatchCalendarMetrics
      :period-label="periodLabel"
      :schedule-count="totalScheduleCount"
      :urgent-count="0"
      :unscheduled-count="filteredUnscheduledOrders.length"
    />

    <DispatchCalendarToolbar
      v-model:mode="mode"
      v-model:region="region"
      v-model:engineer="engineer"
      :period-label="periodLabel"
      :region-options="regionOptions"
      :engineer-options="engineerOptions"
      :schedule-count="totalScheduleCount"
      :engineer-count="filteredEngineers.length"
      @previous="movePeriod(-1)"
      @next="movePeriod(1)"
      @current="returnToCurrentPeriod"
    />

    <div :class="$style.calendarBody">
      <DispatchScheduleGrid
        :days="visibleDays"
        :engineers="filteredEngineers"
      />
      <DispatchUnscheduledPanel
        :orders="filteredUnscheduledOrders"
        @select="openOrder"
      />
    </div>

    <DispatchOrderDetailDialog
      v-model:open="detailOpen"
      :order="selectedOrder"
      @view-details="goToServiceTicket"
    />
  </section>
</template>

<style module>
@reference "../../../styles/index.css";

.page {
  @apply min-w-0 pb-6;
  color: var(--ui-text);
}

.calendarBody {
  @apply flex min-h-[590px] min-w-0 items-stretch gap-3;
}

@media (max-width: 980px) {
  .calendarBody {
    @apply flex-col;
  }
}
</style>
