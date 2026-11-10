<script setup lang="ts">
import { Shield, TrendingUp, TriangleAlert, Users } from 'lucide-vue-next';
import type { CreditMetric } from '../../model/credit-limits.types';

defineProps<{ metrics: readonly CreditMetric[] }>();
const icons = { customers: Users, limit: Shield, usage: TrendingUp, overdue: TriangleAlert } as const;
</script>

<template>
  <section
    :class="$style.metrics"
    aria-label="授信概览"
  >
    <article
      v-for="metric in metrics"
      :key="metric.id"
      :class="[$style.metric, $style[`metric_${metric.tone}`]]"
    >
      <span :class="$style.label">
        <component
          :is="icons[metric.id]"
          :size="16"
          :stroke-width="1.8"
        />
        {{ metric.label }}
      </span>
      <strong>{{ metric.value }}</strong>
      <small>{{ metric.hint }}</small>
    </article>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";
.metrics {
  @apply mt-4 grid grid-cols-4 gap-4;
}
.metric {
  @apply flex min-h-[100px] flex-col justify-center rounded border bg-white px-4;
  border-color: var(--ui-border);
}
.label {
  @apply flex items-center gap-2 text-[11px];
  color: var(--ui-text-subtle);
}
.metric strong {
  @apply mt-2 text-[23px] font-semibold leading-none;
  color: var(--ui-text);
}
.metric small {
  @apply mt-2 text-[10px];
  color: var(--ui-text-subtle);
}
.metric_primary .label {
  color: var(--app-primary);
}
.metric_warning .label {
  color: var(--ui-warning);
}
.metric_danger .label,
.metric_danger strong {
  color: var(--ui-danger);
}
</style>
