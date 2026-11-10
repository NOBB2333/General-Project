<script setup lang="ts">
import type { Component } from 'vue';
interface SummaryItem {
  label: string;
  value: string;
  note: string;
  tone: string;
  icon: Component;
  interactive?: boolean;
}
defineProps<{ items: readonly SummaryItem[] }>();
const emit = defineEmits<{ detail: [label: string] }>();
</script>

<template>
  <section
    :class="$style.grid"
    aria-label="库存摘要"
  >
    <button
      v-for="item in items"
      :key="item.label"
      type="button"
      :data-tone="item.tone"
      :data-interactive="item.interactive || undefined"
      @click="item.interactive && emit('detail', item.label)"
    >
      <header>
        <span>{{ item.label }}</span
        ><component
          :is="item.icon"
          :size="16"
          :stroke-width="1.8"
        />
      </header>
      <strong>{{ item.value }}</strong
      ><small>{{ item.note }}</small>
    </button>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";
.grid {
  @apply mt-4 grid grid-cols-6 gap-3;
}
.grid > button {
  @apply min-h-[96px] cursor-default rounded border bg-white p-4 text-left;
  border-color: var(--ui-border);
}
.grid > button[data-interactive] {
  @apply cursor-pointer transition-colors;
}
.grid > button[data-interactive]:hover {
  border-color: #4c9aff;
  background: #f7faff;
}
.grid header {
  @apply flex items-center justify-between text-[11px];
  color: #6b778c;
}
.grid header svg {
  color: #6b778c;
}
.grid > button > strong {
  @apply mt-2 block text-[23px] font-bold;
  color: #172b4d;
}
.grid > button > small {
  @apply mt-1 block text-[10px];
  color: #97a0af;
}
.grid > button[data-tone='green'] strong {
  color: #00875a;
}
.grid > button[data-tone='blue'] strong {
  color: #0052cc;
}
.grid > button[data-tone='red'] strong {
  color: #ff5630;
}
.grid > button[data-tone='orange'] {
  border-color: #ff8b00;
}
.grid > button[data-tone='orange'] strong {
  color: #ff8b00;
}
</style>
