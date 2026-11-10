<script setup lang="ts">
import { computed, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import { UiButton, UiDialog, type UiSelectOption, toast } from '@/components/ui';
import { purchaseContractsReferenceFixture } from '../model/purchase-contracts.data';
import type {
  PurchaseContractDraft,
  PurchaseContractRow,
  PurchaseContractSortKey,
  PurchaseContractStatus,
} from '../model/purchase-contracts.types';
import PurchaseContractFormDialog from './purchase-contracts/PurchaseContractFormDialog.vue';
import PurchaseContractHero from './purchase-contracts/PurchaseContractHero.vue';
import PurchaseContractOverview from './purchase-contracts/PurchaseContractOverview.vue';
import PurchaseContractWorkspace from './purchase-contracts/PurchaseContractWorkspace.vue';

const rows = ref<PurchaseContractRow[]>(cloneReferenceRows(purchaseContractsReferenceFixture.rows));
const status = ref<'全部' | PurchaseContractStatus>('全部');
const typeFilter = ref('');
const supplier = ref('');
const query = ref('');
const sortKey = ref<PurchaseContractSortKey | ''>('');
const sortDirection = ref<'asc' | 'desc'>('asc');
const detailOpen = ref(false);
const formOpen = ref(false);
const activeRow = ref<PurchaseContractRow | null>(null);
const typeOptions: UiSelectOption[] = [
  { value: '', label: '全部类型' },
  ...['框架合同', '采购合同', '服务合同'].map((value) => ({ value, label: value })),
];
const supplierOptions = computed<UiSelectOption[]>(() => [
  { value: '', label: '全部供应商' },
  ...[...new Set(rows.value.map((r) => r.supplier))].map((value) => ({ value, label: value })),
]);
const filteredRows = computed(() => {
  const k = query.value.trim().toLowerCase();
  const result = rows.value.filter(
    (r) =>
      (status.value === '全部' || r.status === status.value) &&
      (!typeFilter.value || r.type === typeFilter.value) &&
      (!supplier.value || r.supplier === supplier.value) &&
      (!k ||
        r.id.toLowerCase().includes(k) ||
        r.name.toLowerCase().includes(k) ||
        r.supplier.toLowerCase().includes(k)),
  );
  if (!sortKey.value) return result;
  return [...result].sort((a, b) => {
    const key = sortKey.value as PurchaseContractSortKey;
    const av = a[key],
      bv = b[key];
    const c =
      typeof av === 'number' && typeof bv === 'number' ? av - bv : String(av).localeCompare(String(bv), 'zh-CN');
    return sortDirection.value === 'asc' ? c : -c;
  });
});
function updateSort(key: PurchaseContractSortKey) {
  if (sortKey.value !== key) {
    sortKey.value = key;
    sortDirection.value = 'asc';
    return;
  }
  sortDirection.value = sortDirection.value === 'asc' ? 'desc' : 'asc';
}
function view(row: PurchaseContractRow) {
  activeRow.value = row;
  detailOpen.value = true;
}
function save(d: PurchaseContractDraft) {
  const n = Math.max(...rows.value.map((r) => Number(r.id.slice(-4)))) + 1;
  rows.value = [
    {
      id: `PC-2026-${String(n).padStart(4, '0')}`,
      name: d.name.trim(),
      project: d.project.trim() || '-',
      type: d.type,
      supplier: d.supplier.trim(),
      amount: Number(d.amount) || 0,
      executedAmount: 0,
      orderCount: 0,
      retentionRate: d.retentionRate ? Number(d.retentionRate) : null,
      dueDate: d.dueDate,
      status: '待审批',
      warning: false,
      owner: d.owner.trim() || '张经理',
    },
    ...rows.value,
  ];
  formOpen.value = false;
  status.value = '全部';
  toast.success('采购合同已保存到本地台账');
}
function exportRows() {
  const head = ['合同编号', '合同名称', '类型', '供应商', '合同总额', '已下单金额', '质保金', '到期日', '状态'];
  const data = filteredRows.value.map((r) => [
    r.id,
    r.name,
    r.type,
    r.supplier,
    r.amount,
    r.executedAmount,
    r.retentionRate ?? '-',
    r.dueDate,
    r.status,
  ]);
  const csv = [head, ...data]
    .map((line) => line.map((c) => `"${String(c).replaceAll('"', '""')}"`).join(','))
    .join('\n');
  const href = URL.createObjectURL(new Blob([`\uFEFF${csv}`], { type: 'text/csv;charset=utf-8' }));
  const a = document.createElement('a');
  a.href = href;
  a.download = `采购合同_${new Date().toISOString().slice(0, 10)}.csv`;
  a.click();
  URL.revokeObjectURL(href);
  toast.success(`已导出 ${filteredRows.value.length} 条合同`);
}
</script>

<template>
  <main
    :class="$style.page"
    data-page-family="list"
    data-page-route="/purchase/purchase-contracts"
    data-purchase-contracts-page
    :data-active-status="status"
  >
    <PurchaseContractHero /><PurchaseContractOverview :rows="rows" /><PurchaseContractWorkspace
      v-model:status="status"
      v-model:type-filter="typeFilter"
      v-model:supplier="supplier"
      v-model:query="query"
      :rows="filteredRows"
      :sort-key="sortKey"
      :type-options="typeOptions"
      :supplier-options="supplierOptions"
      @sort="updateSort"
      @view="view"
      @export="exportRows"
      @create="formOpen = true"
    />
  </main>
  <PurchaseContractFormDialog
    v-model:open="formOpen"
    @save="save"
  /><UiDialog
    v-model:open="detailOpen"
    :title="activeRow?.name ?? '采购合同详情'"
    :description="activeRow?.id"
    size="lg"
    ><dl
      v-if="activeRow"
      :class="$style.details"
    >
      <div>
        <dt>供应商</dt>
        <dd>{{ activeRow.supplier }}</dd>
      </div>
      <div>
        <dt>合同类型</dt>
        <dd>{{ activeRow.type }}</dd>
      </div>
      <div>
        <dt>合同总额</dt>
        <dd>¥{{ activeRow.amount.toLocaleString('en-US') }}</dd>
      </div>
      <div>
        <dt>已下单执行</dt>
        <dd>¥{{ activeRow.executedAmount.toLocaleString('en-US') }} / {{ activeRow.orderCount }}单</dd>
      </div>
      <div>
        <dt>质保金</dt>
        <dd>{{ activeRow.retentionRate === null ? '—' : `${activeRow.retentionRate}%` }}</dd>
      </div>
      <div>
        <dt>到期日</dt>
        <dd>{{ activeRow.dueDate }}</dd>
      </div>
    </dl>
    <template #footer
      ><UiButton
        variant="primary"
        @click="detailOpen = false"
        >关闭</UiButton
      ></template
    ></UiDialog
  >
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
