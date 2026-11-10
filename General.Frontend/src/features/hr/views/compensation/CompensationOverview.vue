<script setup lang="ts">
import { ChartNoAxesCombined, Gift, ShieldCheck, WalletCards } from 'lucide-vue-next';
import { deductions, salaryComposition } from '../../model/compensation.data';
const props = defineProps<{ hidden: boolean }>();
const amount = (value: string) => (props.hidden ? '••••••' : value);
</script>

<template>
  <section :class="$style.metrics">
    <article data-tone="primary">
      <span>本月实发工资</span
      ><WalletCards
        :size="18"
        :stroke-width="1.8"
      /><strong>{{ amount('¥19,834.00') }}</strong>
    </article>
    <article data-tone="success">
      <span>月均实发(近12月)</span
      ><ChartNoAxesCombined
        :size="18"
        :stroke-width="1.8"
      /><strong>{{ amount('¥20,305.50') }}</strong>
    </article>
    <article data-tone="purple">
      <span>年度社保公积金</span
      ><ShieldCheck
        :size="18"
        :stroke-width="1.8"
      /><strong>{{ amount('¥6.0万') }}</strong>
    </article>
    <article data-tone="warning">
      <span>福利年度价值</span
      ><Gift
        :size="18"
        :stroke-width="1.8"
      /><strong>{{ amount('¥8.2万') }}</strong>
    </article>
  </section>
  <section :class="$style.analysis">
    <article :class="$style.composition">
      <h2>本月薪资构成</h2>
      <div :class="$style.donut">
        <span>{{ amount('¥27,700') }}</span>
      </div>
      <ul>
        <li
          v-for="item in salaryComposition"
          :key="item.id"
        >
          <span>{{ item.label }}</span
          ><strong>{{ amount(item.value) }}</strong>
        </li>
      </ul>
    </article>
    <article :class="$style.trend">
      <h2>近12月收入趋势</h2>
      <div :class="$style.chart">
        <svg
          viewBox="0 0 720 190"
          preserveAspectRatio="none"
          aria-label="近12月税前与实发收入趋势"
        >
          <g>
            <line
              x1="40"
              y1="20"
              x2="710"
              y2="20"
            />
            <line
              x1="40"
              y1="65"
              x2="710"
              y2="65"
            />
            <line
              x1="40"
              y1="110"
              x2="710"
              y2="110"
            />
            <line
              x1="40"
              y1="155"
              x2="710"
              y2="155"
            />
          </g>
          <polyline points="40,91 100,88 160,89 220,91 280,87 340,90 400,85 460,88 520,84 580,89 640,86 700,91" />
          <polyline
            data-net
            points="40,111 100,108 160,109 220,111 280,107 340,109 400,104 460,108 520,104 580,109 640,106 700,111"
          />
        </svg>
        <div :class="$style.months">
          <span
            v-for="month in ['03', '04', '05', '06', '07', '08', '09', '10', '11', '12', '01', '02']"
            :key="month"
            >{{ month }}</span
          >
        </div>
        <p><i />税前 <i />实发</p>
      </div>
    </article>
  </section>
  <section :class="$style.deductions">
    <h2>本月扣除项明细</h2>
    <div>
      <article
        v-for="item in deductions"
        :key="item.id"
      >
        <span>{{ item.label }}</span
        ><strong>{{ amount(item.value) }}</strong
        ><small>{{ item.note }}</small>
      </article>
    </div>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";
.metrics {
  @apply mt-4 grid grid-cols-4 gap-4;
}
.metrics article {
  @apply relative h-[94px] rounded border bg-white p-4;
  border-color: var(--ui-border);
}
.metrics span {
  @apply text-[10px];
  color: var(--ui-text-muted);
}
.metrics svg {
  @apply absolute right-4 top-4 rounded p-1.5;
  box-sizing: content-box;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.metrics strong {
  @apply mt-5 block text-[20px];
  color: var(--app-primary);
}
.metrics article[data-tone='success'] svg,
.metrics article[data-tone='success'] strong {
  color: var(--ui-success);
}
.metrics article[data-tone='purple'] svg,
.metrics article[data-tone='purple'] strong {
  color: #6554c0;
}
.metrics article[data-tone='warning'] svg,
.metrics article[data-tone='warning'] strong {
  color: var(--ui-warning);
}
.analysis {
  @apply mt-4 grid grid-cols-[2fr_3fr] gap-4;
}
.analysis > article,
.deductions {
  @apply rounded border bg-white p-5;
  border-color: var(--ui-border);
}
.analysis h2,
.deductions h2 {
  @apply m-0 text-[13px] font-semibold;
}
.composition {
  @apply grid grid-cols-[150px_1fr] gap-x-6;
}
.composition h2 {
  @apply col-span-2;
}
.donut {
  @apply relative mt-5 flex h-[132px] w-[132px] items-center justify-center rounded-full;
  background: conic-gradient(#0052cc 0 65%, #00875a 65% 76%, #ff8b00 76% 92%, #6554c0 92% 96%, #00b8d9 96%);
}
.donut::before {
  content: '';
  @apply absolute h-[80px] w-[80px] rounded-full bg-white;
}
.donut span {
  @apply relative z-10 text-[11px] font-semibold;
}
.composition ul {
  @apply m-0 mt-5 list-none space-y-2 p-0;
}
.composition li {
  @apply flex justify-between text-[10px];
  color: var(--ui-text-muted);
}
.composition li strong {
  color: var(--ui-text);
}
.trend h2 {
  @apply mb-3;
}
.chart {
  @apply h-[180px];
}
.chart svg {
  @apply h-[145px] w-full overflow-visible;
}
.chart line {
  stroke: #ebecf0;
  stroke-dasharray: 3 4;
}
.chart polyline {
  fill: none;
  stroke: #0052cc;
  stroke-width: 2;
}
.chart polyline[data-net] {
  stroke: #00875a;
}
.months {
  @apply grid grid-cols-12 text-center text-[9px];
  color: var(--ui-text-subtle);
}
.chart p {
  @apply m-0 mt-2 flex justify-center gap-2 text-[9px];
  color: var(--ui-text-muted);
}
.chart i {
  @apply mt-1 h-2 w-2 rounded-full;
  background: var(--app-primary);
}
.chart i:last-of-type {
  background: var(--ui-success);
}
.deductions {
  @apply mt-4;
}
.deductions > div {
  @apply mt-4 grid grid-cols-4 gap-3;
}
.deductions article {
  @apply rounded border p-3;
  border-color: var(--ui-border-subtle);
  background: var(--ui-surface-subtle);
}
.deductions span,
.deductions small {
  @apply block text-[9px];
  color: var(--ui-text-subtle);
}
.deductions strong {
  @apply my-1 block text-[13px];
}
</style>
