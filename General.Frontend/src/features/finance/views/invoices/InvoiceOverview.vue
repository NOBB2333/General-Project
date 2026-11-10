<script setup lang="ts">
import { computed } from 'vue';
import type { InvoiceRange, InvoiceRecord, InvoiceTrendPoint } from '../../model/invoices.types';

const props = defineProps<{
  range: InvoiceRange;
  trend: readonly InvoiceTrendPoint[];
  recentRows: readonly InvoiceRecord[];
}>();

const rangeLabels: Record<InvoiceRange, string> = {
  month: '本月',
  quarter: '本季',
  'half-year': '半年',
  year: '全年',
};

const rangeLabel = computed(() => rangeLabels[props.range]);

function currency(value: number) {
  return `¥${value.toLocaleString('zh-CN')}`;
}
</script>

<template>
  <section :class="$style.dashboard">
    <article :class="$style.trendPanel">
      <header :class="$style.panelHeader">
        <div>
          <h2>月度发票趋势</h2>
          <small>{{ rangeLabel }}统计口径</small>
        </div>
        <div :class="$style.legend">
          <span :class="$style.outboundLegend">销项</span>
          <span :class="$style.inboundLegend">进项</span>
          <span :class="$style.netLegend">净额</span>
        </div>
      </header>
      <div :class="$style.chart">
        <div :class="$style.yLabels">
          <span>105.0万</span>
          <span>70.0万</span>
          <span>35.0万</span>
          <span>0</span>
          <span>-35.0万</span>
        </div>
        <svg
          viewBox="0 0 900 250"
          role="img"
          aria-label="月度销项、进项和净额趋势"
          preserveAspectRatio="none"
        >
          <g :class="$style.gridLines">
            <line
              x1="0"
              y1="15"
              x2="900"
              y2="15"
            />
            <line
              x1="0"
              y1="70"
              x2="900"
              y2="70"
            />
            <line
              x1="0"
              y1="125"
              x2="900"
              y2="125"
            />
            <line
              x1="0"
              y1="180"
              x2="900"
              y2="180"
            />
            <line
              x1="0"
              y1="235"
              x2="900"
              y2="235"
            />
            <line
              v-for="(_, index) in trend"
              :key="index"
              :x1="index * 180"
              y1="15"
              :x2="index * 180"
              y2="235"
            />
          </g>
          <path
            :class="$style.outboundArea"
            d="M0,124 C60,116 120,109 180,112 C250,114 290,141 360,153 L360,180 L0,180 Z"
          />
          <path
            :class="$style.inboundArea"
            d="M0,143 C70,139 120,135 180,137 C250,140 300,145 360,154 L360,180 L0,180 Z"
          />
          <polyline
            :class="$style.outboundLine"
            points="0,124 180,112 360,153"
          />
          <polyline
            :class="$style.inboundLine"
            points="0,143 180,137 360,154"
          />
          <polyline
            :class="$style.netLine"
            points="0,158 180,154 360,181"
          />
        </svg>
        <div :class="$style.xLabels">
          <span
            v-for="point in trend"
            :key="point.month"
            >{{ point.month }}</span
          >
        </div>
      </div>
    </article>

    <aside :class="$style.taxPanel">
      <section>
        <h2>税额构成</h2>
        <dl :class="$style.taxList">
          <div>
            <dt :class="$style.outboundDot">销项税额</dt>
            <dd>¥145,430</dd>
          </div>
          <div>
            <dt :class="$style.inboundDot">进项税额</dt>
            <dd>¥100,106</dd>
          </div>
          <div :class="$style.taxTotal">
            <dt>应纳税额</dt>
            <dd>¥40,887</dd>
          </div>
        </dl>
      </section>
      <section :class="$style.typeSection">
        <h2>发票类型分布</h2>
        <div :class="$style.typeBar">
          <span :class="$style.typeOutboundSpecial" />
          <span :class="$style.typeOutboundNormal" />
          <span :class="$style.typeInboundSpecial" />
        </div>
        <ul :class="$style.typeLegend">
          <li><span :class="$style.outboundDot" />专票(销项) <strong>8张</strong></li>
          <li><span :class="$style.normalDot" />普票(销项) <strong>3张</strong></li>
          <li><span :class="$style.inboundDot" />专票(进项) <strong>10张</strong></li>
        </ul>
      </section>
    </aside>

    <article :class="$style.recentPanel">
      <header :class="$style.recentHeader">
        <h2>最近发票记录</h2>
        <span>共 {{ recentRows.length }} 条展示记录</span>
      </header>
      <div :class="$style.recentRows">
        <div
          v-for="row in recentRows.slice(0, 4)"
          :key="row.id"
          :class="$style.recentRow"
        >
          <span :class="[$style.directionBadge, row.direction === '进项' && $style.directionInbound]">{{
            row.direction
          }}</span>
          <strong>{{ row.party }}</strong>
          <span>{{ row.number }}</span>
          <span>{{ row.date }}</span>
          <strong>{{ currency(row.amount) }}</strong>
        </div>
      </div>
    </article>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.dashboard {
  @apply mt-5 grid grid-cols-[minmax(0,2fr)_minmax(310px,0.76fr)] gap-4;
}

.trendPanel,
.taxPanel,
.recentPanel {
  @apply rounded border bg-white;
  border-color: var(--ui-border);
}

.trendPanel {
  @apply min-h-[370px] p-5;
}

.panelHeader,
.recentHeader {
  @apply flex items-start justify-between gap-4;
}

.panelHeader h2,
.taxPanel h2,
.recentHeader h2 {
  @apply m-0 text-[14px] font-semibold;
  color: var(--ui-text);
}

.panelHeader small,
.recentHeader span {
  @apply mt-1 block text-[10px];
  color: var(--ui-text-subtle);
}

.legend {
  @apply flex items-center gap-4 text-[10px];
  color: var(--ui-text-subtle);
}

.legend span,
.taxList dt,
.typeLegend li {
  @apply flex items-center gap-1.5;
}

.legend span::before,
.taxList dt::before,
.typeLegend li > span {
  @apply block h-2 w-2 rounded-full;
  content: '';
}

.outboundLegend::before,
.outboundDot::before,
.typeLegend .outboundDot {
  background: #0065ff;
}

.inboundLegend::before,
.inboundDot::before,
.typeLegend .inboundDot {
  background: #ff5630;
}

.netLegend::before {
  background: #00875a;
}

.normalDot {
  background: #4c9aff;
}

.chart {
  @apply relative mt-5 pl-14;
}

.chart svg {
  @apply block h-[250px] w-full overflow-visible;
}

.gridLines line {
  stroke: #ebecf0;
  stroke-width: 1;
  stroke-dasharray: 4 4;
}

.outboundArea {
  fill: rgb(0 101 255 / 8%);
}

.inboundArea {
  fill: rgb(255 86 48 / 5%);
}

.outboundLine,
.inboundLine,
.netLine {
  fill: none;
  stroke-width: 2.4;
  stroke-linecap: round;
  stroke-linejoin: round;
}

.outboundLine {
  stroke: #0065ff;
}

.inboundLine {
  stroke: #ff5630;
}

.netLine {
  stroke: #00875a;
}

.yLabels {
  @apply absolute bottom-[6px] left-0 top-0 flex flex-col justify-between text-right text-[9px];
  color: var(--ui-text-subtle);
}

.xLabels {
  @apply flex justify-between pt-1 text-[9px];
  color: var(--ui-text-subtle);
}

.taxPanel {
  @apply min-h-[370px] p-5;
}

.taxList {
  @apply m-0 mt-[115px];
}

.taxList > div {
  @apply flex items-center justify-between py-2 text-[11px];
}

.taxList dt,
.taxList dd {
  @apply m-0;
}

.taxList dd {
  @apply font-semibold;
}

.taxTotal {
  @apply mt-1 border-t font-semibold;
  border-color: var(--ui-border-subtle);
}

.taxTotal dd {
  color: var(--ui-warning);
}

.typeSection {
  @apply mt-3 border-t pt-4;
  border-color: var(--ui-border-subtle);
}

.typeBar {
  @apply mt-4 flex h-2 overflow-hidden rounded-full;
  background: var(--ui-border-subtle);
}

.typeBar span {
  @apply block h-full;
}

.typeOutboundSpecial {
  @apply w-[38%];
  background: #0065ff;
}

.typeOutboundNormal {
  @apply w-[14%];
  background: #4c9aff;
}

.typeInboundSpecial {
  @apply w-[48%];
  background: #ff5630;
}

.typeLegend {
  @apply m-0 mt-3 space-y-1.5 p-0 text-[10px];
  color: var(--ui-text-subtle);
}

.typeLegend li {
  @apply list-none;
}

.typeLegend strong {
  @apply ml-auto;
  color: var(--ui-text);
}

.recentPanel {
  @apply col-span-2 p-5;
}

.recentRows {
  @apply mt-3 divide-y;
  border-color: var(--ui-border-subtle);
}

.recentRow {
  @apply grid min-h-10 grid-cols-[64px_1.2fr_1fr_100px_110px] items-center gap-3 text-[10px];
  color: var(--ui-text-subtle);
}

.recentRow > strong:first-of-type,
.recentRow > strong:last-child {
  color: var(--ui-text);
}

.recentRow > strong:last-child {
  @apply text-right;
}

.directionBadge {
  @apply inline-flex w-fit rounded px-2 py-0.5 font-semibold;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.directionInbound {
  color: #de350b;
  background: #ffebe6;
}
</style>
