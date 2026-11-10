<script setup lang="ts">
import OpportunityEditorDialog from './opportunities/OpportunityEditorDialog.vue';
import OpportunityOverview from './opportunities/OpportunityOverview.vue';
import OpportunityScopeBar from './opportunities/OpportunityScopeBar.vue';
import OpportunitiesHero from './opportunities/OpportunitiesHero.vue';
import OpportunityStageDistribution from './opportunities/OpportunityStageDistribution.vue';
import OpportunitiesWorkspace from './opportunities/OpportunitiesWorkspace.vue';
import { useOpportunitiesPage } from './opportunities/useOpportunitiesPage';

const page = useOpportunitiesPage();
</script>

<template>
  <main
    :class="$style.page"
    data-page-route="/crm/opportunities"
    data-page-implementation="crm-opportunities"
  >
    <OpportunitiesHero />
    <OpportunityScopeBar
      v-model:scope="page.scope.value"
      :counts="page.scopeCounts.value"
    />
    <OpportunityOverview
      :active-count="page.activeRows.value.length"
      :pipeline-amount="page.pipelineAmount.value"
      :weighted-amount="page.weightedAmount.value"
      :won-amount="page.wonAmount.value"
    />
    <OpportunityStageDistribution :counts="page.stageCounts.value" />
    <OpportunitiesWorkspace
      v-model:query="page.query.value"
      v-model:stage="page.stage.value"
      v-model:source="page.source.value"
      v-model:view="page.view.value"
      :rows="page.filteredRows.value"
      :stages="page.stages"
      :sources="page.sources"
      @create="page.editorOpen.value = true"
    />
  </main>

  <OpportunityEditorDialog
    v-model:open="page.editorOpen.value"
    :customers="page.customers"
    :owners="page.owners"
    :stages="page.stages"
    :sources="page.sources"
    @save="page.saveOpportunity"
  />
</template>

<style module>
@reference "../../../styles/index.css";

.page {
  @apply mx-auto min-h-full w-full max-w-[1400px];
  margin-top: -1.5px;
}
</style>
