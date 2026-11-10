<script setup lang="ts">
import { BadgeDollarSign, Check, Layers3, Link2 } from 'lucide-vue-next';

defineProps<{
  total: number;
  allocated: number;
  unallocated: number;
  incomplete: number;
  count: number;
}>();

function currency(value: number) {
  return `¥${value.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
}
</script>

<template>
  <section
    :class="$style.grid"
    aria-label="付款流水概览"
  >
    <article :class="$style.card">
      <p><BadgeDollarSign :size="16" />付款流水总额</p>
      <strong>{{ currency(total) }}</strong>
      <small>{{ count }} 笔流水</small>
    </article>
    <article :class="[$style.card, $style.allocated]">
      <p><Check :size="16" />已分配金额</p>
      <strong>{{ currency(allocated) }}</strong>
      <small>已落到订单</small>
    </article>
    <article :class="[$style.card, $style.unallocated]">
      <p><Link2 :size="16" />未分配金额</p>
      <strong>{{ currency(unallocated) }}</strong>
      <small>待手动分配到订单</small>
    </article>
    <article :class="[$style.card, $style.incomplete]">
      <p><Layers3 :size="16" />未完成分配</p>
      <strong>{{ incomplete }}</strong>
      <small>未分配 + 部分已分配</small>
    </article>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.grid {
  @apply mt-4 grid grid-cols-4 gap-3;
}

.card {
  @apply flex h-[100px] flex-col justify-center rounded border bg-white px-4;
  border-color: var(--ui-border);
}

.card p {
  @apply m-0 flex items-center gap-2 text-[12px];
  color: var(--ui-text-subtle);
}

.card strong {
  @apply mt-2 text-[22px] font-semibold leading-none;
  color: var(--ui-text);
  letter-spacing: 0;
}

.card small {
  @apply mt-2 text-[10px];
  color: #97a0af;
}

.allocated strong,
.allocated p svg {
  color: var(--ui-success);
}

.unallocated {
  border-color: #ffbdad;
}

.unallocated strong,
.unallocated p svg,
.incomplete strong,
.incomplete p svg {
  color: var(--ui-warning);
}
</style>
