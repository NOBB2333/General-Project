<script setup lang="ts">
import { ArrowRightLeft } from 'lucide-vue-next';
import { computed } from 'vue';
import { UiButton, UiDialog } from '@/components/ui';
import { transferLoanOverdueItems } from '../model/transfer-loan.data';
import type { TransferLoanCategory, TransferLoanDraft } from '../model/transfer-loan.types';
import TransferLoanFormDialog from './transfer-loan/TransferLoanFormDialog.vue';
import TransferLoanOverview from './transfer-loan/TransferLoanOverview.vue';
import TransferLoanTable from './transfer-loan/TransferLoanTable.vue';
import { useTransferLoanPage } from './transfer-loan/useTransferLoanPage';

const {
  rows,
  activeKind,
  activeStatus,
  query,
  dialogOpen,
  detailOpen,
  activeRow,
  draft,
  filteredRows,
  totalAmount,
  focusRow,
  openCreate,
  saveDraft,
  openDetails,
  exportRows,
} = useTransferLoanPage();

const categories = computed<TransferLoanCategory[]>(() => [
  { id: 'all', label: '全部类型', description: '所有调拨与借出单据', count: rows.value.length },
  {
    id: 'transfer',
    label: '普通调拨',
    description: '仓库之间库存调配',
    count: rows.value.filter((row) => row.kind === 'transfer').length,
  },
  {
    id: 'loan',
    label: '借出管理',
    description: '设备借出与归还核销',
    count: rows.value.filter((row) => row.kind === 'loan').length,
  },
  {
    id: 'repricing',
    label: '变价调拨',
    description: '调拨并调整价格核算',
    count: rows.value.filter((row) => row.kind === 'repricing').length,
  },
]);

function updateDraft(value: TransferLoanDraft) {
  Object.assign(draft, value);
}

function currency(value: number) {
  return `¥${value.toLocaleString('zh-CN')}`;
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="list"
    data-page-route="/inventory/transfer-loan"
    data-page-label="调拨与借出"
    data-page-module="库存管理"
    data-transfer-loan-page
    :data-local-row-count="rows.length"
    :data-active-kind="activeKind"
  >
    <TransferLoanOverview
      :active-kind="activeKind"
      :categories="categories"
      :overdue-items="transferLoanOverdueItems"
      :total-amount="totalAmount"
      @update:active-kind="activeKind = $event"
      @focus-row="focusRow"
      @export="exportRows"
      @create="openCreate"
    />

    <TransferLoanTable
      :rows="filteredRows"
      :active-status="activeStatus"
      :query="query"
      @update:active-status="activeStatus = $event"
      @update:query="query = $event"
      @details="openDetails"
    />

    <TransferLoanFormDialog
      :open="dialogOpen"
      :draft="draft"
      @update:open="dialogOpen = $event"
      @update:draft="updateDraft"
      @save="saveDraft"
    />

    <UiDialog
      v-model:open="detailOpen"
      :title="activeRow?.code ?? '单据详情'"
      :description="activeRow ? `${activeRow.type} · ${activeRow.status}` : undefined"
      size="lg"
    >
      <template #icon>
        <span :class="$style.dialogIcon"><ArrowRightLeft :size="18" /></span>
      </template>
      <dl
        v-if="activeRow"
        :class="$style.detailGrid"
      >
        <div>
          <dt>发出方</dt>
          <dd>{{ activeRow.sender }}</dd>
        </div>
        <div>
          <dt>接收方</dt>
          <dd>{{ activeRow.receiver }}</dd>
        </div>
        <div>
          <dt>业务原因</dt>
          <dd>{{ activeRow.reason }}</dd>
        </div>
        <div>
          <dt>商品/数量</dt>
          <dd>{{ activeRow.productCount }}种 / {{ activeRow.quantity }}件</dd>
        </div>
        <div>
          <dt>金额</dt>
          <dd>{{ currency(activeRow.amount) }}</dd>
        </div>
        <div>
          <dt>申请信息</dt>
          <dd>{{ activeRow.date }} · {{ activeRow.applicant }}</dd>
        </div>
      </dl>
      <template #footer>
        <UiButton
          variant="secondary"
          size="sm"
          @click="detailOpen = false"
          >关闭</UiButton
        >
      </template>
    </UiDialog>
  </main>
</template>

<style module>
@reference "../../../styles/index.css";

.page {
  @apply min-h-full min-w-[1180px] pb-8;
  color: var(--ui-text);
  background: var(--ui-surface-subtle);
}

.dialogIcon {
  @apply flex h-9 w-9 shrink-0 items-center justify-center rounded-full;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.detailGrid {
  @apply grid grid-cols-2 gap-4 p-5;
}

.detailGrid div {
  @apply rounded border p-3;
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}

.detailGrid dt {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}

.detailGrid dd {
  @apply m-0 mt-1 text-[12px] font-medium;
}
</style>
