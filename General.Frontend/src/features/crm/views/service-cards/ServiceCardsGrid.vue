<script setup lang="ts">
import type { ServiceCardRow } from '@/features/crm/model/service-cards.types';
import ServiceCardItem from './ServiceCardItem.vue';

defineProps<{ cards: readonly ServiceCardRow[] }>();
</script>

<template>
  <section
    v-if="cards.length"
    :class="$style.grid"
    aria-label="服务权益卡列表"
  >
    <ServiceCardItem
      v-for="card in cards"
      :key="card.id"
      :card="card"
    />
  </section>
  <section
    v-else
    :class="$style.empty"
  >
    当前条件下没有服务权益卡
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.grid {
  @apply mt-4 grid grid-cols-3 gap-x-5 gap-y-5;
}

.empty {
  @apply mt-3 flex min-h-52 items-center justify-center rounded-lg border border-dashed bg-white text-[12px];
  color: var(--ui-text-subtle);
  border-color: var(--ui-border);
}

@media (max-width: 1050px) {
  .grid {
    @apply grid-cols-2;
  }
}

@media (max-width: 720px) {
  .grid {
    @apply grid-cols-1;
  }
}
</style>
