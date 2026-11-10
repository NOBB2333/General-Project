<script setup lang="ts">
import type { DispatchMetric } from '../../model/to-dispatch.types';

defineProps<{
  metrics: readonly DispatchMetric[];
  values: Readonly<Record<DispatchMetric['id'], number>>;
}>();
</script>

<template>
  <section
    :class="$style.grid"
    aria-label="待分派工单概览"
  >
    <article
      v-for="metric in metrics"
      :key="metric.id"
      :class="$style.card"
      :data-tone="metric.tone"
    >
      <span>{{ metric.label }}</span>
      <strong>{{ values[metric.id] }}</strong>
    </article>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.grid {
  @apply mb-5 mt-4 grid grid-cols-4 gap-3;
}

.card {
  @apply min-h-[88px] rounded-lg border bg-white p-4;
  border-color: var(--ui-border);
}

.card span {
  @apply block text-[12px];
  color: var(--ui-text-subtle);
}

.card strong {
  @apply mt-1 block text-[24px] font-bold leading-8;
  color: var(--app-primary);
}

.card[data-tone='warning'] strong {
  color: var(--ui-warning);
}

.card[data-tone='danger'] strong {
  color: var(--ui-danger);
}

@media (max-width: 760px) {
  .grid {
    @apply grid-cols-2;
  }
}
</style>
