<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { CalendarClock } from 'lucide-vue-next';
import type { PageAction } from '@/components/page/page.contracts';
import { ListPageSurface } from '@/components/page';
import type { PageColumn } from '@/components/page';
import type { ListPageModel, ListPageRow } from '@/components/page/families/list/list-page.types';
import { createAiApi, type AiApi } from '@/api/core/ai';
import { apiClient } from '@/api/request';
import { canAccess } from '@/directives/permission';
import {
  toast,
  UiAlertDialog,
  UiButton,
  UiDialog,
  UiFormField,
  UiInput,
  UiSelect,
  UiSwitch,
  UiTabPanel,
  UiTabs,
  UiTabsList,
  UiTabsTrigger,
  UiTextarea,
  type UiSelectOption,
} from '@/components/ui';

const api = createAiApi(apiClient);
const jobs = ref<AiApi.Job[]>([]);
const runs = ref<AiApi.JobRun[]>([]);
const activeTab = ref('jobs');
const busy = ref(false);
const actionBusy = ref(false);
const saving = ref(false);
const loadError = ref('');
const formError = ref('');
const dialogOpen = ref(false);
const toggleOpen = ref(false);
const deleteOpen = ref(false);
const runConfirmOpen = ref(false);
const runDetailOpen = ref(false);
const editingJob = ref<AiApi.Job | null>(null);
const toggleTarget = ref<AiApi.Job | null>(null);
const deleteTarget = ref<AiApi.Job | null>(null);
const runTarget = ref<AiApi.Job | null>(null);
const selectedRun = ref<AiApi.JobRun | null>(null);
const canManage = computed(() => canAccess('Ai.Job.Manage'));
const canRun = computed(() => canAccess('Ai.Job.Run'));
const canViewSensitive = computed(() => canAccess('Business.Budget.Sensitive'));

const jobTypeOptions: UiSelectOption[] = [
  { label: '项目周度摘要', value: 'ProjectWeeklySummary' },
  { label: '延期任务识别', value: 'DelayedTaskIdentification' },
  { label: '绩效异常识别', value: 'PerformanceExceptionIdentification' },
  { label: '高风险到期识别', value: 'HighRiskDueIdentification' },
  { label: '逾期回款识别', value: 'OverdueReceivableIdentification' },
  { label: '经营月度摘要', value: 'BusinessMonthlySummary' },
];

function blankJob(): AiApi.JobSaveInput {
  return {
    assistantCode: '',
    contextScopeJson: '{\n  "scheduleCadence": "WeeklyMonday",\n  "maxItems": 12\n}',
    description: '',
    isEnabled: true,
    jobCode: '',
    jobType: 'ProjectWeeklySummary',
    modelId: null,
    name: '',
    outputTargetJson: '{}',
    promptTemplateCode: '',
    schedulerJobKey: 'ai-automation-job',
  };
}

const draft = ref<AiApi.JobSaveInput>(blankJob());

function errorText(error: unknown): string {
  return error instanceof Error ? error.message : '请求失败，请重试。';
}

function parseJsonObject(value: string): boolean {
  try {
    const parsed: unknown = JSON.parse(value);
    return typeof parsed === 'object' && parsed !== null && !Array.isArray(parsed);
  } catch {
    return false;
  }
}

function formatTime(value: null | string): string {
  return value ? new Date(value).toLocaleString('zh-CN') : '—';
}

function jobTypeLabel(value: string): string {
  return jobTypeOptions.find((item) => item.value === value)?.label ?? value;
}

function statusLabel(value: string): string {
  const labels: Record<string, string> = {
    Cancelled: '已取消',
    Failed: '失败',
    Pending: '未运行',
    Running: '运行中',
    Success: '完成',
  };
  return labels[value] ?? (value || '—');
}

const jobRows = computed<ListPageRow[]>(() =>
  jobs.value.map((item) => ({
    description: item.description || '—',
    id: item.id,
    jobCode: item.jobCode,
    jobType: jobTypeLabel(item.jobType),
    lastRun: formatTime(item.lastRunTime),
    lastRunStatus: statusLabel(item.lastRunStatus),
    lastRunSummary: canViewSensitive.value
      ? item.lastRunSummary || '—'
      : item.lastRunStatus === 'Success'
        ? '已生成任务草稿。'
        : '—',
    name: item.name,
    status: item.isEnabled ? '启用' : '停用',
  })),
);

const runRows = computed<ListPageRow[]>(() =>
  runs.value.map((item) => ({
    draftItemCount: item.draftItemCount,
    elapsedMilliseconds: item.elapsedMilliseconds,
    id: item.id,
    jobCode: item.jobCode,
    jobName: item.jobName,
    jobType: jobTypeLabel(item.jobType),
    runMode: item.runMode === 'Scheduled' ? '计划' : '手动',
    startedTime: formatTime(item.startedTime),
    status: statusLabel(item.status),
  })),
);

function pageModel(
  rows: ListPageRow[],
  label: string,
  path: string,
  actions: PageAction[],
  columns: PageColumn[],
): ListPageModel {
  return {
    actions,
    columns,
    demoPath: path,
    description: '任务运行会读取当前权限范围内的项目/经营快照并保存服务端草稿；不会直接写入项目或经营记录。',
    emptyHint: busy.value ? '正在读取数据…' : loadError.value || `暂无${label}。`,
    filters: [],
    key: `AI管理|自动作业|${label}`,
    label,
    listScopes: [{ id: 'all', label: `全部${label}`, count: rows.length }],
    listTabs: [{ id: 'all', label: `全部${label}` }],
    listViews: [{ id: 'list', label: '列表' }],
    module: 'AI 管理',
    moduleSlug: 'ai',
    pageSlug: 'ai-jobs',
    pageSize: 20,
    panels: [],
    path,
    primaryAction: actions.find((action) => action.kind === 'create')?.label ?? '刷新',
    replication: { status: 'implemented', referenceUrl: '/ai/jobs' },
    routeName: 'ai-jobs',
    rows,
    searchPlaceholder: `搜索${label}`,
    stats: [
      { key: 'total', label: `全部${label}`, value: String(rows.length), tone: 'primary' },
      {
        key: 'enabled',
        label: label === '任务' ? '启用任务' : '已完成',
        value: String(
          label === '任务'
            ? jobs.value.filter((item) => item.isEnabled).length
            : runs.value.filter((item) => item.status === 'Success').length,
        ),
        tone: 'success',
      },
    ],
    surface: 'list',
    tabs: [],
    tools: [],
  };
}

const jobsModel = computed(() =>
  pageModel(
    jobRows.value,
    '任务',
    '/ai/jobs',
    [
      { id: 'reload', kind: 'refresh', label: busy.value ? '读取中' : '刷新' },
      ...(canManage.value
        ? [
            { id: 'create', kind: 'create' as const, label: '新增任务' },
            { id: 'edit', kind: 'custom' as const, label: '编辑任务', requiresSelection: true },
            { id: 'toggle', kind: 'custom' as const, label: '切换启停', requiresSelection: true },
            { id: 'delete', kind: 'delete' as const, label: '删除任务', requiresSelection: true },
          ]
        : []),
      ...(canRun.value
        ? [
            {
              id: 'run',
              kind: 'custom' as const,
              label: actionBusy.value ? '执行中…' : '手动运行',
              requiresSelection: true,
            },
          ]
        : []),
    ],
    [
      { key: 'name', title: '任务名称', width: 200 },
      { key: 'jobCode', title: '编码', width: 160 },
      { key: 'jobType', title: '任务类型', width: 170 },
      { key: 'status', title: '调度状态', width: 100, presentation: 'status' },
      { key: 'lastRun', title: '最近运行', width: 180 },
      { key: 'lastRunStatus', title: '运行状态', width: 100, presentation: 'status' },
      { key: 'lastRunSummary', title: '最近结果', width: 260 },
    ],
  ),
);

const runsModel = computed(() =>
  pageModel(
    runRows.value,
    '运行记录',
    '/ai/jobs',
    [{ id: 'reload', kind: 'refresh', label: busy.value ? '读取中' : '刷新' }],
    [
      { key: 'startedTime', title: '开始时间', width: 180 },
      { key: 'jobName', title: '任务', width: 190 },
      { key: 'jobCode', title: '编码', width: 150 },
      { key: 'runMode', title: '触发方式', width: 100 },
      { key: 'status', title: '运行状态', width: 100, presentation: 'status' },
      { key: 'draftItemCount', title: '草稿条数', width: 100 },
      { key: 'elapsedMilliseconds', title: '耗时（毫秒）', width: 130 },
    ],
  ),
);

async function load(): Promise<void> {
  if (!canManage.value) return;
  busy.value = true;
  loadError.value = '';
  try {
    const [jobResult, runResult] = await Promise.all([
      api.getJobs({ includeDisabled: true, maxResultCount: 200 }),
      api.getJobRuns({ maxResultCount: 200 }),
    ]);
    jobs.value = jobResult;
    runs.value = runResult;
  } catch (error) {
    loadError.value = errorText(error);
  } finally {
    busy.value = false;
  }
}

function selected<T extends { id: string }>(rows: ListPageRow[], collection: T[]): T | null {
  const id = rows[0]?.id;
  return id ? (collection.find((item) => item.id === id) ?? null) : null;
}

function openForm(item: AiApi.Job | null): void {
  editingJob.value = item;
  draft.value = item
    ? {
        assistantCode: item.assistantCode,
        contextScopeJson: item.contextScopeJson,
        description: item.description,
        isEnabled: item.isEnabled,
        jobCode: item.jobCode,
        jobType: item.jobType,
        modelId: item.modelId,
        name: item.name,
        outputTargetJson: item.outputTargetJson,
        promptTemplateCode: item.promptTemplateCode,
        schedulerJobKey: item.schedulerJobKey,
      }
    : blankJob();
  formError.value = '';
  dialogOpen.value = true;
}

async function saveJob(): Promise<void> {
  if (!canManage.value || saving.value) return;
  if (!parseJsonObject(draft.value.contextScopeJson) || !parseJsonObject(draft.value.outputTargetJson)) {
    formError.value = '上下文范围和输出目标都必须是合法 JSON 对象。';
    return;
  }
  saving.value = true;
  formError.value = '';
  try {
    if (editingJob.value) await api.updateJob(editingJob.value.id, draft.value);
    else await api.createJob(draft.value);
    dialogOpen.value = false;
    await load();
    toast.success(editingJob.value ? '自动化任务已更新。' : '自动化任务已创建。');
  } catch (error) {
    formError.value = errorText(error);
  } finally {
    saving.value = false;
  }
}

function handleJobAction(action: PageAction, rows: ListPageRow[]): void {
  if (action.id === 'reload') void load();
  if (action.id === 'create' && canManage.value) openForm(null);
  const item = selected(rows, jobs.value);
  if (!item) return;
  if (action.id === 'edit' && canManage.value) openForm(item);
  if (action.id === 'toggle' && canManage.value && !actionBusy.value) {
    toggleTarget.value = item;
    toggleOpen.value = true;
  }
  if (action.id === 'delete' && canManage.value && !actionBusy.value) {
    deleteTarget.value = item;
    deleteOpen.value = true;
  }
  if (action.id === 'run' && canRun.value && !actionBusy.value) {
    runTarget.value = item;
    runConfirmOpen.value = true;
  }
}

async function confirmToggle(): Promise<void> {
  const item = toggleTarget.value;
  if (!item || !canManage.value || actionBusy.value) return;
  actionBusy.value = true;
  try {
    await api.toggleJob(item.id, { isEnabled: !item.isEnabled });
    await load();
    toast.success(item.isEnabled ? '自动化任务已停用。' : '自动化任务已启用。');
  } catch (error) {
    toast.error(errorText(error));
  } finally {
    actionBusy.value = false;
    toggleTarget.value = null;
  }
}

async function confirmDelete(): Promise<void> {
  const item = deleteTarget.value;
  if (!item || !canManage.value || actionBusy.value) return;
  actionBusy.value = true;
  try {
    await api.deleteJob(item.id);
    deleteOpen.value = false;
    await load();
    toast.success('任务及其运行历史已删除。');
  } catch (error) {
    toast.error(errorText(error));
  } finally {
    actionBusy.value = false;
    deleteTarget.value = null;
  }
}

async function confirmRun(): Promise<void> {
  const item = runTarget.value;
  if (!item || !canRun.value || actionBusy.value) return;
  actionBusy.value = true;
  try {
    const result = await api.runJob(item.id);
    runConfirmOpen.value = false;
    selectedRun.value = result.run;
    runDetailOpen.value = true;
    await load();
    if (result.run.status === 'Success') toast.success(result.summary || '任务已完成。');
    else toast.error(result.summary || '任务未完成，请查看运行状态。');
  } catch (error) {
    toast.error(errorText(error));
  } finally {
    actionBusy.value = false;
    runTarget.value = null;
  }
}

function openRun(row: ListPageRow): void {
  const item = runs.value.find((entry) => entry.id === row.id);
  if (!item) return;
  selectedRun.value = item;
  runDetailOpen.value = true;
}

function handleRunAction(action: PageAction): void {
  if (action.id === 'reload') void load();
}

onMounted(() => void load());
</script>

<template>
  <UiTabs v-model="activeTab">
    <UiTabsList label="AI 自动化任务">
      <UiTabsTrigger value="jobs">任务配置</UiTabsTrigger>
      <UiTabsTrigger value="runs">运行记录</UiTabsTrigger>
    </UiTabsList>
    <UiTabPanel value="jobs">
      <ListPageSurface
        :model="jobsModel"
        @action="handleJobAction"
      >
        <template #hero-icon
          ><CalendarClock
            :size="22"
            aria-hidden="true"
        /></template>
        <template #notice>
          <p>手动运行会读取服务端项目/经营快照并保存草稿，不会直接写入业务记录。重复点击会在请求期间锁定。</p>
          <p>当前任务运行代码按服务端规则生成汇总草稿；任务中的模型、助手和模板引用暂不触发模型调用。</p>
          <p v-if="!canViewSensitive">当前账号无敏感金额权限；运行历史中的输入快照与草稿内容由后端隐藏。</p>
          <p
            v-if="loadError"
            role="alert"
          >
            {{ loadError }}
          </p>
        </template>
      </ListPageSurface>
    </UiTabPanel>
    <UiTabPanel value="runs">
      <ListPageSurface
        :model="runsModel"
        @action="handleRunAction"
        @row="openRun"
      >
        <template #notice>
          <p>运行记录按服务端状态、触发方式、草稿条数和耗时展示。点击记录查看权限允许的草稿内容。</p>
          <p v-if="!canViewSensitive">无敏感金额权限时，快照与草稿内容由后端返回空对象。</p>
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
    :open="dialogOpen"
    :title="editingJob ? `编辑自动化任务：${editingJob.name}` : '新增自动化任务'"
    description="调度开关控制计划触发；手动触发仍需 Ai.Job.Run 权限。删除任务会级联删除全部运行历史。"
    size="lg"
    @update:open="dialogOpen = $event"
  >
    <form @submit.prevent="saveJob">
      <UiFormField label="任务编码"
        ><UiInput
          v-model="draft.jobCode"
          maxlength="64"
          required
          :readonly="Boolean(editingJob)"
      /></UiFormField>
      <UiFormField label="任务名称"
        ><UiInput
          v-model="draft.name"
          maxlength="128"
          required
      /></UiFormField>
      <UiFormField label="任务类型"
        ><UiSelect
          v-model="draft.jobType"
          label="任务类型"
          :options="jobTypeOptions"
      /></UiFormField>
      <UiFormField label="启用计划调度"
        ><UiSwitch
          v-model="draft.isEnabled"
          label="允许计划调度触发"
      /></UiFormField>
      <UiFormField
        label="任务说明"
        wide
        ><UiTextarea
          v-model="draft.description"
          rows="2"
          maxlength="512"
      /></UiFormField>
      <UiFormField
        label="调度与上下文 JSON"
        hint="支持 scheduleCadence 与 maxItems；JSON 必须是对象。"
        wide
        ><UiTextarea
          v-model="draft.contextScopeJson"
          rows="5"
          maxlength="32000"
      /></UiFormField>
      <UiFormField
        label="输出目标 JSON"
        hint="当前只作为草稿配置元数据，不会触发实际写回。"
        wide
        ><UiTextarea
          v-model="draft.outputTargetJson"
          rows="4"
          maxlength="32000"
      /></UiFormField>
      <UiFormField label="助手引用（当前不调用模型）"
        ><UiInput
          :model-value="draft.assistantCode || '未配置'"
          readonly
      /></UiFormField>
      <UiFormField label="提示词模板引用（当前不调用模型）"
        ><UiInput
          :model-value="draft.promptTemplateCode || '未配置'"
          readonly
      /></UiFormField>
      <UiFormField label="模型引用（当前不调用模型）"
        ><UiInput
          :model-value="editingJob?.modelName || draft.modelId || '未配置'"
          readonly
      /></UiFormField>
      <p
        v-if="formError"
        role="alert"
      >
        {{ formError }}
      </p>
    </form>
    <template #footer>
      <UiButton
        :disabled="saving"
        @click="dialogOpen = false"
        >取消</UiButton
      >
      <UiButton
        variant="primary"
        :disabled="saving"
        @click="saveJob"
        >{{ saving ? '保存中…' : '保存任务' }}</UiButton
      >
    </template>
  </UiDialog>

  <UiDialog
    :open="runDetailOpen"
    :title="selectedRun ? `运行记录：${selectedRun.jobName}` : '运行记录详情'"
    description="任务草稿只代表服务端生成的建议内容，不代表业务数据已写入。敏感快照由后端权限控制。"
    size="lg"
    @update:open="runDetailOpen = $event"
  >
    <UiFormField label="运行状态"
      ><UiInput
        :model-value="selectedRun ? statusLabel(selectedRun.status) : ''"
        readonly
    /></UiFormField>
    <UiFormField label="触发方式"
      ><UiInput
        :model-value="selectedRun?.runMode === 'Scheduled' ? '计划' : '手动'"
        readonly
    /></UiFormField>
    <UiFormField label="开始时间"
      ><UiInput
        :model-value="selectedRun ? formatTime(selectedRun.startedTime) : ''"
        readonly
    /></UiFormField>
    <UiFormField label="草稿条数"
      ><UiInput
        :model-value="String(selectedRun?.draftItemCount ?? 0)"
        readonly
    /></UiFormField>
    <UiFormField
      v-if="selectedRun?.errorMessage"
      label="错误信息"
      wide
      ><UiTextarea
        :model-value="selectedRun.errorMessage"
        rows="3"
        readonly
    /></UiFormField>
    <UiFormField
      label="运行草稿（纯文本）"
      hint="页面不执行返回内容。"
      wide
      ><UiTextarea
        :model-value="selectedRun?.outputDraftJson ?? '{}'"
        rows="14"
        readonly
    /></UiFormField>
  </UiDialog>

  <UiAlertDialog
    :open="toggleOpen"
    title="确认切换任务调度状态"
    :description="toggleTarget ? `将“${toggleTarget.name}”${toggleTarget.isEnabled ? '停用' : '启用'}计划调度。` : ''"
    confirm-label="确认切换"
    @update:open="toggleOpen = $event"
    @confirm="confirmToggle"
    @cancel="toggleTarget = null"
  />
  <UiAlertDialog
    :open="deleteOpen"
    title="确认删除自动化任务"
    :description="deleteTarget ? `删除“${deleteTarget.name}”会同时永久删除其运行历史，且无法恢复。` : ''"
    confirm-label="删除任务及历史"
    tone="warning"
    @update:open="deleteOpen = $event"
    @confirm="confirmDelete"
    @cancel="deleteTarget = null"
  />
  <UiAlertDialog
    :open="runConfirmOpen"
    title="确认手动运行任务"
    :description="
      runTarget
        ? `“${runTarget.name}”会读取当前账号可见的项目/经营快照并写入运行草稿。该动作可能包含业务数据，不会直接修改项目或经营记录。`
        : ''
    "
    confirm-label="运行并保存草稿"
    tone="warning"
    @update:open="runConfirmOpen = $event"
    @confirm="confirmRun"
    @cancel="runTarget = null"
  />
</template>
