<script setup lang="ts">
import type { BeverageStock } from '../../model/beverages.types';

defineProps<{ rows: readonly BeverageStock[] }>();

const currency = new Intl.NumberFormat('zh-CN', {
  style: 'currency',
  currency: 'CNY',
  maximumFractionDigits: 0,
});
</script>

<template>
  <div :class="$style.scroller">
    <table>
      <thead>
        <tr>
          <th>编号</th>
          <th>名称</th>
          <th>类别</th>
          <th>规格</th>
          <th>库存</th>
          <th>单价</th>
          <th>库存价值</th>
          <th>存放位置</th>
          <th>状态</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="row in rows"
          :key="row.id"
        >
          <td>{{ row.code }}</td>
          <td>
            <strong>{{ row.name }}</strong>
          </td>
          <td>
            <span
              :class="$style.category"
              :data-category="row.category"
              >{{ row.category }}</span
            >
          </td>
          <td>{{ row.specification }}</td>
          <td>
            <strong>{{ row.stock }}</strong>
          </td>
          <td>{{ currency.format(row.unitPrice) }}</td>
          <td>
            <strong>{{ currency.format(row.stockValue) }}</strong>
          </td>
          <td>{{ row.location }}</td>
          <td>
            <span
              :class="$style.status"
              :data-status="row.status"
              >{{ row.status }}</span
            >
          </td>
        </tr>
      </tbody>
    </table>
    <div
      v-if="!rows.length"
      :class="$style.empty"
    >
      当前条件下没有酒水库存
    </div>
  </div>
</template>

<style module>
@reference "../../../../styles/index.css";

.scroller {
  @apply overflow-x-auto;
}
.scroller table {
  @apply w-full min-w-[1180px] border-collapse text-left text-[10px];
}
.scroller th {
  @apply h-10 whitespace-nowrap border-b px-4 font-semibold;
  color: var(--ui-text-subtle);
  border-color: var(--ui-border);
  background: #f4f5f7;
}
.scroller td {
  @apply h-[52px] whitespace-nowrap border-b px-4;
  color: var(--ui-text-muted);
  border-color: var(--ui-border-subtle);
}
.scroller tbody tr:hover {
  background: var(--ui-surface-hover);
}
.scroller td:first-child {
  color: var(--ui-text-subtle);
}
.scroller td strong {
  @apply font-semibold;
  color: var(--ui-text);
}
.category,
.status {
  @apply inline-flex rounded px-2 py-1 text-[9px] font-semibold;
}
.category {
  color: #ff8b00;
  background: #fff0b3;
}
.category[data-category='红酒'] {
  color: #de350b;
  background: #ffebe6;
}
.category[data-category='啤酒'] {
  color: #00875a;
  background: #e3fcef;
}
.category[data-category='饮料'] {
  color: #0052cc;
  background: #deebff;
}
.category[data-category='茶叶'] {
  color: #6554c0;
  background: #eae6ff;
}
.status {
  color: #00875a;
  background: #e3fcef;
}
.status[data-status='预警'] {
  color: #de350b;
  background: #ffebe6;
}
.empty {
  @apply py-14 text-center text-[12px];
  color: var(--ui-text-subtle);
}
</style>
