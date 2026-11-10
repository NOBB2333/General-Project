<script setup lang="ts">
import { Activity } from 'lucide-vue-next';
import type { HrTrendPoint } from '../../model/hr-workbench.types';
defineProps<{ points: readonly HrTrendPoint[] }>();
</script>

<template>
  <section :class="$style.panel">
    <header>
      <Activity
        :size="17"
        :stroke-width="1.8"
      />
      <h2>人员与成本趋势</h2>
    </header>
    <div :class="$style.legend">
      <span>人员曲线（左轴）</span><i data-tone="blue" />汇总人数<i data-tone="green" />正式员工<i
        data-tone="orange"
      />实习/试用<i data-tone="cyan" />人力成本
    </div>
    <div :class="$style.chart">
      <div :class="$style.gridLines"><i /><i /><i /><i /></div>
      <div :class="$style.costArea" />
      <div :class="$style.totalLine" />
      <div :class="$style.regularLine" />
      <div :class="$style.months">
        <span
          v-for="point in points"
          :key="point.month"
          >{{ point.month }}</span
        >
      </div>
      <div :class="$style.axis">
        <span>¥800k</span><span>¥600k</span><span>¥400k</span><span>¥200k</span><span>¥0k</span>
      </div>
    </div>
    <footer>
      <span><small>当前汇总</small><strong>42人</strong><em>+2 环比</em></span
      ><span><small>正式员工</small><strong>38人</strong><em>+1 环比</em></span
      ><span><small>实习/试用</small><strong>4人</strong><em>+0 环比</em></span
      ><span><small>人力成本</small><strong>¥783k</strong><em>+1.7% 环比</em></span>
    </footer>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";
.panel {
  @apply overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}
.panel > header {
  @apply flex h-[58px] items-center gap-2 border-b px-5;
  border-color: var(--ui-border);
  color: var(--app-primary);
}
.panel h2 {
  @apply m-0 text-[14px];
  color: var(--ui-text);
}
.legend {
  @apply flex h-12 items-center gap-3 px-5 text-[9px];
  color: var(--ui-text-subtle);
}
.legend i {
  @apply h-0.5 w-5;
  background: #0c66e4;
}
.legend i[data-tone='green'] {
  background: #00875a;
}
.legend i[data-tone='orange'] {
  border-top: 2px dashed #ff8b00;
  background: transparent;
}
.legend i[data-tone='cyan'] {
  @apply h-3 border;
  border-color: #00b8d9;
  background: #e6fcff;
}
.chart {
  @apply relative mx-5 h-[300px] border-b border-l;
  border-color: #97a0af;
}
.gridLines {
  @apply absolute inset-0 flex flex-col justify-between;
}
.gridLines i {
  @apply block w-full border-t border-dashed;
  border-color: var(--ui-border-subtle);
}
.costArea {
  @apply absolute bottom-0 left-0 h-[86%] w-[38%];
  background: linear-gradient(180deg, rgb(0 184 217 / 13%), transparent);
  border-top: 2px solid #00b8d9;
}
.totalLine,
.regularLine {
  @apply absolute left-0 top-[58%] h-0.5 w-[38%] rotate-[-2deg];
  background: #0c66e4;
}
.regularLine {
  @apply top-[66%];
  background: #00875a;
}
.months {
  @apply absolute inset-x-0 -bottom-7 flex justify-between text-[9px];
  color: var(--ui-text-subtle);
}
.axis {
  @apply absolute -right-12 inset-y-0 flex flex-col justify-between text-[9px];
  color: var(--ui-text-subtle);
}
.panel footer {
  @apply mt-10 grid grid-cols-4 border-t px-5 py-4 text-center;
  border-color: var(--ui-border-subtle);
}
.panel footer span {
  @apply flex flex-col;
}
.panel footer small {
  @apply text-[9px];
  color: #97a0af;
}
.panel footer strong {
  @apply mt-1 text-[14px];
}
.panel footer em {
  @apply mt-1 text-[8px] not-italic;
  color: #00875a;
}
</style>
