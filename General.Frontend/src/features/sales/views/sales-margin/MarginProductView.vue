<script setup lang="ts">
import { ArrowDown, ArrowUp } from 'lucide-vue-next';
import type { MarginProduct, MarginSortDirection, MarginSortKey } from '../../model/sales-margin.types';
defineProps<{ products: readonly MarginProduct[]; sortKey: MarginSortKey; sortDirection: MarginSortDirection }>();
const emit = defineEmits<{ sort: [key: MarginSortKey] }>();
const money = (value: number) => `¥${value.toLocaleString('zh-CN')}`;
</script>
<template>
  <div :class="$style.wrap">
    <div :class="$style.bars">
      <div
        v-for="(row, index) in products
          .slice()
          .sort((a, b) => b.grossProfit - a.grossProfit)
          .slice(0, 8)"
        :key="row.id"
        :class="$style.barRow"
      >
        <span>{{ row.name }}</span>
        <div :class="$style.barTrack">
          <div
            :class="$style.bar"
            :data-rank="index"
          />
        </div>
        <strong>{{ money(row.grossProfit) }}</strong>
      </div>
    </div>
    <table>
      <thead>
        <tr>
          <th>商品名称</th>
          <th>SKU</th>
          <th>品类</th>
          <th>销量</th>
          <th>
            <button
              type="button"
              @click="emit('sort', 'revenue')"
            >
              销售收入
              <ArrowDown
                v-if="sortKey === 'revenue' && sortDirection === 'desc'"
                :size="12"
              /><ArrowUp
                v-else-if="sortKey === 'revenue'"
                :size="12"
              />
            </button>
          </th>
          <th>
            <button
              type="button"
              @click="emit('sort', 'cost')"
            >
              销售成本
            </button>
          </th>
          <th>
            <button
              type="button"
              @click="emit('sort', 'grossProfit')"
            >
              毛利额
            </button>
          </th>
          <th>
            <button
              type="button"
              @click="emit('sort', 'margin')"
            >
              毛利率
            </button>
          </th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="row in products"
          :key="row.id"
        >
          <td :class="$style.name">{{ row.name }}</td>
          <td>{{ row.sku }}</td>
          <td>{{ row.category }}</td>
          <td>{{ row.qty }}</td>
          <td>{{ money(row.revenue) }}</td>
          <td>{{ money(row.cost) }}</td>
          <td :class="$style.profit">{{ money(row.grossProfit) }}</td>
          <td :class="$style.profit">{{ row.margin }}%</td>
        </tr>
      </tbody>
    </table>
  </div>
</template>
<style module>
@reference "../../../../styles/index.css";
.wrap {
  @apply p-5;
}
.bars {
  @apply mb-5 space-y-2;
}
.barRow {
  @apply grid grid-cols-[145px_1fr_105px] items-center gap-3 text-[11px];
}
.barRow > span {
  @apply truncate text-right;
  color: var(--ui-text-muted);
}
.barRow strong {
  @apply text-right text-[11px];
  color: var(--ui-text);
}
.barTrack {
  @apply h-3 overflow-hidden rounded-sm bg-[#EBECF0];
}
.bar {
  @apply h-full rounded-sm bg-[#0052CC];
  width: 100%;
}
.barRow:nth-child(2) .bar {
  width: 78%;
}
.barRow:nth-child(3) .bar {
  width: 70%;
}
.barRow:nth-child(4) .bar {
  width: 63%;
}
.barRow:nth-child(5) .bar {
  width: 55%;
}
.barRow:nth-child(6) .bar {
  width: 44%;
}
.barRow:nth-child(7) .bar {
  width: 36%;
}
.barRow:nth-child(8) .bar {
  width: 30%;
}
table {
  @apply w-full border-collapse text-[12px];
}
th {
  @apply h-9 border-b text-left text-[11px] font-semibold;
  color: var(--ui-text-subtle);
  border-color: var(--ui-border);
}
th button {
  @apply inline-flex items-center gap-1 font-semibold;
}
td {
  @apply h-10 border-b;
  color: var(--ui-text-muted);
  border-color: var(--ui-border-subtle);
}
td.name {
  color: var(--ui-text);
}
td.profit {
  @apply font-medium;
  color: #00875a;
}
th,
td {
  @apply px-2 first:pl-0 last:pr-0;
}
</style>
