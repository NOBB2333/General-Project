<script setup lang="ts">
import { computed, reactive, ref } from 'vue';
import { ShieldCheck } from 'lucide-vue-next';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, toast, type UiSelectOption } from '@/components/ui';
import { analysisPanels, financeReportsFixture, healthScores, reportMetrics } from '../model/finance-reports.data';
import type { FinanceReportMetric, FinanceReportView, HealthScore } from '../model/finance-reports.types';
import FinanceHealthGrid from './finance-reports/FinanceHealthGrid.vue';
import FinanceMetricGrid from './finance-reports/FinanceMetricGrid.vue';
import FinanceReportsHero from './finance-reports/FinanceReportsHero.vue';
import FinanceReportsToolbar from './finance-reports/FinanceReportsToolbar.vue';

const activeView = ref<FinanceReportView>(financeReportsFixture.defaults?.view as FinanceReportView);
const period = ref(String(financeReportsFixture.defaults?.period));
const scheduleOpen = ref(false);
const detailOpen = ref(false);
const detailTitle = ref('');
const detailDescription = ref('');
const schedule = reactive({ recipients: '管理层', frequency: '每月', day: '1' });
const frequencyOptions: readonly UiSelectOption[] = [
  { value: '每周', label: '每周' },
  { value: '每月', label: '每月' },
  { value: '每季度', label: '每季度' },
];
const activePanel = computed(
  () => analysisPanels.find((panel) => panel.view === activeView.value) ?? analysisPanels[0],
);
function inspect(item: HealthScore | FinanceReportMetric) {
  detailTitle.value = item.label;
  detailDescription.value =
    'benchmark' in item ? `${item.detail} · ${item.benchmark || item.status}` : `${item.value} · ${item.description}`;
  detailOpen.value = true;
}
function exportReport(format: 'PDF' | 'Excel') {
  toast.success(`${activeView.value} ${format} 已生成到本地下载队列`);
}
function saveSchedule() {
  scheduleOpen.value = false;
  toast.success(`${schedule.frequency}${schedule.day}日向${schedule.recipients}发送报告`);
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="board"
    data-page-route="/finance/finance-reports"
    data-page-label="财务管理报告"
    data-page-module="财务管理"
    data-finance-reports-page
    :data-active-view="activeView"
    :data-period="period"
  >
    <FinanceReportsHero />
    <FinanceReportsToolbar
      :view="activeView"
      :period="period"
      @update:view="activeView = $event"
      @update:period="period = $event"
      @schedule="scheduleOpen = true"
      @export="exportReport"
    />
    <FinanceHealthGrid
      :scores="healthScores"
      @inspect="inspect"
    />
    <section :class="$style.summary">
      <span
        ><ShieldCheck
          :size="17"
          :stroke-width="1.8"
      /></span>
      <p>
        <strong>经营摘要</strong>本月签约良好(+15.1%)；净利 ¥19.52万（净利率 15.3%）；现金 ¥285.00万 可支撑 3.2
        个月；员工借款逾期 ¥25,000；保证金到期待收回 ¥29.50万；优先推进逾期应收回收。
      </p>
    </section>
    <FinanceMetricGrid
      :metrics="reportMetrics"
      @inspect="inspect"
    />
    <section :class="$style.analysis">
      <header>
        <div>
          <h2>{{ activePanel.title }}</h2>
          <p>{{ activePanel.subtitle }} · {{ period }}</p>
        </div>
        <strong>{{ activeView }}</strong>
      </header>
      <div :class="$style.chart">
        <i
          v-for="(value, index) in activePanel.values"
          :key="`${activeView}-${index}`"
          :data-level="Math.ceil(value / 10)"
        /><span>经营数据已按 {{ period }} 更新</span>
      </div>
    </section>
  </main>

  <UiDialog
    v-model:open="scheduleOpen"
    title="定时发送财务报告"
    description="发送规则仅保存在当前本地会话"
    size="md"
  >
    <form
      :class="$style.form"
      @submit.prevent="saveSchedule"
    >
      <UiFormField
        label="收件范围"
        required
        ><UiInput
          v-model="schedule.recipients"
          aria-label="收件范围"
      /></UiFormField>
      <UiFormField
        label="发送频率"
        required
        ><UiSelect
          v-model="schedule.frequency"
          :options="frequencyOptions"
          label="发送频率"
      /></UiFormField>
      <UiFormField
        label="发送日"
        required
        ><UiInput
          v-model="schedule.day"
          type="number"
          min="1"
          max="28"
          aria-label="发送日"
      /></UiFormField>
    </form>
    <template #footer
      ><UiButton
        size="sm"
        @click="scheduleOpen = false"
        >取消</UiButton
      ><UiButton
        variant="primary"
        size="sm"
        @click="saveSchedule"
        >保存规则</UiButton
      ></template
    >
  </UiDialog>
  <UiDialog
    v-model:open="detailOpen"
    :title="detailTitle"
    :description="detailDescription"
    size="sm"
    ><div :class="$style.detail">该指标来自当前财务报告快照，切换报告期间后将同步更新。</div>
    <template #footer
      ><UiButton
        variant="primary"
        size="sm"
        @click="detailOpen = false"
        >关闭</UiButton
      ></template
    ></UiDialog
  >
</template>

<style module>
@reference "../../../styles/index.css";
.page {
  @apply mx-auto min-h-full min-w-[1180px] max-w-[1400px] pb-8;
  color: var(--ui-text);
}
.summary {
  @apply mt-4 flex min-h-[72px] items-center gap-3 rounded-lg border px-5;
  border-color: #b3d4ff;
  background: linear-gradient(90deg, #deebff, #eae6ff);
}
.summary > span {
  @apply flex h-9 w-9 shrink-0 items-center justify-center rounded;
  color: var(--app-primary);
  background: rgb(255 255 255 / 45%);
}
.summary p {
  @apply m-0 text-[11px] leading-5;
  color: var(--ui-text-muted);
}
.summary strong {
  @apply block text-[11px];
  color: var(--app-primary);
}
.analysis {
  @apply mt-4 rounded border bg-white p-5;
  border-color: var(--ui-border);
}
.analysis header {
  @apply flex items-start justify-between;
}
.analysis h2 {
  @apply m-0 text-[14px];
}
.analysis p {
  @apply m-0 mt-1 text-[10px];
  color: var(--ui-text-subtle);
}
.analysis header > strong {
  @apply rounded px-2 py-1 text-[9px];
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.chart {
  @apply mt-5 flex h-[150px] items-end gap-3 rounded p-4;
  background: var(--ui-surface-subtle);
}
.chart i {
  @apply min-w-5 flex-1 rounded-t;
  background: var(--app-primary);
}
.chart i[data-level='2'] {
  height: 20%;
}
.chart i[data-level='3'] {
  height: 30%;
}
.chart i[data-level='4'] {
  height: 40%;
}
.chart i[data-level='5'] {
  height: 50%;
}
.chart i[data-level='6'] {
  height: 60%;
}
.chart i[data-level='7'] {
  height: 70%;
}
.chart i[data-level='8'] {
  height: 80%;
}
.chart i[data-level='9'] {
  height: 90%;
}
.chart i[data-level='10'] {
  height: 100%;
}
.chart span {
  @apply absolute text-[9px];
  color: transparent;
}
.form {
  @apply grid grid-cols-2 gap-4 p-5;
}
.detail {
  @apply p-5 text-[12px] leading-6;
  color: var(--ui-text-muted);
}
</style>
