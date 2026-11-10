<script setup lang="ts">
import SalesBoardRankingPanel from './SalesBoardRankingPanel.vue';
import SalesBoardTargetPanel from './SalesBoardTargetPanel.vue';
import SalesBoardTrendPanel from './SalesBoardTrendPanel.vue';

const emit = defineEmits<{
  openTrend: [];
  openTarget: [];
  openRanking: [id: string];
}>();
</script>

<template>
  <section
    :class="$style.overview"
    data-board-overview
  >
    <SalesBoardTrendPanel
      :class="$style.trendPanel"
      @open="emit('openTrend')"
    />
    <SalesBoardTargetPanel @open="emit('openTarget')" />
    <SalesBoardRankingPanel @open="emit('openRanking', $event)" />
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.overview {
  @apply mt-4 grid min-w-0 gap-4;
  grid-template-columns: minmax(0, 2fr) minmax(300px, 1fr);
}

.trendPanel {
  @apply row-span-2;
}

@media (max-width: 1120px) {
  .overview {
    grid-template-columns: minmax(0, 1fr);
  }

  .trendPanel {
    @apply row-span-1;
  }
}
</style>
