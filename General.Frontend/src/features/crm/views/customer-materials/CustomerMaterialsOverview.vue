<script setup lang="ts">
import { Box, UsersRound } from 'lucide-vue-next';

defineProps<{
  mappings: number;
  customers: number;
  materials: number;
}>();
</script>

<template>
  <section
    :class="$style.overview"
    aria-label="客户物料对照概览"
  >
    <article>
      <span>对照条数</span>
      <strong>{{ mappings }}</strong>
    </article>
    <article>
      <span><UsersRound :size="13" />涉及客户</span>
      <strong>{{ customers }}</strong>
    </article>
    <article>
      <span><Box :size="13" />涉及物料</span>
      <strong>{{ materials }}</strong>
    </article>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.overview {
  @apply mt-4 grid grid-cols-3 gap-4;
}

.overview article {
  @apply flex min-h-[84px] flex-col justify-center rounded-lg border bg-white p-4;
  border-color: var(--ui-border);
}

.overview span {
  @apply flex items-center gap-1.5 text-[12px];
  color: var(--ui-text-subtle);
}

.overview strong {
  @apply mt-1 text-[22px] font-bold;
  color: var(--ui-text);
}

@media (max-width: 760px) {
  .overview {
    @apply grid-cols-1;
  }
}
</style>
