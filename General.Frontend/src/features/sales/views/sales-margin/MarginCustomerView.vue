<script setup lang="ts">
import { ArrowDown, ArrowUp, Minus, TrendingDown, TrendingUp } from 'lucide-vue-next';
import type { MarginCustomer, MarginSortDirection, MarginSortKey } from '../../model/sales-margin.types';
defineProps<{ customers: readonly MarginCustomer[]; sortKey: MarginSortKey; sortDirection: MarginSortDirection }>();
const emit = defineEmits<{ sort: [key: MarginSortKey] }>();
const money = (value: number) => `¥${value.toLocaleString('zh-CN')}`;
</script>
<template>
  <div :class="$style.wrap">
    <table>
      <thead>
        <tr>
          <th>客户名称</th>
          <th>订单数</th>
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
          <th>销售成本</th>
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
          <th>最近下单</th>
          <th>趋势</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="row in customers"
          :key="row.id"
        >
          <td :class="$style.name">{{ row.name }}</td>
          <td>{{ row.orderCount }}</td>
          <td>{{ money(row.revenue) }}</td>
          <td>{{ money(row.cost) }}</td>
          <td :class="$style.profit">{{ money(row.grossProfit) }}</td>
          <td :class="$style.profit">{{ row.margin.toFixed(1) }}%</td>
          <td>{{ row.lastOrder }}</td>
          <td>
            <TrendingUp
              v-if="row.trend === 'up'"
              :class="$style.up"
              :size="15"
            /><TrendingDown
              v-else-if="row.trend === 'down'"
              :class="$style.down"
              :size="15"
            /><Minus
              v-else
              :class="$style.flat"
              :size="15"
            />
          </td>
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
table {
  @apply w-full border-collapse text-[12px];
}
th {
  @apply h-10 border-b text-left text-[11px] font-semibold;
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
th,
td {
  @apply px-2 first:pl-0 last:pr-0;
}
.name {
  color: var(--ui-text);
}
.profit {
  @apply font-medium;
  color: #00875a;
}
.up {
  color: #00875a;
}
.down {
  color: #de350b;
}
.flat {
  color: #97a0af;
}
</style>
