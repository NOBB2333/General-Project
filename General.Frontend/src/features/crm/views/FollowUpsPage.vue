<script setup lang="ts">
import FollowUpEditorDialog from './follow-ups/FollowUpEditorDialog.vue';
import FollowUpsHero from './follow-ups/FollowUpsHero.vue';
import FollowUpsOverview from './follow-ups/FollowUpsOverview.vue';
import FollowUpsScopeBar from './follow-ups/FollowUpsScopeBar.vue';
import FollowUpsWorkspace from './follow-ups/FollowUpsWorkspace.vue';
import { useFollowUpsPage } from './follow-ups/useFollowUpsPage';

const page = useFollowUpsPage();
</script>

<template>
  <main
    :class="$style.page"
    data-page-route="/crm/follow-ups"
    data-page-implementation="crm-follow-ups"
  >
    <FollowUpsHero />
    <FollowUpsScopeBar v-model="page.scope.value" />
    <FollowUpsOverview
      :total="page.totalCount.value"
      :week-count="page.weekCount.value"
      :pending-count="page.pendingCount.value"
      :customer-count="page.customerCount.value"
    />
    <FollowUpsWorkspace
      v-model:query="page.query.value"
      v-model:type="page.type.value"
      v-model:owner="page.owner.value"
      v-model:role="page.role.value"
      v-model:view="page.view.value"
      :rows="page.filteredRows.value"
      :types="page.types"
      :owners="page.owners"
      @create="page.editorOpen.value = true"
    />
  </main>

  <FollowUpEditorDialog
    v-model:open="page.editorOpen.value"
    :owners="page.owners"
    :types="page.types"
    @save="page.saveFollowUp"
  />
</template>

<style module>
@reference "../../../styles/index.css";

.page {
  @apply mx-auto min-h-full w-full max-w-[1400px];
  margin-top: -1.5px;
}
</style>
