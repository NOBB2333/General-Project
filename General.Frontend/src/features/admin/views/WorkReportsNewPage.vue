<script setup lang="ts">
import { computed, reactive, ref } from 'vue';
import { BarChart3, Bot, FilePenLine, FileText, List, Settings2, SlidersHorizontal, UsersRound } from 'lucide-vue-next';
import { cloneReferenceRows } from '@/components/page';
import { UiButton, UiDialog, UiFormField, UiTabs, UiTabsList, UiTabsTrigger, UiTextarea, toast } from '@/components/ui';
import { workReportNewTemplates, workReportsNewFixture } from '../model/work-reports-new.data';
import type {
  WorkReportNewDraft,
  WorkReportNewMode,
  WorkReportNewSection,
  WorkReportNewSubmission,
} from '../model/work-reports-new.types';
import WorkReportsNewTable from './work-reports-new/WorkReportsNewTable.vue';

const rows = ref<WorkReportNewSubmission[]>(cloneReferenceRows(workReportsNewFixture.rows));
const section = ref<WorkReportNewSection>('reports');
const mode = ref<WorkReportNewMode>('summary');
const activeTemplate = ref('research-weekly');
const reportOpen = ref(false);
const analysisOpen = ref(false);
const ruleOpen = ref(false);
const draft = reactive<WorkReportNewDraft>({ keyResults: '', details: '', nextPlan: '', risks: '' });

const selectedTemplate = computed(
  () => workReportNewTemplates.find((item) => item.id === activeTemplate.value) ?? workReportNewTemplates[0],
);
const submitted = computed(() => rows.value.filter((item) => item.submitted));
const averageScore = computed(() => {
  const scored = submitted.value.filter((item) => item.score !== undefined);
  if (!scored.length) return '0';
  return (scored.reduce((sum, item) => sum + (item.score ?? 0), 0) / scored.length).toFixed(1);
});

function openReport() {
  Object.assign(draft, { keyResults: '', details: '', nextPlan: '', risks: '' });
  reportOpen.value = true;
}

function saveReport() {
  if (!draft.keyResults.trim() || !draft.details.trim()) {
    toast.warning('请填写本周关键成果和工作事项明细');
    return;
  }
  rows.value = rows.value.map((item) =>
    item.person === '李四'
      ? {
          ...item,
          submitted: true,
          keyResults: draft.keyResults.trim(),
          details: draft.details.trim(),
          nextPlan: draft.nextPlan.trim() || '待补充',
          risks: draft.risks.trim() || '暂无风险',
          score: 85,
        }
      : item,
  );
  reportOpen.value = false;
  toast.success('工作汇报已保存到本地');
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="list"
    data-page-route="/admin/work-reports-new"
    data-page-label="工作汇报-新"
    data-page-module="行政管理"
    data-work-reports-new-page
    :data-section="section"
    :data-mode="mode"
    :data-submitted-count="submitted.length"
  >
    <header :class="$style.pageHeader">
      <div :class="$style.brand">
        <span
          ><FileText
            :size="18"
            :stroke-width="1.8"
        /></span>
        <strong>工作汇报</strong>
      </div>
      <UiTabs v-model="section">
        <UiTabsList
          label="工作汇报主视图"
          :class="$style.primaryTabs"
        >
          <UiTabsTrigger
            value="reports"
            :class="$style.primaryTab"
          >
            <FileText
              :size="14"
              :stroke-width="1.8"
            />汇报
          </UiTabsTrigger>
          <UiTabsTrigger
            value="statistics"
            :class="$style.primaryTab"
          >
            <BarChart3
              :size="14"
              :stroke-width="1.8"
            />统计
          </UiTabsTrigger>
          <UiTabsTrigger
            value="templates"
            :class="$style.primaryTab"
          >
            <SlidersHorizontal
              :size="14"
              :stroke-width="1.8"
            />模板
          </UiTabsTrigger>
        </UiTabsList>
      </UiTabs>
      <UiButton
        variant="primary"
        :class="$style.writeButton"
        @click="openReport"
      >
        <template #leading
          ><FilePenLine
            :size="14"
            :stroke-width="1.8"
        /></template>
        写汇报
      </UiButton>
    </header>

    <section
      v-if="section === 'reports'"
      :class="$style.reportWorkspace"
    >
      <aside :class="$style.sidebar">
        <div
          :class="$style.modeSwitch"
          role="group"
          aria-label="汇报查看方式"
        >
          <button
            :data-active="mode === 'person'"
            @click="mode = 'person'"
          >
            <UsersRound
              :size="14"
              :stroke-width="1.8"
            />按人看
          </button>
          <button
            :data-active="mode === 'summary'"
            @click="mode = 'summary'"
          >
            <List
              :size="14"
              :stroke-width="1.8"
            />汇总看
          </button>
        </div>
        <div :class="$style.templateList">
          <p>我收到的</p>
          <button
            v-for="template in workReportNewTemplates.filter((item) => item.state === 'active')"
            :key="template.id"
            :data-active="template.id === activeTemplate"
            :data-tone="template.tone"
            @click="activeTemplate = template.id"
          >
            <i />{{ template.name }}
          </button>
          <p :class="$style.retiredLabel">停止使用的</p>
          <button
            v-for="template in workReportNewTemplates.filter((item) => item.state === 'retired')"
            :key="template.id"
            :data-tone="template.tone"
            @click="activeTemplate = template.id"
          >
            <i />{{ template.name }}
          </button>
        </div>
      </aside>

      <div :class="$style.content">
        <header :class="$style.contentHeader">
          <div>
            <h1>{{ selectedTemplate?.name }}</h1>
            <span>{{ selectedTemplate?.frequency }}</span>
          </div>
          <UiButton
            variant="secondary"
            size="sm"
            @click="analysisOpen = true"
          >
            <template #leading
              ><Bot
                :size="14"
                :stroke-width="1.8"
            /></template>
            AI 分析
          </UiButton>
          <UiButton
            variant="secondary"
            size="sm"
            @click="ruleOpen = true"
          >
            <template #leading
              ><Settings2
                :size="14"
                :stroke-width="1.8"
            /></template>
            配置规则
          </UiButton>
        </header>

        <section :class="$style.dashboard">
          <button
            :class="$style.dashboardTitle"
            @click="section = 'statistics'"
          >
            汇报统计看板 <b>⌄</b>
          </button>
          <div :class="$style.metrics">
            <article>
              <span>累计汇报</span><strong>{{ submitted.length }}</strong>
            </article>
            <article>
              <span>参与人数</span><strong :class="$style.blueMetric">{{ submitted.length }}</strong>
            </article>
            <article>
              <span>AI 平均分</span><strong :class="$style.purpleMetric">{{ averageScore }}</strong>
            </article>
            <article>
              <span>应提交人数</span><strong :class="$style.greenMetric">{{ rows.length }}</strong>
            </article>
          </div>
        </section>

        <section :class="$style.submissionPanel">
          <header>
            <strong>2026-06-22</strong>
            <span>{{ submitted.length }} 人已提交 · {{ rows.length - submitted.length }} 人未提交</span>
          </header>
          <WorkReportsNewTable
            :rows="rows"
            :mode="mode"
          />
        </section>
      </div>
    </section>

    <section
      v-else
      :class="$style.alternateView"
    >
      <component
        :is="section === 'statistics' ? BarChart3 : SlidersHorizontal"
        :size="28"
        :stroke-width="1.8"
      />
      <h1>{{ section === 'statistics' ? '汇报统计' : '汇报模板' }}</h1>
      <p>{{ section === 'statistics' ? '按周期查看汇报提交率和 AI 评分趋势' : '管理汇报模板、字段和适用成员' }}</p>
      <div :class="$style.alternateCards">
        <article
          v-for="template in workReportNewTemplates"
          :key="template.id"
        >
          <strong>{{ template.name }}</strong
          ><span>{{ template.frequency }} · {{ template.state === 'active' ? '使用中' : '已停用' }}</span>
        </article>
      </div>
    </section>
  </main>

  <UiDialog
    v-model:open="reportOpen"
    title="写汇报"
    description="研发周报 · 2026-06-22"
    size="lg"
  >
    <div :class="$style.reportForm">
      <UiFormField
        label="本周关键成果"
        required
        ><UiTextarea
          v-model="draft.keyResults"
          :rows="3"
      /></UiFormField>
      <UiFormField
        label="工作事项明细"
        required
        ><UiTextarea
          v-model="draft.details"
          :rows="3"
      /></UiFormField>
      <UiFormField label="下周计划"
        ><UiTextarea
          v-model="draft.nextPlan"
          :rows="3"
      /></UiFormField>
      <UiFormField label="问题与风险"
        ><UiTextarea
          v-model="draft.risks"
          :rows="3"
      /></UiFormField>
    </div>
    <template #footer>
      <UiButton
        variant="secondary"
        @click="reportOpen = false"
        >取消</UiButton
      >
      <UiButton
        variant="primary"
        @click="saveReport"
        >提交汇报</UiButton
      >
    </template>
  </UiDialog>

  <UiDialog
    v-model:open="analysisOpen"
    title="AI 汇报分析"
    description="基于当前研发周报生成的本地分析"
    size="md"
  >
    <div :class="$style.analysis">
      <strong>整体进展健康，当前平均评分 {{ averageScore }} 分</strong>
      <p>库存核销和支付幂等改造进展明确。建议重点跟踪回调风暴风险，并尽快完成财务口径对齐。</p>
    </div>
    <template #footer
      ><UiButton
        variant="primary"
        @click="analysisOpen = false"
        >知道了</UiButton
      ></template
    >
  </UiDialog>

  <UiDialog
    v-model:open="ruleOpen"
    title="配置汇报规则"
    description="研发周报"
    size="md"
  >
    <div :class="$style.ruleSummary">
      <p><span>提交周期</span><strong>每周一 18:00 前</strong></p>
      <p><span>应提交成员</span><strong>张明、李四、王五、赵敏</strong></p>
      <p><span>提醒方式</span><strong>站内通知</strong></p>
    </div>
    <template #footer
      ><UiButton
        variant="primary"
        @click="ruleOpen = false"
        >完成</UiButton
      ></template
    >
  </UiDialog>
</template>

<style module>
@reference "../../../styles/index.css";
.page {
  @apply min-h-full min-w-[1180px] pb-8;
  color: var(--ui-text);
}
.pageHeader {
  @apply flex h-[68px] items-center gap-6 border-b bg-white px-5;
  border-color: var(--ui-border);
}
.brand {
  @apply flex items-center gap-2 text-[16px];
}
.brand span {
  @apply flex h-8 w-8 items-center justify-center rounded text-white;
  background: linear-gradient(135deg, #0052cc, #00a3bf);
}
.brand strong {
  @apply font-semibold;
}
.primaryTabs {
  @apply h-[68px] gap-2;
}
.primaryTab {
  @apply h-[68px] rounded-none border-b-2 border-transparent px-3 text-[13px];
  background: transparent;
}
.primaryTab[data-state='active'] {
  color: #5243aa;
  border-color: #5243aa;
  background: transparent;
}
.writeButton {
  @apply ml-auto;
  background: #5243aa !important;
}
.reportWorkspace {
  @apply flex gap-4 px-5 py-4;
}
.sidebar {
  @apply w-[244px] shrink-0;
}
.modeSwitch {
  @apply mb-3 grid h-10 grid-cols-2 rounded p-1;
  background: var(--ui-surface-hover);
}
.modeSwitch button {
  @apply flex items-center justify-center gap-1 rounded text-[11px] font-medium;
  color: var(--ui-text-muted);
}
.modeSwitch button[data-active='true'] {
  @apply bg-white shadow-sm;
  color: var(--ui-text);
}
.templateList {
  @apply rounded border bg-white p-2;
  border-color: var(--ui-border);
}
.templateList p {
  @apply px-2 py-2 text-[10px];
  color: var(--ui-text-subtle);
}
.templateList button {
  @apply flex h-9 w-full items-center gap-2 rounded px-3 text-left text-[11px] font-medium;
  color: var(--ui-text-muted);
}
.templateList button:hover {
  background: var(--ui-surface-hover);
}
.templateList button[data-active='true'] {
  color: #5243aa;
  background: #eae6ff;
}
.templateList i {
  @apply h-1.5 w-1.5 rounded-full;
  background: #6554c0;
}
.templateList button[data-tone='teal'] i {
  background: #00a3bf;
}
.templateList button[data-tone='green'] i {
  background: #00875a;
}
.templateList button[data-tone='muted'] {
  color: var(--ui-text-subtle);
}
.templateList button[data-tone='muted'] i {
  background: #c1c7d0;
}
.retiredLabel {
  @apply mt-2;
}
.content {
  @apply min-w-0 flex-1;
}
.contentHeader {
  @apply mb-3 flex h-10 items-center gap-2;
}
.contentHeader > div:first-child {
  @apply mr-auto flex items-center gap-2;
}
.contentHeader h1 {
  @apply text-[18px] font-semibold;
}
.contentHeader span {
  @apply rounded px-2 py-1 text-[10px];
  color: #5243aa;
  background: #eae6ff;
}
.dashboard,
.submissionPanel {
  @apply mb-3 overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}
.dashboardTitle {
  @apply flex h-11 items-center gap-2 px-4 text-[12px] font-semibold;
}
.dashboardTitle b {
  color: var(--ui-text-subtle);
}
.metrics {
  @apply grid grid-cols-4 gap-3 px-4 pb-4;
}
.metrics article {
  @apply min-h-[78px] rounded border p-4;
  border-color: var(--ui-border-subtle);
  background: var(--ui-surface-subtle);
}
.metrics span {
  @apply block text-[10px];
  color: var(--ui-text-subtle);
}
.metrics strong {
  @apply mt-2 block text-[20px] font-semibold;
}
.blueMetric {
  color: #0052cc;
}
.purpleMetric {
  color: #5243aa;
}
.greenMetric {
  color: #00875a;
}
.submissionPanel > header {
  @apply flex h-12 items-center gap-2 border-b px-4;
  border-color: var(--ui-border);
}
.submissionPanel > header strong {
  @apply text-[12px] font-semibold;
}
.submissionPanel > header span {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}
.alternateView {
  @apply mx-5 mt-4 min-h-[480px] rounded border bg-white p-8 text-center;
  border-color: var(--ui-border);
  color: #5243aa;
}
.alternateView h1 {
  @apply mt-3 text-[18px] font-semibold;
  color: var(--ui-text);
}
.alternateView > p {
  @apply mt-1 text-[11px];
  color: var(--ui-text-subtle);
}
.alternateCards {
  @apply mx-auto mt-8 grid max-w-[760px] grid-cols-2 gap-3 text-left;
}
.alternateCards article {
  @apply rounded border p-4;
  border-color: var(--ui-border);
}
.alternateCards strong,
.alternateCards span {
  @apply block;
}
.alternateCards strong {
  @apply text-[12px];
  color: var(--ui-text);
}
.alternateCards span {
  @apply mt-1 text-[10px];
  color: var(--ui-text-subtle);
}
.reportForm {
  @apply grid grid-cols-2 gap-4 p-5;
}
.analysis,
.ruleSummary {
  @apply p-5 text-[11px];
}
.analysis strong {
  @apply text-[12px];
}
.analysis p {
  @apply mt-3 rounded p-4 leading-5;
  background: var(--ui-surface-subtle);
}
.ruleSummary p {
  @apply flex border-b py-3;
  border-color: var(--ui-border-subtle);
}
.ruleSummary span {
  @apply w-28;
  color: var(--ui-text-subtle);
}
.ruleSummary strong {
  @apply font-medium;
}
</style>
