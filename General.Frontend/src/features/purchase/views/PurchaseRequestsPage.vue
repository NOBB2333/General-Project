<script setup lang="ts">
import { computed, ref } from 'vue';
import { useRouter } from 'vue-router';
import { cloneReferenceRows } from '@/components/page';
import { UiButton, UiDialog, type UiSelectOption, toast } from '@/components/ui';
import { purchaseRequestsReferenceFixture } from '../model/purchase-requests.data';
import type {
  PurchaseRequestDraft,
  PurchaseRequestPriority,
  PurchaseRequestRow,
  PurchaseRequestSortKey,
  PurchaseRequestStatus,
} from '../model/purchase-requests.types';
import PurchaseRequestFormDialog from './purchase-requests/PurchaseRequestFormDialog.vue';
import PurchaseRequestHero from './purchase-requests/PurchaseRequestHero.vue';
import PurchaseRequestOverview from './purchase-requests/PurchaseRequestOverview.vue';
import PurchaseRequestTable from './purchase-requests/PurchaseRequestTable.vue';
import PurchaseRequestToolbar from './purchase-requests/PurchaseRequestToolbar.vue';

const router = useRouter();
const rows = ref<PurchaseRequestRow[]>(cloneReferenceRows(purchaseRequestsReferenceFixture.rows));
const status = ref<'全部' | PurchaseRequestStatus>('全部');
const supplier = ref('');
const requestor = ref('');
const timeRange = ref('all');
const query = ref('');
const sortKey = ref<PurchaseRequestSortKey | ''>('');
const sortDirection = ref<'asc' | 'desc'>('asc');
const formOpen = ref(false);
const detailOpen = ref(false);
const activeRow = ref<PurchaseRequestRow | null>(null);

const supplierOptions = computed<UiSelectOption[]>(() => [
  { value: '', label: '全部供应商' },
  ...[...new Set(rows.value.filter((row) => row.supplier !== '-').map((row) => row.supplier))].map((value) => ({
    value,
    label: value,
  })),
]);
const requestorOptions = computed<UiSelectOption[]>(() => [
  { value: '', label: '全部申请人' },
  ...[...new Set(rows.value.map((row) => row.requestor))].map((value) => ({ value, label: value })),
]);

const filteredRows = computed(() => {
  const keyword = query.value.trim().toLowerCase();
  const result = rows.value.filter((row) => {
    const matchesStatus = status.value === '全部' || row.status === status.value;
    const matchesSupplier = !supplier.value || row.supplier === supplier.value;
    const matchesRequestor = !requestor.value || row.requestor === requestor.value;
    const matchesQuery =
      !keyword || row.id.toLowerCase().includes(keyword) || row.title.toLowerCase().includes(keyword);
    return matchesStatus && matchesSupplier && matchesRequestor && matchesQuery && matchesTimeRange(row.date);
  });

  if (!sortKey.value) return result;
  const priorityRanks: Record<PurchaseRequestPriority, number> = { 高: 3, 中: 2, 低: 1 };
  return [...result].sort((left, right) => {
    const key = sortKey.value as PurchaseRequestSortKey;
    const leftValue = key === 'priority' ? priorityRanks[left.priority] : left[key];
    const rightValue = key === 'priority' ? priorityRanks[right.priority] : right[key];
    const comparison =
      typeof leftValue === 'number' && typeof rightValue === 'number'
        ? leftValue - rightValue
        : String(leftValue).localeCompare(String(rightValue), 'zh-CN');
    return sortDirection.value === 'asc' ? comparison : -comparison;
  });
});

function matchesTimeRange(date: string) {
  if (timeRange.value === 'all') return true;
  const today = new Date();
  const target = new Date(`${date}T00:00:00`);
  const start = new Date(today);
  start.setHours(0, 0, 0, 0);
  if (timeRange.value === 'today') return target.toDateString() === start.toDateString();
  if (timeRange.value === '7d') start.setDate(start.getDate() - 6);
  if (timeRange.value === 'month') start.setDate(1);
  if (timeRange.value === '3m') start.setMonth(start.getMonth() - 3);
  return target >= start && target <= today;
}

function updateSort(key: PurchaseRequestSortKey) {
  if (sortKey.value !== key) {
    sortKey.value = key;
    sortDirection.value = 'asc';
    return;
  }
  if (sortDirection.value === 'asc') {
    sortDirection.value = 'desc';
    return;
  }
  sortKey.value = '';
  sortDirection.value = 'asc';
}

function openDetail(row: PurchaseRequestRow) {
  activeRow.value = row;
  detailOpen.value = true;
}

function openOrder(order: string) {
  void router.push({ path: '/purchase/orders', query: { order } });
}

function saveRequest(draft: PurchaseRequestDraft) {
  const sequence = Math.max(...rows.value.map((row) => Number(row.id.slice(-4)))) + 1;
  rows.value = [
    {
      id: `PR-2026-${String(sequence).padStart(4, '0')}`,
      title: draft.title.trim(),
      project: draft.project.trim() || '-',
      customer: '',
      requestor: draft.requestor,
      requestorDept: '采购部',
      supplier: draft.supplier,
      priority: draft.priority,
      amount: Number(draft.amount) || 0,
      items: Number(draft.items) || 1,
      status: '草稿',
      date: new Date().toISOString().slice(0, 10),
      approver: '-',
      poList: [],
    },
    ...rows.value,
  ];
  formOpen.value = false;
  status.value = '全部';
  toast.success('采购申请草稿已保存到本地列表');
}

function exportRows() {
  const header = ['申请编号', '申请标题', '优先级', '关联项目', '申请人', '商品数', '金额', '状态', '日期', '审批人'];
  const csvRows = filteredRows.value.map((row) => [
    row.id,
    row.title,
    row.priority,
    row.project === '-' ? '库存采购' : row.project,
    row.requestor,
    row.items,
    row.amount,
    row.status,
    row.date,
    row.approver,
  ]);
  const csv = [header, ...csvRows]
    .map((line) => line.map((cell) => `"${String(cell).replaceAll('"', '""')}"`).join(','))
    .join('\n');
  const href = URL.createObjectURL(new Blob([`\uFEFF${csv}`], { type: 'text/csv;charset=utf-8' }));
  const anchor = document.createElement('a');
  anchor.href = href;
  anchor.download = `采购申请_${new Date().toISOString().slice(0, 10)}.csv`;
  anchor.click();
  URL.revokeObjectURL(href);
  toast.success(`已导出 ${filteredRows.value.length} 条采购申请记录`);
}
</script>

<template>
  <main
    :class="$style.page"
    data-page-family="list"
    data-page-route="/purchase/requests"
    data-purchase-requests-page
    :data-active-status="status"
  >
    <PurchaseRequestHero />
    <PurchaseRequestOverview :rows="rows" />
    <PurchaseRequestToolbar
      v-model:status="status"
      v-model:supplier="supplier"
      v-model:requestor="requestor"
      v-model:time-range="timeRange"
      v-model:query="query"
      :supplier-options="supplierOptions"
      :requestor-options="requestorOptions"
      @export="exportRows"
      @create="formOpen = true"
    />
    <PurchaseRequestTable
      :rows="filteredRows"
      :sort-key="sortKey"
      :sort-direction="sortDirection"
      @sort="updateSort"
      @view="openDetail"
      @open-order="openOrder"
    />
  </main>

  <PurchaseRequestFormDialog
    v-model:open="formOpen"
    @save="saveRequest"
  />

  <UiDialog
    v-model:open="detailOpen"
    :title="activeRow?.title ?? '采购申请详情'"
    :description="activeRow?.id"
    size="lg"
  >
    <dl
      v-if="activeRow"
      :class="$style.details"
    >
      <div>
        <dt>状态</dt>
        <dd>{{ activeRow.status }}</dd>
      </div>
      <div>
        <dt>优先级</dt>
        <dd>{{ activeRow.priority }}</dd>
      </div>
      <div>
        <dt>申请人</dt>
        <dd>{{ activeRow.requestor }} · {{ activeRow.requestorDept }}</dd>
      </div>
      <div>
        <dt>审批人</dt>
        <dd>{{ activeRow.approver }}</dd>
      </div>
      <div>
        <dt>关联项目</dt>
        <dd>{{ activeRow.project === '-' ? '库存采购' : activeRow.project }}</dd>
      </div>
      <div>
        <dt>供应商</dt>
        <dd>{{ activeRow.supplier }}</dd>
      </div>
      <div>
        <dt>商品数</dt>
        <dd>{{ activeRow.items }}</dd>
      </div>
      <div>
        <dt>预算金额</dt>
        <dd>¥{{ activeRow.amount.toLocaleString('en-US') }}</dd>
      </div>
    </dl>
    <template #footer>
      <UiButton
        variant="primary"
        @click="detailOpen = false"
        >关闭</UiButton
      >
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../../styles/index.css";

.page {
  @apply mx-auto min-h-full w-full max-w-[1400px] pb-8;
  color: var(--ui-text);
}
.details {
  @apply grid grid-cols-2 gap-4 p-5;
}
.details dt {
  @apply text-[11px];
  color: var(--ui-text-subtle);
}
.details dd {
  @apply mt-1 text-[13px] font-medium;
  color: var(--ui-text);
}
</style>
