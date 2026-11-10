<script setup lang="ts">
import { CircleCheck, Clock3, FileText, TrendingUp } from 'lucide-vue-next';

defineProps<{
  totalAmount: number;
  wonAmount: number;
  followUpCount: number;
  winRate: number;
  total: number;
  wonCount: number;
}>();

const formatWan = (value: number) => `¥${(value / 10000).toFixed(1)}万`;
</script>

<template>
  <section
    :class="$style.overview"
    aria-label="报价概览"
  >
    <article>
      <span><FileText :size="14" />报价总额</span>
      <strong>{{ formatWan(totalAmount) }}</strong>
      <small>{{ total }} 份报价单</small>
    </article>
    <article :class="$style.success">
      <span><CircleCheck :size="14" />已成交金额</span>
      <strong>{{ formatWan(wonAmount) }}</strong>
      <small>{{ wonCount }} 份已接受</small>
    </article>
    <article :class="$style.warning">
      <span><Clock3 :size="14" />待跟进</span>
      <strong>{{ followUpCount }}</strong>
      <small>草稿/待审核/已发送</small>
    </article>
    <article :class="$style.primary">
      <span><TrendingUp :size="14" />成交率</span>
      <strong>{{ winRate }}%</strong>
      <small>已接受 / (已接受+已拒绝+已过期)</small>
    </article>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.overview {
  @apply mt-8 grid grid-cols-4 gap-4;
}
.overview article {
  @apply flex min-h-[104px] flex-col justify-center rounded-lg border bg-white p-4;
  border-color: var(--ui-border);
}
.overview span {
  @apply flex items-center gap-2 text-[12px];
  color: var(--ui-text-subtle);
}
.overview strong {
  @apply mt-1 text-[23px] font-bold;
  color: var(--ui-text);
}
.overview small {
  @apply mt-1 text-[10px];
  color: #97a0af;
}
.success span,
.success strong {
  color: var(--ui-success);
}
.warning span,
.warning strong {
  color: var(--ui-warning);
}
.primary span,
.primary strong {
  color: var(--app-primary);
}
@media (max-width: 900px) {
  .overview {
    @apply grid-cols-2;
  }
}
</style>
