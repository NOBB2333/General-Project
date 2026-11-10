<script setup lang="ts">
import { Flame, Gift, Target, TrendingUp } from 'lucide-vue-next';
import type {
  MyPerformanceDetail,
  MyPerformanceMetric,
  MyPerformanceReward,
  MyPerformanceSummary,
} from '../../model/my-performance.types';

defineProps<{
  metrics: readonly MyPerformanceMetric[];
  rewards: readonly MyPerformanceReward[];
  summaries: readonly MyPerformanceSummary[];
}>();

const emit = defineEmits<{
  inspect: [detail: MyPerformanceDetail];
}>();

function inspectMetric(metric: MyPerformanceMetric) {
  emit('inspect', {
    title: metric.label,
    description: '当前绩效周期的奖金核算明细',
    facts: metric.detail,
  });
}

function inspectReward(reward: MyPerformanceReward) {
  emit('inspect', {
    title: reward.label,
    description: '本期已命中的专项奖励',
    facts: [`奖励金额：${reward.amount}`, `核算公式：${reward.formula}`],
  });
}

function inspectSummary(summary: MyPerformanceSummary) {
  emit('inspect', {
    title: summary.label,
    description: '当前考核周期业绩摘要',
    facts: [`${summary.label}：${summary.value}`, '统计口径：已确认并计入本期绩效'],
  });
}

function inspectProgress() {
  emit('inspect', {
    title: '本期目标完成率',
    description: '本期目标与确认业绩进度',
    facts: ['目标：¥1,200,000', '实绩：¥1,296,000', '完成率：108%', '冲刺线：120%'],
  });
}
</script>

<template>
  <section
    :class="$style.overview"
    data-my-performance-overview
  >
    <div
      :class="$style.metricGrid"
      data-performance-metrics
    >
      <button
        v-for="metric in metrics"
        :key="metric.id"
        type="button"
        :class="[$style.metricCard, $style[metric.tone]]"
        :data-metric-id="metric.id"
        @click="inspectMetric(metric)"
      >
        <span :class="$style.metricLabel">{{ metric.label }}</span>
        <strong>{{ metric.value }}</strong>
        <small>{{ metric.hint }}</small>
      </button>
    </div>

    <div :class="$style.progressRow">
      <button
        type="button"
        :class="$style.progressPanel"
        data-performance-progress
        @click="inspectProgress"
      >
        <span :class="$style.progressHeading">
          <span>
            <Target
              :size="16"
              :stroke-width="1.8"
              aria-hidden="true"
            />
            <strong>本期目标完成率</strong>
          </span>
          <b>108%</b>
        </span>
        <span :class="$style.progressMeta">目标 ¥1,200,000 · 实绩 ¥1,296,000</span>
        <span
          :class="$style.progressTrack"
          aria-hidden="true"
        >
          <span :class="$style.progressFill" />
          <span :class="$style.goalMarker" />
        </span>
        <span :class="$style.progressScale">
          <span>0%</span>
          <span>100% 达标</span>
          <span>120% 冲刺</span>
        </span>
      </button>

      <aside :class="$style.sprintPanel">
        <h2>
          <Flame
            :size="16"
            :stroke-width="1.8"
            aria-hidden="true"
          />
          冲刺提示
        </h2>
        <p>
          距离冲刺奖（完成率 120%）还差 <strong>¥144,000</strong>。再签 1 个新客户单可额外 +¥2,000，签 1 笔 ≥50
          万大单额外 +1% 提成。
        </p>
      </aside>
    </div>

    <section :class="$style.rewardsPanel">
      <h2>
        <Gift
          :size="16"
          :stroke-width="1.8"
          aria-hidden="true"
        />
        本期命中的奖励
      </h2>
      <div :class="$style.rewardGrid">
        <button
          v-for="reward in rewards"
          :key="reward.id"
          type="button"
          :class="$style.rewardCard"
          :data-reward-id="reward.id"
          @click="inspectReward(reward)"
        >
          <span :class="$style.rewardHeading">
            <strong>{{ reward.label }}</strong>
            <b>{{ reward.amount }}</b>
          </span>
          <small>{{ reward.formula }}</small>
        </button>
      </div>
    </section>

    <section :class="$style.summaryPanel">
      <h2>
        <TrendingUp
          :size="16"
          :stroke-width="1.8"
          aria-hidden="true"
        />
        业绩摘要
      </h2>
      <div :class="$style.summaryGrid">
        <button
          v-for="summary in summaries"
          :key="summary.id"
          type="button"
          :class="[$style.summaryItem, $style[summary.tone]]"
          @click="inspectSummary(summary)"
        >
          <span>{{ summary.label }}</span>
          <strong>{{ summary.value }}</strong>
        </button>
      </div>
    </section>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.overview {
  @apply grid gap-3.5 pt-3.5;
}

.metricGrid {
  @apply grid grid-cols-4 gap-3.5;
}

.metricCard {
  @apply flex min-h-[102px] min-w-0 flex-col items-start rounded border bg-white px-4 py-3.5 text-left outline-none transition-colors;
  border-color: var(--ui-border);
}

.metricCard:hover,
.progressPanel:hover,
.rewardCard:hover,
.summaryItem:hover {
  border-color: var(--ui-border-strong);
  background: var(--ui-surface-subtle);
}

.metricCard:focus-visible,
.progressPanel:focus-visible,
.rewardCard:focus-visible,
.summaryItem:focus-visible {
  box-shadow: 0 0 0 2px var(--ui-focus);
}

.metricLabel {
  @apply text-[12px];
  color: var(--ui-text-subtle);
}

.metricCard > strong {
  @apply mt-2 text-[23px] font-bold leading-7;
  color: var(--ui-text);
}

.metricCard > small {
  @apply mt-1 text-[11px] leading-4;
  color: var(--ui-text-subtle);
}

.metricCard.primary > strong {
  color: var(--app-primary);
}

.metricCard.success > strong,
.summaryItem.success strong {
  color: var(--ui-success);
}

.metricCard.warning > strong,
.summaryItem.warning strong {
  color: var(--ui-warning);
}

.progressRow {
  @apply grid gap-3.5;
  grid-template-columns: minmax(0, 2fr) minmax(330px, 1fr);
}

.progressPanel,
.sprintPanel,
.rewardsPanel,
.summaryPanel {
  @apply rounded border bg-white;
  border-color: var(--ui-border);
}

.progressPanel {
  @apply min-h-[120px] px-4 py-4 text-left outline-none transition-colors;
}

.progressHeading {
  @apply flex items-center justify-between;
}

.progressHeading > span {
  @apply flex items-center gap-2 text-[13px];
  color: var(--ui-text);
}

.progressHeading > span svg {
  color: var(--app-primary);
}

.progressHeading b {
  @apply text-[23px] font-bold leading-7;
  color: var(--ui-success);
}

.progressMeta {
  @apply mt-0.5 block text-[11px];
  color: var(--ui-text-subtle);
}

.progressTrack {
  @apply relative mt-3 block h-2 overflow-hidden rounded-full;
  background: var(--ui-border-subtle);
}

.progressFill {
  @apply absolute inset-y-0 left-0 w-full rounded-full;
  background: var(--app-primary);
}

.goalMarker {
  @apply absolute inset-y-0 w-px;
  left: 83.333%;
  background: var(--ui-text-muted);
}

.progressScale {
  @apply mt-1.5 grid grid-cols-3 text-[10px];
  color: var(--ui-text-subtle);
}

.progressScale span:nth-child(2) {
  @apply text-center;
}

.progressScale span:last-child {
  @apply text-right;
}

.sprintPanel {
  @apply min-h-[120px] px-4 py-4;
  border-color: var(--ui-warning);
  background: var(--ui-warning-soft);
}

.sprintPanel h2,
.rewardsPanel h2,
.summaryPanel h2 {
  @apply flex items-center gap-2 text-[13px] font-semibold;
  color: var(--ui-text);
}

.sprintPanel h2 svg {
  color: var(--ui-warning);
}

.sprintPanel p {
  @apply mt-2 text-[11px] leading-5;
  color: var(--ui-text-muted);
}

.sprintPanel p strong {
  color: var(--ui-danger);
}

.rewardsPanel {
  @apply min-h-[147px] px-4 py-4;
}

.rewardsPanel h2 svg {
  color: var(--ui-success);
}

.rewardGrid {
  @apply mt-3 grid grid-cols-4 gap-2.5;
}

.rewardCard {
  @apply min-h-[79px] rounded border border-l-2 bg-white px-3 py-3 text-left outline-none transition-colors;
  border-color: var(--ui-border);
  border-left-color: var(--ui-success);
}

.rewardHeading {
  @apply flex items-start justify-between gap-2 text-[12px];
}

.rewardHeading strong {
  @apply font-semibold;
  color: var(--ui-text);
}

.rewardHeading b {
  @apply whitespace-nowrap font-bold;
  color: var(--ui-success);
}

.rewardCard small {
  @apply mt-1 block text-[10px] leading-4;
  color: var(--ui-text-subtle);
}

.summaryPanel {
  @apply min-h-[115px] px-4 py-4;
}

.summaryPanel h2 svg {
  color: var(--app-primary);
}

.summaryGrid {
  @apply mt-3 grid grid-cols-4;
}

.summaryItem {
  @apply flex min-h-[48px] flex-col items-start border border-transparent px-0 text-left outline-none transition-colors;
}

.summaryItem span {
  @apply text-[11px];
  color: var(--ui-text-subtle);
}

.summaryItem strong {
  @apply mt-1 text-[18px] font-bold leading-6;
  color: var(--ui-text);
}

@media (max-width: 1100px) {
  .metricGrid,
  .rewardGrid {
    @apply grid-cols-2;
  }

  .progressRow {
    @apply grid-cols-1;
  }
}

@media (prefers-reduced-motion: reduce) {
  .metricCard,
  .progressPanel,
  .rewardCard,
  .summaryItem {
    transition: none;
  }
}
</style>
