<script setup lang="ts">
import type { PageStat } from '@/components/page';

defineProps<{
  stats: readonly PageStat[];
}>();
</script>

<template>
  <section
    :class="$style.grid"
    aria-label="服务工单概览"
  >
    <article
      v-for="stat in stats"
      :key="stat.key"
      :class="$style.card"
    >
      <span>{{ stat.label }}</span>
      <div :class="$style.valueRow">
        <strong :class="$style[`tone_${stat.tone}`]">{{ stat.value }}</strong>
        <small v-if="stat.hint">{{ stat.hint }}</small>
      </div>
    </article>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.grid {
  @apply mt-4 grid grid-cols-6 gap-3;
}

.card {
  @apply min-w-0 border bg-white px-3.5 py-3;
  min-height: 82px;
  border-color: var(--ui-border);
  border-radius: var(--app-control-radius);
}

.card > span {
  @apply block truncate text-[11px];
  color: var(--ui-text-subtle);
}

.valueRow {
  @apply mt-2.5 flex items-end gap-1.5;
}

.valueRow strong {
  @apply text-[21px] font-bold leading-none;
}

.valueRow small {
  @apply text-[10px] font-semibold;
  color: var(--ui-success);
}

.tone_primary {
  color: var(--app-primary);
}

.tone_warning {
  color: var(--ui-warning);
}

.tone_danger {
  color: var(--ui-danger);
}

.tone_success {
  color: var(--ui-success);
}

.tone_neutral {
  color: var(--ui-text);
}

@media (max-width: 920px) {
  .grid {
    @apply grid-cols-3;
  }
}

@media (max-width: 560px) {
  .grid {
    @apply grid-cols-2;
  }
}
</style>
