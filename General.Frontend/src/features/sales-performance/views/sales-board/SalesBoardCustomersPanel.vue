<script setup lang="ts">
import { ArrowUpRight, UsersRound } from 'lucide-vue-next';
import { salesBoardCustomerActivities, salesBoardCustomerMetrics } from '../../model/sales-board.data';
import type { SalesBoardMetric } from '../../model/sales-board.types';
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

function inspectActivity(name: string) {
  const row = salesBoardCustomerActivities.find((item) => item.name === name);
  if (!row) return;
  emit('inspect', `${row.name} · 客户活动`, row.team, [
    `新增客户：${row.newCustomers} 家`,
    `客户拜访：${row.visits} 次`,
    `跟进记录：${row.followUps} 条`,
    `转化率：${row.conversion}`,
  ]);
}
</script>

<template>
  <section
    :class="$style.section"
    data-board-section="customers"
  >
    <SalesBoardMetrics
      :metrics="salesBoardCustomerMetrics"
      @select="inspectMetric"
    />

    <article :class="$style.activityPanel">
      <header :class="$style.panelHeader">
        <span :class="$style.panelIcon">
          <UsersRound
            :size="18"
            :stroke-width="1.8"
          />
        </span>
        <div>
          <h2>销售人员·客户活动</h2>
          <p>本期共 28 家新客户 · 63 位新联系人 · 125 次拜访 · 168 条跟进</p>
        </div>
        <button
          type="button"
          :class="$style.linkButton"
          @click="inspectActivity('张经理')"
        >
          跟进记录
          <ArrowUpRight
            :size="14"
            :stroke-width="1.8"
          />
        </button>
      </header>

      <div
        :class="$style.activityChart"
        aria-label="销售人员客户活动柱状图"
      >
        <button
          v-for="row in salesBoardCustomerActivities"
          :key="row.id"
          type="button"
          :data-activity-person="row.id"
          @click="inspectActivity(row.name)"
        >
          <span :class="$style.barGroup">
            <i :class="$style.customerBar" />
            <i :class="$style.contactBar" />
            <i :class="$style.visitBar" />
            <i :class="$style.followBar" />
          </span>
          <strong>{{ row.name }}</strong>
        </button>
      </div>

      <div :class="$style.legend">
        <span><i :class="$style.customerLegend" />新增客户</span>
        <span><i :class="$style.contactLegend" />新增联系人</span>
        <span><i :class="$style.visitLegend" />拜访</span>
        <span><i :class="$style.followLegend" />跟进记录</span>
      </div>

      <div :class="$style.tableViewport">
        <table>
          <thead>
            <tr>
              <th>销售员</th>
              <th>所属团队</th>
              <th>新增客户</th>
              <th>新增联系人</th>
              <th>拜访</th>
              <th>电话沟通</th>
              <th>跟进记录</th>
              <th>负责客户</th>
              <th>转化率</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="row in salesBoardCustomerActivities"
              :key="row.id"
            >
              <td>
                <button
                  type="button"
                  @click="inspectActivity(row.name)"
                >
                  {{ row.name }}
                </button>
              </td>
              <td>{{ row.team }}</td>
              <td>{{ row.newCustomers }}</td>
              <td>{{ row.contacts }}</td>
              <td>{{ row.visits }}</td>
              <td>{{ row.calls }}</td>
              <td>{{ row.followUps }}</td>
              <td>{{ row.customers }}</td>
              <td>{{ row.conversion }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </article>

    <aside :class="$style.insight">
      <strong>一句话洞察</strong>
      <p>
        客户活动「冠军」张经理本期新增 6 家客户、28 次拜访；共有 20 家客户超过 30 天未跟进，建议优先安排 A 级客户回访。
      </p>
    </aside>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.section {
  @apply min-w-0 pb-8;
}

.activityPanel {
  @apply mt-4 overflow-hidden rounded border;
  border-color: var(--ui-border);
  background: var(--ui-surface);
}

.panelHeader {
  @apply flex items-center gap-2 px-5 pb-2 pt-4;
}

.panelIcon {
  @apply flex h-7 w-7 items-center justify-center;
  color: #6554c0;
}

.panelHeader h2 {
  @apply text-[14px] font-semibold;
}

.panelHeader p {
  @apply mt-0.5 text-[11px];
  color: var(--ui-text-subtle);
}

.linkButton {
  @apply ml-auto flex items-center gap-1 text-[11px] font-semibold outline-none;
  color: var(--app-primary);
}

.linkButton:focus-visible,
.activityChart button:focus-visible,
.tableViewport button:focus-visible {
  box-shadow: 0 0 0 2px var(--ui-focus);
}

.activityChart {
  @apply grid h-[205px] grid-cols-8 items-end gap-4 border-b px-8 pt-4;
  border-color: var(--ui-border-subtle);
  background-image: repeating-linear-gradient(to top, transparent 0, transparent 47px, var(--ui-border-subtle) 48px);
}

.activityChart button {
  @apply flex h-full min-w-0 flex-col justify-end gap-2 rounded-sm pb-2 outline-none;
}

.barGroup {
  @apply flex h-[160px] items-end justify-center gap-1;
}

.barGroup i {
  @apply block w-[15%] rounded-t-sm;
  min-width: 6px;
}

.customerBar {
  height: 24%;
  background: #6554c0;
}

.contactBar {
  height: 43%;
  background: #0052cc;
}

.visitBar {
  height: 72%;
  background: #36b37e;
}

.followBar {
  height: 96%;
  background: #ff8b00;
}

.activityChart button:nth-child(2) .barGroup,
.activityChart button:nth-child(3) .barGroup {
  transform: scaleY(0.8);
  transform-origin: bottom;
}

.activityChart button:nth-child(4) .barGroup,
.activityChart button:nth-child(5) .barGroup {
  transform: scaleY(0.62);
  transform-origin: bottom;
}

.activityChart button:nth-child(n + 6) .barGroup {
  transform: scaleY(0.45);
  transform-origin: bottom;
}

.activityChart strong {
  @apply truncate text-[10px] font-medium;
  color: var(--ui-text-subtle);
}

.legend {
  @apply flex justify-center gap-4 py-2 text-[10px];
  color: var(--ui-text-subtle);
}

.legend span {
  @apply flex items-center gap-1;
}

.legend i {
  @apply h-2 w-2 rounded-sm;
}

.customerLegend {
  background: #6554c0;
}

.contactLegend {
  background: #0052cc;
}

.visitLegend {
  background: #36b37e;
}

.followLegend {
  background: #ff8b00;
}

.tableViewport {
  @apply max-h-[220px] overflow-auto border-t;
  border-color: var(--ui-border-subtle);
}

.tableViewport table {
  @apply w-full min-w-[940px] border-collapse text-[10px];
}

.tableViewport th,
.tableViewport td {
  @apply border-b px-3 py-2 text-left;
  border-color: var(--ui-border-subtle);
}

.tableViewport th {
  @apply font-semibold;
  color: var(--ui-text-subtle);
}

.tableViewport td:not(:first-child) {
  @apply text-center;
}

.tableViewport button {
  @apply rounded font-semibold outline-none;
  color: var(--ui-text);
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
  color: var(--ui-text);
}

@media (prefers-reduced-motion: reduce) {
  .activityChart button:nth-child(n) .barGroup {
    transition: none;
  }
}
</style>
