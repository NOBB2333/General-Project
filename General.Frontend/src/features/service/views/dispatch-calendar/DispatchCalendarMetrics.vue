<script setup lang="ts">
defineProps<{
  periodLabel: string;
  scheduleCount: number;
  urgentCount: number;
  unscheduledCount: number;
}>();
</script>

<template>
  <section
    :class="$style.metricGrid"
    aria-label="派工日历概览"
  >
    <article :class="$style.metricCard">
      <span :class="$style.metricLabel">本期间排程数</span>
      <strong :class="[$style.metricValue, $style.primaryValue]">{{ scheduleCount }}</strong>
      <small :class="$style.metricHint">{{ periodLabel }}</small>
    </article>
    <article :class="$style.metricCard">
      <span :class="$style.metricLabel">紧急 / 停机</span>
      <strong :class="[$style.metricValue, $style.dangerValue]">{{ urgentCount }}</strong>
    </article>
    <article :class="$style.metricCard">
      <span :class="$style.metricLabel">负载 Top</span>
      <strong :class="[$style.metricValue, $style.loadValue]">-</strong>
    </article>
    <article :class="$style.metricCard">
      <span :class="$style.metricLabel">未排期（已派工·待约时间）</span>
      <strong :class="[$style.metricValue, $style.warningValue]">{{ unscheduledCount }}</strong>
    </article>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.metricGrid {
  @apply mb-4 grid grid-cols-4 gap-3;
}

.metricCard {
  @apply flex min-h-[97px] flex-col rounded border bg-white px-4 py-3.5;
  border-color: var(--ui-border);
  border-radius: var(--app-control-radius);
}

.metricLabel {
  @apply text-[12px] font-medium leading-5;
  color: var(--ui-text-subtle);
}

.metricValue {
  @apply mt-1 text-[22px] font-bold leading-7;
}

.metricHint {
  @apply mt-auto text-[11px];
  color: #97a0af;
}

.primaryValue {
  color: var(--app-primary);
}

.dangerValue {
  color: var(--ui-danger);
}

.loadValue {
  color: #6554c0;
}

.warningValue {
  color: var(--ui-warning);
}

@media (max-width: 960px) {
  .metricGrid {
    @apply grid-cols-2;
  }
}
</style>
