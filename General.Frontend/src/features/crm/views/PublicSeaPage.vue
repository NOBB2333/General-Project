<script setup lang="ts">
import PublicSeaEditorDialog from './public-sea/PublicSeaEditorDialog.vue';
import PublicSeaHero from './public-sea/PublicSeaHero.vue';
import PublicSeaOverview from './public-sea/PublicSeaOverview.vue';
import PublicSeaWorkspace from './public-sea/PublicSeaWorkspace.vue';
import { usePublicSeaPage } from './public-sea/usePublicSeaPage';

const page = usePublicSeaPage();
</script>

<template>
  <main
    :class="$style.page"
    data-page-route="/crm/public-sea"
    data-page-implementation="crm-public-sea"
  >
    <PublicSeaHero />
    <PublicSeaOverview
      :total="page.rows.value.length"
      :valuable="page.valuableCount.value"
      :total-value="page.totalValue.value"
    />
    <PublicSeaWorkspace
      v-model:tab="page.tab.value"
      v-model:industry="page.industry.value"
      v-model:source="page.source.value"
      v-model:query="page.query.value"
      :rows="page.filteredRows.value"
      :counts="page.tabCounts.value"
      :industries="page.industries"
      :sources="page.sources"
      @create="page.editorOpen.value = true"
      @export="page.exportCustomers"
      @follow="page.toggleFollow"
      @claim="page.claimCustomer"
    />
  </main>

  <PublicSeaEditorDialog
    v-model:open="page.editorOpen.value"
    :sources="page.sources"
    @save="page.saveCustomer"
  />
</template>

<style module>
@reference "../../../styles/index.css";

.page {
  @apply mx-auto min-h-full w-full max-w-[1400px];
  margin-top: -1.5px;
}
</style>
