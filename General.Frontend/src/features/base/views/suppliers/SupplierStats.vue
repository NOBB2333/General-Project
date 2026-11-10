<script setup lang="ts">
defineProps<{ total: number; purchase: string; payable: string }>();
</script>

<template>
  <section
    :class="$style.grid"
    aria-label="供应商统计"
  >
    <article :class="$style.card">
      <p>供应商总数</p>
      <strong>{{ total }}</strong>
    </article>
    <article :class="$style.card">
      <p>历史采购额</p>
      <strong>{{ purchase }}</strong>
    </article>
    <article :class="[$style.card, $style.payable]">
      <p>应付余额</p>
      <strong>{{ payable }}</strong>
    </article>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.grid {
  @apply mb-5 grid grid-cols-3 gap-4;
}
.card {
  @apply rounded-lg border bg-white p-4 transition-shadow;
  border-color: var(--ui-border);
}
.card:hover {
  @apply shadow-md;
}
.card p {
  @apply text-[12px];
  color: var(--ui-text-subtle);
}
.card strong {
  @apply mt-1 block text-[22px] font-bold;
  color: var(--ui-text);
}
.payable strong {
  color: #ff5630;
}
@media (max-width: 720px) {
  .grid {
    @apply grid-cols-1;
  }
}
</style>
