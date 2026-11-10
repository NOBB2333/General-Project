<script setup lang="ts">
defineProps<{ total: number; valuable: number; totalValue: number }>();
</script>

<template>
  <section
    :class="$style.overview"
    aria-label="公海客户概览"
  >
    <article>
      <span>公海客户数</span>
      <strong>{{ total }}</strong>
    </article>
    <article :class="$style.primary">
      <span>本周新增</span>
      <strong>1</strong>
    </article>
    <article :class="$style.success">
      <span>高价值客户</span>
      <strong>{{ valuable }}</strong>
    </article>
    <article>
      <span>预估总价值</span>
      <strong>¥{{ Math.round(totalValue / 10000) }}万</strong>
    </article>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.overview {
  @apply mt-4 grid grid-cols-4 gap-4;
}
.overview article {
  @apply flex min-h-[82px] flex-col justify-center rounded border bg-white p-4;
  border-color: var(--ui-border);
}
.overview span {
  @apply text-[12px];
  color: var(--ui-text-subtle);
}
.overview strong {
  @apply mt-2 text-[23px] font-bold;
  color: var(--ui-text);
}
.primary strong {
  color: var(--app-primary);
}
.success strong {
  color: var(--ui-success);
}
@media (max-width: 900px) {
  .overview {
    @apply grid-cols-2;
  }
}
</style>
