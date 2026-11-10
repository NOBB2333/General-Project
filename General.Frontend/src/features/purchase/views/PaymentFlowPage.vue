<script setup lang="ts">
import { computed } from 'vue';
import type { UiSelectOption } from '@/components/ui';
import { paymentFlowSuppliers } from '../model/payment-flow.data';
import type { PaymentAllocationDraft, PaymentFlowDraft, PaymentFlowScope } from '../model/payment-flow.types';
import PaymentFlowDialogs from './payment-flow/PaymentFlowDialogs.vue';
import PaymentFlowHero from './payment-flow/PaymentFlowHero.vue';
import PaymentFlowOverview from './payment-flow/PaymentFlowOverview.vue';
import PaymentFlowTable from './payment-flow/PaymentFlowTable.vue';
import { usePaymentFlowPage } from './payment-flow/usePaymentFlowPage';

const {
  rows,
  activeScope,
  supplier,
  query,
  paymentDialogOpen,
  allocationDialogOpen,
  activeRow,
  paymentDraft,
  allocationDraft,
  filteredRows,
  totals,
  openPayment,
  savePayment,
  openAllocation,
  saveAllocation,
  exportRows,
} = usePaymentFlowPage();

const supplierOptions = computed<UiSelectOption[]>(() =>
  paymentFlowSuppliers.map((value) => ({ value, label: value })),
);

function updateScope(value: PaymentFlowScope) {
  activeScope.value = value;
}

function updatePaymentDraft(value: PaymentFlowDraft) {
  Object.assign(paymentDraft, value);
}

function updateAllocationDraft(value: PaymentAllocationDraft) {
  Object.assign(allocationDraft, value);
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="list"
    data-page-route="/purchase/payment-flow"
    data-page-label="付款流水"
    data-page-module="采购管理"
    data-payment-flow-page
    :data-active-scope="activeScope"
    :data-local-row-count="rows.length"
  >
    <PaymentFlowHero />
    <PaymentFlowOverview
      :total="totals.total"
      :allocated="totals.allocated"
      :unallocated="totals.unallocated"
      :incomplete="totals.incomplete"
      :count="rows.length"
    />
    <PaymentFlowTable
      :rows="filteredRows"
      :all-rows="rows"
      :active-scope="activeScope"
      :supplier="supplier"
      :query="query"
      :supplier-options="supplierOptions"
      @update:active-scope="updateScope"
      @update:supplier="supplier = $event"
      @update:query="query = $event"
      @allocate="openAllocation"
      @export="exportRows"
      @create="openPayment"
    />
  </main>

  <PaymentFlowDialogs
    :payment-open="paymentDialogOpen"
    :allocation-open="allocationDialogOpen"
    :active-row="activeRow"
    :payment-draft="paymentDraft"
    :allocation-draft="allocationDraft"
    :supplier-options="supplierOptions"
    @update:payment-open="paymentDialogOpen = $event"
    @update:allocation-open="allocationDialogOpen = $event"
    @update:payment-draft="updatePaymentDraft"
    @update:allocation-draft="updateAllocationDraft"
    @save-payment="savePayment"
    @save-allocation="saveAllocation"
  />
</template>

<style module>
@reference "../../../styles/index.css";

.page {
  @apply min-h-full min-w-[1180px] pb-8;
  color: var(--ui-text);
  background: var(--ui-surface-subtle);
}
</style>
