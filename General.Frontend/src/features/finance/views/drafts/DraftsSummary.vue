<script setup lang="ts">
import { BadgeAlert, Clock3, ShieldCheck, WalletCards } from 'lucide-vue-next';
import type { DraftMetric } from '../../model/drafts.types';

defineProps<{
  metrics: readonly DraftMetric[];
}>();

const metricIcons = {
  pool: WalletCards,
  expiring: Clock3,
  commercial: BadgeAlert,
  returned: ShieldCheck,
} as const;
</script>

<template>
  <section
    :class="$style.metrics"
    aria-label="汇票统计"
  >
    <article
      v-for="metric in metrics"
      :key="metric.id"
      :class="[$style.metric, $style[`metric_${metric.tone}`]]"
    >
      <span :class="$style.metricLabel">
        <component
          :is="metricIcons[metric.id]"
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

.metricLabel {
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

.metric_primary .metricLabel,
.metric_primary strong {
  color: var(--app-primary);
}

.metric_danger .metricLabel,
.metric_danger strong {
  color: var(--ui-danger);
}

.metric_warning .metricLabel,
.metric_warning strong {
  color: var(--ui-warning);
}
</style>
