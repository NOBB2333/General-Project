<script setup lang="ts">
import { computed, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import { toast } from '@/components/ui';
import { refundMetrics, refundsReferenceFixture } from '../model/refunds.data';
import type {
  RefundDocumentStatus,
  RefundDraft,
  RefundKind,
  RefundRow,
  RefundStatusFilter,
} from '../model/refunds.types';
import RefundCreateDialog from './refunds/RefundCreateDialog.vue';
import RefundDetailDialog from './refunds/RefundDetailDialog.vue';
import RefundsHero from './refunds/RefundsHero.vue';
import RefundsOverview from './refunds/RefundsOverview.vue';
import RefundsWorkspace from './refunds/RefundsWorkspace.vue';

const rows = ref<RefundRow[]>(cloneReferenceRows(refundsReferenceFixture.rows));
const activeKind = ref<RefundKind>(refundsReferenceFixture.defaults.activeKind as RefundKind);
const activeStatus = ref<RefundStatusFilter>(refundsReferenceFixture.defaults.activeStatus as RefundStatusFilter);
const query = ref('');
const createOpen = ref(false);
const detailOpen = ref(false);
const activeRow = ref<RefundRow | null>(null);

const customerCount = computed(() => rows.value.filter((row) => row.kind === 'customer').length);
const supplierCount = computed(() => rows.value.filter((row) => row.kind === 'supplier').length);
const kindRows = computed(() => rows.value.filter((row) => row.kind === activeKind.value));
const statusCounts = computed<Record<RefundDocumentStatus, number>>(() => ({
  草稿: kindRows.value.filter((row) => row.status === '草稿').length,
  待审批: kindRows.value.filter((row) => row.status === '待审批').length,
  审批中: kindRows.value.filter((row) => row.status === '审批中').length,
  已批准: kindRows.value.filter((row) => row.status === '已批准').length,
  已付款: kindRows.value.filter((row) => row.status === '已付款').length,
  已核销: kindRows.value.filter((row) => row.status === '已核销').length,
  已驳回: kindRows.value.filter((row) => row.status === '已驳回').length,
}));
const filteredRows = computed(() => {
  const normalized = query.value.trim().toLocaleLowerCase('zh-CN');
  return kindRows.value.filter((row) => {
    const matchesStatus = activeStatus.value === '全部' || row.status === activeStatus.value;
    const matchesQuery =
      !normalized ||
      `${row.code} ${row.contract} ${row.party} ${row.description}`.toLocaleLowerCase('zh-CN').includes(normalized);
    return matchesStatus && matchesQuery;
  });
});

function updateKind(value: RefundKind) {
  activeKind.value = value;
  activeStatus.value = '全部';
  query.value = '';
}

function openDetail(row: RefundRow) {
  activeRow.value = row;
  detailOpen.value = true;
}

function saveRefund(draft: RefundDraft) {
  const sequence = activeKind.value === 'customer' ? customerCount.value + 1 : supplierCount.value + 1;
  const created: RefundRow = {
    id: `${activeKind.value}-refund-local-${Date.now()}`,
    kind: activeKind.value,
    code: `${activeKind.value === 'customer' ? 'REF' : 'SREF'}-LOCAL-${String(sequence).padStart(4, '0')}`,
    contract: draft.contract.trim(),
    party: draft.party.trim(),
    description: draft.description.trim() || '本地新建退款申请',
    currency: draft.currency,
    amount: Number(draft.amount),
    method: draft.method,
    status: '草稿',
    approvalStatus: '-',
    paymentStatus: '-',
    writeoffStatus: '-',
    requestedAt: '2026-09-16',
  };
  rows.value = [created, ...rows.value];
  activeStatus.value = '全部';
  query.value = '';
  createOpen.value = false;
  toast.success(`${created.code} 已保存到当前本地会话`);
}

function submitRefund(row: RefundRow) {
  rows.value = rows.value.map((item) =>
    item.id === row.id ? { ...item, status: '待审批', approvalStatus: '待审批' } : item,
  );
  activeRow.value = rows.value.find((item) => item.id === row.id) ?? null;
  toast.success(`${row.code} 已在本地提交审批`);
}

function exportRows() {
  const header = [
    '申请单号',
    '关联单据',
    '往来单位',
    '币种',
    '实退金额',
    '退款方式',
    '单据状态',
    '财务审批',
    '付款状态',
    '核销状态',
    '申请日期',
  ];
  const body = filteredRows.value.map((row) => [
    row.code,
    row.contract,
    row.party,
    row.currency,
    row.amount,
    row.method,
    row.status,
    row.approvalStatus,
    row.paymentStatus,
    row.writeoffStatus,
    row.requestedAt,
  ]);
  const csv = [header, ...body]
    .map((line) => line.map((cell) => `"${String(cell).replaceAll('"', '""')}"`).join(','))
    .join('\n');
  const url = URL.createObjectURL(new Blob([`\uFEFF${csv}`], { type: 'text/csv;charset=utf-8' }));
  const link = document.createElement('a');
  link.href = url;
  link.download = `${activeKind.value === 'customer' ? '客户' : '供应商'}退款.csv`;
  link.click();
  URL.revokeObjectURL(url);
  toast.success(`已导出 ${body.length} 条退款申请`);
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="list"
    data-page-route="/finance/refunds"
    data-page-label="退款申请"
    data-page-module="财务管理"
    data-refunds-page
    :data-active-kind="activeKind"
    :data-active-status="activeStatus"
    :data-customer-count="customerCount"
    :data-supplier-count="supplierCount"
    :data-result-count="filteredRows.length"
  >
    <RefundsHero />
    <RefundsOverview
      :active-kind="activeKind"
      :customer-count="customerCount"
      :supplier-count="supplierCount"
      :metrics="refundMetrics"
      @update:active-kind="updateKind"
    />
    <RefundsWorkspace
      :rows="filteredRows"
      :active-kind="activeKind"
      :active-status="activeStatus"
      :status-counts="statusCounts"
      :query="query"
      @update:active-status="activeStatus = $event"
      @update:query="query = $event"
      @export="exportRows"
      @create="createOpen = true"
      @view="openDetail"
    />

    <RefundCreateDialog
      v-model:open="createOpen"
      :kind="activeKind"
      @save="saveRefund"
    />
    <RefundDetailDialog
      v-model:open="detailOpen"
      :row="activeRow"
      @submit="submitRefund"
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
