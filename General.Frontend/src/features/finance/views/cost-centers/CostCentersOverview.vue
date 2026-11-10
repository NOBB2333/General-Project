<script setup lang="ts">
import type { CostCenterMetric } from '../../model/cost-centers.types';

defineProps<{
  metrics: readonly CostCenterMetric[];
}>();
</script>

<template>
  <section
    :class="$style.metrics"
    aria-label="成本概览"
  >
    <article
      v-for="metric in metrics"
      :key="metric.id"
      :class="[$style.metric, $style[`metric_${metric.tone}`]]"
    >
      <span>{{ metric.label }}</span>
      <strong>{{ metric.value }}</strong>
    </article>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.metrics {
  @apply mt-4 grid grid-cols-6 gap-3 px-4;
}

.metric {
  @apply flex min-h-[66px] flex-col justify-center rounded border bg-white px-3;
  border-color: var(--ui-border);
}

.metric span {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}

.metric strong {
  @apply mt-2 text-[19px] font-semibold leading-none;
  color: var(--ui-text);
}

.metric_success strong {
  color: var(--ui-success);
}

.metric_warning strong {
  color: var(--ui-warning);
}

.metric_danger strong {
  color: #de350b;
}

.metric_primary strong {
  color: var(--app-primary);
}
</style>
