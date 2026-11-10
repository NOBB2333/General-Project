<script setup lang="ts">
import { UiAlertDialog } from '@/components/ui';
import CustomerMaterialEditorDialog from './customer-materials/CustomerMaterialEditorDialog.vue';
import CustomerMaterialImportDialog from './customer-materials/CustomerMaterialImportDialog.vue';
import CustomerMaterialsHero from './customer-materials/CustomerMaterialsHero.vue';
import CustomerMaterialsOverview from './customer-materials/CustomerMaterialsOverview.vue';
import CustomerMaterialsTable from './customer-materials/CustomerMaterialsTable.vue';
import CustomerMaterialsToolbar from './customer-materials/CustomerMaterialsToolbar.vue';
import { useCustomerMaterialsPage } from './customer-materials/useCustomerMaterialsPage';

const {
  rows,
  query,
  customer,
  editorOpen,
  editingRow,
  importOpen,
  importMode,
  deleteOpen,
  pendingDelete,
  filteredRows,
  customerCount,
  materialCount,
  availableCustomers,
  openEditor,
  saveRow,
  requestDelete,
  confirmDelete,
  importRows,
  runToolbarAction,
} = useCustomerMaterialsPage();
</script>

<template>
  <main
    :class="$style.page"
    data-page-route="/crm/customer-materials"
    data-page-implementation="crm-customer-materials"
  >
    <CustomerMaterialsHero />
    <CustomerMaterialsOverview
      :mappings="rows.length"
      :customers="customerCount"
      :materials="materialCount"
    />
    <section :class="$style.listCard">
      <CustomerMaterialsToolbar
        v-model:query="query"
        v-model:customer="customer"
        :customers="availableCustomers"
        @action="runToolbarAction"
      />
      <CustomerMaterialsTable
        :rows="filteredRows"
        @edit="openEditor"
        @remove="requestDelete"
      />
    </section>
  </main>

  <CustomerMaterialEditorDialog
    v-model:open="editorOpen"
    :row="editingRow"
    :customers="availableCustomers"
    @save="saveRow"
  />
  <CustomerMaterialImportDialog
    v-model:open="importOpen"
    :mode="importMode"
    @import="importRows"
  />
  <UiAlertDialog
    v-model:open="deleteOpen"
    title="删除物料对照"
    :description="
      pendingDelete
        ? '确定删除「' + pendingDelete.customer + ' / ' + pendingDelete.customerCode + '」的物料对照吗？'
        : ''
    "
    confirm-label="确认删除"
    tone="danger"
    @confirm="confirmDelete"
  />
</template>

<style module>
@reference "../../../styles/index.css";

.page {
  @apply mx-auto min-h-full w-full max-w-[1400px];
}

.listCard {
  @apply mt-4 overflow-hidden rounded-lg border bg-white;
  border-color: var(--ui-border);
}
</style>
