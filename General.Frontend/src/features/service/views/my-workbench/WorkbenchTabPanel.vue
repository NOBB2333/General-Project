<script setup lang="ts">
import { BarChart3, ClipboardList, ReceiptText, Wrench } from 'lucide-vue-next';
import type { WorkbenchTabId } from '../../model/my-workbench.types';

defineProps<{
  tab: Exclude<WorkbenchTabId, 'today'>;
}>();

const trendMonths = ['03', '04', '05', '06', '07', '08'] as const;
</script>

<template>
  <section
    v-if="tab === 'orders'"
    :class="$style.ordersPanel"
    data-workbench-panel="orders"
  >
    <div :class="$style.summaryGrid">
      <article><span>工单总数</span><strong :class="$style.primaryText">0</strong></article>
      <article><span>已完成</span><strong :class="$style.successText">0</strong></article>
      <article><span>进行中</span><strong :class="$style.warningText">0</strong></article>
      <article><span>客户数</span><strong :class="$style.purpleText">0</strong></article>
      <article><span>服务费用合计</span><strong :class="$style.goldText">—</strong></article>
    </div>
    <div :class="$style.tableFrame">
      <div :class="$style.tableHeader">
        <span>工单号</span><span>标题</span><span>客户 / 联系人</span><span>状态</span><span>创建时间</span>
      </div>
      <div :class="$style.emptyState">
        <ClipboardList
          :size="30"
          :stroke-width="1.8"
        />
        <strong>当前筛选下无工单</strong>
        <span>我的服务工单会在这里统一展示</span>
      </div>
    </div>
  </section>

  <section
    v-else-if="tab === 'performance'"
    :class="$style.performancePanel"
    data-workbench-panel="performance"
  >
    <div :class="$style.performanceMetrics">
      <article><span>本月完成工单</span><strong :class="$style.successText">0</strong><small>上月 0</small></article>
      <article>
        <span>累计服务工单</span><strong :class="$style.primaryText">0</strong><small>服务 0 家客户</small>
      </article>
      <article>
        <span>平均响应时长</span><strong :class="$style.purpleText">0 分</strong><small>上月 0 分</small>
      </article>
      <article><span>客户评分</span><strong :class="$style.goldText">—</strong><small>完成率 0%</small></article>
    </div>
    <article :class="$style.trendCard">
      <header>
        <BarChart3
          :size="14"
          :stroke-width="1.8"
        />
        <strong>近 6 月趋势</strong>
      </header>
      <div :class="$style.trendGrid">
        <div
          v-for="month in trendMonths"
          :key="month"
        >
          <span :class="$style.zeroBar" />
          <small>{{ month }}</small>
          <b>0</b>
        </div>
      </div>
    </article>
  </section>

  <section
    v-else
    :class="$style.emptyTab"
    :data-workbench-panel="tab"
  >
    <Wrench
      v-if="tab === 'parts'"
      :size="36"
      :stroke-width="1.8"
    />
    <ReceiptText
      v-else
      :size="36"
      :stroke-width="1.8"
    />
    <strong>{{ tab === 'parts' ? '尚未发起过备件申请' : '尚未创建过费用结算单' }}</strong>
    <p v-if="tab === 'parts'">在工单详情页「备件申请」Tab 可以发起，本页面会展示所有由我发起的申请单与流转状态。</p>
    <p v-else>在工单详情页「费用结算」Tab 可以创建，本页面会展示所有由我创建的费用单与审批流转。</p>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.ordersPanel,
.performancePanel {
  @apply space-y-4;
}

.summaryGrid {
  @apply grid grid-cols-5 gap-2.5;
}

.summaryGrid article,
.performanceMetrics article {
  @apply rounded-lg border bg-white p-3;
  border-color: var(--ui-border);
}

.summaryGrid span,
.performanceMetrics span {
  @apply block text-[11.5px];
  color: var(--ui-text-subtle);
}

.summaryGrid strong,
.performanceMetrics strong {
  @apply mt-1 block text-[22px] font-bold;
}

.primaryText {
  color: #0052cc;
}

.successText {
  color: #00875a;
}

.warningText {
  color: #ff8b00;
}

.purpleText {
  color: #6554c0;
}

.goldText {
  color: #ffab00;
}

.tableFrame,
.trendCard {
  @apply overflow-hidden rounded-lg border bg-white;
  border-color: var(--ui-border);
}

.tableHeader {
  @apply grid grid-cols-5 gap-3 border-b bg-[#F4F5F7] px-3 py-2.5 text-[11px] font-semibold;
  color: #5e6c84;
  border-color: var(--ui-border);
}

.emptyState,
.emptyTab {
  @apply flex flex-col items-center justify-center py-16 text-center;
  color: #97a0af;
}

.emptyState strong,
.emptyTab strong {
  @apply mt-3 text-[14px] font-semibold;
  color: var(--ui-text-muted);
}

.emptyState span,
.emptyTab p {
  @apply mt-1 text-[12px];
}

.performanceMetrics {
  @apply grid grid-cols-4 gap-3;
}

.performanceMetrics small {
  @apply mt-1 block text-[11px];
  color: #97a0af;
}

.trendCard {
  @apply bg-[#FAFBFC] p-4;
}

.trendCard header {
  @apply mb-3 flex items-center gap-1.5 text-[13px];
  color: var(--ui-text);
}

.trendCard header svg {
  color: var(--app-primary);
}

.trendGrid {
  @apply grid h-[160px] grid-cols-6 items-end gap-3;
}

.trendGrid > div {
  @apply flex flex-col items-center gap-1;
}

.zeroBar {
  @apply h-0.5 w-10 rounded-t bg-[#0052CC];
}

.trendGrid small {
  @apply text-[10.5px];
  color: var(--ui-text-subtle);
}

.trendGrid b {
  @apply text-[11px] font-semibold;
  color: var(--ui-text);
}

@media (max-width: 900px) {
  .summaryGrid,
  .performanceMetrics {
    @apply grid-cols-2;
  }
}
</style>
