<script setup lang="ts">
import { MoreHorizontal, Star } from 'lucide-vue-next';
import type { SalesQuoteRow } from '@/features/crm/model/sales-quotes.types';

defineProps<{ row: SalesQuoteRow }>();
const currency = new Intl.NumberFormat('zh-CN');
</script>

<template>
  <tr :class="$style.row">
    <td>
      <strong :class="$style.code">{{ row.code }}</strong>
    </td>
    <td>
      <strong :class="$style.customer">{{ row.customer }}</strong>
    </td>
    <td>{{ row.project }}</td>
    <td>
      <span :class="$style.owner"
        ><i>{{ row.ownerInitial }}</i
        >{{ row.owner }}</span
      >
    </td>
    <td :class="$style.center">{{ row.productCount }}</td>
    <td>
      <strong :class="$style.amount">¥{{ currency.format(row.amount) }}</strong>
    </td>
    <td :class="$style.discount">{{ row.discount === null ? '-' : `${row.discount}%` }}</td>
    <td>
      <span :class="[$style.status, $style[row.status]]">{{ row.status }}</span>
    </td>
    <td :class="$style.validUntil">{{ row.validUntil }}</td>
    <td>{{ row.date }}</td>
    <td>
      <Star
        v-if="row.followed"
        :size="14"
        fill="#FFAB00"
        :class="$style.star"
      />
      <button
        v-else
        type="button"
        :class="$style.more"
        :aria-label="`更多操作：${row.code}`"
        title="更多操作"
      >
        <MoreHorizontal :size="16" />
      </button>
    </td>
  </tr>
</template>

<style module>
@reference "../../../../styles/index.css";

.row {
  @apply h-[45px] border-b transition-colors;
  border-color: var(--ui-border-subtle);
}
.row:hover {
  background: var(--ui-surface-hover);
}
.row td {
  @apply whitespace-nowrap px-4 align-middle;
}
.code {
  @apply font-semibold;
  color: var(--app-primary);
}
.customer,
.amount {
  color: var(--ui-text);
}
.owner {
  @apply inline-flex items-center gap-1.5;
}
.owner i {
  @apply flex h-5 w-5 items-center justify-center rounded-full text-[10px] not-italic text-white;
  background: var(--app-primary);
}
.center {
  @apply text-center;
}
.discount,
.validUntil {
  color: #ff5630;
}
.status {
  @apply inline-flex rounded px-2 py-0.5 text-[11px] font-semibold;
}
.已发送 {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.待审核 {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}
.已接受 {
  color: var(--ui-success);
  background: var(--ui-success-soft);
}
.已拒绝 {
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}
.草稿,
.已过期 {
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
}
.star {
  color: #ffab00;
}
.more {
  @apply flex h-7 w-7 items-center justify-center rounded opacity-0 transition-opacity;
  color: var(--ui-text-subtle);
}
.more:hover {
  background: var(--ui-surface-hover);
}
.row:hover .more,
.more:focus-visible {
  opacity: 1;
}
</style>
