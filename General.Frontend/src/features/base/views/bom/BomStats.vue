<script setup lang="ts">
import { CheckCircle2, ClipboardList, FilePenLine, ListChecks } from 'lucide-vue-next';
defineProps<{ total: number; draft: number; review: number; active: number }>();
const stats = [
  { key: 'total', label: 'BOM 总数', icon: ClipboardList, tone: 'primary' },
  { key: 'draft', label: '草稿', icon: FilePenLine, tone: 'neutral' },
  { key: 'review', label: '待评审', icon: ListChecks, tone: 'warning' },
  { key: 'active', label: '已生效', icon: CheckCircle2, tone: 'success' },
] as const;
</script>

<template>
  <section
    :class="$style.grid"
    aria-label="BOM统计"
  >
    <article
      v-for="stat in stats"
      :key="stat.key"
      :class="$style.card"
    >
      <span :class="[$style.label, $style[stat.tone]]"
        ><component
          :is="stat.icon"
          :size="15"
          :stroke-width="1.8"
        />{{ stat.label }}</span
      >
      <strong :class="$style[stat.tone]">{{ $props[stat.key] }}</strong>
    </article>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";
.grid {
  @apply grid grid-cols-4 gap-3;
}
.card {
  @apply min-h-[84px] rounded border bg-white px-4 py-3;
  border-color: var(--ui-border);
}
.label {
  @apply flex items-center gap-2 text-[12px];
}
.card strong {
  @apply mt-2 block text-[22px] font-bold leading-none;
}
.primary {
  color: #0052cc;
}
.neutral {
  color: #5e6c84;
}
.warning {
  color: #ff8b00;
}
.success {
  color: #00875a;
}
@media (max-width: 900px) {
  .grid {
    @apply grid-cols-2;
  }
}
</style>
