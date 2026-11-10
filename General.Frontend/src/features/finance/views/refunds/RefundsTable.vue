<script setup lang="ts">
import { ArrowLeftRight } from 'lucide-vue-next';
import { UiButton } from '@/components/ui';
import type { RefundKind, RefundRow } from '../../model/refunds.types';

defineProps<{
  rows: readonly RefundRow[];
  activeKind: RefundKind;
}>();

const emit = defineEmits<{
  view: [row: RefundRow];
}>();

function currency(row: RefundRow) {
  const symbols = { CNY: '¥', USD: '$', EUR: '€' } as const;
  return `${symbols[row.currency]}${row.amount.toLocaleString('zh-CN')}`;
}
</script>

<template>
  <div :class="$style.tableScroller">
    <span :class="$style.scrollHint">
      <ArrowLeftRight
        :size="13"
        :stroke-width="1.8"
      />
      左右滑动
    </span>
    <table :class="$style.table">
      <thead>
        <tr>
          <th>申请单号</th>
          <th>{{ activeKind === 'customer' ? '关联合同' : '关联采购单' }}</th>
          <th>{{ activeKind === 'customer' ? '客户' : '供应商' }}</th>
          <th>币种</th>
          <th>实退金额</th>
          <th>退款方式</th>
          <th>单据状态</th>
          <th>财务审批</th>
          <th>付款状态</th>
          <th>核销状态</th>
          <th>申请日期</th>
          <th>操作</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="row in rows"
          :key="row.id"
        >
          <td>
            <button
              type="button"
              :class="$style.code"
              @click="emit('view', row)"
            >
              {{ row.code }}
            </button>
          </td>
          <td>
            <strong :class="$style.contract">{{ row.contract }}</strong>
          </td>
          <td>
            <strong :class="$style.party">{{ row.party }}</strong>
            <small>{{ row.description }}</small>
          </td>
          <td>
            <span :class="$style.currency">{{ row.currency }}</span>
          </td>
          <td>
            <strong :class="$style.amount">{{ currency(row) }}</strong>
          </td>
          <td>{{ row.method }}</td>
          <td>
            <span
              :class="$style.badge"
              :data-status="row.status"
              >{{ row.status }}</span
            >
          </td>
          <td>
            <span
              :class="$style.badge"
              :data-status="row.approvalStatus"
              >{{ row.approvalStatus }}</span
            >
          </td>
          <td>
            <span
              :class="$style.badge"
              :data-status="row.paymentStatus"
              >{{ row.paymentStatus }}</span
            >
          </td>
          <td>
            <span
              :class="$style.badge"
              :data-status="row.writeoffStatus"
              >{{ row.writeoffStatus }}</span
            >
          </td>
          <td>{{ row.requestedAt }}</td>
          <td>
            <UiButton
              variant="link"
              size="xs"
              @click="emit('view', row)"
              >查看</UiButton
            >
          </td>
        </tr>
        <tr v-if="!rows.length">
          <td
            :class="$style.empty"
            colspan="12"
          >
            当前筛选下没有退款申请
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<style module>
@reference "../../../../styles/index.css";

.tableScroller {
  @apply relative overflow-x-auto;
}

.scrollHint {
  @apply absolute right-3 top-2 z-10 inline-flex items-center gap-1 rounded-full border px-2 py-1 text-[9px] font-semibold;
  color: var(--app-primary);
  border-color: var(--app-primary-light);
  background: var(--app-primary-soft);
}

.table {
  @apply w-full min-w-[1600px] border-collapse text-left text-[10px];
  color: var(--ui-text-muted);
}

.table th {
  @apply h-10 border-b px-4 font-semibold;
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}

.table td {
  @apply h-[61px] border-b px-4 align-middle;
  border-color: var(--ui-border-subtle);
}

.table tbody tr:hover {
  background: var(--ui-surface-hover);
}

.table th:nth-child(1) {
  width: 170px;
}
.table th:nth-child(2) {
  width: 150px;
}
.table th:nth-child(3) {
  width: 260px;
}
.table th:nth-child(4) {
  width: 75px;
}
.table th:nth-child(5) {
  width: 130px;
}
.table th:nth-child(6) {
  width: 120px;
}
.table th:nth-child(7),
.table th:nth-child(8),
.table th:nth-child(9),
.table th:nth-child(10) {
  width: 105px;
}
.table th:nth-child(11) {
  width: 115px;
}
.table th:nth-child(12) {
  width: 75px;
}

.code,
.contract {
  @apply font-semibold;
  color: var(--app-primary);
}

.party,
.amount {
  @apply block font-semibold;
  color: var(--ui-text);
}

.table small {
  @apply mt-1 block text-[9px];
  color: var(--ui-text-subtle);
}

.currency {
  @apply inline-flex rounded px-2 py-0.5 text-[9px] font-semibold;
  color: var(--ui-text-muted);
  background: var(--ui-surface-hover);
}

.badge {
  @apply inline-flex whitespace-nowrap rounded-full px-2 py-0.5 text-[9px] font-semibold;
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
}

.badge[data-status='草稿'] {
  color: var(--ui-text-subtle);
  background: var(--ui-border-subtle);
}

.badge[data-status='待审批'],
.badge[data-status='待付款'],
.badge[data-status='待核销'] {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}

.badge[data-status='审批中'] {
  color: #6554c0;
  background: #eae6ff;
}

.badge[data-status='已批准'],
.badge[data-status='已付款'] {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.badge[data-status='已通过'],
.badge[data-status='已核销'] {
  color: var(--ui-success);
  background: #e3fcef;
}

.badge[data-status='已驳回'] {
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}

.empty {
  @apply h-32 text-center;
  color: var(--ui-text-subtle);
}
</style>
