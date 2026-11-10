<script setup lang="ts">
import { Banknote, CircleDollarSign, PackageOpen, Receipt, WalletCards } from 'lucide-vue-next';
import type { FinanceReportMetric } from '../../model/finance-reports.types';
defineProps<{ metrics: readonly FinanceReportMetric[] }>();
const emit = defineEmits<{ inspect: [metric: FinanceReportMetric] }>();
</script>

<template>
  <section :class="$style.grid">
    <button
      v-for="(metric, index) in metrics"
      :key="metric.id"
      type="button"
      :class="[$style.card, $style[metric.size], $style[metric.tone]]"
      @click="emit('inspect', metric)"
    >
      <header>
        <span :class="$style.icon"
          ><CircleDollarSign
            v-if="index % 5 === 0"
            :size="16"
            :stroke-width="1.8" /><WalletCards
            v-else-if="index % 5 === 1"
            :size="16"
            :stroke-width="1.8" /><Receipt
            v-else-if="index % 5 === 2"
            :size="16"
            :stroke-width="1.8" /><Banknote
            v-else-if="index % 5 === 3"
            :size="16"
            :stroke-width="1.8" /><PackageOpen
            v-else
            :size="16"
            :stroke-width="1.8" /></span
        ><span>{{ metric.label }}</span
        ><small v-if="metric.badge">{{ metric.badge }}</small>
      </header>
      <strong>{{ metric.value }}</strong>
      <p>{{ metric.description }}</p>
    </button>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";
.grid {
  @apply mt-4 grid grid-cols-12 gap-3;
}
.card {
  @apply min-w-0 rounded border bg-white p-4 text-left transition-colors;
  border-color: var(--ui-border);
}
.card:hover {
  border-color: var(--app-primary);
}
.large {
  @apply col-span-3 h-[140px];
  border-left: 3px solid #00875a;
}
.small {
  @apply col-span-2 h-[112px];
}
.card header {
  @apply flex items-center gap-2 text-[11px] font-semibold;
  color: var(--ui-text-subtle);
}
.card header small {
  @apply ml-auto rounded-full px-2 py-1 text-[9px];
  color: #00875a;
  background: #e3fcef;
}
.icon {
  @apply flex h-8 w-8 items-center justify-center rounded;
  color: #00875a;
  background: #e3fcef;
}
.card strong {
  @apply mt-4 block text-[24px];
  color: var(--ui-text);
}
.small strong {
  @apply mt-3 text-[19px];
}
.card p {
  @apply m-0 mt-2 text-[10px];
  color: #97a0af;
}
.orange {
  border-left-color: #ff8b00;
}
.orange .icon {
  color: #ff8b00;
  background: #fff7d6;
}
.cyan .icon {
  color: #00a3bf;
  background: #e6fcff;
}
.purple .icon {
  color: #6554c0;
  background: #eae6ff;
}
.red .icon {
  color: #de350b;
  background: #ffebe6;
}
.red header small {
  color: #de350b;
  background: #ffebe6;
}
</style>
