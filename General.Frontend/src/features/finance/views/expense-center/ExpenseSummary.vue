<script setup lang="ts">
import { Building2, CircleAlert, ReceiptText, Tag, WalletCards } from 'lucide-vue-next';
import type { ExpenseMetric } from '../../model/expense-center.types';
defineProps<{ metrics: readonly ExpenseMetric[] }>();
const icons = {
  total: WalletCards,
  pending: CircleAlert,
  reimbursed: ReceiptText,
  enterprise: Building2,
  period: Tag,
} as const;
</script>

<template>
  <section
    :class="$style.metrics"
    aria-label="费用概览"
  >
    <article
      v-for="metric in metrics"
      :key="metric.id"
      :class="[$style.metric, $style[`metric_${metric.tone}`]]"
    >
      <span
        ><component
          :is="icons[metric.id]"
          :size="16"
          :stroke-width="1.8"
        />{{ metric.label }}</span
      ><strong>{{ metric.value }}</strong
      ><small>{{ metric.hint }}</small>
    </article>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";
.metrics {
  @apply mt-4 grid grid-cols-5 gap-4;
}
.metric {
  @apply flex min-h-[100px] flex-col justify-center rounded border bg-white px-4;
  border-color: var(--ui-border);
}
.metric span {
  @apply flex items-center gap-2 text-[11px];
  color: var(--ui-text-subtle);
}
.metric strong {
  @apply mt-2 text-[21px] font-semibold leading-none;
  color: var(--ui-text);
}
.metric small {
  @apply mt-2 text-[10px];
  color: var(--ui-text-subtle);
}
.metric_primary span {
  color: var(--app-primary);
}
.metric_warning span,
.metric_warning strong {
  color: var(--ui-warning);
}
.metric_success span,
.metric_success strong {
  color: var(--ui-success);
}
.metric_purple span,
.metric_purple strong {
  color: #6554c0;
}
.metric_orange span,
.metric_orange strong {
  color: #ff7452;
}
</style>
