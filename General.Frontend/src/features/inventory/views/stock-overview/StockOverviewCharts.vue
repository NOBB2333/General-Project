<script setup lang="ts">
import type { CategoryStock, WarehouseAmount } from '../../model/stock-overview.types';
defineProps<{ warehouses: readonly WarehouseAmount[]; categories: readonly CategoryStock[] }>();
</script>

<template>
  <section :class="$style.grid">
    <article>
      <h2>各仓库库存金额</h2>
      <div :class="$style.warehouseChart">
        <div :class="$style.axis">
          <span>200万</span><span>150万</span><span>100万</span><span>50万</span><span>0万</span>
        </div>
        <div :class="$style.warehouseBars">
          <span
            v-for="item in warehouses"
            :key="item.name"
            ><i :data-amount="item.amount" /><b>{{ item.name }}</b></span
          >
        </div>
      </div>
    </article>
    <article>
      <h2>分类库存数量</h2>
      <div :class="$style.categoryChart">
        <div
          :class="$style.donut"
          aria-hidden="true"
        />
        <ul>
          <li
            v-for="item in categories"
            :key="item.name"
          >
            <i :data-tone="item.tone" /><span>{{ item.name }}</span
            ><strong>{{ item.quantity }} 件</strong>
          </li>
        </ul>
      </div>
    </article>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";
.grid {
  @apply mt-4 grid grid-cols-2 gap-3;
}
.grid > article {
  @apply min-h-[280px] rounded border bg-white p-5;
  border-color: var(--ui-border);
}
.grid h2 {
  @apply text-[15px] font-semibold;
  color: #172b4d;
}
.warehouseChart {
  @apply mt-4 flex h-[210px];
}
.axis {
  @apply flex w-14 flex-col justify-between pb-7 text-right text-[10px];
  color: #6b778c;
}
.warehouseBars {
  @apply flex flex-1 items-end justify-around border-b;
  border-color: #dfe1e6;
  background: repeating-linear-gradient(to bottom, transparent 0 48px, #ebecf0 49px);
}
.warehouseBars span {
  @apply flex h-full w-1/5 flex-col items-center justify-end gap-2;
}
.warehouseBars i {
  @apply block w-10 rounded-t bg-[#0052CC];
}
.warehouseBars i[data-amount='184'] {
  height: 184px;
}
.warehouseBars i[data-amount='28'] {
  height: 28px;
}
.warehouseBars i[data-amount='17'] {
  height: 17px;
}
.warehouseBars i[data-amount='34'] {
  height: 34px;
}
.warehouseBars b {
  @apply h-5 whitespace-nowrap text-[10px] font-normal;
  color: #6b778c;
}
.categoryChart {
  @apply flex h-[220px] items-center justify-center gap-12;
}
.donut {
  @apply h-[150px] w-[150px] rounded-full;
  background: conic-gradient(#0052cc 0 17%, #2684ff 17% 26%, #00875a 26% 54%, #ff8b00 54% 68%, #6554c0 68% 100%);
  box-shadow: inset 0 0 0 44px white;
}
.categoryChart ul {
  @apply space-y-3 text-[11px];
}
.categoryChart li {
  @apply flex min-w-[130px] items-center gap-2;
}
.categoryChart li i {
  @apply h-2.5 w-2.5 rounded-full;
}
.categoryChart li i[data-tone='blue'] {
  background: #0052cc;
}
.categoryChart li i[data-tone='cyan'] {
  background: #2684ff;
}
.categoryChart li i[data-tone='green'] {
  background: #00875a;
}
.categoryChart li i[data-tone='orange'] {
  background: #ff8b00;
}
.categoryChart li i[data-tone='purple'] {
  background: #6554c0;
}
.categoryChart li span {
  @apply flex-1;
}
.categoryChart li strong {
  color: #42526e;
}
</style>
