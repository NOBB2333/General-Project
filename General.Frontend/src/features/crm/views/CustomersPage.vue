<script setup lang="ts">
import { BarChart3, UsersRound, WalletCards } from 'lucide-vue-next';
import { UiAlertDialog } from '@/components/ui';
import { customerOwners, customerTags } from '@/features/crm/model/customers.model';
import CustomerDetailDialog from './customers/CustomerDetailDialog.vue';
import CustomerEditorDialog from './customers/CustomerEditorDialog.vue';
import CustomerFollowUpDialog from './customers/CustomerFollowUpDialog.vue';
import CustomerIntakeDialog from './customers/CustomerIntakeDialog.vue';
import CustomerRecycleDialog from './customers/CustomerRecycleDialog.vue';
import CustomersHero from './customers/CustomersHero.vue';
import CustomersOverview from './customers/CustomersOverview.vue';
import CustomersTable from './customers/CustomersTable.vue';
import CustomersToolbar from './customers/CustomersToolbar.vue';
import { useCustomersPage } from './customers/useCustomersPage';

const {
  rows,
  recycledRows,
  scope,
  activeType,
  query,
  owner,
  tag,
  receivableOnly,
  analyticsOpen,
  visibleColumns,
  activeCustomer,
  editingCustomer,
  detailOpen,
  editorOpen,
  followUpOpen,
  recycleOpen,
  confirmationOpen,
  intakeOpen,
  intakeMode,
  confirmationKind,
  filteredRows,
  receivableShare,
  averageTerms,
  salesLabel,
  receivableLabel,
  topOwners,
  confirmationTitle,
  confirmationDescription,
  openDetails,
  openEditor,
  openFollowUp,
  saveCustomer,
  saveFollowUp,
  cycleTraffic,
  toggleColumn,
  requestConfirmation,
  confirmDestructiveAction,
  restoreCustomer,
  importCustomers,
  createScannedCustomer,
  runToolbarAction,
} = useCustomersPage();
</script>

<template>
  <main
    :class="$style.page"
    data-page-route="/base/customers"
    data-page-implementation="crm-customers"
  >
    <CustomersHero />
    <CustomersOverview
      v-model:scope="scope"
      v-model:type="activeType"
      :total="rows.length"
      :total-sales="salesLabel"
      :receivable="receivableLabel"
      :average-terms="averageTerms"
    />

    <section
      v-if="analyticsOpen"
      :class="$style.analytics"
      aria-label="客户统计分析"
    >
      <header><BarChart3 :size="16" /><strong>负责人销售分布</strong></header>
      <div :class="$style.analyticsGrid">
        <article
          v-for="item in topOwners"
          :key="item.name"
        >
          <span><UsersRound :size="14" />{{ item.name }} · {{ item.customers }} 家客户</span>
          <strong>¥{{ item.sales.toLocaleString('en-US') }}</strong>
          <i><b /></i>
        </article>
        <article>
          <span><WalletCards :size="14" />应收占比</span>
          <strong>{{ receivableShare }}</strong>
          <i><b :class="$style.receivableBar" /></i>
        </article>
      </div>
    </section>

    <section :class="$style.listCard">
      <CustomersToolbar
        v-model:query="query"
        v-model:owner="owner"
        v-model:tag="tag"
        v-model:receivable-only="receivableOnly"
        :owners="customerOwners"
        :tags="customerTags"
        :analytics-open="analyticsOpen"
        :recycle-count="recycledRows.length"
        @action="runToolbarAction"
      />
      <CustomersTable
        :rows="filteredRows"
        :visible-columns="visibleColumns"
        @details="openDetails"
        @edit="openEditor"
        @follow-up="openFollowUp"
        @recycle="requestConfirmation('recycle', $event)"
        @traffic="cycleTraffic"
        @toggle-column="toggleColumn"
      />
    </section>
  </main>

  <CustomerDetailDialog
    v-model:open="detailOpen"
    :customer="activeCustomer"
    @edit="openEditor"
    @follow-up="openFollowUp"
  />
  <CustomerEditorDialog
    v-model:open="editorOpen"
    :customer="editingCustomer"
    @save="saveCustomer"
  />
  <CustomerFollowUpDialog
    v-model:open="followUpOpen"
    :customer="activeCustomer"
    @save="saveFollowUp"
  />
  <CustomerRecycleDialog
    v-model:open="recycleOpen"
    :customers="recycledRows"
    @restore="restoreCustomer"
    @remove="requestConfirmation('purge', $event)"
    @clear="requestConfirmation('clear')"
  />
  <CustomerIntakeDialog
    v-model:open="intakeOpen"
    :mode="intakeMode"
    @import="importCustomers"
    @scan="createScannedCustomer"
  />
  <UiAlertDialog
    v-model:open="confirmationOpen"
    :title="confirmationTitle"
    :description="confirmationDescription"
    :confirm-label="confirmationKind === 'recycle' ? '移入回收站' : '确认删除'"
    tone="danger"
    @confirm="confirmDestructiveAction"
  />
</template>

<style module>
@reference "../../../styles/index.css";

.page {
  @apply mx-auto min-h-full w-full max-w-[1400px];
}

.listCard {
  @apply overflow-hidden rounded-b-lg border border-t-0 bg-white;
  border-color: var(--ui-border);
}

.analytics {
  @apply mb-3 rounded border bg-white p-4;
  border-color: var(--ui-border);
}

.analytics > header {
  @apply mb-3 flex items-center gap-2 text-[13px];
  color: var(--ui-text);
}

.analyticsGrid {
  @apply grid grid-cols-3 gap-3;
}

.analyticsGrid article {
  @apply rounded border px-3 py-2;
  border-color: var(--ui-border-subtle);
  background: var(--ui-surface-subtle);
}

.analyticsGrid span {
  @apply flex items-center gap-1.5 text-[11px];
  color: var(--ui-text-subtle);
}

.analyticsGrid strong {
  @apply mt-1 block text-[14px];
  color: var(--ui-text);
}

.analyticsGrid i {
  @apply mt-2 block h-1.5 overflow-hidden rounded-full;
  background: var(--ui-border-subtle);
}

.analyticsGrid b {
  @apply block h-full rounded-full;
  background: var(--app-primary);
}

.analyticsGrid article:nth-child(1) b {
  width: 62%;
}

.analyticsGrid article:nth-child(2) b {
  width: 48%;
}

.analyticsGrid article:nth-child(3) b {
  width: 28%;
}

.analyticsGrid article:nth-child(4) b {
  width: 35%;
}

.analyticsGrid article:nth-child(5) b {
  width: 18%;
}

.receivableBar {
  width: 16.2%;
  background: var(--ui-danger) !important;
}

@media (max-width: 900px) {
  .analyticsGrid {
    @apply grid-cols-1;
  }
}
</style>
