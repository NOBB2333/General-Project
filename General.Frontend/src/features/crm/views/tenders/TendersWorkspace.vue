<script setup lang="ts">
import { computed } from 'vue';
import { BarChart3, BriefcaseBusiness, FileStack, Filter, LayoutDashboard } from 'lucide-vue-next';
import { toast, UiSelect, UiTabPanel, UiTabs, UiTabsList, UiTabsTrigger } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import type {
  TenderDepositStatus,
  TenderPriority,
  TenderResult,
  TenderRow,
  TenderStage,
  TenderTab,
  TenderTask,
} from '@/features/crm/model/tenders.types';
import TenderTasksPanel from './TenderTasksPanel.vue';
import TendersTable from './TendersTable.vue';

const props = defineProps<{
  tab: TenderTab;
  stage: TenderStage | 'all';
  result: TenderResult | 'all';
  deposit: TenderDepositStatus | 'all';
  owner: string;
  method: TenderRow['method'] | 'all';
  priority: TenderPriority | 'all';
  period: string;
  rows: readonly TenderRow[];
  tasks: readonly TenderTask[];
  stages: readonly TenderStage[];
  results: readonly TenderResult[];
  depositStatuses: readonly TenderDepositStatus[];
  owners: readonly string[];
  methods: readonly TenderRow['method'][];
  priorities: readonly TenderPriority[];
}>();
const emit = defineEmits<{
  'update:tab': [value: TenderTab];
  'update:stage': [value: TenderStage | 'all'];
  'update:result': [value: TenderResult | 'all'];
  'update:deposit': [value: TenderDepositStatus | 'all'];
  'update:owner': [value: string];
  'update:method': [value: TenderRow['method'] | 'all'];
  'update:priority': [value: TenderPriority | 'all'];
  'update:period': [value: string];
  'select-row': [row: TenderRow];
}>();
defineSlots<{ overview: () => unknown }>();

function options<T extends string>(values: readonly T[], allLabel: string): UiSelectOption[] {
  return [{ value: 'all', label: allLabel }, ...values.map((value) => ({ value, label: value }))];
}

const stageOptions = computed(() => options(props.stages, '项目状态'));
const resultOptions = computed(() => options(props.results, '结果状态'));
const depositOptions = computed(() => options(props.depositStatuses, '保证金状态'));
const ownerOptions = computed(() => options(props.owners, '负责人'));
const methodOptions = computed(() => options(props.methods, '招标方式'));
const priorityOptions = computed(() => options(props.priorities, '优先级'));
const periodOptions: UiSelectOption[] = [
  { value: 'all', label: '时间区间' },
  { value: 'month', label: '本月' },
  { value: 'quarter', label: '本季度' },
  { value: 'year', label: '本年度' },
];
</script>

<template>
  <UiTabs
    :model-value="tab"
    @update:model-value="emit('update:tab', $event as TenderTab)"
  >
    <UiTabsList
      label="招投标业务视图"
      :class="$style.tabs"
    >
      <UiTabsTrigger
        value="bids"
        :class="$style.tab"
        ><BriefcaseBusiness :size="14" />招投标</UiTabsTrigger
      >
      <UiTabsTrigger
        value="templates"
        :class="$style.tab"
        ><FileStack :size="14" />模板中心</UiTabsTrigger
      >
      <UiTabsTrigger
        value="analytics"
        :class="$style.tab"
        ><BarChart3 :size="14" />投标分析</UiTabsTrigger
      >
      <UiTabsTrigger
        value="workbench"
        :class="$style.tab"
        ><LayoutDashboard :size="14" />投标工作台</UiTabsTrigger
      >
    </UiTabsList>

    <UiTabPanel value="bids">
      <slot name="overview" />
      <div :class="$style.filters">
        <span><Filter :size="14" />筛选</span>
        <UiSelect
          :model-value="stage"
          :options="stageOptions"
          label="项目状态"
          size="sm"
          @update:model-value="emit('update:stage', $event as TenderStage | 'all')"
        />
        <UiSelect
          :model-value="result"
          :options="resultOptions"
          label="结果状态"
          size="sm"
          @update:model-value="emit('update:result', $event as TenderResult | 'all')"
        />
        <UiSelect
          :model-value="deposit"
          :options="depositOptions"
          label="保证金状态"
          size="sm"
          @update:model-value="emit('update:deposit', $event as TenderDepositStatus | 'all')"
        />
        <UiSelect
          :model-value="owner"
          :options="ownerOptions"
          label="负责人"
          size="sm"
          @update:model-value="emit('update:owner', $event)"
        />
        <UiSelect
          :model-value="method"
          :options="methodOptions"
          label="招标方式"
          size="sm"
          @update:model-value="emit('update:method', $event as TenderRow['method'] | 'all')"
        />
        <UiSelect
          :model-value="priority"
          :options="priorityOptions"
          label="优先级"
          size="sm"
          @update:model-value="emit('update:priority', $event as TenderPriority | 'all')"
        />
        <UiSelect
          :model-value="period"
          :options="periodOptions"
          label="时间区间"
          size="sm"
          @update:model-value="emit('update:period', $event)"
        />
        <small>共 18 个项目</small>
      </div>
      <div :class="$style.contentGrid">
        <TendersTable
          :rows="rows"
          @select="emit('select-row', $event)"
        />
        <TenderTasksPanel
          :tasks="tasks"
          @select="toast.info(`待办: ${$event.title}`)"
        />
      </div>
    </UiTabPanel>

    <UiTabPanel value="templates">
      <section :class="$style.altPanel">
        <header>
          <h3>投标模板中心</h3>
          <p>统一管理技术标、商务标和资质文件模板</p>
        </header>
        <div :class="$style.templateGrid">
          <article
            v-for="item in ['技术方案模板', '商务报价模板', '投标函模板', '资质文件清单']"
            :key="item"
          >
            <FileStack :size="20" /><strong>{{ item }}</strong
            ><span>最近更新 2026-08-18</span>
          </article>
        </div>
      </section>
    </UiTabPanel>
    <UiTabPanel value="analytics">
      <section :class="$style.altPanel">
        <header>
          <h3>投标分析</h3>
          <p>按阶段、结果和负责人查看投标转化情况</p>
        </header>
        <div :class="$style.analysisGrid">
          <article><strong>60%</strong><span>整体中标率</span></article>
          <article><strong>5.8天</strong><span>平均准备周期</span></article>
          <article><strong>¥2860万</strong><span>本月投标金额</span></article>
        </div>
      </section>
    </UiTabPanel>
    <UiTabPanel value="workbench">
      <section :class="$style.altPanel">
        <header>
          <h3>投标工作台</h3>
          <p>集中处理即将截止的项目与保证金任务</p>
        </header>
        <TenderTasksPanel
          :tasks="tasks"
          @select="toast.info(`待办: ${$event.title}`)"
        />
      </section>
    </UiTabPanel>
  </UiTabs>
</template>

<style module>
@reference "../../../../styles/index.css";

.tabs {
  @apply flex h-[46px] w-full items-end gap-2 border-b bg-white px-3;
  border-color: var(--ui-border);
}
.tab {
  @apply h-[44px] rounded-none border-b-2 border-transparent px-4;
  background: transparent;
}
.tab[data-state='active'] {
  color: var(--app-primary);
  background: transparent;
  border-color: var(--app-primary);
}
.filters {
  @apply flex min-h-[60px] items-center gap-2 overflow-x-auto rounded border bg-white px-4;
  border-color: var(--ui-border);
}
.filters > span {
  @apply flex shrink-0 items-center gap-1 text-[11px];
  color: var(--ui-text-subtle);
}
.filters > button {
  @apply min-w-[92px];
}
.filters small {
  @apply ml-auto shrink-0;
  color: var(--ui-text-subtle);
}
.contentGrid {
  @apply mt-5 grid min-w-0 gap-4;
  grid-template-columns: minmax(0, 1fr) 320px;
}
.altPanel {
  @apply mt-4 rounded border bg-white p-6;
  border-color: var(--ui-border);
}
.altPanel > header h3 {
  @apply text-[18px] font-semibold;
}
.altPanel > header p {
  @apply mt-1 text-[12px];
  color: var(--ui-text-subtle);
}
.templateGrid,
.analysisGrid {
  @apply mt-5 grid grid-cols-4 gap-4;
}
.templateGrid article,
.analysisGrid article {
  @apply flex min-h-[110px] flex-col rounded border p-4;
  border-color: var(--ui-border);
}
.templateGrid article > svg {
  color: var(--app-primary);
}
.templateGrid strong {
  @apply mt-4 text-[13px];
}
.templateGrid span,
.analysisGrid span {
  @apply mt-1 text-[11px];
  color: var(--ui-text-subtle);
}
.analysisGrid {
  @apply grid-cols-3;
}
.analysisGrid strong {
  @apply text-[24px];
  color: var(--app-primary);
}
@media (max-width: 1100px) {
  .contentGrid {
    grid-template-columns: 1fr;
  }
}
</style>
