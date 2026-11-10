<script setup lang="ts">
import { ChevronRight, Layers3 } from 'lucide-vue-next';
import { ref } from 'vue';
import { UiButton } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import type { PaymentFlowRow, PaymentFlowScope } from '../../model/payment-flow.types';
import PaymentFlowToolbar from './PaymentFlowToolbar.vue';

defineProps<{
  rows: readonly PaymentFlowRow[];
  allRows: readonly PaymentFlowRow[];
  activeScope: PaymentFlowScope;
  supplier: string;
  query: string;
  supplierOptions: readonly UiSelectOption[];
}>();

const emit = defineEmits<{
  'update:activeScope': [value: PaymentFlowScope];
  'update:supplier': [value: string];
  'update:query': [value: string];
  allocate: [row: PaymentFlowRow];
  export: [];
  create: [];
}>();

const expandedId = ref<string | null>(null);

function currency(value: number) {
  return `¥${value.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
}

function statusLabel(row: PaymentFlowRow) {
  return row.status === 'allocated' ? '已分配' : '未完成分配';
}
</script>

<template>
  <section :class="$style.panel">
    <PaymentFlowToolbar
      :rows="allRows"
      :active-scope="activeScope"
      :supplier="supplier"
      :query="query"
      :supplier-options="supplierOptions"
      @update:active-scope="emit('update:activeScope', $event)"
      @update:supplier="emit('update:supplier', $event)"
      @update:query="emit('update:query', $event)"
      @export="emit('export')"
      @create="emit('create')"
    />

    <div :class="$style.scroller">
      <table :class="$style.table">
        <thead>
          <tr>
            <th aria-label="展开详情" />
            <th>流水号</th>
            <th>供应商</th>
            <th>付款日期</th>
            <th>方式</th>
            <th>付款账户</th>
            <th>付款金额</th>
            <th>已分配 / 未分配</th>
            <th>状态</th>
            <th>经办人</th>
            <th>操作</th>
          </tr>
        </thead>
        <tbody>
          <template
            v-for="row in rows"
            :key="row.id"
          >
            <tr>
              <td>
                <button
                  type="button"
                  :class="[$style.expandButton, expandedId === row.id && $style.expanded]"
                  :aria-expanded="expandedId === row.id"
                  :aria-controls="`${row.id}-details`"
                  :aria-label="expandedId === row.id ? `收起${row.code}` : `展开${row.code}`"
                  @click="expandedId = expandedId === row.id ? null : row.id"
                >
                  <ChevronRight
                    :size="14"
                    :stroke-width="1.8"
                  />
                </button>
              </td>
              <td>
                <strong :class="$style.code">{{ row.code }}</strong>
                <small
                  v-if="row.imported"
                  :class="$style.imported"
                  >导入</small
                >
              </td>
              <td :class="$style.supplier">{{ row.supplier }}</td>
              <td :class="$style.muted">{{ row.date }}</td>
              <td>{{ row.method }}</td>
              <td :class="$style.account">{{ row.account }}</td>
              <td :class="$style.amount">{{ currency(row.amount) }}</td>
              <td>
                <span :class="$style.allocation">
                  <b>{{ currency(row.allocatedAmount) }}</b>
                  <i>/</i>
                  <strong :class="row.amount > row.allocatedAmount && $style.outstanding">
                    {{ currency(row.amount - row.allocatedAmount) }}
                  </strong>
                </span>
              </td>
              <td>
                <span :class="[$style.status, row.status === 'allocated' ? $style.statusDone : $style.statusPending]">
                  {{ statusLabel(row) }}
                </span>
                <small
                  v-if="row.assignmentMode"
                  :class="[$style.mode, row.assignmentMode === '手动分配' && $style.manualMode]"
                >
                  {{ row.assignmentMode }}
                </small>
              </td>
              <td>{{ row.operator }}</td>
              <td>
                <UiButton
                  variant="secondary"
                  size="xs"
                  @click="emit('allocate', row)"
                >
                  <template #leading><Layers3 :size="13" /></template>
                  {{ row.status === 'allocated' ? '重新分配' : '手动分配' }}
                </UiButton>
              </td>
            </tr>
            <tr
              v-if="expandedId === row.id"
              :id="`${row.id}-details`"
              :class="$style.detailRow"
            >
              <td colspan="11">
                <span
                  ><small>流水来源</small><strong>{{ row.imported ? '银行流水导入' : '付款申请登记' }}</strong></span
                >
                <span
                  ><small>分配进度</small
                  ><strong>{{ Math.round((row.allocatedAmount / row.amount) * 100) }}%</strong></span
                >
                <span
                  ><small>分配方式</small><strong>{{ row.assignmentMode ?? '等待手动分配' }}</strong></span
                >
              </td>
            </tr>
          </template>
          <tr v-if="!rows.length">
            <td
              :class="$style.empty"
              colspan="11"
            >
              当前筛选条件下没有付款流水
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <footer :class="$style.footer">
      共 {{ rows.length }} 笔流水 · 金额合计 {{ currency(rows.reduce((sum, row) => sum + row.amount, 0)) }}
    </footer>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.panel {
  @apply mt-4 overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}

.scroller {
  @apply overflow-x-auto;
  scrollbar-width: thin;
}

.table {
  @apply w-full min-w-[1360px] border-collapse text-left text-[11px];
  color: var(--ui-text-muted);
}

.table th {
  @apply h-[39px] border-b px-3 font-semibold;
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}

.table th:first-child,
.table td:first-child {
  @apply w-10 px-2 text-center;
}

.table td {
  @apply h-[70px] border-b px-3 align-middle;
  border-color: var(--ui-border-subtle);
}

.table tbody tr:not(.detailRow):hover {
  background: var(--ui-surface-hover);
}

.expandButton {
  @apply inline-flex h-6 w-6 items-center justify-center rounded transition-transform;
  color: var(--ui-text-subtle);
}

.expanded {
  transform: rotate(90deg);
}

.code,
.amount {
  @apply font-semibold;
  color: var(--app-primary);
}

.amount {
  color: var(--ui-text);
}

.imported {
  @apply ml-1 rounded px-1 py-0.5 text-[9px];
  color: #6554c0;
  background: #eae6ff;
}

.supplier {
  @apply font-medium;
  color: var(--ui-text);
}

.muted {
  color: var(--ui-text-subtle);
}

.account {
  @apply max-w-[155px] whitespace-normal leading-4;
}

.allocation {
  @apply flex items-center gap-1 whitespace-nowrap;
}

.allocation b {
  @apply font-medium;
  color: var(--ui-success);
}

.allocation i {
  @apply not-italic;
  color: #97a0af;
}

.allocation strong {
  @apply font-semibold;
  color: #97a0af;
}

.outstanding {
  color: var(--ui-danger) !important;
}

.status,
.mode {
  @apply inline-flex rounded px-2 py-1 text-[10px] font-semibold;
}

.statusDone {
  color: var(--ui-success);
  background: var(--ui-success-soft);
}

.statusPending {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}

.mode {
  @apply mt-1 py-0.5;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.manualMode {
  color: #6554c0;
  background: #eae6ff;
}

.detailRow td {
  @apply h-auto bg-[#fafbfc] px-14 py-3;
}

.detailRow td > span {
  @apply mr-12 inline-flex flex-col gap-1;
}

.detailRow small {
  color: var(--ui-text-subtle);
}

.detailRow strong {
  @apply font-medium;
  color: var(--ui-text);
}

.empty {
  @apply py-16 text-center;
  color: var(--ui-text-subtle);
}

.footer {
  @apply flex h-[58px] items-center px-5 text-[11px];
  color: var(--ui-text-subtle);
}
</style>
