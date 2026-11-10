<script setup lang="ts">
import type { WorkbenchMetric } from '../../model/my-workbench.types';

defineProps<{
  metrics: readonly WorkbenchMetric[];
}>();
</script>

<template>
  <section
    :class="$style.metricGrid"
    aria-label="工作台指标"
  >
    <article
      v-for="metric in metrics"
      :key="metric.id"
      :class="$style.metricCard"
    >
      <span :class="$style.metricLabel">{{ metric.label }}</span>
      <strong :class="[$style.metricValue, $style[metric.tone]]">{{ metric.value }}</strong>
      <small v-if="metric.hint">{{ metric.hint }}</small>
    </article>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.metricGrid {
  @apply mb-4 grid grid-cols-5 gap-3;
}

.metricCard {
  @apply min-h-[98px] rounded-lg border bg-white p-3.5;
  border-color: var(--ui-border);
}

.metricLabel {
  @apply block text-[11.5px];
  color: var(--ui-text-subtle);
}

.metricValue {
  @apply mt-1.5 block text-[22px] font-bold leading-none;
}

.metricCard small {
  @apply mt-1.5 block text-[10.5px];
  color: #97a0af;
}

.primary {
  color: #0052cc;
}

.cyan {
  color: #00a3bf;
}

.success {
  color: #00875a;
}

.purple {
  color: #6554c0;
}

.warning {
  color: #ffab00;
}

@media (max-width: 900px) {
  .metricGrid {
    @apply grid-cols-2;
  }
}
</style>
