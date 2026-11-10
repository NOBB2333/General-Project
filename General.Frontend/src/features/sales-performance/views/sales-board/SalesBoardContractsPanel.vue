<script setup lang="ts">
import { ChartNoAxesCombined, WalletCards } from 'lucide-vue-next';
import { salesBoardContractMetrics, salesBoardContractRows } from '../../model/sales-board.data';
import type { SalesBoardContractRow, SalesBoardMetric } from '../../model/sales-board.types';
import SalesBoardMetrics from './SalesBoardMetrics.vue';

const emit = defineEmits<{
  inspect: [title: string, description: string, facts: string[]];
}>();

const trendMonths = ['10月', '11月', '12月', '1月', '2月', '3月'] as const;

function inspectMetric(metric: SalesBoardMetric) {
  emit('inspect', metric.label, metric.detail, [
    `当前数值：${metric.value}${metric.unit ?? ''}`,
    `较上期：${metric.trend >= 0 ? '增长' : '下降'} ${Math.abs(metric.trend)}%`,
  ]);
}

function inspectContract(row: SalesBoardContractRow) {
  emit('inspect', row.code, row.name, [
    `客户：${row.customer}`,
    `负责人：${row.owner}`,
    `合同金额：${row.amount}`,
    `未回金额：${row.outstanding}`,
  ]);
}
</script>

<template>
  <section
    :class="$style.section"
    data-board-section="contracts"
  >
    <SalesBoardMetrics
      :metrics="salesBoardContractMetrics"
      @select="inspectMetric"
    />

    <div :class="$style.analysisGrid">
      <article :class="$style.panel">
        <header :class="$style.panelHeader">
          <WalletCards
            :size="18"
            :stroke-width="1.8"
          />
          <div>
            <h2>应收账龄分析</h2>
            <p>应收总额 ¥2,960,000.00</p>
          </div>
        </header>
        <div :class="$style.agingChart">
          <div data-aging="current"><span>0-30 天</span><i /></div>
          <div data-aging="month-two"><span>31-60 天</span><i /></div>
          <div data-aging="month-three"><span>61-90 天</span><i /></div>
          <div data-aging="half-year"><span>91-180 天</span><i /></div>
          <div data-aging="older"><span>180+ 天</span><i /></div>
        </div>
        <dl :class="$style.agingSummary">
          <div>
            <dt>逾期应收（90+ 天）</dt>
            <dd>¥400,000.00</dd>
          </div>
          <div>
            <dt>本期回款</dt>
            <dd>¥510,000.00</dd>
          </div>
        </dl>
      </article>

      <article :class="$style.panel">
        <header :class="$style.panelHeader">
          <ChartNoAxesCombined
            :size="18"
            :stroke-width="1.8"
          />
          <div>
            <h2>回款趋势 vs 目标</h2>
            <p>过去 6 个月</p>
          </div>
        </header>
        <div :class="$style.trendChart">
          <div
            v-for="(month, index) in trendMonths"
            :key="month"
            :data-month-index="index"
          >
            <i />
            <span>{{ month }}</span>
          </div>
          <svg
            viewBox="0 0 600 180"
            preserveAspectRatio="none"
            aria-hidden="true"
          >
            <polyline points="30,115 135,95 240,78 345,92 450,58 565,40" />
          </svg>
        </div>
        <div :class="$style.legend"><span>实际回款</span><span>月度目标</span></div>
      </article>
    </div>

    <article :class="$style.contractPanel">
      <header>
        <div>
          <h2>重点合同关注</h2>
          <p>即将进入交付节点 / 大额未回款</p>
        </div>
        <span>全部合同</span>
      </header>
      <div :class="$style.contractGrid">
        <button
          v-for="row in salesBoardContractRows"
          :key="row.id"
          type="button"
          @click="inspectContract(row)"
        >
          <span :class="$style.contractTopline"
            ><strong>{{ row.code }}</strong
            ><i>{{ row.status }}</i></span
          >
          <b>{{ row.customer }} - {{ row.name }}</b>
          <small>{{ row.customer }} · {{ row.owner }}</small>
          <span :class="$style.amounts"
            ><em>合同 {{ row.amount }}</em
            ><em>未回 {{ row.outstanding }}</em></span
          >
        </button>
      </div>
    </article>

    <aside :class="$style.insight">
      <strong>一句话洞察</strong>
      <p>90 天以上逾期应收 ¥400,000.00，占应收总额 13.5%。本月回款 ¥510,000.00 已超目标 2%，势头良好。</p>
    </aside>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.section {
  @apply min-w-0 pb-8;
}

.analysisGrid {
  @apply mt-4 grid min-w-0 grid-cols-2 gap-4;
}

.panel,
.contractPanel {
  @apply min-w-0 rounded border;
  border-color: var(--ui-border);
  background: var(--ui-surface);
}

.panel {
  @apply min-h-[340px] p-5;
}

.panelHeader {
  @apply flex items-center gap-2;
  color: #ff8b00;
}

.panelHeader h2,
.contractPanel h2 {
  @apply text-[14px] font-semibold;
  color: var(--ui-text);
}

.panelHeader p,
.contractPanel header p {
  @apply mt-0.5 text-[10px];
  color: var(--ui-text-subtle);
}

.agingChart {
  @apply mt-6 grid gap-3;
}

.agingChart div {
  @apply grid grid-cols-[72px_1fr] items-center gap-2;
}

.agingChart span {
  @apply text-[10px] font-medium;
}

.agingChart i {
  @apply block h-7 rounded-r;
  width: 84%;
  background: #36b37e;
}

.agingChart div[data-aging='month-two'] i {
  width: 58%;
  background: #00b8d9;
}

.agingChart div[data-aging='month-three'] i {
  width: 28%;
  background: #ffab00;
}

.agingChart div[data-aging='half-year'] i {
  width: 19%;
  background: #ff8b00;
}

.agingChart div[data-aging='older'] i {
  width: 8%;
  background: #ff5630;
}

.agingSummary {
  @apply mt-5 grid grid-cols-2 gap-4 border-t pt-4;
  border-color: var(--ui-border-subtle);
}

.agingSummary dt {
  @apply text-[9px];
  color: var(--ui-text-subtle);
}

.agingSummary dd {
  @apply mt-1 text-[14px] font-semibold;
  color: #ff5630;
}

.agingSummary div:last-child dd {
  color: #36b37e;
}

.trendChart {
  @apply relative mt-6 flex h-[220px] items-end justify-around border-b;
  border-color: var(--ui-border-subtle);
  background-image: repeating-linear-gradient(to top, transparent 0, transparent 53px, var(--ui-border-subtle) 54px);
}

.trendChart > div {
  @apply z-[1] flex h-full w-[12%] flex-col items-center justify-end gap-2;
}

.trendChart i {
  @apply block w-6 rounded-t;
  height: 38%;
  background: #36b37e;
}

.trendChart div[data-month-index='1'] i {
  height: 52%;
}

.trendChart div[data-month-index='2'] i {
  height: 66%;
}

.trendChart div[data-month-index='3'] i {
  height: 62%;
}

.trendChart div[data-month-index='4'] i {
  height: 75%;
}

.trendChart div[data-month-index='5'] i {
  height: 92%;
}

.trendChart span {
  @apply text-[9px];
  color: var(--ui-text-subtle);
}

.trendChart svg {
  @apply pointer-events-none absolute inset-x-0 top-2 h-[180px] w-full;
}

.trendChart polyline {
  fill: none;
  stroke: #ff8b00;
  stroke-width: 3;
  stroke-dasharray: 7 5;
}

.legend {
  @apply mt-3 flex justify-center gap-5 text-[9px];
}

.legend span:first-child {
  color: #36b37e;
}

.legend span:last-child {
  color: #ff8b00;
}

.contractPanel {
  @apply mt-4 p-5;
}

.contractPanel > header {
  @apply flex items-center justify-between;
}

.contractPanel > header > span {
  @apply text-[10px] font-semibold;
  color: var(--app-primary);
}

.contractGrid {
  @apply mt-4 grid grid-cols-4 gap-3;
}

.contractGrid button {
  @apply flex min-w-0 flex-col rounded border p-3 text-left outline-none transition-colors;
  border-color: var(--ui-border-subtle);
}

.contractGrid button:hover {
  border-color: #b3d4ff;
  background: var(--app-primary-soft);
}

.contractGrid button:focus-visible {
  box-shadow: 0 0 0 2px var(--ui-focus);
}

.contractTopline,
.amounts {
  @apply flex items-center justify-between gap-2;
}

.contractTopline strong {
  @apply text-[10px];
  color: var(--app-primary);
}

.contractTopline i {
  @apply rounded px-1.5 py-0.5 text-[8px] not-italic;
  color: #00875a;
  background: #e3fcef;
}

.contractGrid b {
  @apply mt-2 truncate text-[10px] font-semibold;
}

.contractGrid small {
  @apply mt-1 truncate text-[9px];
  color: var(--ui-text-subtle);
}

.amounts {
  @apply mt-3 border-t pt-2;
  border-color: var(--ui-border-subtle);
}

.amounts em {
  @apply text-[8px] not-italic;
  color: var(--ui-text-subtle);
}

.insight {
  @apply mt-4 rounded border-l-2 px-4 py-3 text-[11px];
  border-color: #6554c0;
  background: #f7f5ff;
}

.insight strong {
  color: #6554c0;
}

.insight p {
  @apply mt-1 leading-5;
}

@media (max-width: 1180px) {
  .analysisGrid,
  .contractGrid {
    grid-template-columns: minmax(0, 1fr);
  }
}
</style>
