<script setup lang="ts">
import { computed, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import { UiButton, UiDialog, UiFormField, UiInput, UiTabPanel, UiTabs, toast } from '@/components/ui';
import { costCenterMetrics, costCentersReferenceFixture, initialCostRules } from '../model/cost-centers.data';
import type {
  CostCenterRow,
  CostCenterView,
  CostCollectionTab,
  CostQueue,
  CostRule,
} from '../model/cost-centers.types';
import CostCentersHero from './cost-centers/CostCentersHero.vue';
import CostCentersOverview from './cost-centers/CostCentersOverview.vue';
import CostCollectionTable from './cost-centers/CostCollectionTable.vue';
import CostResultsPanel from './cost-centers/CostResultsPanel.vue';
import CostRulesPanel from './cost-centers/CostRulesPanel.vue';

const localRows = ref<CostCenterRow[]>(cloneReferenceRows(costCentersReferenceFixture.rows));
const localRules = ref<CostRule[]>(initialCostRules.map((rule) => ({ ...rule })));
const activeView = ref<CostCenterView>(costCentersReferenceFixture.defaults.activeView as CostCenterView);
const activeCollectionTab = ref<CostCollectionTab>(
  costCentersReferenceFixture.defaults.activeCollectionTab as CostCollectionTab,
);
const activeQueue = ref<CostQueue>(costCentersReferenceFixture.defaults.activeQueue as CostQueue);
const query = ref('');
const source = ref<string>(costCentersReferenceFixture.defaults.source);
const status = ref<string>(costCentersReferenceFixture.defaults.status);
const collectOpen = ref(false);
const activeRow = ref<CostCenterRow | null>(null);
const collectAmount = ref('');
const collectContract = ref('');

const normalizedQuery = computed(() => query.value.trim().toLocaleLowerCase('zh-CN'));
const filteredRows = computed(() =>
  localRows.value.filter((row) => {
    const openAmount = row.totalAmount - row.collectedAmount;
    const matchesTab =
      activeCollectionTab.value === 'pending-costs'
        ? openAmount > 0
        : activeCollectionTab.value === 'sales-transfer'
          ? row.sourceType === '付款核销'
          : row.collectedAmount > 0;
    const matchesQueue = activeQueue.value === 'suspended' ? row.status === '暂挂' : row.status !== '暂挂';
    const matchesSource = !source.value || row.sourceType === source.value;
    const matchesStatus = !status.value || row.status === status.value;
    const matchesQuery =
      !normalizedQuery.value ||
      `${row.code} ${row.name} ${row.customer} ${row.contract}`
        .toLocaleLowerCase('zh-CN')
        .includes(normalizedQuery.value);

    return matchesTab && matchesQueue && matchesSource && matchesStatus && matchesQuery;
  }),
);

function updateView(value: string) {
  activeView.value = value as CostCenterView;
}

function openCollection(row: CostCenterRow) {
  activeRow.value = row;
  collectAmount.value = String(row.totalAmount - row.collectedAmount);
  collectContract.value = row.contract === '-' ? '' : row.contract;
  collectOpen.value = true;
}

function confirmCollection() {
  if (!activeRow.value) return;

  const amount = Number(collectAmount.value);
  const openAmount = activeRow.value.totalAmount - activeRow.value.collectedAmount;
  if (!Number.isFinite(amount) || amount <= 0 || amount > openAmount) {
    toast.warning(`归集金额需大于 0 且不超过 ¥${openAmount.toLocaleString('zh-CN')}`);
    return;
  }

  const rowId = activeRow.value.id;
  localRows.value = localRows.value.map((row) => {
    if (row.id !== rowId) return row;
    const collectedAmount = row.collectedAmount + amount;
    return {
      ...row,
      collectedAmount,
      contract: collectContract.value.trim() || row.contract,
      status: collectedAmount >= row.totalAmount ? '已归集' : '部分归集',
    };
  });
  collectOpen.value = false;
  toast.success(`${activeRow.value.code} 已在本地归集 ¥${amount.toLocaleString('zh-CN')}`);
}

function suspendRow(row: CostCenterRow) {
  localRows.value = localRows.value.map((item) => (item.id === row.id ? { ...item, status: '暂挂' as const } : item));
  activeQueue.value = 'suspended';
  toast.success(`${row.code} 已移入本地暂挂队列`);
}

function toggleRule(rule: CostRule, enabled: boolean) {
  localRules.value = localRules.value.map((item) => (item.id === rule.id ? { ...item, enabled } : item));
  toast.success(`${rule.name}已${enabled ? '启用' : '停用'}`);
}

function exportRows() {
  const header = [
    '成本编号',
    '来源类型',
    '成本名称',
    '成本类型',
    '客户',
    '合同/项目',
    '发生日期',
    '总金额',
    '已归集',
    '未归集',
    '状态',
  ];
  const lines = filteredRows.value.map((row) => [
    row.code,
    row.sourceType,
    row.name,
    row.costType,
    row.customer,
    row.contract,
    row.date,
    row.totalAmount,
    row.collectedAmount,
    row.totalAmount - row.collectedAmount,
    row.status,
  ]);
  const csv = [header, ...lines]
    .map((line) => line.map((cell) => `"${String(cell).replace(/"/g, '""')}"`).join(','))
    .join('\n');
  const link = document.createElement('a');
  link.href = URL.createObjectURL(new Blob([`\uFEFF${csv}`], { type: 'text/csv;charset=utf-8' }));
  link.download = 'finance-cost-centers.csv';
  link.click();
  URL.revokeObjectURL(link.href);
  toast.success(`已导出 ${lines.length} 条成本记录`);
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="list"
    data-page-route="/finance/cost-centers"
    data-page-label="成本中心"
    data-page-module="财务管理"
    data-cost-centers-page
    :data-active-view="activeView"
    :data-active-collection-tab="activeCollectionTab"
    :data-active-queue="activeQueue"
    :data-local-row-count="localRows.length"
    :data-visible-row-count="filteredRows.length"
  >
    <UiTabs
      :model-value="activeView"
      @update:model-value="updateView"
    >
      <CostCentersHero />

      <UiTabPanel value="collection">
        <CostCentersOverview :metrics="costCenterMetrics" />
        <CostCollectionTable
          :rows="filteredRows"
          :active-tab="activeCollectionTab"
          :active-queue="activeQueue"
          :query="query"
          :source="source"
          :status="status"
          @update:active-tab="activeCollectionTab = $event"
          @update:active-queue="activeQueue = $event"
          @update:query="query = $event"
          @update:source="source = $event"
          @update:status="status = $event"
          @collect="openCollection"
          @suspend="suspendRow"
          @export="exportRows"
        />
      </UiTabPanel>

      <UiTabPanel value="results">
        <CostResultsPanel :rows="localRows" />
      </UiTabPanel>

      <UiTabPanel value="rules">
        <CostRulesPanel
          :rules="localRules"
          @toggle="toggleRule"
        />
      </UiTabPanel>
    </UiTabs>

    <UiDialog
      v-model:open="collectOpen"
      :title="activeRow ? `成本归集 · ${activeRow.code}` : '成本归集'"
      :description="activeRow ? activeRow.name : undefined"
      size="md"
    >
      <form
        :class="$style.collectForm"
        @submit.prevent="confirmCollection"
      >
        <div :class="$style.collectSummary">
          <span>待归集金额</span>
          <strong>
            ¥{{ activeRow ? (activeRow.totalAmount - activeRow.collectedAmount).toLocaleString('zh-CN') : '0' }}
          </strong>
        </div>
        <UiFormField
          label="归集金额"
          required
        >
          <UiInput
            v-model="collectAmount"
            type="number"
            min="0.01"
            step="0.01"
            aria-label="归集金额"
          />
        </UiFormField>
        <UiFormField
          label="合同/项目"
          hint="留空时保留当前关联信息"
        >
          <UiInput
            v-model="collectContract"
            placeholder="例如 HT-2026-0010"
            aria-label="合同或项目"
          />
        </UiFormField>
      </form>
      <template #footer>
        <UiButton
          variant="secondary"
          size="sm"
          @click="collectOpen = false"
          >取消</UiButton
        >
        <UiButton
          variant="primary"
          size="sm"
          @click="confirmCollection"
          >确认归集</UiButton
        >
      </template>
    </UiDialog>
  </main>
</template>

<style module>
@reference "../../../styles/index.css";

.page {
  @apply mx-auto min-h-full min-w-[1120px] max-w-[1400px] pb-8;
  width: calc(100% - 42px);
  margin-top: 23px;
  color: var(--ui-text);
  background: var(--ui-surface-subtle);
}

.collectForm {
  @apply grid grid-cols-2 gap-4 p-5;
}

.collectSummary {
  @apply col-span-2 flex min-h-[62px] items-center justify-between rounded border px-4;
  border-color: var(--ui-border-subtle);
  background: var(--ui-surface-subtle);
}

.collectSummary span {
  @apply text-[11px];
  color: var(--ui-text-subtle);
}

.collectSummary strong {
  @apply text-[20px] font-semibold;
  color: var(--ui-warning);
}
</style>
