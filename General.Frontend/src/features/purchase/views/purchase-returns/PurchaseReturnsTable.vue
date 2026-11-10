<script setup lang="ts">
import { ChevronLeft, ChevronRight, Paperclip } from 'lucide-vue-next';
import { UiButton } from '@/components/ui';
import type { PurchaseReturnRecord, PurchaseReturnStatus } from '../../model/purchase-returns.types';

defineProps<{ rows: readonly PurchaseReturnRecord[] }>();
const emit = defineEmits<{ open: [row: PurchaseReturnRecord] }>();

function statusTone(status: PurchaseReturnStatus) {
  return {
    草稿: 'draft',
    待审核: 'pending',
    已审核: 'approved',
    退货中: 'returning',
    已退货: 'returned',
    已退款: 'refunded',
  }[status];
}
</script>

<template>
  <div :class="$style.tableWrap">
    <table :class="$style.table">
      <thead>
        <tr>
          <th>退货单号</th>
          <th>关联采购单</th>
          <th>供应商</th>
          <th>退货原因</th>
          <th :class="$style.center">退货数量</th>
          <th :class="$style.amount">退货金额</th>
          <th :class="$style.center">退款</th>
          <th :class="$style.center">状态</th>
          <th>日期</th>
          <th :class="$style.center">附件</th>
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
              @click="emit('open', row)"
            >
              {{ row.id }}
            </button>
          </td>
          <td>
            <button
              v-if="row.poId !== '-'"
              type="button"
              :class="$style.order"
              @click="emit('open', row)"
            >
              {{ row.poId }}
            </button>
            <span
              v-else
              :class="$style.muted"
              >普通退货</span
            >
          </td>
          <td :class="$style.supplier">{{ row.supplier }}</td>
          <td>
            <span :class="$style.reason">{{ row.reason }}</span>
          </td>
          <td :class="$style.center">{{ row.items }}</td>
          <td :class="[$style.amount, $style.returnAmount]">-¥{{ row.amount.toLocaleString('zh-CN') }}</td>
          <td :class="$style.center">
            <span
              v-if="row.refunded"
              :class="$style.refundBadge"
              >已退款</span
            >
            <span
              v-else
              :class="$style.muted"
              >-</span
            >
          </td>
          <td :class="$style.center">
            <span :class="[$style.status, $style[statusTone(row.status)]]">{{ row.status }}</span>
          </td>
          <td :class="$style.date">{{ row.date }}</td>
          <td :class="$style.center">
            <span
              v-if="row.attachments"
              :class="$style.attachments"
            >
              <Paperclip
                :size="12"
                :stroke-width="1.8"
              />{{ row.attachments }}
            </span>
            <span
              v-else
              :class="$style.muted"
              >-</span
            >
          </td>
        </tr>
        <tr v-if="!rows.length">
          <td
            colspan="10"
            :class="$style.empty"
          >
            当前筛选下没有采购退货
          </td>
        </tr>
      </tbody>
    </table>
  </div>
  <footer :class="$style.pagination">
    <span>共 {{ rows.length }} 条</span>
    <div>
      <UiButton
        variant="ghost"
        size="icon-sm"
        aria-label="上一页"
        ><ChevronLeft
          :size="14"
          :stroke-width="1.8"
      /></UiButton>
      <button
        type="button"
        aria-current="page"
      >
        1
      </button>
      <UiButton
        variant="ghost"
        size="icon-sm"
        aria-label="下一页"
        ><ChevronRight
          :size="14"
          :stroke-width="1.8"
      /></UiButton>
    </div>
  </footer>
</template>

<style module>
@reference "../../../../styles/index.css";

.tableWrap {
  @apply overflow-auto;
}
.table {
  @apply w-full min-w-[1200px] border-collapse text-left text-[13px];
  color: var(--ui-text);
}
.table thead {
  background: var(--ui-surface-hover);
}
.table th {
  @apply whitespace-nowrap px-5 py-3 text-[11px] font-semibold;
  color: #5e6c84;
}
.table td {
  @apply border-t px-5 py-3;
  border-color: var(--ui-border-subtle);
}
.table tbody tr {
  @apply transition-colors;
}
.table tbody tr:hover {
  background: var(--ui-surface-subtle);
}
.code,
.order {
  @apply whitespace-nowrap font-medium hover:underline;
  color: var(--app-primary);
}
.order {
  color: #0065ff;
}
.supplier {
  @apply min-w-[150px];
}
.reason {
  @apply inline-flex whitespace-nowrap rounded px-2 py-0.5 text-[11px] font-medium;
  color: #ff5630;
  background: #ffebe6;
}
.center {
  @apply text-center;
}
.amount {
  @apply text-right tabular-nums;
}
.returnAmount {
  @apply whitespace-nowrap font-medium;
  color: #ff5630;
}
.muted {
  @apply text-[11px];
  color: #97a0af;
}
.refundBadge {
  @apply rounded px-2 py-0.5 text-[10px] font-semibold;
  color: #0065ff;
  background: #e6fcff;
}
.status {
  @apply inline-flex whitespace-nowrap rounded px-2.5 py-1 text-[11px] font-semibold;
}
.draft {
  color: #6b778c;
  background: #ebeef0;
}
.pending {
  color: #ffab00;
  background: #fffae6;
}
.approved {
  color: #0052cc;
  background: #deebff;
}
.returning {
  color: #6554c0;
  background: #eae6ff;
}
.returned {
  color: #00875a;
  background: #e3fcef;
}
.refunded {
  color: #0065ff;
  background: #e6fcff;
}
.date {
  @apply whitespace-nowrap;
  color: var(--ui-text-subtle);
}
.attachments {
  @apply inline-flex items-center justify-center gap-1;
  color: var(--ui-text-subtle);
}
.empty {
  @apply py-20 text-center text-[12px];
  color: #97a0af;
}
.pagination {
  @apply flex items-center justify-between border-t px-5 py-3 text-[12px];
  color: var(--ui-text-subtle);
  border-color: var(--ui-border);
}
.pagination > div {
  @apply flex items-center gap-1;
}
.pagination > div > button:not(:first-child):not(:last-child) {
  @apply flex h-8 w-8 items-center justify-center rounded text-white;
  background: var(--app-primary);
}
</style>
