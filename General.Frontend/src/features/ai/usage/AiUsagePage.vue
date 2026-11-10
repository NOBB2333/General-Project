<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue';
import { Activity, BarChart3 } from 'lucide-vue-next';
import type { PageAction } from '@/components/page/page.contracts';
import { ListPageSurface } from '@/components/page';
import type { PageColumn } from '@/components/page';
import type { ListPageModel, ListPageRow } from '@/components/page/families/list/list-page.types';
import { createAiApi, type AiApi } from '@/api/core/ai';
import { apiClient } from '@/api/request';
import { canAccess } from '@/directives/permission';
import {
  UiButton,
  UiDialog,
  UiFormField,
  UiInput,
  UiTabPanel,
  UiTabs,
  UiTabsList,
  UiTabsTrigger,
  UiTextarea,
} from '@/components/ui';

const api = createAiApi(apiClient);
const canView = computed(() => canAccess('Ai.Usage.View'));
const records = ref<AiApi.UsageRecord[]>([]);
const dashboard = ref<AiApi.UsageDashboard>({
  callCount: 0,
  costAmount: 0,
  currency: 'USD',
  failedCount: 0,
  highRiskPromptCount: 0,
  modelBuckets: [],
  projectBuckets: [],
  scenarioBuckets: [],
  templateBuckets: [],
  toolCallCount: 0,
  totalTokens: 0,
  userBuckets: [],
  warnings: [],
});
const activeTab = ref('overview');
const activeBucket = ref('model');
const loading = ref(false);
const loadError = ref('');
const appliedFrom = ref('');
const appliedTo = ref('');
const draftFrom = ref('');
const draftTo = ref('');
const selectedSource = ref('全部');
const selectedStatus = ref('全部');
const sourceRawByLabel = ref<Record<string, string>>({});
const statusRawByLabel = ref<Record<string, string>>({});
const recordDetailOpen = ref(false);
const selectedRecord = ref<AiApi.UsageRecord | null>(null);
let activeController: AbortController | null = null;
let loadVersion = 0;

const sourceLabelMap: Record<string, string> = {
  BuiltinTool: '内置工具',
  McpTool: 'MCP 工具',
  ModelChat: '模型对话',
};
const statusLabelMap: Record<string, string> = {
  Blocked: '已拦截',
  Completed: '已完成',
  Failed: '失败',
  Success: '成功',
};
const bucketTabs = [
  { id: 'model', label: '按模型' },
  { id: 'project', label: '按项目' },
  { id: 'scenario', label: '按场景' },
  { id: 'template', label: '按模板' },
  { id: 'user', label: '按用户' },
];
const emptyBucket: AiApi.UsageBucket[] = [];

function errorText(error: unknown): string {
  return error instanceof Error ? error.message : '请求失败，请重试。';
}

function formatTime(value: string): string {
  return value ? new Date(value).toLocaleString('zh-CN') : '—';
}

function formatCount(value: number): string {
  return new Intl.NumberFormat('zh-CN').format(value);
}

function formatCost(value: number, currency: string): string {
  try {
    return new Intl.NumberFormat('zh-CN', { style: 'currency', currency, maximumFractionDigits: 4 }).format(value);
  } catch {
    return `${value} ${currency}`;
  }
}

function sourceLabel(value: string): string {
  return sourceLabelMap[value] ?? (value ? `其他（${value}）` : '未归类');
}

function statusLabel(value: string): string {
  return statusLabelMap[value] ?? (value || '未知');
}

const sourceOptions = computed(() =>
  ['全部', ...Object.keys(sourceRawByLabel.value)].sort((left, right) => {
    if (left === '全部') return -1;
    if (right === '全部') return 1;
    return left.localeCompare(right, 'zh-CN');
  }),
);
const statusOptions = computed(() =>
  ['全部', ...Object.keys(statusRawByLabel.value)].sort((left, right) => {
    if (left === '全部') return -1;
    if (right === '全部') return 1;
    return left.localeCompare(right, 'zh-CN');
  }),
);

function rowForRecord(item: AiApi.UsageRecord): ListPageRow {
  return {
    cost: formatCost(item.costAmount, item.currency),
    creationTime: formatTime(item.creationTime),
    elapsedMilliseconds: item.elapsedMilliseconds,
    id: item.id,
    modelName: item.modelName || item.modelCode || '—',
    promptInjectionRiskScore: item.promptInjectionRiskScore,
    source: sourceLabel(item.source),
    status: statusLabel(item.status),
    totalTokens: item.totalTokens,
    userName: item.userName || '系统',
    scenario: item.scenario || '未归类',
  };
}

const recordRows = computed<ListPageRow[]>(() => records.value.map(rowForRecord));
const currentBuckets = computed<AiApi.UsageBucket[]>(() => {
  const map: Record<string, AiApi.UsageBucket[]> = {
    model: dashboard.value.modelBuckets,
    project: dashboard.value.projectBuckets,
    scenario: dashboard.value.scenarioBuckets,
    template: dashboard.value.templateBuckets,
    user: dashboard.value.userBuckets,
  };
  return map[activeBucket.value] ?? emptyBucket;
});
const bucketRows = computed<ListPageRow[]>(() =>
  currentBuckets.value.map((item) => ({
    cost: formatCost(item.costAmount, dashboard.value.currency),
    count: item.count,
    id: item.key || item.label,
    label: item.label,
    totalTokens: item.totalTokens,
  })),
);
const overviewStats = computed(() => [
  { key: 'calls', label: '调用次数', value: formatCount(dashboard.value.callCount), tone: 'primary' as const },
  { key: 'tokens', label: '总 Token', value: formatCount(dashboard.value.totalTokens), tone: 'neutral' as const },
  {
    key: 'cost',
    label: '服务端成本',
    value: formatCost(dashboard.value.costAmount, dashboard.value.currency),
    tone: 'success' as const,
  },
  {
    key: 'failed',
    label: '失败调用',
    value: formatCount(dashboard.value.failedCount),
    tone: dashboard.value.failedCount ? ('danger' as const) : ('neutral' as const),
  },
  {
    key: 'highRisk',
    label: '高风险输入',
    value: formatCount(dashboard.value.highRiskPromptCount),
    tone: dashboard.value.highRiskPromptCount ? ('warning' as const) : ('neutral' as const),
  },
  { key: 'tools', label: '工具调用', value: formatCount(dashboard.value.toolCallCount), tone: 'neutral' as const },
]);

function makeModel(
  rows: ListPageRow[],
  label: string,
  path: string,
  actions: PageAction[],
  columns: PageColumn[],
  stats: ListPageModel['stats'],
  filters: ListPageModel['filters'] = [],
): ListPageModel {
  return {
    actions,
    columns,
    demoPath: path,
    description:
      label === '用量概览'
        ? '调用量、Token 与成本均来自服务端统计；分组金额不会在页面端重新估算。'
        : '查看调用来源、风险评分、响应耗时与脱敏后的请求摘要。',
    emptyHint: loading.value ? '正在读取数据…' : loadError.value || `暂无${label}数据。`,
    filters,
    key: `AI管理|用量与成本|${label}`,
    label,
    listScopes: [{ id: 'all', label: `全部${label}`, count: rows.length }],
    listTabs: [{ id: 'all', label: `全部${label}` }],
    listViews: [{ id: 'list', label: '列表' }],
    module: 'AI 管理',
    moduleSlug: 'ai',
    pageSlug: 'ai-usage',
    pageSize: 20,
    panels: [],
    path,
    primaryAction: '刷新',
    replication: { status: 'implemented', referenceUrl: '/ai/usage' },
    routeName: 'ai-usage',
    rows,
    searchPlaceholder: label === '用量概览' ? '搜索分组名称' : '搜索模型、用户、场景或状态',
    stats,
    surface: 'list',
    tabs: [],
    tools: [],
  };
}

const overviewModel = computed(() =>
  makeModel(
    bucketRows.value,
    '用量概览',
    '/ai/usage',
    [{ id: 'reload', kind: 'refresh', label: loading.value ? '读取中' : '刷新' }],
    [
      { key: 'label', title: '分组', width: 260 },
      { key: 'count', title: '调用次数', width: 130 },
      { key: 'totalTokens', title: 'Token 数', width: 160 },
      { key: 'cost', title: '服务端成本', width: 180 },
    ],
    overviewStats.value,
  ),
);
const recordsModel = computed(() =>
  makeModel(
    recordRows.value,
    '调用记录',
    '/ai/usage',
    [{ id: 'reload', kind: 'refresh', label: loading.value ? '读取中' : '刷新' }],
    [
      { key: 'creationTime', title: '调用时间', width: 180 },
      { key: 'userName', title: '用户', width: 150 },
      { key: 'modelName', title: '模型', width: 210 },
      { key: 'scenario', title: '场景', width: 160 },
      { key: 'source', title: '来源', width: 130 },
      { key: 'totalTokens', title: 'Token 数', width: 110 },
      { key: 'cost', title: '服务端成本', width: 150 },
      { key: 'elapsedMilliseconds', title: '耗时（毫秒）', width: 130 },
      { key: 'promptInjectionRiskScore', title: '输入风险分', width: 110 },
      { key: 'status', title: '状态', width: 100, presentation: 'status' },
    ],
    overviewStats.value,
    [
      { key: 'source', label: '来源', options: sourceOptions.value },
      { key: 'status', label: '状态', options: statusOptions.value },
    ],
  ),
);
const remoteFilterValues = computed(() => ({ source: selectedSource.value, status: selectedStatus.value }));

function applyLocalFilters(): { source?: string; status?: string } {
  const source = sourceRawByLabel.value[selectedSource.value] ?? '';
  const status = statusRawByLabel.value[selectedStatus.value] ?? '';
  return {
    ...(source ? { source } : {}),
    ...(status ? { status } : {}),
  };
}

async function load(): Promise<void> {
  if (!canView.value) return;
  const version = ++loadVersion;
  activeController?.abort();
  const controller = new AbortController();
  activeController = controller;
  loading.value = true;
  loadError.value = '';
  const filters = applyLocalFilters();
  const query: AiApi.UsageQueryInput = {
    createdFrom: appliedFrom.value || undefined,
    createdTo: appliedTo.value || undefined,
    maxResultCount: 500,
    ...filters,
  };
  try {
    const [dashboardResult, recordsResult] = await Promise.all([
      api.getUsageDashboard(query, controller.signal),
      api.getUsageRecords(query, controller.signal),
    ]);
    if (version !== loadVersion) return;
    dashboard.value = dashboardResult;
    records.value = recordsResult;
    const nextSources = { ...sourceRawByLabel.value };
    const nextStatuses = { ...statusRawByLabel.value };
    for (const item of recordsResult) {
      nextSources[sourceLabel(item.source)] = item.source;
      nextStatuses[statusLabel(item.status)] = item.status;
    }
    sourceRawByLabel.value = nextSources;
    statusRawByLabel.value = nextStatuses;
  } catch (error) {
    if (!controller.signal.aborted && version === loadVersion) loadError.value = errorText(error);
  } finally {
    if (version === loadVersion) {
      loading.value = false;
      activeController = null;
    }
  }
}

function handleAction(action: PageAction): void {
  if (action.id === 'reload') void load();
}

function handleFilterChange(key: string, value: string): void {
  if (key === 'source') selectedSource.value = value;
  if (key === 'status') selectedStatus.value = value;
  void load();
}

function applyDateFilters(): void {
  if (draftFrom.value && draftTo.value && draftFrom.value > draftTo.value) {
    loadError.value = '开始日期不能晚于结束日期。';
    return;
  }
  appliedFrom.value = draftFrom.value;
  appliedTo.value = draftTo.value;
  void load();
}

function resetDateFilters(): void {
  draftFrom.value = '';
  draftTo.value = '';
  appliedFrom.value = '';
  appliedTo.value = '';
  void load();
}

function openRecord(row: ListPageRow): void {
  selectedRecord.value = records.value.find((item) => item.id === row.id) ?? null;
  recordDetailOpen.value = Boolean(selectedRecord.value);
}

function switchBucket(value: string): void {
  activeBucket.value = value;
}

onMounted(() => void load());
onBeforeUnmount(() => {
  loadVersion += 1;
  activeController?.abort();
});
</script>

<template>
  <UiTabs v-model="activeTab">
    <UiTabsList label="AI 用量管理">
      <UiTabsTrigger value="overview">用量概览</UiTabsTrigger>
      <UiTabsTrigger value="records">调用记录</UiTabsTrigger>
    </UiTabsList>
    <UiTabPanel value="overview">
      <ListPageSurface
        :model="overviewModel"
        @action="handleAction"
      >
        <template #hero-icon
          ><BarChart3
            :size="22"
            aria-hidden="true"
        /></template>
        <template #notice>
          <section aria-label="用量日期筛选">
            <UiFormField label="开始日期"
              ><UiInput
                v-model="draftFrom"
                type="date"
                aria-label="用量开始日期"
            /></UiFormField>
            <UiFormField label="结束日期"
              ><UiInput
                v-model="draftTo"
                type="date"
                aria-label="用量结束日期"
            /></UiFormField>
            <UiButton
              variant="primary"
              @click="applyDateFilters"
              >应用日期</UiButton
            >
            <UiButton @click="resetDateFilters">重置</UiButton>
          </section>
          <UiTabs
            :model-value="activeBucket"
            @update:model-value="switchBucket"
          >
            <UiTabsList label="用量分组维度">
              <UiTabsTrigger
                v-for="tab in bucketTabs"
                :key="tab.id"
                :value="tab.id"
                >{{ tab.label }}</UiTabsTrigger
              >
            </UiTabsList>
          </UiTabs>
          <p
            v-for="warning in dashboard.warnings"
            :key="warning"
            role="status"
          >
            {{ warning }}
          </p>
          <p
            v-if="loadError"
            role="alert"
          >
            {{ loadError }}
          </p>
        </template>
      </ListPageSurface>
    </UiTabPanel>
    <UiTabPanel value="records">
      <ListPageSurface
        :model="recordsModel"
        :remote-filter-values="remoteFilterValues"
        @action="handleAction"
        @filter-change="handleFilterChange"
        @row="openRecord"
      >
        <template #hero-icon
          ><Activity
            :size="22"
            aria-hidden="true"
        /></template>
        <template #notice>
          <p>
            日期、来源和状态通过服务端筛选；关键词在最多 500
            条已读取记录中筛选。Token、成本和风险评分直接展示后端记录值。
          </p>
          <section aria-label="调用日期筛选">
            <UiFormField label="开始日期"
              ><UiInput
                v-model="draftFrom"
                type="date"
                aria-label="调用开始日期"
            /></UiFormField>
            <UiFormField label="结束日期"
              ><UiInput
                v-model="draftTo"
                type="date"
                aria-label="调用结束日期"
            /></UiFormField>
            <UiButton
              variant="primary"
              @click="applyDateFilters"
              >应用日期</UiButton
            >
            <UiButton @click="resetDateFilters">重置日期</UiButton>
          </section>
          <p
            v-if="loadError"
            role="alert"
          >
            {{ loadError }}
          </p>
        </template>
      </ListPageSurface>
    </UiTabPanel>
  </UiTabs>

  <UiDialog
    :open="recordDetailOpen"
    :title="
      selectedRecord
        ? `调用记录：${selectedRecord.modelName || selectedRecord.modelCode || '未知模型'}`
        : '调用记录详情'
    "
    description="请求与响应摘要经过服务端脱敏；页面以纯文本展示，不解析或执行记录内容。"
    size="lg"
    @update:open="recordDetailOpen = $event"
  >
    <UiFormField label="调用来源"
      ><UiInput
        :model-value="selectedRecord ? sourceLabel(selectedRecord.source) : ''"
        readonly
    /></UiFormField>
    <UiFormField label="调用状态"
      ><UiInput
        :model-value="selectedRecord ? statusLabel(selectedRecord.status) : ''"
        readonly
    /></UiFormField>
    <UiFormField label="输入 Token"
      ><UiInput
        :model-value="String(selectedRecord?.promptTokens ?? 0)"
        readonly
    /></UiFormField>
    <UiFormField label="输出 Token"
      ><UiInput
        :model-value="String(selectedRecord?.completionTokens ?? 0)"
        readonly
    /></UiFormField>
    <UiFormField label="服务端成本"
      ><UiInput
        :model-value="selectedRecord ? formatCost(selectedRecord.costAmount, selectedRecord.currency) : ''"
        readonly
    /></UiFormField>
    <UiFormField
      label="风险信号 JSON"
      wide
      ><UiTextarea
        :model-value="selectedRecord?.promptInjectionSignalsJson ?? '[]'"
        rows="4"
        readonly
    /></UiFormField>
    <UiFormField
      label="请求摘要（纯文本）"
      wide
      ><UiTextarea
        :model-value="selectedRecord?.requestPreview ?? ''"
        rows="7"
        readonly
    /></UiFormField>
    <UiFormField
      label="响应摘要（纯文本）"
      wide
      ><UiTextarea
        :model-value="selectedRecord?.responsePreview ?? ''"
        rows="7"
        readonly
    /></UiFormField>
    <UiFormField
      v-if="selectedRecord?.errorMessage"
      label="错误信息"
      wide
      ><UiTextarea
        :model-value="selectedRecord.errorMessage"
        rows="3"
        readonly
    /></UiFormField>
  </UiDialog>
</template>
