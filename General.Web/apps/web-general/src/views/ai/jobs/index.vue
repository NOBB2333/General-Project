<script lang="ts" setup>
import type { AiApi } from '#/api/core';

import { computed, onMounted, reactive, ref } from 'vue';

import { Page } from '@vben/common-ui';

import {
  Button,
  Card,
  Drawer,
  Form,
  Input,
  Popconfirm,
  Select,
  Space,
  Switch,
  Table,
  Tag,
  message,
} from 'ant-design-vue';

import {
  createAiJobApi,
  deleteAiJobApi,
  getAiJobRunsApi,
  getAiJobsApi,
  getAiModelsApi,
  runAiJobApi,
  toggleAiJobApi,
  updateAiJobApi,
} from '#/api/core';
import { useActionLoading } from '#/composables/platform/use-action-loading';

defineOptions({ name: 'AiJobsPage' });

const schedulerJobKey = 'ai-automation-job';

const jobTypeOptions = [
  { label: '全部类型', value: '' },
  { label: '每周项目状态摘要', value: 'ProjectWeeklySummary' },
  { label: '延期任务识别', value: 'DelayedTaskIdentification' },
  { label: 'SPI/CPI 异常识别', value: 'PerformanceExceptionIdentification' },
  { label: '高风险到期识别', value: 'HighRiskDueIdentification' },
  { label: '逾期回款识别', value: 'OverdueReceivableIdentification' },
  { label: '经营管理月度摘要', value: 'BusinessMonthlySummary' },
];

const assistantOptions = [
  { label: 'PM 项目助手', value: 'pm-assistant' },
  { label: 'PMO 管控助手', value: 'pmo-assistant' },
  { label: '经营分析助手', value: 'business-assistant' },
];

const runModeOptions = [
  { label: '全部来源', value: '' },
  { label: '手动触发', value: 'Manual' },
  { label: '调度触发', value: 'Scheduled' },
];

const runStatusOptions = [
  { label: '全部状态', value: '' },
  { label: '成功', value: 'Success' },
  { label: '失败', value: 'Failed' },
  { label: '执行中', value: 'Running' },
  { label: '已取消', value: 'Cancelled' },
];

const jobColumns = [
  { dataIndex: 'name', key: 'name', title: '任务', width: 280 },
  { dataIndex: 'jobType', key: 'jobType', title: '类型', width: 160 },
  { dataIndex: 'schedulerJobKey', key: 'schedulerJobKey', title: '调度绑定', width: 170 },
  { dataIndex: 'lastRunStatus', key: 'lastRunStatus', title: '最近执行', width: 220 },
  { dataIndex: 'isEnabled', key: 'isEnabled', title: '启用', width: 90 },
  { key: 'action', title: '操作', width: 310 },
];

const runColumns = [
  { dataIndex: 'jobName', key: 'jobName', title: '任务', width: 260 },
  { dataIndex: 'runMode', key: 'runMode', title: '来源', width: 110 },
  { dataIndex: 'status', key: 'status', title: '状态', width: 100 },
  { dataIndex: 'draftItemCount', key: 'draftItemCount', title: '草稿项', width: 90 },
  { dataIndex: 'elapsedMilliseconds', key: 'elapsedMilliseconds', title: '耗时', width: 100 },
  { dataIndex: 'startedTime', key: 'startedTime', title: '开始时间', width: 170 },
  { key: 'action', title: '操作', width: 180 },
];

const jobs = ref<AiApi.JobItem[]>([]);
const runs = ref<AiApi.JobRunItem[]>([]);
const models = ref<AiApi.ModelItem[]>([]);
const loadingJobs = ref(false);
const loadingRuns = ref(false);
const loadingModels = ref(false);
const savingJob = ref(false);
const jobDrawerVisible = ref(false);
const draftDrawerVisible = ref(false);
const editingJobId = ref('');
const activeRun = ref<AiApi.JobRunItem>();
const { actionLoadingKey, runAction } = useActionLoading();

const jobQuery = reactive<AiApi.JobQuery>({
  includeDisabled: true,
  jobType: '',
  keyword: '',
  maxResultCount: 500,
});

const runQuery = reactive<AiApi.JobRunQuery>({
  aiJobId: undefined,
  jobType: '',
  maxResultCount: 200,
  runMode: '',
  status: '',
});

const jobForm = reactive<AiApi.JobSaveInput>({
  assistantCode: 'pmo-assistant',
  contextScopeJson: defaultContextScopeJson('ProjectWeeklySummary'),
  description: '',
  isEnabled: true,
  jobCode: '',
  jobType: 'ProjectWeeklySummary',
  modelId: undefined,
  name: '',
  outputTargetJson: defaultOutputTargetJson(),
  promptTemplateCode: 'project-weekly-report',
  schedulerJobKey,
});

const modelOptions = computed(() => [
  { label: '使用系统默认模型', value: '' },
  ...models.value.map((item) => ({
    label: `${item.displayName} · ${item.providerName || item.modelCode}`,
    value: item.id,
  })),
]);

const enabledCount = computed(() => jobs.value.filter((item) => item.isEnabled).length);
const failedRunCount = computed(() => runs.value.filter((item) => item.status === 'Failed').length);
const successRunCount = computed(() => runs.value.filter((item) => item.status === 'Success').length);
const draftItemCount = computed(() => runs.value.reduce((sum, item) => sum + item.draftItemCount, 0));
const selectedJobName = computed(() => {
  if (!runQuery.aiJobId) {
    return '全部任务';
  }

  return jobs.value.find((item) => item.id === runQuery.aiJobId)?.name || '已选任务';
});
const draftText = computed(() => formatJson(activeRun.value?.outputDraftJson || '{}'));
const inputSnapshotText = computed(() => formatJson(activeRun.value?.inputSnapshotJson || '{}'));

function defaultContextScopeJson(jobType: string) {
  const common = { maxItems: 20, scope: jobType.includes('Business') || jobType.includes('Receivable') ? 'BusinessPortfolio' : 'ProjectPortfolio' };
  const cadence = jobType === 'ProjectWeeklySummary' || jobType === 'DelayedTaskIdentification' || jobType === 'PerformanceExceptionIdentification'
    ? 'WeeklyMonday'
    : jobType === 'BusinessMonthlySummary'
      ? 'MonthlyFirstWorkday'
      : 'Weekday';
  return JSON.stringify({ ...common, scheduleCadence: cadence }, null, 2);
}

function defaultOutputTargetJson() {
  return JSON.stringify(
    {
      allowWriteBack: false,
      channel: 'AiJobRun',
      manualReviewRequired: true,
      targetType: 'Draft',
    },
    null,
    2,
  );
}

function defaultPromptTemplateCode(jobType: string) {
  if (jobType === 'OverdueReceivableIdentification') {
    return 'receivable-collection-suggestion';
  }

  if (jobType === 'BusinessMonthlySummary') {
    return 'business-risk-explanation';
  }

  if (jobType === 'ProjectWeeklySummary') {
    return 'project-weekly-report';
  }

  return 'risk-analysis';
}

function defaultAssistantCode(jobType: string) {
  if (jobType === 'OverdueReceivableIdentification' || jobType === 'BusinessMonthlySummary') {
    return 'business-assistant';
  }

  return jobType === 'DelayedTaskIdentification' ? 'pm-assistant' : 'pmo-assistant';
}

function jobTypeLabel(value: string) {
  return jobTypeOptions.find((item) => item.value === value)?.label || value || '-';
}

function runModeLabel(value: string) {
  return runModeOptions.find((item) => item.value === value)?.label || value || '-';
}

function statusColor(value: string) {
  if (value === 'Success') {
    return 'green';
  }

  if (value === 'Failed') {
    return 'red';
  }

  if (value === 'Running') {
    return 'blue';
  }

  if (value === 'Cancelled') {
    return 'orange';
  }

  return 'default';
}

function formatDate(value?: string) {
  return value ? new Date(value).toLocaleString() : '-';
}

function formatDuration(value: number) {
  if (!value) {
    return '-';
  }

  return value >= 1000 ? `${(value / 1000).toFixed(1)} 秒` : `${value} ms`;
}

function formatJson(value: string) {
  try {
    return JSON.stringify(JSON.parse(value || '{}'), null, 2);
  } catch {
    return value || '{}';
  }
}

function validateJsonObject(value: string, label: string) {
  let parsed: unknown;
  try {
    parsed = JSON.parse(value.trim() || '{}');
  } catch {
    throw new Error(`${label} 必须是合法 JSON`);
  }

  if (!parsed || Array.isArray(parsed) || typeof parsed !== 'object') {
    throw new Error(`${label} 必须是 JSON 对象`);
  }
}

function normalizeJobQuery() {
  return {
    ...jobQuery,
    jobType: jobQuery.jobType || undefined,
    keyword: jobQuery.keyword?.trim() || undefined,
  };
}

function normalizeRunQuery() {
  return {
    ...runQuery,
    aiJobId: runQuery.aiJobId || undefined,
    jobType: runQuery.jobType || undefined,
    runMode: runQuery.runMode || undefined,
    status: runQuery.status || undefined,
  };
}

function resetJobForm(record?: AiApi.JobItem) {
  editingJobId.value = record?.id || '';
  Object.assign(jobForm, {
    assistantCode: record?.assistantCode || defaultAssistantCode(record?.jobType || 'ProjectWeeklySummary'),
    contextScopeJson: formatJson(record?.contextScopeJson || defaultContextScopeJson(record?.jobType || 'ProjectWeeklySummary')),
    description: record?.description || '',
    isEnabled: record?.isEnabled ?? true,
    jobCode: record?.jobCode || '',
    jobType: record?.jobType || 'ProjectWeeklySummary',
    modelId: record?.modelId || undefined,
    name: record?.name || '',
    outputTargetJson: formatJson(record?.outputTargetJson || defaultOutputTargetJson()),
    promptTemplateCode: record?.promptTemplateCode || defaultPromptTemplateCode(record?.jobType || 'ProjectWeeklySummary'),
    schedulerJobKey: record?.schedulerJobKey || schedulerJobKey,
  });
}

function openCreateDrawer() {
  resetJobForm();
  jobDrawerVisible.value = true;
}

function openEditDrawer(record: AiApi.JobItem | Record<string, any>) {
  resetJobForm(record as AiApi.JobItem);
  jobDrawerVisible.value = true;
}

function handleJobTypeChange(value: unknown) {
  const selectedValue = Array.isArray(value) ? value[0] : value;
  const jobType = String(selectedValue || 'ProjectWeeklySummary');
  jobForm.assistantCode = defaultAssistantCode(jobType);
  jobForm.promptTemplateCode = defaultPromptTemplateCode(jobType);
  jobForm.contextScopeJson = defaultContextScopeJson(jobType);
}

async function loadJobs() {
  loadingJobs.value = true;
  try {
    jobs.value = await getAiJobsApi(normalizeJobQuery());
  } finally {
    loadingJobs.value = false;
  }
}

async function loadRuns() {
  loadingRuns.value = true;
  try {
    runs.value = await getAiJobRunsApi(normalizeRunQuery());
  } finally {
    loadingRuns.value = false;
  }
}

async function loadModels() {
  loadingModels.value = true;
  try {
    models.value = await getAiModelsApi();
  } finally {
    loadingModels.value = false;
  }
}

async function saveJob() {
  try {
    if (!jobForm.jobCode.trim() || !jobForm.name.trim()) {
      throw new Error('请填写任务编码和任务名称');
    }

    validateJsonObject(jobForm.contextScopeJson, '上下文范围配置');
    validateJsonObject(jobForm.outputTargetJson, '输出目标配置');
    savingJob.value = true;
    const payload: AiApi.JobSaveInput = {
      ...jobForm,
      jobCode: jobForm.jobCode.trim(),
      modelId: jobForm.modelId || undefined,
      name: jobForm.name.trim(),
    };
    if (editingJobId.value) {
      await updateAiJobApi(editingJobId.value, payload);
    } else {
      await createAiJobApi(payload);
    }

    message.success('AI 自动化任务已保存');
    jobDrawerVisible.value = false;
    await loadJobs();
  } catch (error) {
    message.error(error instanceof Error ? error.message : '保存失败');
  } finally {
    savingJob.value = false;
  }
}

async function toggleJob(record: AiApi.JobItem | Record<string, any>, checked: boolean) {
  const job = record as AiApi.JobItem;
  await runAction(`toggle-${job.id}`, async () => {
    await toggleAiJobApi(job.id, { isEnabled: checked });
    await loadJobs();
  });
}

async function removeJob(record: AiApi.JobItem | Record<string, any>) {
  const job = record as AiApi.JobItem;
  await runAction(`delete-${job.id}`, async () => {
    await deleteAiJobApi(job.id);
    message.success('AI 自动化任务已删除');
    await Promise.all([loadJobs(), loadRuns()]);
  });
}

async function runJob(record: AiApi.JobItem | Record<string, any>) {
  const job = record as AiApi.JobItem;
  await runAction(`run-${job.id}`, async () => {
    const result = await runAiJobApi(job.id);
    activeRun.value = result.run;
    draftDrawerVisible.value = true;
    message.success(result.summary || 'AI 自动化任务已执行');
    await Promise.all([loadJobs(), loadRuns()]);
  });
}

async function retryRun(record: AiApi.JobRunItem | Record<string, any>) {
  const run = record as AiApi.JobRunItem;
  const job = jobs.value.find((item) => item.id === run.aiJobId);
  if (!job) {
    message.error('对应的 AI 自动化任务不存在，无法重试');
    return;
  }

  await runJob(job);
}

function filterRunsByJob(record?: AiApi.JobItem | Record<string, any>) {
  runQuery.aiJobId = record ? (record as AiApi.JobItem).id : undefined;
  loadRuns();
}

function openRunDrawer(record: AiApi.JobRunItem | Record<string, any>) {
  activeRun.value = record as AiApi.JobRunItem;
  draftDrawerVisible.value = true;
}

onMounted(async () => {
  await Promise.all([loadModels(), loadJobs(), loadRuns()]);
});
</script>

<template>
  <Page description="定义 AI 自动化任务，复用平台调度器触发，所有输出先进入草稿记录，等待人工确认。" title="AI 自动化任务">
    <div class="ai-jobs">
      <div class="ai-jobs__metrics">
        <Card :bordered="false" class="ai-jobs__metric rounded-2xl" size="small">
          <div class="ai-jobs__metric-body">
            <span>任务数量</span>
            <strong>{{ jobs.length }}</strong>
          </div>
        </Card>
        <Card :bordered="false" class="ai-jobs__metric rounded-2xl" size="small">
          <div class="ai-jobs__metric-body">
            <span>已启用</span>
            <strong>{{ enabledCount }}</strong>
          </div>
        </Card>
        <Card :bordered="false" class="ai-jobs__metric rounded-2xl" size="small">
          <div class="ai-jobs__metric-body">
            <span>成功运行</span>
            <strong>{{ successRunCount }}</strong>
          </div>
        </Card>
        <Card :bordered="false" class="ai-jobs__metric rounded-2xl" size="small">
          <div class="ai-jobs__metric-body">
            <span>失败运行</span>
            <strong>{{ failedRunCount }}</strong>
          </div>
        </Card>
        <Card :bordered="false" class="ai-jobs__metric rounded-2xl" size="small">
          <div class="ai-jobs__metric-body">
            <span>草稿项</span>
            <strong>{{ draftItemCount }}</strong>
          </div>
        </Card>
      </div>

      <Card :bordered="false" class="ai-jobs__card rounded-2xl" title="任务定义">
        <template #extra>
          <Space>
            <Input v-model:value="jobQuery.keyword" allow-clear placeholder="搜索任务编码 / 名称" style="width: 220px" @press-enter="loadJobs" />
            <Select v-model:value="jobQuery.jobType" :options="jobTypeOptions" style="width: 190px" @change="loadJobs" />
            <Button @click="loadJobs">刷新</Button>
            <Button type="primary" @click="openCreateDrawer">新增任务</Button>
          </Space>
        </template>

        <Table
          :columns="jobColumns"
          :data-source="jobs"
          :loading="loadingJobs"
          :pagination="{ pageSize: 10, showSizeChanger: true }"
          row-key="id"
          size="middle"
          :scroll="{ x: 1180 }"
        >
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'name'">
              <div class="ai-jobs__primary-cell">
                <strong>{{ record.name }}</strong>
                <span>{{ record.jobCode }}</span>
                <em>{{ record.description || '未填写说明' }}</em>
              </div>
            </template>
            <template v-else-if="column.key === 'jobType'">
              <Tag color="blue">{{ jobTypeLabel(record.jobType) }}</Tag>
            </template>
            <template v-else-if="column.key === 'schedulerJobKey'">
              <Tag color="purple">{{ record.schedulerJobKey }}</Tag>
            </template>
            <template v-else-if="column.key === 'lastRunStatus'">
              <div class="ai-jobs__run-cell">
                <Tag :color="statusColor(record.lastRunStatus)">{{ record.lastRunStatus || 'Pending' }}</Tag>
                <span>{{ formatDate(record.lastRunTime) }}</span>
                <em>{{ record.lastRunSummary || '等待执行' }}</em>
              </div>
            </template>
            <template v-else-if="column.key === 'isEnabled'">
              <Switch
                :checked="record.isEnabled"
                :loading="actionLoadingKey === `toggle-${record.id}`"
                @change="(checked) => toggleJob(record, Boolean(checked))"
              />
            </template>
            <template v-else-if="column.key === 'action'">
              <Space>
                <Button size="small" type="link" @click="openEditDrawer(record)">编辑</Button>
                <Button :loading="actionLoadingKey === `run-${record.id}`" size="small" type="link" @click="runJob(record)">手动触发</Button>
                <Button size="small" type="link" @click="filterRunsByJob(record)">运行记录</Button>
                <Popconfirm title="确认删除该 AI 自动化任务？" @confirm="removeJob(record)">
                  <Button danger :loading="actionLoadingKey === `delete-${record.id}`" size="small" type="link">删除</Button>
                </Popconfirm>
              </Space>
            </template>
          </template>
        </Table>
      </Card>

      <Card :bordered="false" class="ai-jobs__card rounded-2xl" :title="`运行记录 · ${selectedJobName}`">
        <template #extra>
          <Space>
            <Select v-model:value="runQuery.runMode" :options="runModeOptions" style="width: 130px" @change="loadRuns" />
            <Select v-model:value="runQuery.status" :options="runStatusOptions" style="width: 130px" @change="loadRuns" />
            <Select v-model:value="runQuery.jobType" :options="jobTypeOptions" style="width: 190px" @change="loadRuns" />
            <Button @click="filterRunsByJob()">查看全部</Button>
            <Button @click="loadRuns">刷新</Button>
          </Space>
        </template>

        <Table
          :columns="runColumns"
          :data-source="runs"
          :loading="loadingRuns"
          :pagination="{ pageSize: 10, showSizeChanger: true }"
          row-key="id"
          size="middle"
          :scroll="{ x: 980 }"
        >
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'jobName'">
              <div class="ai-jobs__primary-cell">
                <strong>{{ record.jobName }}</strong>
                <span>{{ record.jobCode }}</span>
              </div>
            </template>
            <template v-else-if="column.key === 'runMode'">
              <Tag>{{ runModeLabel(record.runMode) }}</Tag>
            </template>
            <template v-else-if="column.key === 'status'">
              <Tag :color="statusColor(record.status)">{{ record.status }}</Tag>
            </template>
            <template v-else-if="column.key === 'elapsedMilliseconds'">
              {{ formatDuration(record.elapsedMilliseconds) }}
            </template>
            <template v-else-if="column.key === 'startedTime'">
              {{ formatDate(record.startedTime) }}
            </template>
            <template v-else-if="column.key === 'action'">
              <Space>
                <Button size="small" type="link" @click="openRunDrawer(record)">查看草稿</Button>
                <Button
                  v-if="record.status === 'Failed'"
                  :loading="actionLoadingKey === `run-${record.aiJobId}`"
                  size="small"
                  type="link"
                  @click="retryRun(record)"
                >
                  重试
                </Button>
              </Space>
            </template>
          </template>
        </Table>
      </Card>
    </div>

    <Drawer
      v-model:open="jobDrawerVisible"
      destroy-on-close
      :title="editingJobId ? '编辑 AI 自动化任务' : '新增 AI 自动化任务'"
      width="860px"
    >
      <Form :label-col="{ span: 6 }" :model="jobForm" :wrapper-col="{ span: 17 }">
        <Form.Item label="任务编码" required>
          <Input v-model:value="jobForm.jobCode" :disabled="Boolean(editingJobId)" :maxlength="64" placeholder="ai-weekly-project-summary" />
        </Form.Item>
        <Form.Item label="任务名称" required>
          <Input v-model:value="jobForm.name" :maxlength="128" placeholder="每周项目状态摘要" />
        </Form.Item>
        <Form.Item label="任务类型" required>
          <Select v-model:value="jobForm.jobType" :options="jobTypeOptions.filter((item) => item.value)" @change="handleJobTypeChange" />
        </Form.Item>
        <Form.Item label="助手角色">
          <Select v-model:value="jobForm.assistantCode" :options="assistantOptions" />
        </Form.Item>
        <Form.Item label="场景模板">
          <Input v-model:value="jobForm.promptTemplateCode" :maxlength="128" />
        </Form.Item>
        <Form.Item label="模型">
          <Select
            v-model:value="jobForm.modelId"
            allow-clear
            :loading="loadingModels"
            :options="modelOptions"
            placeholder="使用系统默认模型"
          />
        </Form.Item>
        <Form.Item label="平台调度绑定">
          <Input v-model:value="jobForm.schedulerJobKey" :maxlength="64" />
        </Form.Item>
        <Form.Item label="启用任务">
          <Switch v-model:checked="jobForm.isEnabled" />
        </Form.Item>
        <Form.Item label="任务说明">
          <Input.TextArea v-model:value="jobForm.description" :auto-size="{ minRows: 2, maxRows: 4 }" :maxlength="512" />
        </Form.Item>
        <Form.Item label="上下文范围">
          <Input.TextArea v-model:value="jobForm.contextScopeJson" :auto-size="{ minRows: 5, maxRows: 10 }" :maxlength="32_000" />
        </Form.Item>
        <Form.Item label="输出目标">
          <Input.TextArea v-model:value="jobForm.outputTargetJson" :auto-size="{ minRows: 4, maxRows: 8 }" :maxlength="32_000" />
        </Form.Item>
      </Form>
      <template #footer>
        <Space>
          <Button @click="jobDrawerVisible = false">取消</Button>
          <Button :loading="savingJob" type="primary" @click="saveJob">保存</Button>
        </Space>
      </template>
    </Drawer>

    <Drawer v-model:open="draftDrawerVisible" title="AI 自动化草稿" width="780px">
      <div v-if="activeRun" class="ai-jobs__drawer">
        <div class="ai-jobs__drawer-head">
          <Tag :color="statusColor(activeRun.status)">{{ activeRun.status }}</Tag>
          <span>{{ activeRun.jobName }}</span>
          <em>{{ formatDate(activeRun.startedTime) }}</em>
        </div>
        <div v-if="activeRun.errorMessage" class="ai-jobs__error">{{ activeRun.errorMessage }}</div>
        <h3>输出草稿</h3>
        <pre>{{ draftText }}</pre>
        <h3>输入快照</h3>
        <pre>{{ inputSnapshotText }}</pre>
      </div>
    </Drawer>
  </Page>
</template>

<style scoped>
@reference "@vben/tailwind-config/theme";
.ai-jobs {
  display: flex;
  flex-direction: column;
  @apply gap-4;
}

.ai-jobs__metrics {
  display: grid;
  grid-template-columns: repeat(5, minmax(0, 1fr));
  @apply gap-3;
}

.ai-jobs__metric-body {
  display: flex;
  flex-direction: column;
  @apply gap-1.5;
}

.ai-jobs__metric span {
  color: var(--ant-color-text-secondary);
  font-size: 13px;
}

.ai-jobs__metric strong {
  color: var(--ant-color-text);
  font-size: 24px;
  font-weight: 650;
  line-height: 1.2;
}

.ai-jobs__primary-cell,
.ai-jobs__run-cell {
  display: flex;
  flex-direction: column;
  @apply gap-1;
}

.ai-jobs__primary-cell strong,
.ai-jobs__run-cell strong {
  color: var(--ant-color-text);
  font-weight: 600;
}

.ai-jobs__primary-cell span,
.ai-jobs__run-cell span,
.ai-jobs__drawer-head em {
  color: var(--ant-color-text-secondary);
  font-size: 12px;
  font-style: normal;
}

.ai-jobs__primary-cell em,
.ai-jobs__run-cell em {
  color: var(--ant-color-text-tertiary);
  font-size: 12px;
  font-style: normal;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.ai-jobs__drawer {
  display: flex;
  flex-direction: column;
  @apply gap-3.5;
}

.ai-jobs__drawer-head {
  align-items: center;
  display: flex;
  @apply gap-2;
}

.ai-jobs__drawer h3 {
  font-size: 14px;
  font-weight: 600;
  @apply m-0;
}

.ai-jobs__drawer pre {
  background: var(--ant-color-fill-quaternary);
  border: 1px solid var(--ant-color-border-secondary);
  @apply rounded-md;
  color: var(--ant-color-text);
  font-size: 12px;
  line-height: 1.6;
  @apply m-0;
  max-height: 420px;
  overflow: auto;
  @apply p-3;
  white-space: pre-wrap;
  word-break: break-word;
}

.ai-jobs__error {
  background: var(--ant-color-error-bg);
  border: 1px solid var(--ant-color-error-border);
  @apply rounded-md;
  color: var(--ant-color-error-text);
  @apply px-3 py-2.5;
}

@media (max-width: 1200px) {
  .ai-jobs__metrics {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}
</style>
