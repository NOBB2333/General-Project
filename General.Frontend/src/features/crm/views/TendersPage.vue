<script setup lang="ts">
import TenderDetailDialog from './tenders/TenderDetailDialog.vue';
import TenderEditorDialog from './tenders/TenderEditorDialog.vue';
import TendersHero from './tenders/TendersHero.vue';
import TendersOverview from './tenders/TendersOverview.vue';
import TendersTitleBar from './tenders/TendersTitleBar.vue';
import TendersWorkspace from './tenders/TendersWorkspace.vue';
import { useTendersPage } from './tenders/useTendersPage';

const page = useTendersPage();
</script>

<template>
  <main
    :class="$style.page"
    data-page-route="/crm/tenders"
    data-page-implementation="crm-tenders"
  >
    <TendersHero />
    <TendersTitleBar
      v-model:query="page.query.value"
      @create="page.editorOpen.value = true"
      @export="page.exportRows"
    />
    <TendersWorkspace
      v-model:tab="page.tab.value"
      v-model:stage="page.stage.value"
      v-model:result="page.result.value"
      v-model:deposit="page.deposit.value"
      v-model:owner="page.owner.value"
      v-model:method="page.method.value"
      v-model:priority="page.priority.value"
      v-model:period="page.period.value"
      :rows="page.filteredRows.value"
      :tasks="page.tasks"
      :stages="page.stages"
      :results="page.results"
      :deposit-statuses="page.depositStatuses"
      :owners="page.owners"
      :methods="page.methods"
      :priorities="page.priorities"
      @select-row="page.selectedRow.value = $event"
    >
      <template #overview>
        <TendersOverview
          :month-count="page.monthCount.value"
          :active-count="page.activeCount.value"
          :won-count="page.wonCount.value"
          :win-rate="page.winRate.value"
          :opening-count="page.openingCount.value"
          :pending-deposit="page.pendingDeposit.value"
        />
      </template>
    </TendersWorkspace>
  </main>

  <TenderEditorDialog
    v-model:open="page.editorOpen.value"
    :owners="page.owners"
    :stages="page.stages"
    :priorities="page.priorities"
    :methods="page.methods"
    @save="page.saveTender"
  />
  <TenderDetailDialog
    :open="Boolean(page.selectedRow.value)"
    :row="page.selectedRow.value"
    @update:open="!$event && (page.selectedRow.value = null)"
  />
</template>

<style module>
@reference "../../../styles/index.css";

.page {
  @apply mx-auto min-h-full w-full max-w-[1400px] px-5 pt-3.5;
  margin-top: -1.5px;
}
</style>
