<script setup lang="ts">
import { computed } from 'vue';
import { toast } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import { purchaseOrderSuppliers } from '../model/purchase-orders.data';
import type { PurchaseOrderDraft } from '../model/purchase-orders.types';
import PurchaseOrderDialogs from './purchase-orders/PurchaseOrderDialogs.vue';
import PurchaseOrderItemsTable from './purchase-orders/PurchaseOrderItemsTable.vue';
import PurchaseOrderScopeBar from './purchase-orders/PurchaseOrderScopeBar.vue';
import PurchaseOrdersHero from './purchase-orders/PurchaseOrdersHero.vue';
import PurchaseOrderTable from './purchase-orders/PurchaseOrderTable.vue';
import PurchaseOrderToolbar from './purchase-orders/PurchaseOrderToolbar.vue';
import { usePurchaseOrdersPage } from './purchase-orders/usePurchaseOrdersPage';

const {
  rows,
  activeScope,
  activeView,
  activeStatus,
  supplier,
  progressFilter,
  query,
  createOpen,
  detailOpen,
  paymentOpen,
  activeRow,
  draft,
  scopeCounts,
  statusCounts,
  filteredRows,
  openCreate,
  saveOrder,
  openDetails,
  openPayment,
  toggleFollow,
  submitPayment,
  exportRows,
} = usePurchaseOrdersPage();

const supplierOptions = computed<UiSelectOption[]>(() =>
  purchaseOrderSuppliers.map((value) => ({ value, label: value })),
);

function updateDraft(value: PurchaseOrderDraft) {
  Object.assign(draft, value);
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="list"
    data-page-route="/purchase/orders"
    data-page-label="采购订单"
    data-page-module="采购管理"
    data-purchase-orders-page
    :data-active-scope="activeScope"
    :data-active-view="activeView"
    :data-active-status="activeStatus"
    :data-local-row-count="rows.length"
  >
    <PurchaseOrdersHero />
    <PurchaseOrderScopeBar
      :active-scope="activeScope"
      :counts="scopeCounts"
      :supplier="supplier"
      :supplier-options="supplierOptions"
      @update:active-scope="activeScope = $event"
      @update:supplier="supplier = $event"
    />

    <section :class="$style.workspace">
      <PurchaseOrderToolbar
        :active-view="activeView"
        :active-status="activeStatus"
        :status-counts="statusCounts"
        :progress-filter="progressFilter"
        :query="query"
        @update:active-view="activeView = $event"
        @update:active-status="activeStatus = $event"
        @update:progress-filter="progressFilter = $event"
        @update:query="query = $event"
        @invoice="toast.info('已打开进项发票入口，本地演示不会提交外部数据')"
        @export="exportRows"
        @create="openCreate"
      />
      <PurchaseOrderTable
        v-if="activeView === 'orders'"
        :rows="filteredRows"
        @details="openDetails"
        @payment="openPayment"
        @toggle-follow="toggleFollow"
      />
      <PurchaseOrderItemsTable
        v-else
        :rows="filteredRows"
      />
    </section>
  </main>

  <PurchaseOrderDialogs
    :create-open="createOpen"
    :detail-open="detailOpen"
    :payment-open="paymentOpen"
    :active-row="activeRow"
    :draft="draft"
    :supplier-options="supplierOptions"
    @update:create-open="createOpen = $event"
    @update:detail-open="detailOpen = $event"
    @update:payment-open="paymentOpen = $event"
    @update:draft="updateDraft"
    @save="saveOrder"
    @submit-payment="submitPayment"
  />
</template>

<style module>
@reference "../../../styles/index.css";

.page {
  @apply min-h-full min-w-[1180px] pb-8;
  color: var(--ui-text);
  background: var(--ui-surface-subtle);
}

.workspace {
  @apply mt-4 overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}
</style>
