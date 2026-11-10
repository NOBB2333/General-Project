<script setup lang="ts">
import { Activity, CircleDollarSign, TrendingUp, Zap } from 'lucide-vue-next';

defineProps<{
  activeCount: number;
  pipelineAmount: number;
  weightedAmount: number;
  wonAmount: number;
}>();

const formatWan = (value: number) => `¥${(value / 10000).toFixed(1)}万`;
</script>

<template>
  <section
    :class="$style.overview"
    aria-label="商机概览"
  >
    <article>
      <span><Activity :size="14" />活跃商机</span>
      <strong>{{ activeCount }}</strong>
    </article>
    <article>
      <span><CircleDollarSign :size="14" />管道总金额</span>
      <strong>{{ formatWan(pipelineAmount) }}</strong>
    </article>
    <article :class="$style.primary">
      <span><TrendingUp :size="14" />加权金额</span>
      <strong>{{ formatWan(weightedAmount) }}</strong>
    </article>
    <article :class="$style.success">
      <span><Zap :size="14" />已赢单</span>
      <strong>{{ formatWan(wonAmount) }}</strong>
    </article>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.overview {
  @apply mt-4 grid grid-cols-4 gap-4;
}
.overview article {
  @apply flex min-h-[82px] flex-col justify-center rounded border bg-white p-4;
  border-color: var(--ui-border);
}
.overview span {
  @apply flex items-center gap-1.5 text-[12px];
  color: var(--ui-text-subtle);
}
.overview strong {
  @apply mt-2 text-[23px] font-bold;
  color: var(--ui-text);
}
.primary strong {
  color: var(--app-primary);
}
.success strong {
  color: var(--ui-success);
}
@media (max-width: 900px) {
  .overview {
    @apply grid-cols-2;
  }
}
</style>
