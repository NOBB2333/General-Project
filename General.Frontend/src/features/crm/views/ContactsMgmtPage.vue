<script setup lang="ts">
import ContactEditorDialog from './contacts-mgmt/ContactEditorDialog.vue';
import ContactsHero from './contacts-mgmt/ContactsHero.vue';
import ContactsOverview from './contacts-mgmt/ContactsOverview.vue';
import ContactsTable from './contacts-mgmt/ContactsTable.vue';
import ContactsToolbar from './contacts-mgmt/ContactsToolbar.vue';
import { useContactsMgmtPage } from './contacts-mgmt/useContactsMgmtPage';

const {
  rows,
  query,
  status,
  owner,
  editorOpen,
  filteredRows,
  owners,
  counts,
  activeCount,
  awayCount,
  keyPersonCount,
  saveContact,
} = useContactsMgmtPage();
</script>

<template>
  <main
    :class="$style.page"
    data-page-route="/crm/contacts-mgmt"
    data-page-implementation="crm-contacts-mgmt"
  >
    <ContactsHero />
    <ContactsOverview
      :total="rows.length"
      :active="activeCount"
      :away="awayCount"
      :key-people="keyPersonCount"
    />
    <ContactsToolbar
      v-model:query="query"
      v-model:status="status"
      v-model:owner="owner"
      :owners="owners"
      :counts="counts"
      :total="rows.length"
      :visible-count="filteredRows.length"
      @create="editorOpen = true"
    >
      <ContactsTable :rows="filteredRows" />
    </ContactsToolbar>
  </main>

  <ContactEditorDialog
    v-model:open="editorOpen"
    :owners="owners"
    @save="saveContact"
  />
</template>

<style module>
@reference "../../../styles/index.css";

.page {
  @apply mx-auto min-h-full w-full max-w-[1400px];
}
</style>
