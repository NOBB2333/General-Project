<script setup lang="ts">
import { ChartNoAxesColumnIncreasing, ListChecks } from 'lucide-vue-next';
import type { ReceiptMonthlyRow, ReceiptStatusRow } from '../../model/receipts.types';

defineProps<{
  monthlyRows: readonly ReceiptMonthlyRow[];
  statusRows: readonly ReceiptStatusRow[];
}>();

function currency(value: number) {
  return `¥${value.toLocaleString('zh-CN')}`;
}
</script>

<template>
  <section :class="$style.analysisGrid">
    <article :class="$style.chartCard">
      <header :class="$style.panelTitle">
        <div>
          <ChartNoAxesColumnIncreasing
            :size="16"
            :stroke-width="1.8"
          />
          <strong>月度回款 · 计划 vs 实际</strong>
        </div>
        <span>含未来计划 · 单位 元</span>
      </header>
      <div :class="$style.chartArea">
        <div :class="$style.yAxis">
          <span>¥220K</span>
          <span>¥165K</span>
          <span>¥110K</span>
          <span>¥55K</span>
          <span>¥0</span>
        </div>
        <div :class="$style.plot">
          <span
            :class="$style.gridLine"
            data-line="1"
          />
          <span
            :class="$style.gridLine"
            data-line="2"
          />
          <span
            :class="$style.gridLine"
            data-line="3"
          />
          <span
            :class="$style.gridLine"
            data-line="4"
          />
          <div
            v-for="row in monthlyRows"
            :key="row.id"
            :class="$style.barGroup"
            :data-month="row.month"
          >
            <div :class="$style.barPair">
              <span
                :class="$style.plannedBar"
                :aria-label="`${row.month} 计划 ${currency(row.planned)}`"
              />
              <span
                :class="$style.actualBar"
                :aria-label="`${row.month} 实际 ${currency(row.actual)}`"
              />
            </div>
            <small>{{ row.month }}</small>
          </div>
        </div>
      </div>
    </article>

    <article :class="$style.statusCard">
      <header :class="$style.panelTitle">
        <div>
          <ListChecks
            :size="16"
            :stroke-width="1.8"
          />
          <strong>计划状态分布（按金额）</strong>
        </div>
      </header>
      <div
        :class="$style.statusDonut"
        aria-label="逾期金额占计划总额 94.6%"
      >
        <div :class="$style.donutChart"><strong>94.6%</strong><span>已逾期</span></div>
        <ul>
          <li
            v-for="row in statusRows"
            :key="row.id"
            :data-tone="row.tone"
          >
            <span><i />{{ row.label }}</span>
            <strong>{{ currency(row.amount) }}</strong>
          </li>
        </ul>
      </div>
    </article>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.analysisGrid {
  @apply mt-4 grid grid-cols-[2fr_1fr] gap-4;
}

.chartCard,
.statusCard {
  @apply min-h-[274px] rounded border bg-white;
  border-color: var(--ui-border);
}

.panelTitle {
  @apply flex h-14 items-center justify-between border-b px-4;
  border-color: var(--ui-border-subtle);
}

.panelTitle > div {
  @apply flex items-center gap-2;
  color: var(--app-primary);
}

.panelTitle strong {
  @apply text-[12px] font-semibold;
  color: var(--ui-text);
}

.panelTitle > span {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}

.chartArea {
  @apply grid h-[210px] grid-cols-[54px_1fr] px-4 pb-4 pt-3;
}

.yAxis {
  @apply flex flex-col justify-between pb-[25px] text-right text-[9px];
  color: var(--ui-text-subtle);
}

.plot {
  @apply relative grid grid-cols-7 gap-2 border-b border-l pl-2;
  border-color: var(--ui-border);
}

.gridLine {
  @apply absolute left-0 right-0 border-t border-dashed;
  border-color: var(--ui-border-subtle);
}

.gridLine[data-line='1'] {
  top: 0;
}

.gridLine[data-line='2'] {
  top: 25%;
}

.gridLine[data-line='3'] {
  top: 50%;
}

.gridLine[data-line='4'] {
  top: 75%;
}

.barGroup {
  @apply relative z-10 flex min-w-0 flex-col justify-end;
}

.barPair {
  @apply flex h-[160px] items-end justify-center gap-0.5;
}

.plannedBar,
.actualBar {
  @apply block w-[40%] rounded-t-sm;
}

.plannedBar {
  background: #a8c9f7;
}

.actualBar {
  @apply h-0.5;
  background: var(--ui-success);
}

.barGroup[data-month='2026-01'] .plannedBar {
  height: 47%;
}

.barGroup[data-month='2026-02'] .plannedBar,
.barGroup[data-month='2026-06'] .plannedBar {
  height: 28%;
}

.barGroup[data-month='2026-03'] .plannedBar,
.barGroup[data-month='2026-05'] .plannedBar {
  height: 64%;
}

.barGroup[data-month='2026-04'] .plannedBar {
  height: 89%;
}

.barGroup[data-month='2026-07'] .plannedBar {
  height: 76%;
}

.barGroup small {
  @apply mt-2 truncate text-center text-[8px];
  color: var(--ui-text-subtle);
}

.statusDonut {
  @apply grid grid-cols-[130px_1fr] items-center gap-3 p-5;
}

.donutChart {
  @apply flex h-[118px] w-[118px] flex-col items-center justify-center rounded-full;
  background:
    radial-gradient(circle, white 0 47%, transparent 48%),
    conic-gradient(var(--ui-danger) 0 94.6%, var(--ui-border) 94.6% 100%);
}

.donutChart strong {
  @apply text-[18px] font-semibold;
  color: var(--ui-danger);
}

.donutChart span {
  @apply mt-1 text-[9px];
  color: var(--ui-text-subtle);
}

.statusDonut ul {
  @apply m-0 list-none space-y-3 p-0;
}

.statusDonut li {
  @apply flex items-center justify-between gap-2 text-[9px];
  color: var(--ui-text-subtle);
}

.statusDonut li > span {
  @apply flex items-center gap-1.5;
}

.statusDonut li i {
  @apply block h-2 w-2;
  background: var(--ui-border);
}

.statusDonut li[data-tone='success'] i {
  background: var(--ui-success);
}

.statusDonut li[data-tone='danger'] i {
  background: var(--ui-danger);
}

.statusDonut li[data-tone='warning'] i {
  background: var(--ui-warning);
}

.statusDonut li strong {
  @apply font-semibold;
  color: var(--ui-text-muted);
}
</style>
