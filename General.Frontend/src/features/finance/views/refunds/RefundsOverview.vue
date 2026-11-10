<script setup lang="ts">
import { ArrowDownLeft, ArrowUpRight, CircleCheck, Clock3, RefreshCw, WalletCards } from 'lucide-vue-next';
import type { RefundKind, RefundMetric } from '../../model/refunds.types';

defineProps<{
  activeKind: RefundKind;
  customerCount: number;
  supplierCount: number;
  metrics: readonly RefundMetric[];
}>();

const emit = defineEmits<{
  'update:activeKind': [value: RefundKind];
}>();

const metricIcons = {
  approval: Clock3,
  processing: RefreshCw,
  pendingAmount: WalletCards,
  writtenOff: CircleCheck,
} as const;
</script>

<template>
  <div
    :class="$style.kindSwitch"
    role="group"
    aria-label="退款类型"
  >
    <button
      type="button"
      :class="[$style.kindButton, activeKind === 'customer' && $style.kindActive]"
      :aria-pressed="activeKind === 'customer'"
      @click="emit('update:activeKind', 'customer')"
    >
      <ArrowUpRight
        :size="14"
        :stroke-width="1.8"
      />
      客户退款 <span>{{ customerCount }}</span>
    </button>
    <button
      type="button"
      :class="[$style.kindButton, activeKind === 'supplier' && $style.kindActive]"
      :aria-pressed="activeKind === 'supplier'"
      @click="emit('update:activeKind', 'supplier')"
    >
      <ArrowDownLeft
        :size="14"
        :stroke-width="1.8"
      />
      供应商退款 <span>{{ supplierCount }}</span>
    </button>
  </div>

  <section
    :class="$style.metrics"
    aria-label="退款概览"
  >
    <article
      v-for="metric in metrics"
      :key="metric.id"
      :class="[$style.metric, $style[`metric_${metric.tone}`]]"
    >
      <div :class="$style.metricLabel">
        <component
          :is="metricIcons[metric.id]"
          :size="16"
          :stroke-width="1.8"
        />
        {{ metric.label }}
      </div>
      <strong>{{ metric.value }}</strong>
    </article>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.kindSwitch {
  @apply mt-4 flex items-center gap-1;
}

.kindButton {
  @apply inline-flex h-10 min-w-[145px] items-center justify-center gap-2 rounded border px-4 text-[12px] font-semibold transition-colors active:scale-[.97];
  color: var(--ui-text-muted);
  border-color: var(--ui-border);
  background: var(--ui-surface);
}

.kindButton:hover {
  background: var(--ui-surface-hover);
}

.kindButton span {
  @apply inline-flex h-[19px] min-w-[19px] items-center justify-center rounded-full px-1 text-[9px];
  color: var(--ui-text-subtle);
  background: var(--ui-border-subtle);
}

.kindActive {
  color: white;
  border-color: var(--app-primary);
  background: var(--app-primary);
  box-shadow: 0 2px 4px rgb(9 30 66 / 18%);
}

.kindActive:hover {
  background: color-mix(in srgb, var(--app-primary) 85%, black);
}

.kindActive span {
  color: var(--app-primary);
  background: rgb(255 255 255 / 88%);
}

.metrics {
  @apply mt-4 grid grid-cols-4 gap-4;
}

.metric {
  @apply flex min-h-[86px] flex-col justify-center rounded border bg-white px-4;
  border-color: var(--ui-border);
}

.metricLabel {
  @apply flex items-center gap-2 text-[11px];
  color: var(--ui-text-subtle);
}

.metric strong {
  @apply mt-2 text-[21px] font-semibold leading-none;
}

.metric_warning strong,
.metric_warning .metricLabel {
  color: var(--ui-warning);
}

.metric_primary strong,
.metric_primary .metricLabel {
  color: var(--app-primary);
}

.metric_violet strong,
.metric_violet .metricLabel {
  color: #6554c0;
}

.metric_success strong,
.metric_success .metricLabel {
  color: var(--ui-success);
}
</style>
