<script setup lang="ts">
import { LayoutGrid, ListChecks } from 'lucide-vue-next';
import { UiTabPanel, UiTabs, UiTabsList, UiTabsTrigger } from '@/components/ui';
import type {
  ReceiptDashboardView,
  ReceiptFutureMetric,
  ReceiptMonthlyRow,
  ReceiptPlanMetric,
  ReceiptScheduleRow,
  ReceiptStatusRow,
} from '../../model/receipts.types';
import ReceiptsAnalysisCharts from './ReceiptsAnalysisCharts.vue';
import ReceiptScheduleTable from './ReceiptScheduleTable.vue';

defineProps<{
  view: ReceiptDashboardView;
  metrics: readonly ReceiptPlanMetric[];
  futureMetrics: readonly ReceiptFutureMetric[];
  monthlyRows: readonly ReceiptMonthlyRow[];
  statusRows: readonly ReceiptStatusRow[];
  scheduleRows: readonly ReceiptScheduleRow[];
  query: string;
  riskFilter: string;
}>();

const emit = defineEmits<{
  'update:view': [value: ReceiptDashboardView];
  'update:query': [value: string];
  'update:riskFilter': [value: string];
  inspect: [row: ReceiptScheduleRow];
}>();

function updateView(value: string) {
  emit('update:view', value as ReceiptDashboardView);
}
</script>

<template>
  <UiTabs
    :model-value="view"
    @update:model-value="updateView"
  >
    <header :class="$style.dashboardHeader">
      <div :class="$style.dashboardTitle">
        <LayoutGrid
          :size="17"
          :stroke-width="1.8"
        />
        <strong>回款看板</strong>
        <span>· 综合分析视图</span>
      </div>
      <UiTabsList
        label="回款看板显示方式"
        :class="$style.viewSwitch"
      >
        <UiTabsTrigger
          value="board"
          :class="$style.viewButton"
        >
          <LayoutGrid
            :size="14"
            :stroke-width="1.8"
          />
          看板
        </UiTabsTrigger>
        <UiTabsTrigger
          value="details"
          :class="$style.viewButton"
        >
          <ListChecks
            :size="14"
            :stroke-width="1.8"
          />
          详情
        </UiTabsTrigger>
      </UiTabsList>
    </header>

    <UiTabPanel value="board">
      <div :class="$style.boardBody">
        <section
          :class="$style.planMetrics"
          aria-label="回款计划指标"
        >
          <article
            v-for="metric in metrics"
            :key="metric.id"
            :class="[$style.planMetric, $style[`plan_${metric.tone}`]]"
          >
            <span>{{ metric.label }}</span>
            <strong>{{ metric.value }}</strong>
            <small>{{ metric.hint }}</small>
          </article>
        </section>

        <section :class="$style.progressGrid">
          <article :class="$style.progressCard">
            <header>
              <strong>整体回款进度</strong>
              <span>0%</span>
            </header>
            <div
              :class="$style.progressTrack"
              role="progressbar"
              aria-label="整体回款进度"
              aria-valuemin="0"
              aria-valuemax="100"
              aria-valuenow="0"
            >
              <span :class="$style.progressOverdue" />
              <span :class="$style.progressFuture" />
            </div>
            <div :class="$style.progressLegend">
              <span data-tone="success"><i />已到账 ¥0</span>
              <span data-tone="danger"><i />逾期 ¥938,713</span>
              <span data-tone="warning"><i />即将到期 ¥0</span>
              <span data-tone="neutral"><i />未来待回 ¥53,370</span>
            </div>
          </article>

          <article
            v-for="metric in futureMetrics"
            :key="metric.id"
            :class="$style.futureCard"
          >
            <span>{{ metric.label }}</span>
            <strong>{{ metric.value }}</strong>
            <small>{{ metric.hint }}</small>
          </article>
        </section>

        <ReceiptsAnalysisCharts
          :monthly-rows="monthlyRows"
          :status-rows="statusRows"
        />
      </div>
    </UiTabPanel>

    <UiTabPanel value="details">
      <ReceiptScheduleTable
        title="回款计划详情"
        :rows="scheduleRows"
        :query="query"
        :risk-filter="riskFilter"
        compact
        @update:query="emit('update:query', $event)"
        @update:risk-filter="emit('update:riskFilter', $event)"
        @inspect="emit('inspect', $event)"
      />
    </UiTabPanel>
  </UiTabs>
</template>

<style module>
@reference "../../../../styles/index.css";

.dashboardHeader {
  @apply flex min-h-[80px] items-center justify-between gap-4 px-5;
}

.dashboardTitle {
  @apply flex items-center gap-2 text-[13px];
  color: var(--app-primary);
}

.dashboardTitle strong {
  @apply text-[14px] font-semibold;
}

.dashboardTitle span {
  color: var(--ui-text-subtle);
}

.viewSwitch {
  @apply rounded bg-[var(--ui-surface-subtle)] p-1;
}

.viewButton {
  @apply min-h-8 gap-1.5 px-3 py-1 text-[11px];
}

.viewButton[data-state='active'] {
  color: var(--app-primary);
  background: var(--ui-surface);
  box-shadow: 0 1px 2px rgb(9 30 66 / 12%);
}

.boardBody {
  @apply px-5 pb-5;
}

.planMetrics {
  @apply grid grid-cols-6 gap-3;
}

.planMetric {
  @apply flex min-h-[110px] flex-col justify-center rounded border px-3.5 py-3;
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}

.planMetric > span {
  @apply text-[11px];
  color: var(--ui-text-subtle);
}

.planMetric strong {
  @apply mt-2 text-[19px] font-semibold leading-none;
  color: var(--ui-text);
  letter-spacing: 0;
}

.planMetric small {
  @apply mt-2 text-[10px];
  color: var(--ui-text-subtle);
}

.plan_success strong {
  color: var(--ui-success);
}

.plan_primary strong {
  color: var(--app-primary);
}

.plan_danger {
  border-color: color-mix(in srgb, var(--ui-danger) 38%, var(--ui-border));
  background: color-mix(in srgb, var(--ui-danger-soft) 34%, var(--ui-surface));
}

.plan_danger strong {
  color: var(--ui-danger);
}

.progressGrid {
  @apply mt-4 grid grid-cols-[4fr_1fr_1fr_1fr] gap-3;
}

.progressCard,
.futureCard {
  @apply min-h-[112px] rounded border bg-white p-4;
  border-color: var(--ui-border);
}

.progressCard header {
  @apply flex items-center justify-between text-[11px];
}

.progressCard header strong {
  @apply font-semibold;
  color: var(--ui-text);
}

.progressCard header span {
  @apply font-semibold;
  color: var(--app-primary);
}

.progressTrack {
  @apply relative mt-3 flex h-3 overflow-hidden rounded-full;
  background: var(--ui-border-subtle);
}

.progressOverdue {
  @apply h-full w-[94.6%];
  background: var(--ui-danger);
}

.progressFuture {
  @apply h-full flex-1;
  background: var(--ui-border);
}

.progressLegend {
  @apply mt-3 flex flex-wrap items-center gap-x-4 gap-y-1 text-[10px];
  color: var(--ui-text-subtle);
}

.progressLegend span {
  @apply flex items-center gap-1;
}

.progressLegend i {
  @apply block h-2 w-2;
  background: var(--ui-border);
}

.progressLegend span[data-tone='success'] i {
  background: var(--ui-success);
}

.progressLegend span[data-tone='danger'] i {
  background: var(--ui-danger);
}

.progressLegend span[data-tone='warning'] i {
  background: var(--ui-warning);
}

.futureCard {
  @apply flex flex-col justify-center;
}

.futureCard span,
.futureCard small {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}

.futureCard strong {
  @apply my-2 text-[17px] font-semibold;
  color: var(--ui-text);
}

@media (max-width: 1250px) {
  .planMetrics {
    @apply grid-cols-3;
  }

  .progressGrid {
    @apply grid-cols-3;
  }

  .progressCard {
    @apply col-span-3;
  }
}

@media (prefers-reduced-motion: reduce) {
  .viewButton {
    transition: none;
  }
}
</style>
