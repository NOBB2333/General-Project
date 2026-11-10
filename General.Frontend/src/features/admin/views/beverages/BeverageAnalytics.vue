<script setup lang="ts">
import { beverageUsageTrend, beverageValueDistribution } from '../../model/beverages.data';

const currency = new Intl.NumberFormat('zh-CN', {
  style: 'currency',
  currency: 'CNY',
  maximumFractionDigits: 0,
});
</script>

<template>
  <section :class="$style.analytics">
    <article :class="$style.distribution">
      <h2>库存价值分布</h2>
      <div :class="$style.distributionBody">
        <div
          :class="$style.donut"
          aria-label="库存价值按品类分布"
        >
          <span>¥6.55万<small>库存总值</small></span>
        </div>
        <ul>
          <li
            v-for="item in beverageValueDistribution"
            :key="item.label"
            :data-category="item.label"
          >
            <i /><span>{{ item.label }}</span
            ><strong>{{ currency.format(item.value) }}</strong>
          </li>
        </ul>
      </div>
    </article>
    <article :class="$style.trend">
      <h2>近6月领用趋势</h2>
      <div :class="$style.chart">
        <div :class="$style.axis">
          <span>¥20k</span><span>¥15k</span><span>¥10k</span><span>¥5k</span><span>¥0k</span>
        </div>
        <div :class="$style.bars">
          <div
            v-for="item in beverageUsageTrend"
            :key="item.label"
          >
            <i /><span>{{ item.label }}</span>
          </div>
        </div>
      </div>
    </article>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.analytics {
  @apply mb-4 grid grid-cols-2 gap-4;
}
.analytics article {
  @apply rounded border bg-white p-5;
  border-color: var(--ui-border);
}
.analytics h2 {
  @apply text-[13px] font-semibold;
  color: var(--ui-text);
}
.distributionBody {
  @apply flex h-[150px] items-center justify-center gap-16;
}
.donut {
  @apply relative flex h-28 w-28 items-center justify-center rounded-full;
  background: conic-gradient(#ff8b00 0 60%, #ff5630 60% 87%, #0052cc 87% 96%, #0065ff 96% 98%, #00875a 98%);
}
.donut::after {
  @apply absolute h-[70px] w-[70px] rounded-full bg-white;
  content: '';
}
.donut span {
  @apply relative z-[1] text-center text-[13px] font-semibold;
}
.donut small {
  @apply mt-1 block text-[8px] font-normal;
  color: var(--ui-text-subtle);
}
.distribution ul {
  @apply min-w-[250px] space-y-2;
}
.distribution li {
  @apply grid grid-cols-[10px_1fr_auto] items-center gap-2 text-[10px];
  color: var(--ui-text-muted);
}
.distribution li i {
  @apply h-2 w-2 rounded-full;
  background: #ff8b00;
}
.distribution li[data-category='茶叶'] i {
  background: #ff5630;
}
.distribution li[data-category='红酒'] i {
  background: #0052cc;
}
.distribution li[data-category='饮料'] i {
  background: #0065ff;
}
.distribution li[data-category='啤酒'] i {
  background: #00875a;
}
.distribution li strong {
  @apply font-semibold;
  color: var(--ui-text);
}
.chart {
  @apply mt-4 flex h-[150px];
}
.axis {
  @apply flex w-12 flex-col justify-between pb-5 text-right text-[9px];
  color: var(--ui-text-subtle);
}
.bars {
  @apply ml-3 grid flex-1 grid-cols-6 items-end border-b border-l px-5;
  border-color: var(--ui-border-subtle);
  background-image: repeating-linear-gradient(to bottom, transparent 0, transparent 29px, var(--ui-border-subtle) 30px);
}
.bars div {
  @apply flex h-full flex-col items-center justify-end gap-2;
}
.bars i {
  @apply block w-7 rounded-t;
  background: #ff5630;
}
.bars div:nth-child(1) i {
  height: 41%;
}
.bars div:nth-child(2) i {
  height: 63%;
}
.bars div:nth-child(3) i {
  height: 34%;
}
.bars div:nth-child(4) i {
  height: 97%;
}
.bars div:nth-child(5) i {
  height: 47%;
}
.bars div:nth-child(6) i {
  height: 29%;
}
.bars span {
  @apply text-[9px];
  color: var(--ui-text-subtle);
}
</style>
