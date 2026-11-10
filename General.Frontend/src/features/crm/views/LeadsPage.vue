<script setup lang="ts">
import LeadEditorDialog from './leads/LeadEditorDialog.vue';
import LeadsHero from './leads/LeadsHero.vue';
import LeadsOverview from './leads/LeadsOverview.vue';
import LeadsWorkspace from './leads/LeadsWorkspace.vue';
import { useLeadsPage } from './leads/useLeadsPage';

const page = useLeadsPage();
</script>

<template>
  <main
    :class="$style.page"
    data-page-route="/crm/leads"
    data-page-implementation="crm-leads"
  >
    <LeadsHero />
    <LeadsOverview
      :total="page.rows.value.length"
      :new-count="page.newCount.value"
      :converted="page.convertedCount.value"
      :public-count="page.publicCount.value"
    />
    <LeadsWorkspace
      v-model:scope="page.scope.value"
      v-model:status="page.status.value"
      v-model:source="page.source.value"
      v-model:query="page.query.value"
      :rows="page.filteredRows.value"
      :selected-ids="page.selectedIds.value"
      :all-visible-selected="page.allVisibleSelected.value"
      :scope-counts="page.scopeCounts.value"
      :sources="page.sources"
      @select="page.toggleSelected"
      @select-all="page.toggleAll"
      @convert="page.convertLead"
      @create="page.editorOpen.value = true"
      @export="page.exportLeads"
    />
  </main>

  <LeadEditorDialog
    v-model:open="page.editorOpen.value"
    :sources="page.sources"
    @save="page.saveLead"
  />
</template>

<style module>
@reference "../../../styles/index.css";

.page {
  @apply mx-auto min-h-full w-full max-w-[1400px];
  margin-top: -1.5px;
}
</style>
