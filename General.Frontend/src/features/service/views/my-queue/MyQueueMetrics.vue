<script setup lang="ts">
import type { MyQueueMetric, MyQueueTabId } from '../../model/my-queue.types';

defineProps<{
  metrics: readonly MyQueueMetric[];
  counts: Readonly<Record<MyQueueTabId, number>>;
  activeTab: MyQueueTabId;
}>();

const emit = defineEmits<{
  select: [tab: MyQueueTabId];
}>();
</script>

<template>
  <section
    :class="$style.grid"
    aria-label="待我处理概览"
  >
    <button
      v-for="metric in metrics"
      :key="metric.id"
      type="button"
      :class="[$style.card, activeTab === metric.id && $style.activeCard]"
      :data-tone="metric.tone"
      :aria-pressed="activeTab === metric.id"
      @click="emit('select', metric.id)"
    >
      <span>{{ metric.label }}</span>
      <strong>{{ counts[metric.id] }}</strong>
    </button>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.grid {
  @apply mt-4 grid grid-cols-4 gap-3;
}

.card {
  @apply min-h-[88px] rounded-lg border bg-white p-4 text-left outline-none transition-colors;
  border-color: var(--ui-border);
}

.card:hover {
  border-color: #b3d4ff;
}

.card:focus-visible {
  box-shadow: 0 0 0 2px var(--ui-focus);
}

.activeCard {
  border-color: var(--app-primary);
  box-shadow: 0 1px 3px rgba(9, 30, 66, 0.14);
}

.card span {
  @apply block text-[12px];
  color: var(--ui-text-subtle);
}

.card strong {
  @apply mt-1 block text-[24px] font-bold leading-8;
  color: var(--app-primary);
}

.card[data-tone='cyan'] strong {
  color: #00a3bf;
}

.card[data-tone='success'] strong {
  color: var(--ui-success);
}

@media (max-width: 760px) {
  .grid {
    @apply grid-cols-2;
  }
}
</style>
