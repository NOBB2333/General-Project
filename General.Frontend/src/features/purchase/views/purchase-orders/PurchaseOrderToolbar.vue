<script setup lang="ts">
import { Download, FileSpreadsheet, ListTree, PackageSearch, Plus, Search } from 'lucide-vue-next';
import { UiButton, UiInput, UiSelect, UiTabs, UiTabsList, UiTabsTrigger } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import type {
  PurchaseOrderProgressFilter,
  PurchaseOrderStatus,
  PurchaseOrderView,
} from '../../model/purchase-orders.types';

defineProps<{
  activeView: PurchaseOrderView;
  activeStatus: '全部' | PurchaseOrderStatus;
  statusCounts: Record<'全部' | PurchaseOrderStatus, number>;
  progressFilter: PurchaseOrderProgressFilter;
  query: string;
}>();

const emit = defineEmits<{
  'update:activeView': [value: PurchaseOrderView];
  'update:activeStatus': [value: '全部' | PurchaseOrderStatus];
  'update:progressFilter': [value: PurchaseOrderProgressFilter];
  'update:query': [value: string];
  invoice: [];
  export: [];
  create: [];
}>();

const statuses: readonly ('全部' | PurchaseOrderStatus)[] = [
  '全部',
  '草稿',
  '待审核',
  '已审核',
  '部分到货',
  '已完成',
  '已取消',
];
const progressOptions: readonly UiSelectOption[] = [
  { value: 'all', label: '进度筛选' },
  { value: 'invoice-pending', label: '开票未完成' },
  { value: 'stock-pending', label: '入库未完成' },
  { value: 'payment-pending', label: '付款未完成' },
];
</script>

<template>
  <header :class="$style.toolbar">
    <div :class="$style.navigation">
      <UiTabs
        :model-value="activeView"
        @update:model-value="emit('update:activeView', $event as PurchaseOrderView)"
      >
        <UiTabsList
          label="采购订单视图"
          :class="$style.views"
        >
          <UiTabsTrigger
            value="orders"
            :class="$style.viewTab"
          >
            <PackageSearch :size="14" />订单列表
          </UiTabsTrigger>
          <UiTabsTrigger
            value="items"
            :class="$style.viewTab"
          >
            <ListTree :size="14" />订单明细 <span>18</span>
          </UiTabsTrigger>
        </UiTabsList>
      </UiTabs>

      <UiTabs
        :model-value="activeStatus"
        @update:model-value="emit('update:activeStatus', $event as '全部' | PurchaseOrderStatus)"
      >
        <UiTabsList
          label="采购订单状态"
          :class="$style.statuses"
        >
          <UiTabsTrigger
            v-for="status in statuses"
            :key="status"
            :value="status"
            :class="$style.statusTab"
          >
            {{ status }}<span v-if="status !== '全部'">({{ statusCounts[status] }})</span>
          </UiTabsTrigger>
        </UiTabsList>
      </UiTabs>
    </div>

    <div :class="$style.tools">
      <UiSelect
        :model-value="progressFilter"
        :options="progressOptions"
        label="订单进度筛选"
        size="sm"
        :class="$style.progressSelect"
        @update:model-value="emit('update:progressFilter', $event as PurchaseOrderProgressFilter)"
      />
      <label :class="$style.searchField">
        <Search
          :size="14"
          :stroke-width="1.8"
        />
        <UiInput
          :model-value="query"
          aria-label="搜索采购订单"
          placeholder="搜索订单号、供应商..."
          @update:model-value="emit('update:query', $event)"
        />
      </label>
      <UiButton
        variant="secondary"
        size="sm"
        @click="emit('invoice')"
      >
        <template #leading><FileSpreadsheet :size="14" /></template>
        进项发票
      </UiButton>
      <UiButton
        variant="secondary"
        size="sm"
        @click="emit('export')"
      >
        <template #leading><Download :size="14" /></template>
        导出
      </UiButton>
      <UiButton
        variant="primary"
        size="sm"
        @click="emit('create')"
      >
        <template #leading><Plus :size="14" /></template>
        新建采购单
      </UiButton>
    </div>
  </header>
</template>

<style module>
@reference "../../../../styles/index.css";

.toolbar {
  @apply flex min-h-[78px] items-end justify-between gap-4 border-b bg-white px-2;
  border-color: var(--ui-border);
}

.navigation {
  @apply flex min-w-0 items-end gap-6;
}

.views,
.statuses {
  @apply gap-0;
}

.viewTab,
.statusTab {
  @apply h-[48px] rounded-none border-b-2 border-transparent px-3 text-[11px];
}

.viewTab span {
  @apply flex h-5 min-w-5 items-center justify-center rounded-full px-1 text-[9px];
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
}

.viewTab[data-state='active'],
.statusTab[data-state='active'] {
  color: var(--app-primary);
  border-bottom-color: var(--app-primary);
  background: transparent;
}

.statuses {
  @apply flex-nowrap;
}

.statusTab {
  @apply px-2;
}

.statusTab span {
  @apply ml-0.5;
  color: #97a0af;
}

.tools {
  @apply flex items-center gap-2 pb-2;
}

.progressSelect {
  @apply w-[108px];
}

.searchField {
  @apply flex h-8 w-[210px] items-center gap-2 rounded px-2;
  color: var(--ui-text-subtle);
  background: var(--ui-surface-subtle);
}

.searchField input {
  @apply h-full border-0 bg-transparent p-0 shadow-none;
}
</style>
