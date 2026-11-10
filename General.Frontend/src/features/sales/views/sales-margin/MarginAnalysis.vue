<script setup lang="ts">
import { computed } from 'vue';
import { Box, Download, FileText, Users } from 'lucide-vue-next';
import { UiButton, UiSelect } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import type {
  MarginCustomer,
  MarginOrder,
  MarginProduct,
  MarginRange,
  MarginSortDirection,
  MarginSortKey,
  MarginView,
} from '../../model/sales-margin.types';
import MarginCustomerView from './MarginCustomerView.vue';
import MarginOrderView from './MarginOrderView.vue';
import MarginProductView from './MarginProductView.vue';

const props = defineProps<{
  view: MarginView;
  range: MarginRange;
  category: string;
  orderStatus: string;
  products: readonly MarginProduct[];
  orders: readonly MarginOrder[];
  customers: readonly MarginCustomer[];
  sortKey: MarginSortKey;
  sortDirection: MarginSortDirection;
}>();
const emit = defineEmits<{
  'update:view': [value: MarginView];
  'update:range': [value: MarginRange];
  'update:category': [value: string];
  'update:orderStatus': [value: string];
  sort: [value: MarginSortKey];
  export: [];
}>();
const tabs: readonly { value: MarginView; label: string; icon: typeof Box }[] = [
  { value: 'product', label: '按商品分析', icon: Box },
  { value: 'order', label: '按销售订单分析', icon: FileText },
  { value: 'customer', label: '按客户分析', icon: Users },
];
const ranges: readonly MarginRange[] = ['本月', '本季', '半年', '全年'];
const categoryOptions = computed<UiSelectOption[]>(() => [
  { value: '全部品类', label: '全部品类' },
  ...[...new Set(props.products.map((row) => row.category))].map((value) => ({ value, label: value })),
]);
const statusOptions: readonly UiSelectOption[] = [
  { value: '全部状态', label: '全部状态' },
  { value: '待审核', label: '待审核' },
  { value: '执行中', label: '执行中' },
  { value: '部分发货', label: '部分发货' },
  { value: '已完成', label: '已完成' },
  { value: '已取消', label: '已取消' },
];
const filteredProducts = computed(() =>
  props.category === '全部品类' ? props.products : props.products.filter((row) => row.category === props.category),
);
const filteredOrders = computed(() =>
  props.orderStatus === '全部状态' ? props.orders : props.orders.filter((row) => row.status === props.orderStatus),
);
</script>

<template>
  <section
    :class="$style.analysis"
    aria-label="毛利分析明细"
  >
    <header :class="$style.header">
      <nav
        :class="$style.tabs"
        aria-label="毛利分析维度"
      >
        <button
          v-for="tab in tabs"
          :key="tab.value"
          type="button"
          :class="[$style.tab, view === tab.value && $style.tabActive]"
          :aria-selected="view === tab.value"
          @click="emit('update:view', tab.value)"
        >
          <component
            :is="tab.icon"
            :size="16"
            :stroke-width="1.8"
          />{{ tab.label }}
        </button>
      </nav>
      <div :class="$style.actions">
        <div :class="$style.range">
          <button
            v-for="item in ranges"
            :key="item"
            type="button"
            :class="[$style.rangeButton, range === item && $style.rangeActive]"
            @click="emit('update:range', item)"
          >
            {{ item }}
          </button>
        </div>
        <UiButton
          variant="secondary"
          size="sm"
          @click="emit('export')"
          ><template #leading
            ><Download
              :size="14"
              :stroke-width="1.8" /></template
          >导出</UiButton
        >
      </div>
    </header>
    <div
      v-if="view === 'product'"
      :class="$style.filterRow"
    >
      <span>商品毛利 TOP 8</span
      ><UiSelect
        :model-value="category"
        label="品类"
        :options="categoryOptions"
        size="sm"
        @update:model-value="emit('update:category', $event)"
      />
    </div>
    <div
      v-else-if="view === 'order'"
      :class="$style.filterRow"
    >
      <span>销售订单毛利分析</span
      ><UiSelect
        :model-value="orderStatus"
        label="订单状态"
        :options="statusOptions"
        size="sm"
        @update:model-value="emit('update:orderStatus', $event)"
      />
    </div>
    <MarginProductView
      v-if="view === 'product'"
      :products="filteredProducts"
      :sort-key="sortKey"
      :sort-direction="sortDirection"
      @sort="emit('sort', $event)"
    />
    <MarginOrderView
      v-else-if="view === 'order'"
      :orders="filteredOrders"
      :sort-key="sortKey"
      :sort-direction="sortDirection"
      @sort="emit('sort', $event)"
    />
    <MarginCustomerView
      v-else
      :customers="customers"
      :sort-key="sortKey"
      :sort-direction="sortDirection"
      @sort="emit('sort', $event)"
    />
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";
.analysis {
  @apply overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}
.header {
  @apply flex min-h-[54px] items-end justify-between border-b px-0;
  border-color: var(--ui-border);
}
.tabs {
  @apply flex h-full items-end gap-1;
}
.tab {
  @apply inline-flex min-h-[52px] items-center gap-1.5 border-b-2 border-transparent px-5 text-[13px] font-medium transition-colors;
  color: var(--ui-text-subtle);
}
.tab:hover {
  color: var(--app-primary);
}
.tabActive {
  @apply font-semibold;
  color: var(--app-primary);
  border-bottom-color: var(--app-primary);
}
.actions {
  @apply flex items-center gap-2 pb-2 pr-3;
}
.range {
  @apply flex items-center overflow-hidden rounded border;
  border-color: var(--ui-border);
}
.rangeButton {
  @apply min-h-8 border-r px-3 text-[12px] font-medium last:border-r-0;
  color: var(--ui-text-muted);
  border-color: var(--ui-border);
}
.rangeActive {
  @apply text-white;
  background: var(--app-primary);
}
.filterRow {
  @apply flex min-h-[54px] items-center justify-between border-b px-5 text-[15px] font-semibold;
  border-color: var(--ui-border);
}
</style>
