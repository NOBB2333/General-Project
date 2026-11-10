<script setup lang="ts">
import { Landmark } from 'lucide-vue-next';
import { computed, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import { UiButton, UiDialog, UiTabPanel, UiTabs, toast } from '@/components/ui';
import {
  receiptFutureMetrics,
  receiptOverviewMetrics,
  receiptPlanMetrics,
  receiptsReferenceFixture,
} from '../model/receipts.data';
import type {
  ReceiptDashboardView,
  ReceiptMonthlyRow,
  ReceiptScheduleRow,
  ReceiptSection,
  ReceiptStatusRow,
} from '../model/receipts.types';
import ReceiptScheduleTable from './receipts/ReceiptScheduleTable.vue';
import ReceiptsDashboard from './receipts/ReceiptsDashboard.vue';
import ReceiptsHero from './receipts/ReceiptsHero.vue';
import ReceiptsSectionNav from './receipts/ReceiptsSectionNav.vue';

const secondarySections: readonly ReceiptSection[] = ['receipts', 'writeoffs', 'advances', 'auto-match', 'unmatched'];
const sectionLabels: Record<ReceiptSection, string> = {
  receipts: '收款单',
  writeoffs: '收款核销',
  advances: '预收款管理',
  'auto-match': '自动流水匹配',
  unmatched: '未匹配收款流水',
  dashboard: '回款看板',
};

const localRows = ref(cloneReferenceRows(receiptsReferenceFixture.rows));
const activeSection = ref<ReceiptSection>(receiptsReferenceFixture.defaults.activeSection as ReceiptSection);
const dashboardView = ref<ReceiptDashboardView>(
  receiptsReferenceFixture.defaults.dashboardView as ReceiptDashboardView,
);
const query = ref('');
const riskFilter = ref(String(receiptsReferenceFixture.defaults.riskFilter));
const detailOpen = ref(false);
const activeRow = ref<ReceiptScheduleRow | null>(null);

const scheduleRows = computed(() =>
  localRows.value.filter((row): row is ReceiptScheduleRow => row.kind === 'schedule'),
);
const monthlyRows = computed(() => localRows.value.filter((row): row is ReceiptMonthlyRow => row.kind === 'monthly'));
const statusRows = computed(() => localRows.value.filter((row): row is ReceiptStatusRow => row.kind === 'status'));
const filteredSchedules = computed(() => {
  const normalized = query.value.trim().toLocaleLowerCase('zh-CN');
  return scheduleRows.value.filter((row) => {
    const matchesRisk = riskFilter.value === '全部风险' || row.risk === riskFilter.value;
    const matchesQuery =
      !normalized || `${row.orderCode} ${row.customer}`.toLocaleLowerCase('zh-CN').includes(normalized);
    return matchesRisk && matchesQuery;
  });
});
const activeSectionLabel = computed(() => sectionLabels[activeSection.value]);

function updateSection(value: string) {
  activeSection.value = value as ReceiptSection;
}

function openDetails(row: ReceiptScheduleRow) {
  activeRow.value = row;
  detailOpen.value = true;
}

function currency(value: number) {
  return `¥${value.toLocaleString('zh-CN')}`;
}

function exportSchedules() {
  const header = ['风险', '订单', '客户', '计划金额', '已到账', '逾期金额', '最长逾期', '下一期', '状态'];
  const lines = filteredSchedules.value.map((row) => [
    row.risk,
    row.orderCode,
    row.customer,
    currency(row.planned),
    currency(row.received),
    currency(row.overdueAmount),
    `${row.longestOverdueDays} 天`,
    row.nextDate,
    row.status,
  ]);
  const csv = [header, ...lines]
    .map((line) => line.map((cell) => `"${String(cell).replace(/"/g, '""')}"`).join(','))
    .join('\n');
  const link = document.createElement('a');
  link.href = URL.createObjectURL(new Blob([`\uFEFF${csv}`], { type: 'text/csv;charset=utf-8' }));
  link.download = 'finance-receipts.csv';
  link.click();
  URL.revokeObjectURL(link.href);
  toast.success(`已导出 ${filteredSchedules.value.length} 条回款计划`);
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="list"
    data-page-route="/finance/receipts"
    data-page-label="收款管理"
    data-page-module="财务管理"
    data-module-finance-receipts
    :data-active-section="activeSection"
    :data-local-row-count="localRows.length"
  >
    <ReceiptsHero :metrics="receiptOverviewMetrics" />

    <UiTabs
      :model-value="activeSection"
      @update:model-value="updateSection"
    >
      <section :class="$style.workspace">
        <ReceiptsSectionNav @export="exportSchedules" />

        <UiTabPanel value="dashboard">
          <ReceiptsDashboard
            :view="dashboardView"
            :metrics="receiptPlanMetrics"
            :future-metrics="receiptFutureMetrics"
            :monthly-rows="monthlyRows"
            :status-rows="statusRows"
            :schedule-rows="filteredSchedules"
            :query="query"
            :risk-filter="riskFilter"
            @update:view="dashboardView = $event"
            @update:query="query = $event"
            @update:risk-filter="riskFilter = $event"
            @inspect="openDetails"
          />
        </UiTabPanel>

        <UiTabPanel
          v-for="section in secondarySections"
          :key="section"
          :value="section"
        >
          <ReceiptScheduleTable
            :title="`${activeSectionLabel} · 业务明细`"
            :rows="filteredSchedules"
            :query="query"
            :risk-filter="riskFilter"
            @update:query="query = $event"
            @update:risk-filter="riskFilter = $event"
            @inspect="openDetails"
          />
        </UiTabPanel>
      </section>
    </UiTabs>

    <UiDialog
      v-model:open="detailOpen"
      :title="activeRow?.orderCode ?? '回款计划详情'"
      :description="activeRow ? `${activeRow.customer} · ${activeRow.risk}` : undefined"
      size="lg"
    >
      <dl
        v-if="activeRow"
        :class="$style.detailGrid"
      >
        <div>
          <dt>计划金额</dt>
          <dd>{{ currency(activeRow.planned) }}</dd>
        </div>
        <div>
          <dt>已到账</dt>
          <dd>{{ currency(activeRow.received) }}</dd>
        </div>
        <div>
          <dt>逾期金额</dt>
          <dd :class="$style.dangerValue">{{ currency(activeRow.overdueAmount) }}</dd>
        </div>
        <div>
          <dt>逾期 / 即将</dt>
          <dd>{{ activeRow.overdueCount }} / {{ activeRow.upcomingCount }}</dd>
        </div>
        <div>
          <dt>最长逾期</dt>
          <dd>{{ activeRow.longestOverdueDays }} 天</dd>
        </div>
        <div>
          <dt>下一期</dt>
          <dd>{{ activeRow.nextDate }}</dd>
        </div>
        <div>
          <dt>倒计时</dt>
          <dd :class="$style.dangerValue">已逾期 {{ Math.abs(activeRow.countdownDays) }} 天</dd>
        </div>
        <div>
          <dt>计划状态</dt>
          <dd>{{ activeRow.status }}</dd>
        </div>
      </dl>
      <template #icon>
        <span :class="$style.dialogIcon">
          <Landmark
            :size="18"
            :stroke-width="1.8"
          />
        </span>
      </template>
      <template #footer>
        <UiButton
          variant="primary"
          size="sm"
          @click="detailOpen = false"
          >知道了</UiButton
        >
      </template>
    </UiDialog>
  </main>
</template>

<style module>
@reference "../../../styles/index.css";

.page {
  @apply min-h-full min-w-[1080px] px-[26px] pb-8 pt-[22px];
  color: var(--ui-text);
  background: var(--ui-surface-subtle);
}

.workspace {
  @apply mt-5 overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}

.detailGrid {
  @apply grid grid-cols-2 gap-3 p-5;
}

.detailGrid > div {
  @apply rounded border px-3 py-3;
  border-color: var(--ui-border-subtle);
  background: var(--ui-surface-subtle);
}

.detailGrid dt {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}

.detailGrid dd {
  @apply mt-1.5 text-[12px] font-semibold;
  color: var(--ui-text);
}

.detailGrid .dangerValue {
  color: var(--ui-danger);
}

.dialogIcon {
  @apply flex h-9 w-9 shrink-0 items-center justify-center rounded-full;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
</style>
