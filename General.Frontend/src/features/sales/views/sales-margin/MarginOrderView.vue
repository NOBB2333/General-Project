<script setup lang="ts">
import { ArrowDown, ArrowUp } from 'lucide-vue-next';
import type { MarginOrder, MarginSortDirection, MarginSortKey } from '../../model/sales-margin.types';
defineProps<{ orders: readonly MarginOrder[]; sortKey: MarginSortKey; sortDirection: MarginSortDirection }>();
const emit = defineEmits<{ sort: [key: MarginSortKey] }>();
const money = (value: number) => `¥${value.toLocaleString('zh-CN')}`;
</script>
<template>
  <div :class="$style.wrap">
    <table>
      <thead>
        <tr>
          <th>订单号</th>
          <th>客户</th>
          <th>下单日期</th>
          <th>类型</th>
          <th>商品数</th>
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
          <th>毛利额</th>
          <th>毛利率</th>
          <th>状态</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="row in orders"
          :key="row.id"
        >
          <td :class="$style.code">{{ row.id }}</td>
          <td>{{ row.customer }}</td>
          <td>{{ row.date }}</td>
          <td>{{ row.type }}</td>
          <td>{{ row.items }}</td>
          <td>{{ money(row.revenue) }}</td>
          <td>{{ money(row.cost) }}</td>
          <td :class="$style.profit">{{ money(row.grossProfit) }}</td>
          <td :class="$style.profit">{{ row.margin }}%</td>
          <td>
            <span :class="[$style.status, $style[`status_${row.status}`]]">{{ row.status }}</span>
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
.code {
  color: var(--app-primary);
}
.profit {
  @apply font-medium;
  color: #00875a;
}
.status {
  @apply inline-flex rounded px-2 py-0.5 text-[10px];
  background: #f4f5f7;
  color: #6b778c;
}
.status_待审核 {
  color: #ff8b00;
  background: #fff7e6;
}
.status_执行中 {
  color: #0052cc;
  background: #deebff;
}
.status_已完成 {
  color: #00875a;
  background: #e3fcef;
}
.status_已取消 {
  color: #de350b;
  background: #ffebe6;
}
</style>
