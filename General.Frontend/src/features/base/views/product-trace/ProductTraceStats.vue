<script setup lang="ts">
defineProps<{ total: number; stock: number; outbound: number; repair: number; scrapped: number }>();
const stats = [
  { key: 'total', label: '总实例', tone: 'primary' },
  { key: 'stock', label: '在库', tone: 'success' },
  { key: 'outbound', label: '已出库', tone: 'primary' },
  { key: 'repair', label: '报修/维修', tone: 'danger' },
  { key: 'scrapped', label: '已报废', tone: 'neutral' },
] as const;
</script>

<template>
  <section
    :class="$style.grid"
    aria-label="产品实例统计"
  >
    <article
      v-for="stat in stats"
      :key="stat.key"
      :class="$style.card"
    >
      <span>{{ stat.label }}</span
      ><strong :class="$style[stat.tone]">{{ $props[stat.key] }}</strong>
    </article>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";
.grid {
  @apply grid grid-cols-5 gap-3;
}
.card {
  @apply min-h-[66px] rounded border bg-white px-3 py-2.5;
  border-color: var(--ui-border);
}
.card span {
  @apply block text-[11px];
  color: var(--ui-text-subtle);
}
.card strong {
  @apply mt-1 block text-[19px] font-bold leading-none;
}
.primary {
  color: #0052cc;
}
.success {
  color: #00875a;
}
.danger {
  color: #ff5630;
}
.neutral {
  color: #5e6c84;
}
@media (max-width: 900px) {
  .grid {
    @apply grid-cols-2;
  }
}
</style>
