<script setup lang="ts">
import { computed, nextTick, reactive, ref } from 'vue';
import {
  CheckCircle2,
  ClipboardList,
  FileText,
  Hourglass,
  Plus,
  Search,
  Settings2,
  Star,
  UserRound,
  UsersRound,
  XCircle,
} from 'lucide-vue-next';
import { cloneReferenceRows } from '@/components/page';
import {
  UiButton,
  UiDialog,
  UiFormField,
  UiInput,
  UiSelect,
  UiTabs,
  UiTabsList,
  UiTabsTrigger,
  UiTextarea,
  toast,
} from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import { workReportRules, workReportsFixture } from '../model/work-reports.data';
import type { WorkReport, WorkReportDraft, WorkReportStatus, WorkReportType } from '../model/work-reports.types';
import WorkReportsTable from './work-reports/WorkReportsTable.vue';
type View = 'mine' | 'team' | 'rules';
const reports = ref<WorkReport[]>(cloneReferenceRows(workReportsFixture.rows));
const activeView = ref<View>('mine');
const type = ref<WorkReportType | '全部类型'>('全部类型');
const status = ref<WorkReportStatus | '全部状态'>('全部状态');
const query = ref('');
const formOpen = ref(false);
const detailOpen = ref(false);
const selected = ref<WorkReport | null>(null);
const draft = reactive<WorkReportDraft>({
  title: '',
  type: '周报',
  period: '2026-09-14 ~ 2026-09-20',
  approver: '王总监',
  summary: '',
});
const typeOptions: readonly UiSelectOption[] = ['全部类型', '日报', '周报', '月报', '项目汇报', '专项汇报'].map(
  (value) => ({ value, label: value }),
);
const createTypeOptions = typeOptions.slice(1);
const statusOptions: readonly UiSelectOption[] = ['全部状态', '草稿', '待审批', '已通过', '已驳回', '已撤回'].map(
  (value) => ({ value, label: value }),
);
const scoped = computed(() =>
  reports.value.filter((item) => item.scope === (activeView.value === 'mine' ? 'mine' : 'team')),
);
const filtered = computed(() => {
  const needle = query.value.trim().toLocaleLowerCase('zh-CN');
  return scoped.value.filter(
    (item) =>
      (type.value === '全部类型' || item.type === type.value) &&
      (status.value === '全部状态' || item.status === status.value) &&
      (!needle || `${item.title} ${item.approver} ${item.period}`.toLocaleLowerCase('zh-CN').includes(needle)),
  );
});
function openCreate() {
  Object.assign(draft, { title: '', type: '周报', period: '2026-09-14 ~ 2026-09-20', approver: '王总监', summary: '' });
  formOpen.value = true;
}
async function createReport() {
  if (!draft.title.trim() || !draft.summary.trim()) {
    toast.warning('请填写汇报标题和汇报内容');
    return;
  }
  const next = reports.value.length + 1;
  reports.value.unshift({
    id: `report-local-${next}`,
    title: draft.title.trim(),
    type: draft.type,
    period: draft.period.trim(),
    approver: draft.approver.trim() || '王总监',
    status: '草稿',
    submittedAt: '—',
    scope: 'mine',
    summary: draft.summary.trim(),
  });
  type.value = '全部类型';
  status.value = '全部状态';
  query.value = '';
  formOpen.value = false;
  await nextTick();
  activeView.value = 'mine';
  toast.success('工作汇报已保存为本地草稿');
}
function openDetail(report: WorkReport) {
  selected.value = report;
  detailOpen.value = true;
}
</script>
<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="list"
    data-page-route="/admin/work-reports"
    data-page-label="工作汇报"
    data-page-module="行政管理"
    data-work-reports-page
    :data-active-view="activeView"
    :data-result-count="activeView === 'rules' ? workReportRules.length : filtered.length"
    :data-total-count="reports.length"
  >
    <header :class="$style.hero">
      <div :class="$style.heroCopy">
        <nav aria-label="面包屑"><span>行政管理</span><b>›</b><strong>工作汇报</strong></nav>
        <div :class="$style.identity">
          <span :class="$style.heroIcon"
            ><ClipboardList
              :size="24"
              :stroke-width="1.8"
          /></span>
          <div>
            <h1>工作汇报</h1>
            <p>周期性工作汇报提交、审阅评分与组织复盘的统一入口</p>
          </div>
        </div>
      </div>
      <div
        :class="$style.heroArt"
        aria-hidden="true"
      >
        <i /><i /><i /><i /><i /><b>85%</b><em />
      </div>
    </header>
    <section :class="$style.metrics">
      <article>
        <FileText
          :size="17"
          :stroke-width="1.8"
        />
        <div><span>总汇报数</span><strong>11</strong></div>
      </article>
      <article :class="$style.pending">
        <Hourglass
          :size="17"
          :stroke-width="1.8"
        />
        <div><span>待审批</span><strong>3</strong></div>
      </article>
      <article :class="$style.approved">
        <CheckCircle2
          :size="17"
          :stroke-width="1.8"
        />
        <div><span>已通过</span><strong>5</strong></div>
      </article>
      <article :class="$style.rejected">
        <XCircle
          :size="17"
          :stroke-width="1.8"
        />
        <div><span>已驳回</span><strong>1</strong></div>
      </article>
      <article :class="$style.scoreMetric">
        <Star
          :size="17"
          :stroke-width="1.8"
        />
        <div>
          <span>平均评分</span><strong>90<small>分</small></strong>
        </div>
      </article>
    </section>
    <section :class="$style.workspace">
      <header :class="$style.tabsBar">
        <UiTabs v-model="activeView"
          ><UiTabsList
            label="工作汇报业务区"
            :class="$style.tabs"
            ><UiTabsTrigger
              value="mine"
              :class="$style.tab"
              ><UserRound
                :size="14"
                :stroke-width="1.8"
              />我的汇报 <em>4</em></UiTabsTrigger
            ><UiTabsTrigger
              value="team"
              :class="$style.tab"
              ><UsersRound
                :size="14"
                :stroke-width="1.8"
              />团队汇报 <em>11</em></UiTabsTrigger
            ><UiTabsTrigger
              value="rules"
              :class="$style.tab"
              ><Settings2
                :size="14"
                :stroke-width="1.8"
              />汇报规则 <em>7</em></UiTabsTrigger
            ></UiTabsList
          ></UiTabs
        ><UiButton
          variant="primary"
          size="sm"
          @click="openCreate"
          ><template #leading
            ><Plus
              :size="14"
              :stroke-width="1.8" /></template
          >新建汇报</UiButton
        >
      </header>
      <template v-if="activeView !== 'rules'"
        ><div :class="$style.filters">
          <label :class="$style.search"
            ><Search
              :size="15"
              :stroke-width="1.8" /><UiInput
              v-model="query"
              aria-label="搜索工作汇报"
              placeholder="搜索汇报..." /></label
          ><UiSelect
            v-model="type"
            label="汇报类型"
            :options="typeOptions"
            :class="$style.filter"
          /><UiSelect
            v-model="status"
            label="汇报状态"
            :options="statusOptions"
            :class="$style.filter"
          />
        </div>
        <WorkReportsTable
          :rows="filtered"
          @view="openDetail"
      /></template>
      <div
        v-else
        :class="$style.rules"
      >
        <article
          v-for="rule in workReportRules"
          :key="rule.id"
        >
          <span :class="$style.ruleIcon"
            ><Settings2
              :size="16"
              :stroke-width="1.8"
          /></span>
          <div>
            <strong>{{ rule.name }}</strong>
            <p>{{ rule.department }} · {{ rule.type }} · {{ rule.frequency }}</p>
          </div>
          <span>审批人：{{ rule.approver }}</span
          ><em :data-enabled="rule.enabled">{{ rule.enabled ? '启用' : '停用' }}</em>
        </article>
      </div>
    </section>
  </main>
  <UiDialog
    v-model:open="formOpen"
    title="新建汇报"
    description="保存后进入我的汇报草稿"
    size="lg"
    ><div :class="$style.form">
      <UiFormField
        label="汇报标题"
        required
        :class="$style.formWide"
        ><UiInput
          v-model="draft.title"
          aria-label="汇报标题" /></UiFormField
      ><UiFormField label="汇报类型"
        ><UiSelect
          v-model="draft.type"
          label="新建汇报类型"
          :options="createTypeOptions" /></UiFormField
      ><UiFormField label="审批人"
        ><UiInput
          v-model="draft.approver"
          aria-label="汇报审批人" /></UiFormField
      ><UiFormField
        label="汇报周期"
        :class="$style.formWide"
        ><UiInput
          v-model="draft.period"
          aria-label="汇报周期" /></UiFormField
      ><UiFormField
        label="汇报内容"
        required
        :class="$style.formWide"
        ><UiTextarea
          v-model="draft.summary"
          aria-label="汇报内容"
          :rows="4"
      /></UiFormField>
    </div>
    <template #footer
      ><UiButton
        variant="secondary"
        @click="formOpen = false"
        >取消</UiButton
      ><UiButton
        variant="primary"
        @click="createReport"
        >保存草稿</UiButton
      ></template
    ></UiDialog
  >
  <UiDialog
    v-model:open="detailOpen"
    :title="selected?.title || '汇报详情'"
    :description="selected?.period"
    size="lg"
    ><div
      v-if="selected"
      :class="$style.detail"
    >
      <dl>
        <div>
          <dt>类型</dt>
          <dd>{{ selected.type }}</dd>
        </div>
        <div>
          <dt>审批人</dt>
          <dd>{{ selected.approver }}</dd>
        </div>
        <div>
          <dt>状态</dt>
          <dd>{{ selected.status }}</dd>
        </div>
        <div>
          <dt>评分</dt>
          <dd>{{ selected.score ?? '-' }}</dd>
        </div>
      </dl>
      <p>{{ selected.summary }}</p>
    </div>
    <template #footer
      ><UiButton
        variant="primary"
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
.hero {
  @apply relative mb-3 min-h-[120px] overflow-hidden px-6 py-5 text-white;
  border-radius: 14px;
  background: linear-gradient(108deg, #5d3eb0, #325db4 52%, #06a7bc);
}
.heroCopy {
  @apply relative z-[2];
}
.hero nav {
  @apply mb-4 flex items-center gap-2 text-[10px];
  color: rgba(255, 255, 255, 0.78);
}
.hero nav strong {
  color: white;
}
.identity {
  @apply flex items-center gap-3;
}
.heroIcon {
  @apply flex h-10 w-10 items-center justify-center rounded-lg border;
  border-color: rgba(255, 255, 255, 0.25);
  background: rgba(255, 255, 255, 0.12);
}
.identity h1 {
  @apply text-[20px] font-semibold;
}
.identity p {
  @apply mt-1 text-[11px];
  color: rgba(255, 255, 255, 0.78);
}
.heroArt {
  @apply absolute inset-y-0 right-[10%] w-[58%];
  background: linear-gradient(90deg, rgba(255, 255, 255, 0.12), transparent);
}
.heroArt i {
  @apply absolute bottom-5 w-4 rounded-t;
  background: #76a6d5;
}
.heroArt i:nth-child(1) {
  left: 8%;
  height: 38px;
}
.heroArt i:nth-child(2) {
  left: 12%;
  height: 52px;
}
.heroArt i:nth-child(3) {
  left: 16%;
  height: 30px;
}
.heroArt i:nth-child(4) {
  left: 20%;
  height: 66px;
}
.heroArt i:nth-child(5) {
  left: 24%;
  height: 48px;
}
.heroArt b {
  @apply absolute left-[45%] top-5 flex h-16 w-16 items-center justify-center rounded-full text-[13px];
  color: #3655a5;
  background: conic-gradient(#ef8939 0 30%, #38a67c 30% 65%, #65bdd6 65%);
}
.heroArt b::after {
  @apply absolute h-10 w-10 rounded-full;
  content: '';
  background: white;
  z-index: -1;
}
.heroArt em {
  @apply absolute right-[5%] bottom-0 h-24 w-40 rounded-t-lg;
  background: #f4f5f7;
  box-shadow: inset 0 5px #ff5630;
}
.metrics {
  @apply mb-4 grid grid-cols-5 gap-3;
}
.metrics article {
  @apply flex min-h-[94px] items-start gap-3 rounded border bg-white p-4;
  border-color: var(--ui-border);
  color: #0065ff;
}
.metrics span {
  @apply block text-[10px];
  color: var(--ui-text-subtle);
}
.metrics strong {
  @apply mt-3 block text-[20px] font-semibold;
  color: var(--ui-text);
}
.metrics strong small {
  @apply text-[9px];
  color: var(--ui-text-subtle);
}
.pending {
  color: #ff8b00 !important;
}
.approved {
  color: #00875a !important;
}
.rejected {
  color: #ff5630 !important;
}
.scoreMetric {
  color: #6554c0 !important;
}
.workspace {
  @apply overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}
.tabsBar {
  @apply flex h-14 items-center border-b px-4;
  border-color: var(--ui-border);
}
.tabs {
  @apply mr-auto gap-4;
}
.tab {
  @apply rounded-none border-b-2 border-transparent px-1;
}
.tab[data-state='active'] {
  border-color: var(--app-primary);
  background: transparent;
}
.tab em {
  @apply rounded-full px-2 py-0.5 text-[9px] not-italic;
  background: var(--ui-surface-hover);
}
.filters {
  @apply flex h-14 items-center gap-2 border-b px-4;
  border-color: var(--ui-border);
}
.search {
  @apply flex w-[250px] items-center gap-2 rounded px-3;
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
}
.search input {
  @apply border-0 bg-transparent px-0;
  box-shadow: none;
}
.filter {
  @apply w-[110px];
}
.rules {
  @apply grid grid-cols-2 gap-3 p-4;
}
.rules article {
  @apply flex items-center gap-3 rounded border p-4;
  border-color: var(--ui-border);
}
.ruleIcon {
  @apply flex h-9 w-9 items-center justify-center rounded;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.rules article > div {
  @apply min-w-0 flex-1;
}
.rules strong {
  @apply text-[11px] font-semibold;
}
.rules p,
.rules article > span {
  @apply mt-1 text-[9px];
  color: var(--ui-text-subtle);
}
.rules em {
  @apply rounded px-2 py-1 text-[8px] not-italic;
  color: #de350b;
  background: #ffebe6;
}
.rules em[data-enabled='true'] {
  color: #00875a;
  background: #e3fcef;
}
.form {
  @apply grid grid-cols-2 gap-4 p-5;
}
.formWide {
  @apply col-span-2;
}
.detail {
  @apply p-5;
}
.detail dl {
  @apply grid grid-cols-2 gap-3;
}
.detail dl div {
  @apply rounded border p-3;
  border-color: var(--ui-border-subtle);
}
.detail dt {
  @apply text-[9px];
  color: var(--ui-text-subtle);
}
.detail dd {
  @apply mt-1 text-[11px] font-medium;
}
.detail > p {
  @apply mt-4 rounded p-4 text-[11px] leading-5;
  background: var(--ui-surface-subtle);
}
</style>
