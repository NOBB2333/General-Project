<script lang="ts" setup>
import type { NativeVxeGridEvents, VxeGridProps } from '#/adapter/vxe-table';
import type { IntegrationApi } from '#/api/core';

import { computed, onMounted, reactive, ref } from 'vue';

import { useAccess } from '@vben/access';
import { Page } from '@vben/common-ui';

import { Alert, Button, Card, Drawer, Form, Input, Modal, Popconfirm, Select, Space, Switch, Tag, message } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import {
  getIntegrationConnectorsApi,
  getIntegrationSyncJobsApi,
  getIntegrationSyncRecordsApi,
  retryIntegrationSyncRecordApi,
  runIntegrationSyncJobApi,
  saveIntegrationSyncJobApi,
  skipIntegrationSyncRecordApi,
  toggleIntegrationSyncJobApi,
} from '#/api/core';
import { useActionLoading } from '#/composables/platform/use-action-loading';

defineOptions({ name: 'IntegrationSyncJobsPage' });

const SYNC_MANAGE_CODE = 'Integration.Sync.Manage';
const SYNC_RUN_CODE = 'Integration.Sync.Run';

const statusOptions = [
  { label: '全部状态', value: 'All' },
  { label: '启用', value: 'Active' },
  { label: '停用', value: 'Disabled' },
];

const recordStatusOptions = [
  { label: '全部状态', value: 'All' },
  { label: '待执行', value: 'Pending' },
  { label: '执行中', value: 'Running' },
  { label: '成功', value: 'Succeeded' },
  { label: '失败', value: 'Failed' },
  { label: '已跳过', value: 'Skipped' },
];

const syncTypeOptions = [
  { label: '全部对象', value: '' },
  { label: '部门', value: 'Department' },
  { label: '用户', value: 'User' },
  { label: '项目群', value: 'Group' },
  { label: '会议', value: 'Meeting' },
  { label: '自定义', value: 'Custom' },
];

const modeOptions = [
  { label: '增量同步', value: 'Incremental' },
  { label: '全量同步', value: 'Full' },
  { label: '手动同步', value: 'Manual' },
];

const directionOptions = [
  { label: '外部到内部', value: 'Inbound' },
  { label: '内部到外部', value: 'Outbound' },
  { label: '双向', value: 'Bidirectional' },
];

const providerOptions = [
  { label: '全部平台', value: '' },
  { label: '飞书', value: 'Feishu' },
  { label: '钉钉', value: 'DingTalk' },
  { label: '企业微信', value: 'WeCom' },
];

const jobColumns = [
  { field: 'name', slots: { default: 'jobName' }, title: '同步任务', minWidth: 240 },
  { field: 'connectorCode', slots: { default: 'connector' }, title: '连接器', width: 150 },
  { field: 'syncType', slots: { default: 'syncType' }, title: '同步对象', width: 120 },
  { field: 'mode', slots: { default: 'mode' }, title: '默认模式', width: 120 },
  { field: 'direction', slots: { default: 'direction' }, title: '方向', width: 130 },
  { field: 'cronExpression', showOverflow: 'tooltip', title: 'Cron', minWidth: 160 },
  { field: 'lastRunStatus', slots: { default: 'lastRun' }, title: '最近执行', minWidth: 220 },
  { field: 'isEnabled', slots: { default: 'enabled' }, title: '启停', width: 100 },
  { fixed: 'right', slots: { default: 'jobActions' }, title: '操作', width: 280 },
] satisfies VxeGridProps<IntegrationApi.SyncJobItem>['columns'];

const recordColumns = [
  { field: 'creationTime', slots: { default: 'creationTime' }, title: '创建时间', width: 170 },
  { field: 'connectorCode', slots: { default: 'recordConnector' }, title: '连接器', width: 150 },
  { field: 'status', slots: { default: 'recordStatus' }, title: '状态', width: 110 },
  { field: 'syncType', slots: { default: 'syncType' }, title: '对象', width: 110 },
  { field: 'mode', slots: { default: 'mode' }, title: '模式', width: 110 },
  { field: 'totalCount', slots: { default: 'counts' }, title: '统计', minWidth: 260 },
  { field: 'summary', showOverflow: 'tooltip', title: '摘要 / 错误', minWidth: 280 },
  { field: 'durationMilliseconds', slots: { default: 'duration' }, title: '耗时', width: 100 },
  { fixed: 'right', slots: { default: 'recordActions' }, title: '操作', width: 220 },
] satisfies VxeGridProps<IntegrationApi.SyncRecordItem>['columns'];

const { hasAccessByCodes } = useAccess();
const { actionLoadingKey, runAction } = useActionLoading();
const canManage = computed(() => hasAccessByCodes([SYNC_MANAGE_CODE]));
const canRun = computed(() => hasAccessByCodes([SYNC_RUN_CODE]));
const connectors = ref<IntegrationApi.ConnectorItem[]>([]);
const jobs = ref<IntegrationApi.SyncJobItem[]>([]);
const records = ref<IntegrationApi.SyncRecordItem[]>([]);
const jobsLoading = ref(false);
const recordsLoading = ref(false);
const jobTotalCount = ref(0);
const recordTotalCount = ref(0);
const jobPageIndex = ref(1);
const jobPageSize = ref(20);
const recordPageIndex = ref(1);
const recordPageSize = ref(20);
const drawerOpen = ref(false);
const runModalOpen = ref(false);
const detailOpen = ref(false);
const skipModalOpen = ref(false);
const editingId = ref<string>();
const activeJob = ref<IntegrationApi.SyncJobItem>();
const activeRecord = ref<IntegrationApi.SyncRecordItem>();
const skipReason = ref('');

const query = reactive<IntegrationApi.SyncJobQuery>({
  connectorCode: '',
  keyword: '',
  providerType: '',
  status: 'All',
  syncType: '',
});

const recordQuery = reactive<IntegrationApi.SyncRecordQuery>({
  connectorCode: '',
  keyword: '',
  mode: '',
  providerType: '',
  status: 'All',
  syncJobId: '',
  syncType: '',
});

const form = reactive<IntegrationApi.SyncJobSaveInput>(createForm());
const runForm = reactive<IntegrationApi.SyncRunInput>({
  force: false,
  mode: 'Incremental',
  requestJson: '{}',
});

const connectorOptions = computed(() => [
  { label: '全部连接器', value: '' },
  ...connectors.value.map((item) => ({
    label: item.displayName,
    value: item.connectorCode,
  })),
]);

const editableConnectorOptions = computed(() =>
  connectors.value.map((item) => ({
    label: `${item.displayName} · ${item.providerType}`,
    value: item.id,
  })),
);

const jobOptions = computed(() => [
  { label: '全部任务', value: '' },
  ...jobs.value.map((item) => ({
    label: item.name,
    value: item.id,
  })),
]);

const jobGridOptions = computed<VxeGridProps<IntegrationApi.SyncJobItem>>(() => ({
  cellConfig: { height: 58 },
  columns: jobColumns,
  data: jobs.value,
  emptyText: '暂无同步任务',
  loading: jobsLoading.value,
  pagerConfig: {
    currentPage: jobPageIndex.value,
    pageSize: jobPageSize.value,
    pageSizes: [10, 20, 50, 100],
    total: jobTotalCount.value,
  },
  rowConfig: { keyField: 'id' },
  scrollX: { enabled: true },
  size: 'medium',
}));

const recordGridOptions = computed<VxeGridProps<IntegrationApi.SyncRecordItem>>(() => ({
  cellConfig: { height: 58 },
  columns: recordColumns,
  data: records.value,
  emptyText: '暂无同步记录',
  loading: recordsLoading.value,
  pagerConfig: {
    currentPage: recordPageIndex.value,
    pageSize: recordPageSize.value,
    pageSizes: [10, 20, 50, 100],
    total: recordTotalCount.value,
  },
  rowConfig: { keyField: 'id' },
  scrollX: { enabled: true },
  size: 'medium',
}));

const jobGridEvents: NativeVxeGridEvents<IntegrationApi.SyncJobItem> = {
  'page-change': ({ currentPage, pageSize: nextPageSize }) => {
    const normalizedPageSize = nextPageSize || jobPageSize.value;
    jobPageIndex.value = normalizedPageSize === jobPageSize.value ? currentPage || 1 : 1;
    jobPageSize.value = normalizedPageSize;
    void loadJobs();
  },
};

const recordGridEvents: NativeVxeGridEvents<IntegrationApi.SyncRecordItem> = {
  'page-change': ({ currentPage, pageSize: nextPageSize }) => {
    const normalizedPageSize = nextPageSize || recordPageSize.value;
    recordPageIndex.value = normalizedPageSize === recordPageSize.value ? currentPage || 1 : 1;
    recordPageSize.value = normalizedPageSize;
    void loadRecords();
  },
};

const [JobGrid] = useVbenVxeGrid<IntegrationApi.SyncJobItem>({
  gridEvents: jobGridEvents,
  gridOptions: {},
});

const [RecordGrid] = useVbenVxeGrid<IntegrationApi.SyncRecordItem>({
  gridEvents: recordGridEvents,
  gridOptions: {},
});

function createForm(): IntegrationApi.SyncJobSaveInput {
  return {
    configJson: '{}',
    connectorId: '',
    cronExpression: '',
    direction: 'Inbound',
    isEnabled: true,
    jobCode: '',
    mode: 'Incremental',
    name: '',
    remark: '',
    syncType: 'User',
  };
}

function resetForm() {
  Object.assign(form, createForm());
  editingId.value = undefined;
}

function buildJobQuery() {
  return {
    connectorCode: query.connectorCode || undefined,
    keyword: query.keyword?.trim() || undefined,
    maxResultCount: jobPageSize.value,
    providerType: query.providerType || undefined,
    skipCount: (jobPageIndex.value - 1) * jobPageSize.value,
    status: query.status || 'All',
    syncType: query.syncType || undefined,
  };
}

function buildRecordQuery() {
  return {
    connectorCode: recordQuery.connectorCode || undefined,
    keyword: recordQuery.keyword?.trim() || undefined,
    maxResultCount: recordPageSize.value,
    mode: recordQuery.mode || undefined,
    providerType: recordQuery.providerType || undefined,
    skipCount: (recordPageIndex.value - 1) * recordPageSize.value,
    status: recordQuery.status || 'All',
    syncJobId: recordQuery.syncJobId || undefined,
    syncType: recordQuery.syncType || undefined,
  };
}

function connectorLabel(code: string) {
  return connectorOptions.value.find((item) => item.value === code)?.label || code || '-';
}

function optionLabel(options: Array<{ label: string; value: string }>, value: string) {
  return options.find((item) => item.value === value)?.label || value || '-';
}

function formatDate(value?: string) {
  return value ? new Date(value).toLocaleString() : '-';
}

function formatJson(value: string) {
  try {
    return JSON.stringify(JSON.parse(value || '{}'), null, 2);
  } catch {
    return value || '{}';
  }
}

function statusColor(value: string) {
  if (value === 'Succeeded') {
    return 'green';
  }

  if (value === 'Failed') {
    return 'red';
  }

  if (value === 'Running' || value === 'Pending') {
    return 'blue';
  }

  return 'default';
}

function validateJson(value: string, label: string) {
  try {
    JSON.parse(value.trim() || '{}');
  } catch {
    throw new Error(`${label} 必须是合法 JSON`);
  }
}

async function loadConnectors() {
  connectors.value = await getIntegrationConnectorsApi({ includeDisabled: true });
}

async function loadJobs() {
  jobsLoading.value = true;
  try {
    const result = await getIntegrationSyncJobsApi(buildJobQuery());
    jobs.value = result.items;
    jobTotalCount.value = result.totalCount;
  } finally {
    jobsLoading.value = false;
  }
}

async function loadRecords() {
  recordsLoading.value = true;
  try {
    const result = await getIntegrationSyncRecordsApi(buildRecordQuery());
    records.value = result.items;
    recordTotalCount.value = result.totalCount;
  } finally {
    recordsLoading.value = false;
  }
}

async function refreshJobs(resetPage = false) {
  if (resetPage) {
    jobPageIndex.value = 1;
  }

  await loadJobs();
}

async function refreshRecords(resetPage = false) {
  if (resetPage) {
    recordPageIndex.value = 1;
  }

  await loadRecords();
}

function openCreate() {
  resetForm();
  form.connectorId = connectors.value[0]?.id || '';
  drawerOpen.value = true;
}

function openEdit(job: IntegrationApi.SyncJobItem) {
  editingId.value = job.id;
  Object.assign(form, {
    configJson: job.configJson || '{}',
    connectorId: job.connectorId,
    cronExpression: job.cronExpression,
    direction: job.direction,
    id: job.id,
    isEnabled: job.isEnabled,
    jobCode: job.jobCode,
    mode: job.mode,
    name: job.name,
    remark: job.remark,
    syncType: job.syncType,
  });
  drawerOpen.value = true;
}

function openRun(job: IntegrationApi.SyncJobItem, mode = '') {
  activeJob.value = job;
  runForm.force = !job.isEnabled;
  runForm.mode = mode || job.mode || 'Incremental';
  runForm.requestJson = '{}';
  runModalOpen.value = true;
}

function openRecordDetail(record: IntegrationApi.SyncRecordItem) {
  activeRecord.value = record;
  detailOpen.value = true;
}

function openSkip(record: IntegrationApi.SyncRecordItem) {
  activeRecord.value = record;
  skipReason.value = '';
  skipModalOpen.value = true;
}

async function submitJob() {
  try {
    validateJson(form.configJson, '配置 JSON');
  } catch (error) {
    message.warning((error as Error).message);
    return;
  }

  await runAction('save-sync-job', async () => {
    await saveIntegrationSyncJobApi({
      ...form,
      id: editingId.value,
      cronExpression: form.cronExpression.trim(),
      jobCode: form.jobCode.trim(),
      name: form.name.trim(),
      remark: form.remark.trim(),
    });
    message.success('同步任务已保存。');
    drawerOpen.value = false;
    await refreshJobs(!editingId.value);
  });
}

async function toggleJob(job: IntegrationApi.SyncJobItem, checked: boolean | number | string) {
  await runAction(`toggle:${job.id}`, async () => {
    await toggleIntegrationSyncJobApi(job.id, { isEnabled: Boolean(checked) });
    message.success(Boolean(checked) ? '同步任务已启用。' : '同步任务已停用。');
    await loadJobs();
  });
}

async function submitRun() {
  if (!activeJob.value) {
    return;
  }

  try {
    validateJson(runForm.requestJson, '执行请求 JSON');
  } catch (error) {
    message.warning((error as Error).message);
    return;
  }

  await runAction(`run:${activeJob.value.id}`, async () => {
    await runIntegrationSyncJobApi(activeJob.value!.id, {
      force: runForm.force,
      mode: runForm.mode,
      requestJson: runForm.requestJson.trim() || '{}',
    });
    message.success('同步执行记录已创建。');
    runModalOpen.value = false;
    await Promise.all([loadJobs(), refreshRecords(true)]);
  });
}

async function retryRecord(record: IntegrationApi.SyncRecordItem) {
  await runAction(`retry:${record.id}`, async () => {
    await retryIntegrationSyncRecordApi(record.id);
    message.success('已重新执行同步记录。');
    await Promise.all([loadJobs(), loadRecords()]);
  });
}

async function submitSkip() {
  if (!activeRecord.value) {
    return;
  }

  await runAction(`skip:${activeRecord.value.id}`, async () => {
    await skipIntegrationSyncRecordApi(activeRecord.value!.id, { reason: skipReason.value.trim() });
    message.success('已标记跳过。');
    skipModalOpen.value = false;
    await loadRecords();
  });
}

onMounted(async () => {
  await loadConnectors();
  await Promise.all([loadJobs(), loadRecords()]);
});
</script>

<template>
  <Page
    description="统一管理飞书、钉钉、企业微信等外部系统的数据同步任务，记录执行统计、冲突、失败重试和人工跳过。"
    title="同步任务"
  >
    <div class="integration-sync flex flex-col gap-4">
      <Alert
        show-icon
        type="info"
        message="当前已落地同步任务基座：任务定义、手动执行、执行记录、失败重试、冲突字段承载和人工跳过。真实组织、用户、群和会议同步由各平台适配器继续接入。"
      />

      <Card :bordered="false" class="rounded-2xl" title="任务列表">
        <div class="integration-sync__filters mb-3 grid gap-3">
          <Input v-model:value="query.keyword" allow-clear placeholder="搜索任务编码、名称、备注、执行摘要" @press-enter="refreshJobs(true)" />
          <Select v-model:value="query.connectorCode" :options="connectorOptions" @change="refreshJobs(true)" />
          <Select v-model:value="query.providerType" :options="providerOptions" @change="refreshJobs(true)" />
          <Select v-model:value="query.syncType" :options="syncTypeOptions" @change="refreshJobs(true)" />
          <Select v-model:value="query.status" :options="statusOptions" @change="refreshJobs(true)" />
          <Button type="primary" @click="refreshJobs(true)">查询</Button>
          <Button v-if="canManage" @click="openCreate">新增任务</Button>
        </div>

        <JobGrid :grid-events="jobGridEvents" :grid-options="jobGridOptions" class="app-vxe-card-grid">
          <template #jobName="{ row }">
            <div class="integration-sync__stack-cell">
              <strong>{{ row.name }}</strong>
              <span>{{ row.jobCode }}</span>
            </div>
          </template>

          <template #connector="{ row }">
            <div class="integration-sync__stack-cell">
              <strong>{{ connectorLabel(row.connectorCode) }}</strong>
              <span>{{ row.providerType }}</span>
            </div>
          </template>

          <template #syncType="{ row }">
            <Tag>{{ optionLabel(syncTypeOptions, row.syncType) }}</Tag>
          </template>

          <template #mode="{ row }">
            {{ optionLabel(modeOptions, row.mode) }}
          </template>

          <template #direction="{ row }">
            {{ optionLabel(directionOptions, row.direction) }}
          </template>

          <template #lastRun="{ row }">
            <div class="integration-sync__stack-cell">
              <Tag v-if="row.lastRunStatus" :color="statusColor(row.lastRunStatus)">{{ optionLabel(recordStatusOptions, row.lastRunStatus) }}</Tag>
              <span>{{ formatDate(row.lastRunTime) }}</span>
              <span>{{ row.lastRunSummary || row.lastRunError || '-' }}</span>
            </div>
          </template>

          <template #enabled="{ row }">
            <Switch
              :checked="row.isEnabled"
              :disabled="!canManage"
              :loading="actionLoadingKey === `toggle:${row.id}`"
              checked-children="启用"
              un-checked-children="停用"
              @change="(checked) => toggleJob(row, checked)"
            />
          </template>

          <template #jobActions="{ row }">
            <Space>
              <Button v-if="canRun" size="small" type="link" @click="openRun(row, 'Incremental')">增量</Button>
              <Button v-if="canRun" size="small" type="link" @click="openRun(row, 'Full')">全量</Button>
              <Button v-if="canManage" size="small" type="link" @click="openEdit(row)">编辑</Button>
              <Button
                size="small"
                type="link"
                @click="
                  recordQuery.syncJobId = row.id;
                  refreshRecords(true);
                "
              >
                记录
              </Button>
            </Space>
          </template>
        </JobGrid>
      </Card>

      <Card :bordered="false" class="rounded-2xl" title="执行记录">
        <div class="integration-sync__filters integration-sync__record-filters mb-3 grid gap-3">
          <Input v-model:value="recordQuery.keyword" allow-clear placeholder="搜索摘要、错误、请求 JSON" @press-enter="refreshRecords(true)" />
          <Select v-model:value="recordQuery.syncJobId" :options="jobOptions" @change="refreshRecords(true)" />
          <Select v-model:value="recordQuery.connectorCode" :options="connectorOptions" @change="refreshRecords(true)" />
          <Select v-model:value="recordQuery.syncType" :options="syncTypeOptions" @change="refreshRecords(true)" />
          <Select v-model:value="recordQuery.status" :options="recordStatusOptions" @change="refreshRecords(true)" />
          <Button type="primary" @click="refreshRecords(true)">查询</Button>
        </div>

        <RecordGrid :grid-events="recordGridEvents" :grid-options="recordGridOptions" class="app-vxe-card-grid">
          <template #creationTime="{ row }">
            {{ formatDate(row.creationTime) }}
          </template>

          <template #recordConnector="{ row }">
            <div class="integration-sync__stack-cell">
              <strong>{{ connectorLabel(row.connectorCode) }}</strong>
              <span>{{ row.providerType }}</span>
            </div>
          </template>

          <template #recordStatus="{ row }">
            <Tag :color="statusColor(row.status)">{{ optionLabel(recordStatusOptions, row.status) }}</Tag>
          </template>

          <template #syncType="{ row }">
            {{ optionLabel(syncTypeOptions, row.syncType) }}
          </template>

          <template #mode="{ row }">
            {{ optionLabel(modeOptions, row.mode) }}
          </template>

          <template #counts="{ row }">
            <Space wrap>
              <Tag>总数 {{ row.totalCount }}</Tag>
              <Tag color="green">新增 {{ row.createdCount }}</Tag>
              <Tag color="blue">更新 {{ row.updatedCount }}</Tag>
              <Tag color="orange">冲突 {{ row.conflictCount }}</Tag>
              <Tag color="red">失败 {{ row.failedCount }}</Tag>
            </Space>
          </template>

          <template #duration="{ row }">
            {{ row.durationMilliseconds }} ms
          </template>

          <template #recordActions="{ row }">
            <Space>
              <Button size="small" type="link" @click="openRecordDetail(row)">详情</Button>
              <Popconfirm v-if="canRun && row.status === 'Failed'" title="重新执行该同步记录？" @confirm="retryRecord(row)">
                <Button :loading="actionLoadingKey === `retry:${row.id}`" size="small" type="link">重试</Button>
              </Popconfirm>
              <Button v-if="canManage && !['Succeeded', 'Skipped'].includes(row.status)" size="small" type="link" danger @click="openSkip(row)">跳过</Button>
            </Space>
          </template>
        </RecordGrid>
      </Card>
    </div>

    <Drawer v-model:open="drawerOpen" :title="editingId ? '编辑同步任务' : '新增同步任务'" width="760">
      <Form :label-col="{ span: 6 }" :model="form" :wrapper-col="{ span: 18 }">
        <Form.Item label="任务编码" required>
          <Input v-model:value="form.jobCode" :disabled="Boolean(editingId)" :maxlength="64" placeholder="例如 feishu-user-sync" />
        </Form.Item>
        <Form.Item label="任务名称" required>
          <Input v-model:value="form.name" :maxlength="128" placeholder="例如 飞书用户增量同步" />
        </Form.Item>
        <Form.Item label="连接器" required>
          <Select v-model:value="form.connectorId" :options="editableConnectorOptions" />
        </Form.Item>
        <Form.Item label="同步对象">
          <Select v-model:value="form.syncType" :options="syncTypeOptions.filter((item) => item.value)" />
        </Form.Item>
        <Form.Item label="同步方向">
          <Select v-model:value="form.direction" :options="directionOptions" />
        </Form.Item>
        <Form.Item label="默认模式">
          <Select v-model:value="form.mode" :options="modeOptions" />
        </Form.Item>
        <Form.Item label="Cron">
          <Input v-model:value="form.cronExpression" :maxlength="128" placeholder="例如 0 */30 * * * ?" />
        </Form.Item>
        <Form.Item label="启用">
          <Switch v-model:checked="form.isEnabled" checked-children="启用" un-checked-children="停用" />
        </Form.Item>
        <Form.Item label="配置 JSON">
          <Input.TextArea v-model:value="form.configJson" :auto-size="{ minRows: 5, maxRows: 10 }" :maxlength="16000" />
        </Form.Item>
        <Form.Item label="备注">
          <Input.TextArea v-model:value="form.remark" :auto-size="{ minRows: 3, maxRows: 6 }" :maxlength="512" />
        </Form.Item>
      </Form>

      <template #footer>
        <Space>
          <Button @click="drawerOpen = false">取消</Button>
          <Button :loading="actionLoadingKey === 'save-sync-job'" type="primary" @click="submitJob">保存</Button>
        </Space>
      </template>
    </Drawer>

    <Modal
      v-model:open="runModalOpen"
      :confirm-loading="actionLoadingKey === `run:${activeJob?.id}`"
      :title="activeJob ? `执行同步 · ${activeJob.name}` : '执行同步'"
      width="720px"
      @ok="submitRun"
    >
      <Form :label-col="{ span: 6 }" :model="runForm" :wrapper-col="{ span: 18 }">
        <Form.Item label="执行模式">
          <Select v-model:value="runForm.mode" :options="modeOptions" />
        </Form.Item>
        <Form.Item label="强制执行">
          <Switch v-model:checked="runForm.force" checked-children="是" un-checked-children="否" />
        </Form.Item>
        <Form.Item label="请求 JSON">
          <Input.TextArea v-model:value="runForm.requestJson" :auto-size="{ minRows: 5, maxRows: 10 }" :maxlength="16000" />
        </Form.Item>
      </Form>
    </Modal>

    <Drawer v-model:open="detailOpen" :title="activeRecord ? `同步记录 · ${activeRecord.connectorCode}` : '同步记录'" width="820">
      <template v-if="activeRecord">
        <div class="integration-sync__detail flex flex-col gap-4">
          <div class="integration-sync__detail-grid grid items-center gap-3">
            <span>状态</span>
            <Tag :color="statusColor(activeRecord.status)">{{ optionLabel(recordStatusOptions, activeRecord.status) }}</Tag>
            <span>同步对象</span>
            <strong>{{ optionLabel(syncTypeOptions, activeRecord.syncType) }}</strong>
            <span>执行模式</span>
            <strong>{{ optionLabel(modeOptions, activeRecord.mode) }}</strong>
            <span>统计</span>
            <strong>总 {{ activeRecord.totalCount }} / 新增 {{ activeRecord.createdCount }} / 更新 {{ activeRecord.updatedCount }} / 冲突 {{ activeRecord.conflictCount }} / 失败 {{ activeRecord.failedCount }}</strong>
            <span>摘要</span>
            <strong>{{ activeRecord.summary || activeRecord.errorMessage || '-' }}</strong>
            <span>开始时间</span>
            <strong>{{ formatDate(activeRecord.startedTime) }}</strong>
            <span>完成时间</span>
            <strong>{{ formatDate(activeRecord.finishedTime) }}</strong>
          </div>

          <section>
            <h3>请求 JSON</h3>
            <pre>{{ formatJson(activeRecord.requestJson) }}</pre>
          </section>
          <section>
            <h3>结果 JSON</h3>
            <pre>{{ formatJson(activeRecord.resultJson) }}</pre>
          </section>
          <section>
            <h3>冲突 JSON</h3>
            <pre>{{ formatJson(activeRecord.conflictJson) }}</pre>
          </section>
        </div>
      </template>
    </Drawer>

    <Modal
      v-model:open="skipModalOpen"
      :confirm-loading="actionLoadingKey === `skip:${activeRecord?.id}`"
      title="标记跳过"
      @ok="submitSkip"
    >
      <Input.TextArea v-model:value="skipReason" :auto-size="{ minRows: 4, maxRows: 8 }" :maxlength="1000" placeholder="请输入跳过原因" />
    </Modal>
  </Page>
</template>

<style scoped>
@reference "@vben/tailwind-config/theme";
.integration-sync {
  display: flex;
  flex-direction: column;
   @apply gap-4;
}

.integration-sync__filters {
  display: grid;
   @apply gap-3;
  grid-template-columns: minmax(240px, 1.4fr) repeat(4, minmax(140px, 0.7fr)) auto auto;
   @apply mb-3;
}

.integration-sync__record-filters {
  grid-template-columns: minmax(240px, 1.4fr) repeat(4, minmax(140px, 0.7fr)) auto;
}

.integration-sync__stack-cell {
  display: flex;
  flex-direction: column;
   @apply gap-0.5;
  min-width: 0;
}

.integration-sync__stack-cell span {
  overflow: hidden;
  color: var(--ant-color-text-secondary);
  font-size: 12px;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.integration-sync__detail {
  display: flex;
  flex-direction: column;
   @apply gap-4;
}

.integration-sync__detail-grid {
  display: grid;
  align-items: center;
   @apply gap-x-4 gap-y-3;
  grid-template-columns: 110px minmax(0, 1fr);
}

.integration-sync__detail-grid > span {
  color: var(--ant-color-text-secondary);
}

.integration-sync__detail h3 {
   @apply mb-2;
  color: var(--ant-color-text);
  font-size: 14px;
  font-weight: 600;
}

.integration-sync__detail pre {
  max-height: 260px;
   @apply p-3;
  overflow: auto;
  border: 1px solid var(--ant-color-border-secondary);
   @apply rounded-md;
  background: var(--ant-color-fill-quaternary);
  color: var(--ant-color-text);
  font-size: 12px;
}

@media (max-width: 1200px) {
  .integration-sync__filters,
  .integration-sync__record-filters {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}
</style>
