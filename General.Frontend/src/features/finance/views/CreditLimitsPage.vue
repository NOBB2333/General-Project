<script setup lang="ts">
import { computed, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import { toast } from '@/components/ui';
import { creditLimitsReferenceFixture, creditMetrics, pendingCreditRequests } from '../model/credit-limits.data';
import type {
  CreditFreezeFilter,
  CreditLimitRow,
  CreditRiskFilter,
  CreditView,
  PendingCreditRequest,
} from '../model/credit-limits.types';
import CreditLimitDetailDialog from './credit-limits/CreditLimitDetailDialog.vue';
import CreditLimitsHero from './credit-limits/CreditLimitsHero.vue';
import CreditLimitsOverview from './credit-limits/CreditLimitsOverview.vue';
import CreditLimitsWorkspace from './credit-limits/CreditLimitsWorkspace.vue';

const rows = ref<CreditLimitRow[]>(cloneReferenceRows(creditLimitsReferenceFixture.rows));
const pendingRows = ref<PendingCreditRequest[]>(cloneReferenceRows(pendingCreditRequests));
const activeView = ref<CreditView>(creditLimitsReferenceFixture.defaults?.activeView as CreditView);
const riskFilter = ref<CreditRiskFilter>(creditLimitsReferenceFixture.defaults?.riskFilter as CreditRiskFilter);
const freezeFilter = ref<CreditFreezeFilter>(creditLimitsReferenceFixture.defaults?.freezeFilter as CreditFreezeFilter);
const query = ref('');
const detailOpen = ref(false);
const activeRow = ref<CreditLimitRow | null>(null);
const activeRequest = ref<PendingCreditRequest | null>(null);

const customerCount = computed(() => rows.value.length);
const pendingCount = computed(() => pendingRows.value.length);
const riskCounts = computed<Record<CreditRiskFilter, number>>(() => ({
  全部: rows.value.length,
  正常: rows.value.filter((row) => row.riskStatus === '正常').length,
  预警: rows.value.filter((row) => row.riskStatus === '预警').length,
  超限: rows.value.filter((row) => row.riskStatus === '超限').length,
}));
const freezeCounts = computed<Record<CreditFreezeFilter, number>>(() => ({
  全部: rows.value.length,
  正常: rows.value.filter((row) => row.freezeStatus === '正常').length,
  已冻结: rows.value.filter((row) => row.freezeStatus === '已冻结').length,
}));

const filteredRows = computed(() => {
  const normalized = query.value.trim().toLocaleLowerCase('zh-CN');
  return rows.value.filter((row) => {
    const matchesRisk = riskFilter.value === '全部' || row.riskStatus === riskFilter.value;
    const matchesFreeze = freezeFilter.value === '全部' || row.freezeStatus === freezeFilter.value;
    const matchesQuery =
      !normalized ||
      `${row.customer} ${row.id} ${row.owner} ${row.customerType}`.toLocaleLowerCase('zh-CN').includes(normalized);
    return matchesRisk && matchesFreeze && matchesQuery;
  });
});

function updateView(value: CreditView) {
  activeView.value = value;
  query.value = '';
  if (value === 'pending') {
    riskFilter.value = '全部';
    freezeFilter.value = '全部';
  }
}

function openRow(row: CreditLimitRow) {
  activeRow.value = row;
  activeRequest.value = null;
  detailOpen.value = true;
}

function openRequest(request: PendingCreditRequest) {
  activeRequest.value = request;
  activeRow.value = null;
  detailOpen.value = true;
}

function toggleFreeze(row: CreditLimitRow) {
  const nextStatus = row.freezeStatus === '已冻结' ? '正常' : '已冻结';
  rows.value = rows.value.map((item) => (item.id === row.id ? { ...item, freezeStatus: nextStatus } : item));
  activeRow.value = rows.value.find((item) => item.id === row.id) ?? null;
  toast.success(`${row.customer} 已${nextStatus === '已冻结' ? '冻结' : '解除冻结'}授信`);
}

function approveRequest(request: PendingCreditRequest) {
  const created: CreditLimitRow = {
    id: `credit-local-${Date.now()}`,
    customer: request.customer,
    customerType: request.customerType,
    limit: request.requestedLimit,
    used: 0,
    overdueAmount: 0,
    overdueCount: 0,
    riskStatus: '正常',
    freezeStatus: '正常',
    owner: request.owner,
  };
  rows.value = [created, ...rows.value];
  pendingRows.value = pendingRows.value.filter((item) => item.id !== request.id);
  activeView.value = 'customers';
  activeRequest.value = null;
  detailOpen.value = false;
  toast.success(`${request.customer} 的授信申请已批准并写入本地客户列表`);
}

function exportRows() {
  const header = [
    '客户名称',
    '类型',
    '授信额度',
    '已用额度',
    '可用额度',
    '使用率',
    '超期金额',
    '超期笔数',
    '状态',
    '冻结状态',
    '负责人',
  ];
  const body = filteredRows.value.map((row) => [
    row.customer,
    row.customerType,
    row.limit,
    row.used,
    row.limit - row.used,
    `${Math.round((row.used / row.limit) * 100)}%`,
    row.overdueAmount,
    row.overdueCount,
    row.riskStatus,
    row.freezeStatus,
    row.owner,
  ]);
  const csv = [header, ...body]
    .map((line) => line.map((cell) => `"${String(cell).replaceAll('"', '""')}"`).join(','))
    .join('\n');
  const url = URL.createObjectURL(new Blob([`\uFEFF${csv}`], { type: 'text/csv;charset=utf-8' }));
  const link = document.createElement('a');
  link.href = url;
  link.download = '授信额度.csv';
  link.click();
  URL.revokeObjectURL(url);
  toast.success(`已导出 ${body.length} 条授信客户记录`);
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="list"
    data-page-route="/finance/credit-limits"
    data-page-label="授信额度管理"
    data-page-module="财务管理"
    data-credit-limits-page
    :data-active-view="activeView"
    :data-risk-filter="riskFilter"
    :data-freeze-filter="freezeFilter"
    :data-result-count="activeView === 'customers' ? filteredRows.length : pendingRows.length"
    :data-frozen-count="freezeCounts['已冻结']"
    :data-pending-count="pendingCount"
  >
    <CreditLimitsHero />
    <CreditLimitsOverview :metrics="creditMetrics" />
    <CreditLimitsWorkspace
      :view="activeView"
      :rows="filteredRows"
      :pending-rows="pendingRows"
      :customer-count="customerCount"
      :pending-count="pendingCount"
      :risk-counts="riskCounts"
      :freeze-counts="freezeCounts"
      :risk-filter="riskFilter"
      :freeze-filter="freezeFilter"
      :query="query"
      @update:view="updateView"
      @update:risk-filter="riskFilter = $event"
      @update:freeze-filter="freezeFilter = $event"
      @update:query="query = $event"
      @export="exportRows"
      @view-row="openRow"
      @view-request="openRequest"
    />
    <CreditLimitDetailDialog
      v-model:open="detailOpen"
      :row="activeRow"
      :request="activeRequest"
      @freeze="toggleFreeze"
      @approve="approveRequest"
    />
  </main>
</template>

<style module>
@reference "../../../styles/index.css";

.page {
  @apply mx-auto min-h-full min-w-[1180px] max-w-[1400px] pb-8;
  color: var(--ui-text);
}
</style>
