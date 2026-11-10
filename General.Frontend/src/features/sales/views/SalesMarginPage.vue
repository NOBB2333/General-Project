<script setup lang="ts">
import { computed, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import { toast } from '@/components/ui';
import { marginCategories, marginMonths, marginOrders, salesMarginReferenceFixture } from '../model/sales-margin.data';
import type {
  MarginCustomer,
  MarginRange,
  MarginScope,
  MarginSortDirection,
  MarginSortKey,
  MarginView,
} from '../model/sales-margin.types';
import MarginAnalysis from './sales-margin/MarginAnalysis.vue';
import SalesMarginCharts from './sales-margin/SalesMarginCharts.vue';
import SalesMarginHero from './sales-margin/SalesMarginHero.vue';
import SalesMarginOverview from './sales-margin/SalesMarginOverview.vue';

const products = cloneReferenceRows(salesMarginReferenceFixture.rows);
const scope = ref<MarginScope>('all');
const view = ref<MarginView>('product');
const range = ref<MarginRange>('本月');
const category = ref('全部品类');
const orderStatus = ref('全部状态');
const sortKey = ref<MarginSortKey>('grossProfit');
const sortDirection = ref<MarginSortDirection>('desc');
const scopeFactor = computed(() => ({ all: 1, mine: 0.38, team: 0.72, followed: 0.24 })[scope.value]);
const monthCount = computed(() => ({ 本月: 1, 本季: 3, 半年: 6, 全年: 6 })[range.value]);
const visibleMonths = computed(() => marginMonths.slice(-monthCount.value));
const totals = computed(() => {
  const raw = visibleMonths.value.reduce(
    (sum, month) => ({
      revenue: sum.revenue + month.revenue,
      cost: sum.cost + month.cost,
      grossProfit: sum.grossProfit + month.grossProfit,
    }),
    { revenue: 0, cost: 0, grossProfit: 0 },
  );
  const factor = scopeFactor.value;
  const revenue = Math.round(raw.revenue * factor);
  const cost = Math.round(raw.cost * factor);
  const grossProfit = Math.round(raw.grossProfit * factor);
  return { revenue, cost, grossProfit, margin: revenue ? (grossProfit / revenue) * 100 : 0, change: 43.2 };
});
const scopedProducts = computed(() => {
  if (scope.value === 'all') return products;
  const divisor = scope.value === 'team' ? 4 : scope.value === 'mine' ? 3 : 5;
  return products.filter((_, index) => index % divisor !== 0);
});
const customers = computed<MarginCustomer[]>(() => {
  const grouped = new Map<string, MarginCustomer>();
  marginOrders.forEach((order) => {
    const current = grouped.get(order.customer);
    if (current) {
      current.orderCount += 1;
      current.revenue += order.revenue;
      current.cost += order.cost;
      current.grossProfit += order.grossProfit;
      if (order.date > current.lastOrder) current.lastOrder = order.date;
      current.margin = current.revenue ? (current.grossProfit / current.revenue) * 100 : 0;
      return;
    }
    grouped.set(order.customer, {
      id: order.customer,
      name: order.customer,
      orderCount: 1,
      revenue: order.revenue,
      cost: order.cost,
      grossProfit: order.grossProfit,
      margin: order.margin,
      lastOrder: order.date,
      trend: order.margin >= 30 ? 'up' : order.margin < 26 ? 'down' : 'flat',
    });
  });
  return [...grouped.values()];
});
function updateSort(key: MarginSortKey) {
  if (sortKey.value === key) sortDirection.value = sortDirection.value === 'desc' ? 'asc' : 'desc';
  else {
    sortKey.value = key;
    sortDirection.value = 'desc';
  }
}
function exportCsv() {
  const header = ['商品名称', 'SKU', '品类', '销量', '销售收入', '销售成本', '毛利额', '毛利率'];
  const body = scopedProducts.value.map((row) => [
    row.name,
    row.sku,
    row.category,
    row.qty,
    row.revenue,
    row.cost,
    row.grossProfit,
    `${row.margin}%`,
  ]);
  const csv = [header, ...body].map((line) => line.map((value) => `"${value}"`).join(',')).join('\n');
  const link = document.createElement('a');
  link.href = URL.createObjectURL(new Blob([`\uFEFF${csv}`], { type: 'text/csv;charset=utf-8' }));
  link.download = `销售毛利-${range.value}.csv`;
  link.click();
  URL.revokeObjectURL(link.href);
  toast.success('销售毛利数据已导出');
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="dashboard"
    data-page-route="/sales/sales-margin"
    data-page-label="销售毛利"
    data-page-module="销售业务"
    data-sales-margin-page
    :data-scope="scope"
    :data-view="view"
    :data-range="range"
  >
    <SalesMarginHero />
    <SalesMarginOverview
      v-model:scope="scope"
      :totals="totals"
    />
    <SalesMarginCharts
      :months="visibleMonths"
      :categories="marginCategories"
    />
    <MarginAnalysis
      v-model:view="view"
      v-model:range="range"
      v-model:category="category"
      v-model:order-status="orderStatus"
      :products="scopedProducts"
      :orders="marginOrders"
      :customers="customers"
      :sort-key="sortKey"
      :sort-direction="sortDirection"
      @sort="updateSort"
      @export="exportCsv"
    />
  </main>
</template>

<style module>
@reference "../../../styles/index.css";
.page {
  @apply mx-auto min-h-full min-w-[1180px] max-w-[1400px] pb-8;
  color: var(--ui-text);
}
</style>
