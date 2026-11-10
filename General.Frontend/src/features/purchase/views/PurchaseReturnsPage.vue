<script setup lang="ts">
import { computed, reactive, ref } from 'vue';
import { toast } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import {
  purchaseReturnReasons,
  purchaseReturnReferenceFixture,
  purchaseReturnSuppliers,
  purchaseReturnTimeOptions,
} from '../model/purchase-returns.data';
import type {
  PurchaseReturnDraft,
  PurchaseReturnRecord,
  PurchaseReturnStatus,
  PurchaseReturnTab,
  PurchaseReturnTimeFilter,
} from '../model/purchase-returns.types';
import PurchaseReturnDialogs from './purchase-returns/PurchaseReturnDialogs.vue';
import PurchaseReturnsHero from './purchase-returns/PurchaseReturnsHero.vue';
import PurchaseReturnsOverview from './purchase-returns/PurchaseReturnsOverview.vue';
import PurchaseReturnsWorkspace from './purchase-returns/PurchaseReturnsWorkspace.vue';

const rows = ref<PurchaseReturnRecord[]>(purchaseReturnReferenceFixture.rows.map((row) => ({ ...row })));
const activeStatus = ref<PurchaseReturnTab>('全部');
const supplier = ref('');
const query = ref('');
const timeFilter = ref<PurchaseReturnTimeFilter>('all');
const startDate = ref('');
const endDate = ref('');
const createOpen = ref(false);
const detailOpen = ref(false);
const activeRow = ref<PurchaseReturnRecord | null>(null);

const draft = reactive<PurchaseReturnDraft>({
  poId: '',
  supplier: '',
  reason: '',
  items: '1',
  amount: '',
  date: new Date().toISOString().slice(0, 10),
  note: '',
});

const supplierOptions: readonly UiSelectOption[] = [
  { value: '', label: '全部供应商' },
  ...purchaseReturnSuppliers.map((value) => ({ value, label: value })),
];

const reasonOptions: readonly UiSelectOption[] = purchaseReturnReasons.map((value) => ({ value, label: value }));

const dateRange = computed<[string, string]>(() => {
  if (timeFilter.value === 'all') return ['', ''];
  if (timeFilter.value === 'custom') return [startDate.value, endDate.value];
  const now = new Date();
  const end = now.toISOString().slice(0, 10);
  if (timeFilter.value === 'today') return [end, end];
  if (timeFilter.value === '7d') {
    const start = new Date(now);
    start.setDate(start.getDate() - 6);
    return [start.toISOString().slice(0, 10), end];
  }
  if (timeFilter.value === 'month') {
    const start = new Date(now.getFullYear(), now.getMonth(), 1);
    return [start.toISOString().slice(0, 10), end];
  }
  const start = new Date(now);
  start.setMonth(start.getMonth() - 3);
  return [start.toISOString().slice(0, 10), end];
});

const filteredRows = computed(() => {
  const needle = query.value.trim().toLocaleLowerCase();
  const [from, to] = dateRange.value;
  return rows.value.filter((row) => {
    const matchesStatus = activeStatus.value === '全部' || row.status === activeStatus.value;
    const matchesSupplier = !supplier.value || row.supplier === supplier.value;
    const matchesQuery =
      !needle || row.id.toLocaleLowerCase().includes(needle) || row.supplier.toLocaleLowerCase().includes(needle);
    const matchesTime = !from || !to || (row.date >= from && row.date <= to);
    return matchesStatus && matchesSupplier && matchesQuery && matchesTime;
  });
});

const totals = computed(() => ({
  total: rows.value.length,
  pending: rows.value.filter((row) => row.status === '草稿' || row.status === '待审核').length,
  processing: rows.value.filter((row) => row.status === '已审核' || row.status === '退货中').length,
  refundedAmount: rows.value.filter((row) => row.status === '已退款').reduce((sum, row) => sum + row.amount, 0),
}));

const timeLabel = computed(
  () => purchaseReturnTimeOptions.find((option) => option.value === timeFilter.value)?.label ?? '全部时间',
);

function resetDraft() {
  Object.assign(draft, {
    poId: '',
    supplier: '',
    reason: '',
    items: '1',
    amount: '',
    date: new Date().toISOString().slice(0, 10),
    note: '',
  });
}

function openCreate() {
  resetDraft();
  createOpen.value = true;
}

function saveReturn() {
  if (!draft.supplier || !draft.reason || Number(draft.items) < 1 || Number(draft.amount) <= 0) {
    toast.warning('请完整填写供应商、退货原因、数量和金额');
    return;
  }
  const row: PurchaseReturnRecord = {
    id: `PR-RET-LOCAL-${String(rows.value.length + 1).padStart(4, '0')}`,
    poId: draft.poId.trim() || '-',
    supplier: draft.supplier,
    reason: draft.reason,
    items: Number(draft.items),
    amount: Number(draft.amount),
    status: '草稿',
    date: draft.date,
    attachments: 0,
    refunded: false,
    operator: '张经理',
  };
  rows.value.unshift(row);
  createOpen.value = false;
  activeStatus.value = '全部';
  toast.success(`${row.id} 已保存为草稿`);
}

function openDetail(row: PurchaseReturnRecord) {
  activeRow.value = row;
  detailOpen.value = true;
}

function advanceStatus() {
  if (!activeRow.value) return;
  const next: Partial<Record<PurchaseReturnStatus, PurchaseReturnStatus>> = {
    草稿: '待审核',
    待审核: '已审核',
    已审核: '退货中',
    退货中: '已退货',
    已退货: '已退款',
  };
  const status = next[activeRow.value.status];
  if (!status) return;
  activeRow.value.status = status;
  if (status === '已退款') activeRow.value.refunded = true;
  toast.success(`${activeRow.value.id} 已更新为${status}`);
}

function updateDraft(value: PurchaseReturnDraft) {
  Object.assign(draft, value);
}

function exportRows() {
  const header = [
    '退货单号',
    '关联采购单',
    '供应商',
    '退货原因',
    '退货数量',
    '退货金额',
    '退款',
    '状态',
    '日期',
    '附件',
  ];
  const body = filteredRows.value.map((row) => [
    row.id,
    row.poId,
    row.supplier,
    row.reason,
    row.items,
    row.amount,
    row.refunded ? '已退款' : '-',
    row.status,
    row.date,
    row.attachments,
  ]);
  const escape = (value: string | number) => `"${String(value).replaceAll('"', '""')}"`;
  const csv = [header, ...body].map((line) => line.map(escape).join(',')).join('\n');
  const url = URL.createObjectURL(new Blob([`\uFEFF${csv}`], { type: 'text/csv;charset=utf-8' }));
  const link = document.createElement('a');
  link.href = url;
  link.download = '采购退货.csv';
  link.click();
  URL.revokeObjectURL(url);
  toast.success(`已导出 ${body.length} 条退货记录`);
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="list"
    data-page-route="/purchase/returns"
    data-page-label="采购退货"
    data-page-module="采购管理"
    data-purchase-returns-page
    :data-active-status="activeStatus"
    :data-result-count="filteredRows.length"
    :data-local-row-count="rows.length"
  >
    <PurchaseReturnsHero />
    <PurchaseReturnsOverview
      :total="totals.total"
      :pending="totals.pending"
      :processing="totals.processing"
      :refunded-amount="totals.refundedAmount"
    />
    <PurchaseReturnsWorkspace
      :rows="filteredRows"
      :active-status="activeStatus"
      :supplier="supplier"
      :query="query"
      :supplier-options="supplierOptions"
      :time-filter="timeFilter"
      :time-label="timeLabel"
      :start-date="startDate"
      :end-date="endDate"
      :pending-count="totals.pending"
      @update:active-status="activeStatus = $event"
      @update:supplier="supplier = $event"
      @update:query="query = $event"
      @update:time-filter="timeFilter = $event"
      @update:start-date="startDate = $event"
      @update:end-date="endDate = $event"
      @open="openDetail"
      @export="exportRows"
      @create="openCreate"
    />
  </main>

  <PurchaseReturnDialogs
    :create-open="createOpen"
    :detail-open="detailOpen"
    :active-row="activeRow"
    :draft="draft"
    :supplier-options="supplierOptions"
    :reason-options="reasonOptions"
    @update:create-open="createOpen = $event"
    @update:detail-open="detailOpen = $event"
    @update:draft="updateDraft"
    @save="saveReturn"
    @advance="advanceStatus"
  />
</template>

<style module>
@reference "../../../styles/index.css";

.page {
  @apply mx-auto min-h-full min-w-[1180px] max-w-[1400px] pb-8;
  color: var(--ui-text);
}
</style>
