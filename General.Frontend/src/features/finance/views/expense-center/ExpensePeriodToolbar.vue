<script setup lang="ts">
import { CalendarDays, ChevronLeft, ChevronRight, Filter } from 'lucide-vue-next';
defineProps<{ period: string }>();
const emit = defineEmits<{ 'update:period': [value: string] }>();
const periods = ['全部', '25/10', '25/11', '25/12', '26/01', '26/02', 'Q1', 'Q2', 'Q3', 'Q4'] as const;
</script>

<template>
  <nav
    :class="$style.toolbar"
    aria-label="费用期间"
  >
    <button
      v-for="item in periods"
      :key="item"
      type="button"
      :class="[$style.period, period === item && $style.active]"
      @click="emit('update:period', item)"
    >
      {{ item }}
    </button>
    <ChevronLeft
      :size="14"
      :stroke-width="1.8"
    /><button
      type="button"
      :class="$style.year"
    >
      2026</button
    ><ChevronRight
      :size="14"
      :stroke-width="1.8"
    />
    <button
      type="button"
      :class="$style.custom"
    >
      <CalendarDays
        :size="14"
        :stroke-width="1.8"
      />自定义
    </button>
    <span :class="$style.allTime"
      ><Filter
        :size="14"
        :stroke-width="1.8"
      />全部时间（20 笔）</span
    >
  </nav>
</template>

<style module>
@reference "../../../../styles/index.css";
.toolbar {
  @apply mt-4 flex h-9 items-center gap-1;
  color: var(--ui-text-subtle);
}
.period,
.year,
.custom {
  @apply flex h-9 items-center gap-1 rounded border border-transparent px-3 text-[11px] font-semibold transition-colors;
}
.period:hover {
  background: var(--ui-surface-hover);
}
.active {
  color: var(--app-primary);
  border-color: var(--ui-border);
  background: white;
  box-shadow: 0 1px 2px rgb(9 30 66 / 10%);
}
.year,
.custom {
  @apply border;
  border-color: var(--ui-border);
  background: white;
}
.allTime {
  @apply ml-2 flex h-9 items-center gap-1 rounded px-3 text-[11px];
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
</style>
