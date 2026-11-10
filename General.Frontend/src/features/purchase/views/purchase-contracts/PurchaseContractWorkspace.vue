<script setup lang="ts">
import { ArrowUpDown, Download, Eye, Plus, Search, TriangleAlert } from 'lucide-vue-next';
import { UiButton, UiInput, UiSelect, UiTabs, UiTabsList, UiTabsTrigger, type UiSelectOption } from '@/components/ui';
import type {
  PurchaseContractRow,
  PurchaseContractSortKey,
  PurchaseContractStatus,
} from '../../model/purchase-contracts.types';
defineProps<{
  rows: readonly PurchaseContractRow[];
  status: '全部' | PurchaseContractStatus;
  typeFilter: string;
  supplier: string;
  query: string;
  sortKey: PurchaseContractSortKey | '';
  typeOptions: readonly UiSelectOption[];
  supplierOptions: readonly UiSelectOption[];
}>();
const emit = defineEmits<{
  'update:status': [v: '全部' | PurchaseContractStatus];
  'update:typeFilter': [v: string];
  'update:supplier': [v: string];
  'update:query': [v: string];
  sort: [v: PurchaseContractSortKey];
  view: [v: PurchaseContractRow];
  export: [];
  create: [];
}>();
const statuses: readonly ('全部' | PurchaseContractStatus)[] = [
  '全部',
  '履约中',
  '待审批',
  '已完成',
  '已逾期',
  '草稿',
  '已关闭',
];
const heads = [
  ['id', '合同编号'],
  ['name', '合同名称'],
  ['type', '类型'],
  ['supplier', '供应商'],
  ['amount', '合同总额'],
  ['execution', '已下单执行'],
  ['retention', '质保金'],
  ['dueDate', '到期日'],
  ['status', '状态'],
  ['action', '操作'],
] as const;
function sortable(k: string): k is PurchaseContractSortKey {
  return ['id', 'name', 'amount', 'dueDate'].includes(k);
}
function pct(r: PurchaseContractRow) {
  return Math.round((r.executedAmount / r.amount) * 100);
}
</script>
<template>
  <section>
    <div :class="$style.toolbar">
      <UiTabs
        :model-value="status"
        @update:model-value="emit('update:status', $event as '全部' | PurchaseContractStatus)"
        ><UiTabsList
          :class="$style.tabs"
          label="合同状态"
          ><UiTabsTrigger
            v-for="item in statuses"
            :key="item"
            :class="$style.tab"
            :value="item"
            >{{ item }}</UiTabsTrigger
          ></UiTabsList
        ></UiTabs
      >
      <div :class="$style.controls">
        <UiSelect
          :model-value="typeFilter"
          :options="typeOptions"
          label="合同类型"
          size="sm"
          @update:model-value="emit('update:typeFilter', $event)"
        /><UiSelect
          :model-value="supplier"
          :options="supplierOptions"
          label="供应商"
          size="sm"
          @update:model-value="emit('update:supplier', $event)"
        /><label :class="$style.search"
          ><Search
            :size="14"
            :stroke-width="1.8" /><UiInput
            :model-value="query"
            aria-label="搜索采购合同"
            placeholder="搜索合同号、名称、供应商..."
            @update:model-value="emit('update:query', String($event))" /></label
        ><UiButton
          size="sm"
          @click="emit('export')"
          ><template #leading
            ><Download
              :size="13"
              :stroke-width="1.8" /></template
          >导出</UiButton
        ><UiButton
          variant="primary"
          size="sm"
          @click="emit('create')"
          ><template #leading
            ><Plus
              :size="13"
              :stroke-width="1.8" /></template
          >新建合同</UiButton
        >
      </div>
    </div>
    <div :class="$style.panel">
      <div :class="$style.scroller">
        <table :class="$style.table">
          <thead>
            <tr>
              <th
                v-for="head in heads"
                :key="head[0]"
                :class="sortable(head[0]) && $style.sortable"
                @click="sortable(head[0]) && emit('sort', head[0])"
              >
                <span
                  >{{ head[1]
                  }}<ArrowUpDown
                    v-if="sortable(head[0])"
                    :size="10"
                    :stroke-width="1.8"
                    :class="sortKey === head[0] && $style.sortActive"
                /></span>
              </th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="row in rows"
              :key="row.id"
              @click="emit('view', row)"
            >
              <td>
                <button
                  type="button"
                  :class="$style.code"
                  @click.stop="emit('view', row)"
                >
                  {{ row.id }}
                </button>
              </td>
              <td>
                <strong :class="$style.name"
                  >{{ row.name
                  }}<TriangleAlert
                    v-if="row.warning"
                    :size="12"
                    :stroke-width="1.8" /></strong
                ><span
                  v-if="row.project !== '-'"
                  :class="$style.subline"
                  >{{ row.project }}</span
                >
              </td>
              <td>
                <span
                  :class="$style.type"
                  :data-type="row.type"
                  >{{ row.type }}</span
                >
              </td>
              <td>{{ row.supplier }}</td>
              <td :class="$style.amount">¥{{ row.amount.toLocaleString('en-US') }}</td>
              <td>
                <div :class="$style.execution">
                  <div>
                    <progress
                      :value="pct(row)"
                      max="100"
                    /><strong>{{ pct(row) }}%</strong>
                  </div>
                  <span>¥{{ row.executedAmount.toLocaleString('en-US') }} / {{ row.orderCount }}单</span>
                </div>
              </td>
              <td>{{ row.retentionRate === null ? '—' : `${row.retentionRate}%` }}</td>
              <td :class="row.status === '已逾期' || row.status === '待审批' ? $style.dueWarning : ''">
                {{ row.dueDate }}
              </td>
              <td>
                <span
                  :class="$style.status"
                  :data-status="row.status"
                  >{{ row.status }}</span
                >
              </td>
              <td>
                <UiButton
                  variant="ghost"
                  size="icon-sm"
                  title="查看合同"
                  :aria-label="`查看${row.id}`"
                  @click.stop="emit('view', row)"
                  ><Eye
                    :size="14"
                    :stroke-width="1.8"
                /></UiButton>
              </td>
            </tr>
            <tr v-if="!rows.length">
              <td
                colspan="10"
                :class="$style.empty"
              >
                暂无匹配的采购合同
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </section>
</template>
<style module>
@reference "../../../../styles/index.css";
.toolbar {
  @apply flex min-h-[58px] items-end border-b;
  border-color: var(--ui-border);
}
.tabs {
  @apply gap-0;
}
.tab {
  @apply min-h-[48px] rounded-none px-4 py-2.5 text-[13px] font-normal;
  background: transparent !important;
}
.tab[data-state='active'] {
  @apply font-semibold;
  color: #0052cc;
}
.tab[data-state='active']::after {
  @apply absolute bottom-0 left-0 right-0 h-0.5;
  content: '';
  background: #0052cc;
}
.controls {
  @apply ml-auto flex items-center gap-2 pb-2;
}
.search {
  @apply flex h-8 w-[240px] items-center gap-1.5 rounded px-3;
  color: #6b778c;
  background: #f4f5f7;
}
.search :global(input) {
  @apply h-auto border-0 bg-transparent p-0 shadow-none;
}
.panel {
  @apply overflow-hidden rounded-b-lg border border-t-0 bg-white;
  border-color: var(--ui-border);
}
.scroller {
  @apply overflow-x-auto;
  scrollbar-width: thin;
}
.table {
  @apply w-full min-w-[1250px] border-collapse;
}
.table th {
  @apply border-b px-5 py-3 text-left text-[11px] font-semibold;
  color: #5e6c84;
  background: #f4f5f7;
  border-color: var(--ui-border);
}
.table th span {
  @apply inline-flex items-center;
}
.sortable {
  @apply cursor-pointer select-none;
}
.sortActive {
  color: #0052cc;
}
.table td {
  @apply whitespace-nowrap border-b px-5 py-3 text-[12px];
  color: #172b4d;
  border-color: #f4f5f7;
}
.table tbody tr {
  @apply cursor-pointer transition-colors;
}
.table tbody tr:hover td {
  background: #fafbfc;
}
.table th:nth-child(n + 3),
.table td:nth-child(n + 3) {
  @apply text-center;
}
.table th:nth-child(5),
.table td:nth-child(5) {
  @apply text-right;
}
.code {
  @apply text-[13px] font-medium hover:underline;
  color: #0052cc;
}
.name {
  @apply flex min-w-[250px] items-center gap-1 font-normal;
}
.name svg {
  color: #ff8b00;
}
.subline {
  @apply mt-0.5 block text-[10px];
  color: #97a0af;
}
.type,
.status {
  @apply rounded px-2 py-1 text-[10px] font-semibold;
}
.type {
  color: #0052cc;
  background: #deebff;
}
.type[data-type='框架合同'] {
  color: #6554c0;
  background: #eae6ff;
}
.type[data-type='服务合同'] {
  color: #00875a;
  background: #e3fcef;
}
.amount {
  @apply text-[13px] font-semibold;
}
.execution {
  @apply min-w-[130px];
}
.execution > div {
  @apply flex items-center gap-2;
}
.execution progress {
  @apply h-1.5 w-24 overflow-hidden rounded;
  border: 0;
  background: #ebecf0;
}
.execution progress::-webkit-progress-bar {
  background: #ebecf0;
}
.execution progress::-webkit-progress-value {
  background: #0052cc;
}
.execution strong {
  @apply text-[11px];
  color: #6b778c;
}
.execution span {
  @apply mt-1 block text-left text-[10px];
  color: #97a0af;
}
.status {
  color: #0052cc;
  background: #deebff;
}
.status[data-status='已完成'] {
  color: #00875a;
  background: #e3fcef;
}
.status[data-status='待审批'] {
  color: #ff8b00;
  background: #fff7e6;
}
.status[data-status='已逾期'] {
  color: #de350b;
  background: #ffebe6;
}
.dueWarning {
  color: #ff5630 !important;
}
.empty {
  @apply py-12;
  color: #97a0af !important;
  text-align: center !important;
}
</style>
