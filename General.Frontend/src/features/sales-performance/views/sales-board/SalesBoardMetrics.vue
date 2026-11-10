<script setup lang="ts">
import type { SalesBoardMetric } from '../../model/sales-board.types';

defineProps<{ metrics: readonly SalesBoardMetric[] }>();

const emit = defineEmits<{
  select: [metric: SalesBoardMetric];
}>();
</script>

<template>
  <section
    :class="$style.grid"
    aria-label="销售核心指标"
    data-board-metrics
  >
    <button
      v-for="metric in metrics"
      :key="metric.id"
      type="button"
      :class="$style.card"
      :data-metric-id="metric.id"
      :data-tone="metric.tone"
      :aria-label="`${metric.label} ${metric.value}${metric.unit ?? ''}，较上期${metric.trend >= 0 ? '增长' : '下降'} ${Math.abs(metric.trend)}%`"
      @click="emit('select', metric)"
    >
      <span :class="$style.iconBox">
        <component
          :is="metric.icon"
          :size="19"
          :stroke-width="1.8"
        />
      </span>
      <span
        :class="$style.trend"
        :data-direction="metric.trend >= 0 ? 'up' : 'down'"
      >
        {{ metric.trend >= 0 ? '↑' : '↓' }} {{ Math.abs(metric.trend) }}%
      </span>
      <span :class="$style.label">{{ metric.label }}</span>
      <strong :class="$style.value">
        {{ metric.value }}<small v-if="metric.unit">{{ metric.unit }}</small>
      </strong>
    </button>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.grid {
  @apply mt-4 grid gap-3;
  grid-template-columns: repeat(20, minmax(0, 1fr));
}

.card {
  @apply relative col-span-4 min-h-[128px] rounded border p-4 text-left outline-none transition-colors active:scale-[.99];
  color: var(--ui-text);
  background: var(--ui-surface);
  border-color: var(--ui-border);
}

.card:nth-child(n + 6) {
  @apply col-span-5;
}

.card:hover {
  border-color: #b3d4ff;
  background: color-mix(in srgb, var(--app-primary-soft) 14%, var(--ui-surface));
}

.card:focus-visible {
  box-shadow: 0 0 0 2px var(--ui-focus);
}

.iconBox {
  @apply flex h-9 w-9 items-center justify-center rounded;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.card[data-tone='green'] .iconBox {
  color: var(--ui-success);
  background: var(--ui-success-soft);
}

.card[data-tone='red'] .iconBox {
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}

.card[data-tone='orange'] .iconBox,
.card[data-tone='yellow'] .iconBox {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}

.card[data-tone='purple'] .iconBox {
  color: #6554c0;
  background: #eae6ff;
}

.card[data-tone='cyan'] .iconBox {
  color: #00a3bf;
  background: #e6fcff;
}

.trend {
  @apply absolute right-4 top-5 rounded px-2 py-1 text-[11px] font-semibold;
}

.trend[data-direction='up'] {
  color: var(--ui-success);
  background: var(--ui-success-soft);
}

.trend[data-direction='down'] {
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}

.label {
  @apply mt-3 block text-[12px];
  color: var(--ui-text-subtle);
}

.value {
  @apply mt-1 block truncate text-[22px] font-bold leading-8;
  color: var(--ui-text);
}

.value small {
  @apply ml-1 text-[11px] font-medium;
  color: var(--ui-text-subtle);
}

@media (max-width: 1220px) {
  .grid {
    grid-template-columns: repeat(12, minmax(0, 1fr));
  }

  .card,
  .card:nth-child(n + 6) {
    @apply col-span-4;
  }
}

@media (prefers-reduced-motion: reduce) {
  .card {
    transition: none;
  }
}
</style>
