<script setup lang="ts">
import { BadgeDollarSign, Box, Gift, Ticket } from 'lucide-vue-next';
</script>
<template>
  <section :class="$style.grid">
    <article data-tone="primary">
      <BadgeDollarSign
        :size="18"
        :stroke-width="1.8"
      /><span>累计发放价值</span><strong>¥4,579</strong><small>6 次发放</small>
    </article>
    <article data-tone="success">
      <Gift
        :size="18"
        :stroke-width="1.8"
      /><span>累计发放数量</span><strong>53</strong><small>兑换码+实物</small>
    </article>
    <article data-tone="purple">
      <Ticket
        :size="18"
        :stroke-width="1.8"
      /><span>兑换码剩余</span><strong>352</strong><small>6 个活跃码池</small>
    </article>
    <article data-tone="warning">
      <Box
        :size="18"
        :stroke-width="1.8"
      /><span>实物库存剩余</span><strong>360</strong><small>6 种在库</small>
    </article>
  </section>
</template>
<style module>
@reference "../../../../styles/index.css";
.grid {
  @apply mt-4 grid grid-cols-4 gap-4;
}
.grid article {
  @apply relative flex h-[112px] flex-col rounded border bg-white p-4;
  border-color: var(--ui-border);
}
.grid svg {
  @apply absolute right-4 top-4 rounded p-1.5;
  box-sizing: content-box;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.grid span {
  @apply text-[10px];
  color: var(--ui-text-muted);
}
.grid strong {
  @apply mt-4 text-[21px];
  color: var(--app-primary);
}
.grid small {
  @apply mt-1 text-[9px];
  color: var(--ui-text-subtle);
}
.grid article[data-tone='success'] svg,
.grid article[data-tone='success'] strong {
  color: var(--ui-success);
}
.grid article[data-tone='purple'] svg,
.grid article[data-tone='purple'] strong {
  color: #6554c0;
}
.grid article[data-tone='warning'] svg,
.grid article[data-tone='warning'] strong {
  color: var(--ui-warning);
}
</style>
