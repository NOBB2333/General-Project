<script setup lang="ts">
import { computed } from 'vue';
import type { MarginCategory, MarginMonth } from '../../model/sales-margin.types';
const props = defineProps<{ months: readonly MarginMonth[]; categories: readonly MarginCategory[] }>();
const maxValue = computed(() =>
  Math.max(...props.months.flatMap((month) => [month.revenue, month.cost, month.grossProfit]), 1),
);
const pointX = (index: number) => 72 + index * (590 / Math.max(props.months.length - 1, 1));
const pointY = (value: number) => 176 - (value / maxValue.value) * 134;
const linePoints = (key: 'revenue' | 'cost' | 'grossProfit') =>
  props.months.map((month, index) => `${pointX(index)},${pointY(month[key])}`).join(' ');
const categoryTotal = computed(() => props.categories.reduce((sum, item) => sum + item.value, 0));
const arcs = computed(() => {
  let offset = 0;
  return props.categories.map((category) => {
    const size = (category.value / categoryTotal.value) * 100;
    const arc = { ...category, dash: `${size} ${100 - size}`, offset: -offset };
    offset += size;
    return arc;
  });
});
</script>
<template>
  <section
    :class="$style.grid"
    aria-label="毛利图表"
  >
    <article :class="$style.panel">
      <h2>月度毛利趋势</h2>
      <svg
        :class="$style.trend"
        viewBox="0 0 720 220"
        role="img"
        aria-label="收入、成本与毛利月度趋势"
      >
        <g :class="$style.guides">
          <line
            v-for="y in [42, 87, 132, 177]"
            :key="y"
            x1="64"
            :y1="y"
            x2="684"
            :y2="y"
          />
        </g>
        <polyline
          :class="$style.revenueLine"
          :points="linePoints('revenue')"
        />
        <polyline
          :class="$style.costLine"
          :points="linePoints('cost')"
        />
        <polyline
          :class="$style.profitLine"
          :points="linePoints('grossProfit')"
        />
        <g
          v-for="(month, index) in months"
          :key="month.id"
        >
          <circle
            :class="$style.revenuePoint"
            :cx="pointX(index)"
            :cy="pointY(month.revenue)"
            r="3.5"
          />
          <circle
            :class="$style.costPoint"
            :cx="pointX(index)"
            :cy="pointY(month.cost)"
            r="3.5"
          />
          <circle
            :class="$style.profitPoint"
            :cx="pointX(index)"
            :cy="pointY(month.grossProfit)"
            r="3.5"
          />
          <text
            :x="pointX(index)"
            y="204"
            text-anchor="middle"
          >
            {{ month.month }}
          </text>
        </g>
      </svg>
      <div :class="$style.legend">
        <span :class="$style.blue">收入</span><span :class="$style.red">成本</span
        ><span :class="$style.green">毛利</span>
      </div>
    </article>
    <article :class="$style.panel">
      <h2>品类毛利构成</h2>
      <div :class="$style.donutArea">
        <svg
          :class="$style.donut"
          viewBox="0 0 42 42"
          role="img"
          aria-label="品类毛利构成"
        >
          <circle
            :class="$style.donutTrack"
            cx="21"
            cy="21"
            r="15.9155"
          />
          <circle
            v-for="arc in arcs"
            :key="arc.id"
            :class="$style.donutArc"
            cx="21"
            cy="21"
            r="15.9155"
            :stroke="arc.color"
            :stroke-dasharray="arc.dash"
            :stroke-dashoffset="arc.offset"
          />
          <text
            x="21"
            y="20"
            text-anchor="middle"
          >
            毛利额
          </text>
          <text
            x="21"
            y="24"
            text-anchor="middle"
          >
            {{ (categoryTotal / 10000).toFixed(1) }}万
          </text>
        </svg>
      </div>
      <div :class="$style.categoryLegend">
        <span
          v-for="item in categories"
          :key="item.id"
          ><i :class="$style[`dot_${item.id}`]" />{{ item.name }}</span
        >
      </div>
    </article>
  </section>
</template>
<style module>
@reference "../../../../styles/index.css";
.grid {
  @apply mb-4 grid grid-cols-[2fr_1fr] gap-3;
}
.panel {
  @apply min-h-[290px] rounded border bg-white p-5;
  border-color: var(--ui-border);
}
.panel h2 {
  @apply text-[15px] font-semibold;
  color: var(--ui-text);
}
.trend {
  @apply mt-2 h-[215px] w-full overflow-visible;
}
.guides line {
  stroke: #ebecf0;
  stroke-width: 1;
  stroke-dasharray: 3 4;
}
.trend polyline {
  fill: none;
  stroke-width: 2.5;
  stroke-linecap: round;
  stroke-linejoin: round;
}
.revenueLine {
  stroke: #0052cc;
}
.costLine {
  stroke: #ff5630;
}
.profitLine {
  stroke: #00875a;
}
.trend circle {
  stroke: white;
  stroke-width: 1.5;
}
.revenuePoint {
  fill: #0052cc;
}
.costPoint {
  fill: #ff5630;
}
.profitPoint {
  fill: #00875a;
}
.trend text {
  @apply text-[9px];
  fill: #6b778c;
}
.legend {
  @apply -mt-1 flex items-center justify-center gap-4 text-[11px];
  color: var(--ui-text-subtle);
}
.legend span::before {
  @apply mr-1 inline-block h-2 w-2 rounded-full;
  content: '';
}
.blue::before {
  background: #0052cc;
}
.red::before {
  background: #ff5630;
}
.green::before {
  background: #00875a;
}
.donutArea {
  @apply flex h-[210px] items-center justify-center;
}
.donut {
  @apply h-[188px] w-[188px];
  transform: rotate(-90deg);
}
.donutTrack,
.donutArc {
  fill: transparent;
  stroke-width: 5;
}
.donutTrack {
  stroke: #f4f5f7;
}
.donutArc {
  transition: stroke-dashoffset 160ms ease;
}
.donut text {
  @apply text-[2.5px] font-semibold;
  fill: #42526e;
  transform: rotate(90deg);
  transform-origin: center;
}
.donut text:last-child {
  @apply text-[3px] font-bold;
  fill: #172b4d;
}
.categoryLegend {
  @apply flex flex-wrap justify-center gap-x-3 gap-y-1 text-[10px];
  color: var(--ui-text-subtle);
}
.categoryLegend span {
  @apply inline-flex items-center gap-1;
}
.categoryLegend i {
  @apply h-2 w-2 rounded-full;
}
.dot_plc {
  background: #0052cc;
}
.dot_dcs {
  background: #0065ff;
}
.dot_service {
  background: #00875a;
}
.dot_io {
  background: #6554c0;
}
.dot_instrument {
  background: #ff8b00;
}
.dot_other {
  background: #97a0af;
}
</style>
