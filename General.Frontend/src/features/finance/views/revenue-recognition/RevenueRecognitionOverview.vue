<script setup lang="ts">
import { CircleDollarSign, Clock3, FileCheck2, TrendingUp } from 'lucide-vue-next';
import type { RevenueMetric } from '../../model/revenue-recognition.types';

defineProps<{ metrics: readonly RevenueMetric[] }>();
const icons = { month: TrendingUp, year: CircleDollarSign, pending: Clock3, uncollected: FileCheck2 } as const;
</script>

<template>
  <section
    :class="$style.metrics"
    aria-label="收入确认概览"
  >
    <article
      v-for="metric in metrics"
      :key="metric.id"
      :class="[$style.metric, $style[`metric_${metric.tone}`]]"
    >
      <span :class="$style.label"
        ><component
          :is="icons[metric.id]"
          :size="16"
          :stroke-width="1.8"
        />{{ metric.label }}<small v-if="metric.id === 'month'">2026-08</small></span
      >
      <strong>{{ metric.value }}</strong>
      <div
        v-if="metric.id === 'month'"
        :class="$style.miniChart"
        aria-hidden="true"
      >
        <i /><i /><i /><i /><i /><i />
      </div>
      <small v-else>{{ metric.hint }}</small>
    </article>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";
.metrics {
  @apply mt-4 grid grid-cols-4 gap-4;
}
.metric {
  @apply flex min-h-[118px] flex-col justify-center rounded border bg-white px-4;
  border-color: var(--ui-border);
}
.label {
  @apply flex items-center gap-2 text-[11px];
  color: var(--ui-text-subtle);
}
.label small {
  @apply ml-auto;
}
.metric > strong {
  @apply mt-2 text-[23px] font-semibold leading-none;
  color: var(--ui-text);
}
.metric > small {
  @apply mt-2 text-[10px];
  color: var(--ui-text-subtle);
}
.metric_primary .label,
.metric_primary strong {
  color: var(--app-primary);
}
.metric_success .label,
.metric_success strong {
  color: var(--ui-success);
}
.metric_warning .label,
.metric_warning strong {
  color: var(--ui-warning);
}
.miniChart {
  @apply mt-1 flex h-8 items-end gap-1;
}
.miniChart i {
  @apply flex-1 border-b-2;
  border-color: var(--app-primary);
  background: var(--app-primary-soft);
}
.miniChart i:nth-child(1) {
  height: 24px;
}
.miniChart i:nth-child(2) {
  height: 5px;
}
.miniChart i:nth-child(3) {
  height: 4px;
}
.miniChart i:nth-child(4) {
  height: 3px;
}
.miniChart i:nth-child(5) {
  height: 2px;
}
.miniChart i:nth-child(6) {
  height: 2px;
}
</style>
