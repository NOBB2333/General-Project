<script setup lang="ts">
import ServiceCardEditorDialog from './service-cards/ServiceCardEditorDialog.vue';
import ServiceCardsGrid from './service-cards/ServiceCardsGrid.vue';
import ServiceCardsHero from './service-cards/ServiceCardsHero.vue';
import ServiceCardsOverview from './service-cards/ServiceCardsOverview.vue';
import ServiceCardsToolbar from './service-cards/ServiceCardsToolbar.vue';
import { useServiceCardsPage } from './service-cards/useServiceCardsPage';

const {
  rows,
  query,
  customer,
  status,
  editorOpen,
  filteredRows,
  customers,
  activeCount,
  expiringCount,
  expiredCount,
  saveCard,
} = useServiceCardsPage();
</script>

<template>
  <main
    :class="$style.page"
    data-page-route="/crm/service-cards"
    data-page-implementation="crm-service-cards"
  >
    <ServiceCardsHero />
    <ServiceCardsOverview
      :total="rows.length"
      :active="activeCount"
      :expiring="expiringCount"
      :expired="expiredCount"
    />
    <ServiceCardsToolbar
      v-model:query="query"
      v-model:customer="customer"
      v-model:status="status"
      :customers="customers"
      @create="editorOpen = true"
    />
    <ServiceCardsGrid :cards="filteredRows" />
  </main>

  <ServiceCardEditorDialog
    v-model:open="editorOpen"
    :customers="customers"
    @save="saveCard"
  />
</template>

<style module>
@reference "../../../styles/index.css";

.page {
  @apply mx-auto min-h-full w-full max-w-[1400px];
}
</style>
