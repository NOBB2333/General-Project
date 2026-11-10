<script setup lang="ts">
import { ChartNoAxesCombined, PieChart } from 'lucide-vue-next';
import {
  salesBoardOpportunityStages,
  salesBoardPipelineMetrics,
  salesBoardPipelineStages,
} from '../../model/sales-board.data';
import type { SalesBoardMetric, SalesBoardPipelineStage } from '../../model/sales-board.types';
import SalesBoardMetrics from './SalesBoardMetrics.vue';

const emit = defineEmits<{
  inspect: [title: string, description: string, facts: string[]];
}>();

function inspectMetric(metric: SalesBoardMetric) {
  emit('inspect', metric.label, metric.detail, [
    `当前数值：${metric.value}${metric.unit ?? ''}`,
    `较上期：${metric.trend >= 0 ? '增长' : '下降'} ${Math.abs(metric.trend)}%`,
  ]);
}

function inspectStage(stage: SalesBoardPipelineStage) {
  emit('inspect', `商机阶段 · ${stage.label}`, '销售转化漏斗阶段详情', [
    `当前数量：${stage.count} 个`,
    ...(stage.amount ? [`阶段金额：${stage.amount}`] : []),
    ...(stage.conversion ? [`阶段转化：${stage.conversion}`] : []),
  ]);
}
</script>

<template>
  <section
    :class="$style.section"
    data-board-section="pipeline"
  >
    <SalesBoardMetrics
      :metrics="salesBoardPipelineMetrics"
      @select="inspectMetric"
    />

    <div :class="$style.primaryGrid">
      <article :class="$style.panel">
        <header :class="$style.panelHeader">
          <ChartNoAxesCombined
            :size="18"
            :stroke-width="1.8"
          />
          <div>
            <h2>销售转化漏斗</h2>
            <p>从线索到回款的端到端转化</p>
          </div>
        </header>
        <div :class="$style.funnelLayout">
          <div :class="$style.funnel">
            <button
              v-for="stage in salesBoardPipelineStages"
              :key="stage.id"
              type="button"
              :data-stage="stage.id"
              @click="inspectStage(stage)"
            >
              <strong>{{ stage.count }}</strong>
              <span>{{ stage.label }}</span>
            </button>
          </div>
          <div :class="$style.stageList">
            <button
              v-for="stage in salesBoardPipelineStages"
              :key="stage.id"
              type="button"
              @click="inspectStage(stage)"
            >
              <span>{{ stage.label }}</span>
              <strong>{{ stage.count }}</strong>
              <small>{{ stage.conversion ?? '已进入回款阶段' }}</small>
            </button>
          </div>
        </div>
      </article>

      <article :class="$style.panel">
        <header :class="$style.panelHeader">
          <PieChart
            :size="18"
            :stroke-width="1.8"
          />
          <h2>商机阶段（按金额）</h2>
        </header>
        <div :class="$style.stageSummary">
          <div
            :class="$style.pie"
            aria-label="商机阶段金额占比图"
          />
          <ul>
            <li
              v-for="stage in salesBoardOpportunityStages"
              :key="stage.id"
            >
              <button
                type="button"
                @click="inspectStage(stage)"
              >
                <span :data-stage-tone="stage.tone" />
                <strong>{{ stage.label }}</strong>
                <small>{{ stage.count }} 个 · {{ stage.amount }}</small>
              </button>
            </li>
          </ul>
        </div>
      </article>
    </div>

    <div :class="$style.secondaryGrid">
      <article :class="$style.compactPanel">
        <h3>商机来源 · 成单分析</h3>
        <div :class="$style.sourceBars">
          <span data-source="referral">客户介绍</span>
          <span data-source="events">市场活动</span>
          <span data-source="bidding">招投标</span>
          <span data-source="outbound">销售陌拜</span>
          <span data-source="online">线上询盘</span>
          <span data-source="partner">代理推荐</span>
        </div>
      </article>
      <article :class="$style.compactPanel">
        <h3>阶段停留时长 · 卡点诊断</h3>
        <dl :class="$style.durationGrid">
          <div>
            <dt>需求确认</dt>
            <dd>平均 8 天 · 3 个超期</dd>
          </div>
          <div>
            <dt>方案沟通</dt>
            <dd>平均 14 天 · 5 个超期</dd>
          </div>
          <div>
            <dt>报价谈判</dt>
            <dd>平均 22 天 · 8 个超期</dd>
          </div>
          <div>
            <dt>合同评审</dt>
            <dd>平均 11 天 · 2 个超期</dd>
          </div>
        </dl>
      </article>
    </div>

    <aside :class="$style.insight">
      <strong>一句话洞察</strong>
      <p>报价谈判阶段平均停留 22 天，是当前管道最大卡点（8 个超期）；客户介绍来源转化率最高，可加大老客户裂变激励。</p>
    </aside>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.section {
  @apply min-w-0 pb-8;
}

.primaryGrid,
.secondaryGrid {
  @apply mt-4 grid min-w-0 grid-cols-[2fr_1fr] gap-4;
}

.panel,
.compactPanel {
  @apply min-w-0 rounded border;
  border-color: var(--ui-border);
  background: var(--ui-surface);
}

.panel {
  @apply min-h-[330px] p-5;
}

.panelHeader {
  @apply flex items-center gap-2;
  color: var(--app-primary);
}

.panelHeader h2 {
  @apply text-[14px] font-semibold;
  color: var(--ui-text);
}

.panelHeader p {
  @apply mt-0.5 text-[10px];
  color: var(--ui-text-subtle);
}

.funnelLayout {
  @apply mt-5 grid grid-cols-[1fr_1.45fr] gap-5;
}

.funnel {
  @apply flex flex-col items-center pt-1;
}

.funnel button {
  @apply flex h-[45px] items-center justify-center gap-3 border-0 text-white outline-none;
  background: #0052cc;
}

.funnel button[data-stage='lead'] {
  width: 100%;
  clip-path: polygon(0 0, 100% 0, 86% 100%, 14% 100%);
}

.funnel button[data-stage='opportunity'] {
  width: 72%;
  background: #0c66e4;
  clip-path: polygon(0 0, 100% 0, 84% 100%, 16% 100%);
}

.funnel button[data-stage='quote'] {
  width: 50%;
  background: #36b37e;
  clip-path: polygon(0 0, 100% 0, 78% 100%, 22% 100%);
}

.funnel button[data-stage='contract'] {
  width: 30%;
  background: #00875a;
  clip-path: polygon(0 0, 100% 0, 67% 100%, 33% 100%);
}

.funnel button[data-stage='collection'] {
  width: 16%;
  background: #006644;
  clip-path: polygon(0 0, 100% 0, 50% 100%);
}

.funnel strong {
  @apply text-[13px];
}

.funnel span {
  @apply text-[10px];
}

.funnel button:focus-visible,
.stageList button:focus-visible,
.stageSummary button:focus-visible {
  box-shadow: 0 0 0 2px var(--ui-focus);
}

.stageList {
  @apply grid gap-2;
}

.stageList button {
  @apply grid grid-cols-[1fr_auto] rounded border px-3 py-2 text-left outline-none;
  border-color: var(--ui-border-subtle);
}

.stageList span,
.stageList strong {
  @apply text-[11px] font-semibold;
}

.stageList small {
  @apply col-span-2 mt-1 text-[9px];
  color: var(--ui-text-subtle);
}

.stageSummary {
  @apply mt-8 grid justify-items-center gap-5;
}

.pie {
  @apply h-[150px] w-[150px] rounded-full;
  background: conic-gradient(#6554c0 0 16%, #0c66e4 16% 37%, #00b8d9 37% 63%, #36b37e 63% 79%, #00875a 79% 100%);
}

.stageSummary ul {
  @apply grid w-full gap-1;
}

.stageSummary button {
  @apply grid w-full grid-cols-[10px_1fr_auto] items-center gap-2 rounded px-1 py-0.5 text-left outline-none;
}

.stageSummary button > span {
  @apply h-2 w-2 rounded-full;
  background: #6554c0;
}

.stageSummary button > span[data-stage-tone='blue'] {
  background: #0c66e4;
}

.stageSummary button > span[data-stage-tone='cyan'] {
  background: #00b8d9;
}

.stageSummary button > span[data-stage-tone='green'] {
  background: #36b37e;
}

.stageSummary strong,
.stageSummary small {
  @apply text-[9px];
}

.stageSummary small {
  color: var(--ui-text-subtle);
}

.secondaryGrid {
  grid-template-columns: repeat(2, minmax(0, 1fr));
}

.compactPanel {
  @apply p-4;
}

.compactPanel h3 {
  @apply text-[12px] font-semibold;
}

.sourceBars {
  @apply mt-4 grid gap-2;
}

.sourceBars span {
  @apply flex h-5 items-center rounded-sm px-2 text-[9px] text-white;
  width: 72%;
  background: #0c66e4;
}

.sourceBars span[data-source='events'] {
  width: 58%;
  background: #36b37e;
}

.sourceBars span[data-source='bidding'] {
  width: 48%;
  background: #ff8b00;
}

.sourceBars span[data-source='outbound'] {
  width: 42%;
  background: #6554c0;
}

.sourceBars span[data-source='online'] {
  width: 33%;
  background: #00b8d9;
}

.sourceBars span[data-source='partner'] {
  width: 26%;
  background: #6b778c;
}

.durationGrid {
  @apply mt-3 grid grid-cols-2 gap-2;
}

.durationGrid div {
  @apply rounded border p-3;
  border-color: var(--ui-border-subtle);
  background: var(--ui-surface-subtle);
}

.durationGrid dt {
  @apply text-[10px] font-semibold;
}

.durationGrid dd {
  @apply mt-1 text-[9px];
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
  .primaryGrid {
    grid-template-columns: minmax(0, 1fr);
  }
}
</style>
