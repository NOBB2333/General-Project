<script setup lang="ts">
import { computed, ref } from 'vue';
import {
  BarChart3,
  CheckCircle2,
  Clock3,
  DollarSign,
  Download,
  Eye,
  FileText,
  Gift as GiftIcon,
  Play,
  Search,
  Settings2,
  Tag,
  Target as TargetIcon,
  TrendingUp,
} from 'lucide-vue-next';
import { cloneReferenceRows } from '@/components/page';
import { UiButton, UiDialog, UiInput, UiSelect, UiTabPanel, UiTabs, UiTabsList, UiTabsTrigger } from '@/components/ui';
import { perfMgmtReferenceFixture, perfMgmtTabs } from '../model/perf-mgmt.data';
import type { PerfMgmtDetail, PerfMgmtRow, PerfMgmtStatus, PerfMgmtTab } from '../model/perf-mgmt.types';

const periods = ['2026-Q2', '2026-Q1', '2026-03', '2025-Q4'] as const;
const planOptions = [
  { value: 'all', label: '全部方案' },
  { value: 'standard', label: '标准销售提成方案（2026年度）' },
  { value: 'key-account', label: '大客户经理提成方案' },
] as const;
const statusOptions = [
  { value: 'all', label: '全部状态' },
  { value: 'unreviewed', label: '未审' },
  { value: 'pending', label: '待审批' },
  { value: 'approved', label: '已审批' },
  { value: 'paid', label: '已发放' },
] as const;

const localRows = ref(cloneReferenceRows(perfMgmtReferenceFixture.rows));
const activeTab = ref<PerfMgmtTab>(perfMgmtReferenceFixture.defaults.activeTab as PerfMgmtTab);
const activePeriod = ref(String(perfMgmtReferenceFixture.defaults.activePeriod));
const selectedPlan = ref(String(perfMgmtReferenceFixture.defaults.plan));
const selectedStatus = ref(String(perfMgmtReferenceFixture.defaults.status));
const search = ref(String(perfMgmtReferenceFixture.defaults.search));
const detailOpen = ref(false);
const detail = ref<PerfMgmtDetail>({ title: '', description: '', facts: [] });
const notice = ref('');

const filteredRows = computed(() =>
  localRows.value.filter((row) => {
    const query = search.value.trim().toLowerCase();
    const matchesSearch = !query || `${row.owner} ${row.planName} ${row.planCode}`.toLowerCase().includes(query);
    const matchesPlan =
      selectedPlan.value === 'all' ||
      (selectedPlan.value === 'standard' ? row.planCode === 'TC-2026-001' : row.planCode === 'TC-2026-003');
    const statusMap: Record<string, PerfMgmtStatus | undefined> = {
      unreviewed: '未审',
      pending: '待审批',
      approved: '已审批',
      paid: '已发放',
    };
    const matchesStatus = selectedStatus.value === 'all' || row.status === statusMap[selectedStatus.value];
    return matchesSearch && matchesPlan && matchesStatus;
  }),
);
const pendingCount = computed(() => Math.max(3, localRows.value.filter((row) => row.status === '待审批').length));

function updateTab(value: string) {
  activeTab.value = value as PerfMgmtTab;
}
function openDetail(row: PerfMgmtRow) {
  detail.value = {
    title: `${row.owner} · 奖金明细`,
    description: `${row.period} · ${row.planName}`,
    facts: [
      `核算基数：${row.baseAmount}`,
      `主提成：${row.commission}`,
      `奖励：${row.reward}（${row.rewardCount}）`,
      `扣减：${row.deduction || '无'}`,
      `应发金额：${row.amount}`,
      `状态：${row.status}`,
    ],
  };
  detailOpen.value = true;
}
function submitRow(row: PerfMgmtRow) {
  row.status = '待审批';
  notice.value = `${row.owner} 的绩效已提交审批`;
}
function executeCalculation() {
  notice.value = `已在本地运行态完成 ${activePeriod.value} 绩效计算，共 ${localRows.value.length} 条记录`;
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="list"
    data-page-route="/sales-perf/perf-mgmt"
    data-page-label="绩效管理"
    data-page-module="销售绩效"
    data-module-perf-management
    :data-active-tab="activeTab"
    :data-active-period="activePeriod"
    :data-local-row-count="localRows.length"
  >
    <UiTabs
      :model-value="activeTab"
      @update:model-value="updateTab"
    >
      <header :class="$style.header">
        <div :class="$style.headingRow">
          <TrendingUp
            :size="25"
            :stroke-width="1.8"
            aria-hidden="true"
          />
          <div>
            <div :class="$style.titleLine">
              <h1>绩效管理</h1>
              <span>管理视角</span>
            </div>
            <p>全员奖金计算、审批、发放的管理驾驶舱 · 业绩排名与团队分析</p>
          </div>
        </div>
        <UiTabsList
          label="绩效管理功能"
          :class="$style.tabList"
          data-perf-management-tabs
        >
          <UiTabsTrigger
            v-for="tab in perfMgmtTabs"
            :key="tab.id"
            :value="tab.id"
            :class="$style.tabTrigger"
          >
            <component
              :is="
                tab.id === 'calculate'
                  ? Play
                  : tab.id === 'review'
                    ? CheckCircle2
                    : tab.id === 'paid'
                      ? DollarSign
                      : tab.id === 'pricing'
                        ? Tag
                        : tab.id === 'params'
                          ? Settings2
                          : BarChart3
              "
              :size="15"
              :stroke-width="1.8"
              aria-hidden="true"
            />
            <strong>{{ tab.label }}</strong
            ><span v-if="tab.badge">{{ tab.badge }}</span>
          </UiTabsTrigger>
        </UiTabsList>
      </header>

      <UiTabPanel value="calculate">
        <section :class="$style.content">
          <div :class="$style.actionBar">
            <span
              aria-live="polite"
              :class="$style.notice"
              >{{ notice }}</span
            >
            <UiButton
              variant="secondary"
              size="md"
              @click="notice = '明细已导出到本地运行态'"
            >
              <template #leading
                ><Download
                  :size="15"
                  :stroke-width="1.8" /></template
              >导出明细
            </UiButton>
            <UiButton
              variant="primary"
              size="md"
              @click="executeCalculation"
            >
              <template #leading
                ><Play
                  :size="15"
                  :stroke-width="1.8" /></template
              >执行本期计算
            </UiButton>
          </div>
          <div :class="$style.metricGrid">
            <article :class="[$style.metricCard, $style.metricPrimary]">
              <span>应发总额</span><strong>¥213,655</strong><small>4 条记录</small><DollarSign :size="24" />
            </article>
            <article :class="$style.metricCard">
              <span>主提成合计</span><strong>¥192,255</strong><small>基础业绩产出</small><TargetIcon :size="24" />
            </article>
            <article :class="[$style.metricCard, $style.metricSuccess]">
              <span>附加奖励合计</span><strong>+ ¥36,400</strong><small>各奖励叠加</small><GiftIcon :size="24" />
            </article>
            <article :class="[$style.metricCard, $style.metricWarning]">
              <span>待处理</span><strong>{{ pendingCount }}</strong
              ><small>待提交/待审批</small><Clock3 :size="24" />
            </article>
          </div>
          <section :class="$style.filterBar">
            <div :class="$style.periodGroup">
              <b>周期:</b
              ><button
                v-for="period in periods"
                :key="period"
                type="button"
                :class="[$style.periodButton, activePeriod === period && $style.periodActive]"
                @click="activePeriod = period"
              >
                {{ period }}
              </button>
            </div>
            <div :class="$style.filters">
              <UiSelect
                v-model="selectedPlan"
                :options="planOptions"
                label="绩效方案"
              />
              <UiSelect
                v-model="selectedStatus"
                :options="statusOptions"
                label="绩效状态"
              />
              <label :class="$style.searchField"
                ><Search
                  :size="16"
                  :stroke-width="1.8"
                  aria-hidden="true" /><UiInput
                  v-model="search"
                  placeholder="搜索销售/方案"
                  aria-label="搜索销售或方案"
              /></label>
            </div>
          </section>
          <section :class="$style.tablePanel">
            <div :class="$style.tableTitle">
              <div>
                <strong>奖金核算明细</strong><span>{{ filteredRows.length }} 条记录</span>
              </div>
              <span>周期：{{ activePeriod }}</span>
            </div>
            <div :class="$style.tableShell">
              <table>
                <thead>
                  <tr>
                    <th>销售员</th>
                    <th>适用方案</th>
                    <th>周期</th>
                    <th>核算基数</th>
                    <th>主提成</th>
                    <th>奖励 / 扣减</th>
                    <th>应发金额</th>
                    <th>状态</th>
                    <th>操作</th>
                  </tr>
                </thead>
                <tbody>
                  <tr
                    v-for="row in filteredRows"
                    :key="row.id"
                  >
                    <td>
                      <div :class="$style.owner">
                        <span>{{ row.owner.slice(0, 1) }}</span
                        ><strong>{{ row.owner }}</strong
                        ><small>{{ row.orderCount }} 单</small>
                      </div>
                    </td>
                    <td>
                      <div>{{ row.planName }}</div>
                      <small>{{ row.planCode }}</small>
                    </td>
                    <td>{{ row.period }}</td>
                    <td>{{ row.baseAmount }}</td>
                    <td :class="$style.blueValue">{{ row.commission }}</td>
                    <td>
                      <span :class="$style.rewardValue">{{ row.reward }}</span> <small>({{ row.rewardCount }})</small
                      ><br /><span :class="$style.deduction">{{ row.deduction }}</span>
                    </td>
                    <td>
                      <strong>{{ row.amount }}</strong>
                    </td>
                    <td>
                      <span
                        :class="[
                          $style.statusBadge,
                          row.status === '已发放' && $style.paid,
                          row.status === '未审' && $style.unreviewed,
                        ]"
                        >{{ row.status }}</span
                      >
                    </td>
                    <td>
                      <div :class="$style.rowActions">
                        <UiButton
                          variant="subtle"
                          size="xs"
                          title="查看明细"
                          @click="openDetail(row)"
                          ><template #leading
                            ><Eye
                              :size="13"
                              :stroke-width="1.8" /></template
                          >明细</UiButton
                        ><UiButton
                          v-if="row.status === '未审'"
                          variant="warning"
                          size="xs"
                          @click="submitRow(row)"
                          >提交</UiButton
                        >
                      </div>
                    </td>
                  </tr>
                  <tr v-if="!filteredRows.length">
                    <td
                      colspan="9"
                      :class="$style.empty"
                    >
                      当前筛选下没有绩效核算记录
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </section>
        </section>
      </UiTabPanel>

      <UiTabPanel
        v-for="tab in perfMgmtTabs.slice(1)"
        :key="tab.id"
        :value="tab.id"
      >
        <section :class="$style.placeholder">
          <FileText
            :size="24"
            :stroke-width="1.8"
          />
          <h2>{{ tab.label }}</h2>
          <p>该管理视图已接入本地交互入口，数据范围可从“计算与执行”明细继续查看。</p>
          <UiButton
            variant="secondary"
            size="sm"
            @click="activeTab = 'calculate'"
            >返回计算与执行</UiButton
          >
        </section>
      </UiTabPanel>
    </UiTabs>

    <UiDialog
      v-model:open="detailOpen"
      :title="detail.title"
      :description="detail.description"
      size="sm"
    >
      <dl
        :class="$style.detailFacts"
        data-perf-management-detail
      >
        <div
          v-for="fact in detail.facts"
          :key="fact"
        >
          <dt>{{ fact.slice(0, fact.indexOf('：')) }}</dt>
          <dd>{{ fact.slice(fact.indexOf('：') + 1) }}</dd>
        </div>
      </dl>
      <template #footer
        ><UiButton
          variant="primary"
          size="sm"
          @click="detailOpen = false"
          >知道了</UiButton
        ></template
      >
    </UiDialog>
  </main>
</template>

<style module>
@reference "../../../styles/index.css";
.page {
  @apply min-h-full min-w-[1180px] px-[26px] pb-8 pt-[22px];
  color: var(--ui-text);
  background: var(--ui-surface-subtle);
}
.header {
  @apply shrink-0;
}
.headingRow {
  @apply flex items-start gap-3 px-1 pt-0.5;
  color: var(--app-primary);
}
.titleLine {
  @apply flex items-center gap-2;
}
.titleLine h1 {
  @apply text-[20px] font-bold leading-7;
  color: var(--ui-text);
}
.titleLine span {
  @apply rounded px-2 py-0.5 text-[11px] font-semibold;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.headingRow p {
  @apply mt-0.5 text-[12px] leading-5;
  color: var(--ui-text-subtle);
}
.tabList {
  @apply mt-10 flex w-full items-stretch gap-0 border-b;
  border-color: var(--ui-border);
}
.tabTrigger {
  @apply min-h-[40px] rounded-none px-3 py-2 text-[12px];
  color: var(--ui-text-subtle);
  background: transparent;
}
.tabTrigger:hover {
  color: var(--app-primary);
  background: var(--ui-surface-subtle);
}
.tabTrigger strong {
  @apply font-semibold;
}
.tabTrigger span {
  @apply ml-0.5 rounded px-1.5 py-0.5 text-[10px] font-semibold;
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
}
.tabTrigger[data-state='active'] {
  color: var(--app-primary);
  box-shadow: inset 0 -2px 0 var(--app-primary);
}
.content {
  @apply pt-3.5;
}
.actionBar {
  @apply flex min-h-10 items-center justify-end gap-2;
}
.notice {
  @apply mr-auto text-[11px];
  color: var(--ui-success);
}
.metricGrid {
  @apply mt-3 grid grid-cols-4 gap-3.5;
}
.metricCard {
  @apply relative flex min-h-[116px] flex-col items-start rounded-lg border bg-white px-4 py-4;
  border-color: var(--ui-border);
}
.metricCard > span {
  @apply text-[12px];
  color: var(--ui-text-subtle);
}
.metricCard strong {
  @apply mt-2 text-[24px] font-bold leading-7;
  color: var(--app-primary);
}
.metricCard small {
  @apply mt-1 text-[11px];
  color: var(--ui-text-subtle);
}
.metricCard svg {
  @apply absolute right-4 top-4;
  color: var(--app-primary);
}
.metricPrimary {
  color: white;
  background: linear-gradient(135deg, #0052cc, #0c66e4);
  border-color: #0052cc;
}
.metricPrimary > span,
.metricPrimary small,
.metricPrimary strong,
.metricPrimary svg {
  color: white;
}
.metricSuccess strong {
  color: var(--ui-success);
}
.metricSuccess svg {
  color: var(--ui-success);
}
.metricWarning strong {
  color: var(--ui-warning);
}
.metricWarning svg {
  color: var(--ui-warning);
}
.filterBar {
  @apply mt-5 flex min-h-[72px] items-center justify-between gap-4 rounded-lg border bg-white px-4;
  border-color: var(--ui-border);
}
.periodGroup {
  @apply flex items-center gap-2;
}
.periodGroup b {
  @apply mr-2 text-[12px];
  color: var(--ui-text-muted);
}
.periodButton {
  @apply rounded-full border px-4 py-1.5 text-[12px] font-medium transition-colors;
  color: var(--ui-text-muted);
  border-color: var(--ui-border);
  background: white;
}
.periodButton:hover {
  border-color: var(--app-primary);
  color: var(--app-primary);
}
.periodActive {
  color: white;
  border-color: var(--app-primary);
  background: var(--app-primary);
}
.filters {
  @apply flex items-center gap-2;
}
.filters :global([data-ui-select]) {
  @apply min-w-[150px];
}
.searchField {
  @apply flex h-9 w-[210px] items-center gap-2 rounded border px-2.5;
  border-color: var(--ui-border);
  color: var(--ui-text-subtle);
}
.searchField input {
  @apply min-w-0 flex-1 border-0 p-0 text-[12px] outline-none;
}
.tablePanel {
  @apply mt-3.5 overflow-hidden rounded-lg border bg-white;
  border-color: var(--ui-border);
}
.tableTitle {
  @apply flex items-center justify-between border-b px-4 py-3 text-[12px];
  border-color: var(--ui-border);
  color: var(--ui-text-subtle);
}
.tableTitle div {
  @apply flex items-center gap-3;
}
.tableTitle strong {
  color: var(--ui-text);
}
.tableShell {
  @apply overflow-x-auto;
}
.tableShell table {
  @apply w-full min-w-[1300px] border-collapse text-left text-[12px];
}
.tableShell th {
  @apply whitespace-nowrap border-b px-3 py-3 font-semibold;
  color: var(--ui-text-muted);
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}
.tableShell td {
  @apply border-b px-3 py-3;
  color: var(--ui-text-muted);
  border-color: var(--ui-border-subtle);
}
.tableShell tbody tr:last-child td {
  border-bottom: 0;
}
.owner {
  @apply grid grid-cols-[30px_1fr] items-center gap-x-2;
}
.owner > span {
  @apply row-span-2 flex h-7 w-7 items-center justify-center rounded-full text-[13px] font-semibold;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.owner strong {
  @apply text-[13px];
  color: var(--ui-text);
}
.owner small,
.tableShell td small {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}
.blueValue {
  color: var(--app-primary) !important;
  font-weight: 600;
}
.rewardValue {
  color: var(--ui-success);
}
.deduction {
  color: var(--ui-danger) !important;
}
.statusBadge {
  @apply inline-flex whitespace-nowrap rounded-full px-2 py-1 text-[10px] font-semibold;
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}
.statusBadge.paid {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.statusBadge.unreviewed {
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
}
.rowActions {
  @apply flex items-center gap-1;
}
.empty {
  @apply py-12 text-center;
  color: var(--ui-text-subtle);
}
.placeholder {
  @apply mt-4 flex min-h-[300px] flex-col items-center justify-center gap-3 rounded-lg border bg-white text-center;
  border-color: var(--ui-border);
  color: var(--app-primary);
}
.placeholder h2 {
  @apply text-[16px] font-semibold;
  color: var(--ui-text);
}
.placeholder p {
  @apply text-[12px];
  color: var(--ui-text-subtle);
}
.detailFacts {
  @apply grid gap-2 p-5;
}
.detailFacts div {
  @apply grid grid-cols-[120px_1fr] gap-3 rounded border px-3 py-2.5;
  border-color: var(--ui-border-subtle);
  background: var(--ui-surface-subtle);
}
.detailFacts dt {
  @apply text-[11px];
  color: var(--ui-text-subtle);
}
.detailFacts dd {
  @apply text-right text-[12px] font-semibold;
  color: var(--ui-text);
}
@media (max-width: 1200px) {
  .filterBar {
    @apply items-start flex-col py-3;
  }
  .filters {
    @apply w-full;
  }
  .filters :global([data-ui-select]) {
    @apply flex-1;
  }
  .searchField {
    @apply flex-1;
  }
}
</style>
