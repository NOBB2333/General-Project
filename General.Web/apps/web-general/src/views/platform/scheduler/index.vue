<script lang="ts" setup>
import type { NativeVxeGridEvents, VxeGridProps } from '#/adapter/vxe-table';

import { computed, onMounted, reactive, ref } from 'vue';

import { Page } from '@vben/common-ui';
import { useAccess } from '@vben/access';

import {
  Button,
  Card,
  Drawer,
  Form,
  Input,
  Popconfirm,
  Select,
  Space,
  Statistic,
  Switch,
  Tag,
  message,
} from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';

import {
  cancelSchedulerJobApi,
  clearSchedulerJobRecordsBatchApi,
  clearSchedulerJobRecordsApi,
  createSchedulerJobApi,
  createSchedulerJobTriggerApi,
  deleteSchedulerJobApi,
  deleteSchedulerJobTriggerApi,
  getSchedulerClusterNodesApi,
  getSchedulerDashboardApi,
  getSchedulerHandlersApi,
  getSchedulerJobRecordsApi,
  getSchedulerJobTriggersApi,
  getSchedulerListApi,
  runSchedulerJobApi,
  runSchedulerJobsBatchApi,
  toggleSchedulerJobApi,
  toggleSchedulerJobsBatchApi,
  toggleSchedulerJobTriggerApi,
  updateSchedulerJobApi,
  updateSchedulerJobTriggerApi,
  type SchedulerApi,
} from '#/api/core';
import { useLocalVxePagination } from '#/composables/platform/use-local-vxe-pagination';

defineOptions({ name: 'PlatformSchedulerPage' });

const SCHEDULER_MANAGE_CODE = 'Platform.Scheduler.Manage';
const SCHEDULER_EXECUTE_CODE = 'Platform.Scheduler.Execute';
const { hasAccessByCodes } = useAccess();

const loading = ref(false);
const actionLoadingKey = ref('');
const items = ref<SchedulerApi.JobItem[]>([]);
const handlers = ref<SchedulerApi.HandlerItem[]>([]);
const dashboard = ref<SchedulerApi.Dashboard>();
const selectedJobKeys = ref<string[]>([]);
const recordDrawerOpen = ref(false);
const recordLoading = ref(false);
const triggerDrawerOpen = ref(false);
const triggerLoading = ref(false);
const nodeDrawerOpen = ref(false);
const nodeLoading = ref(false);
const jobDrawerOpen = ref(false);
const triggerEditorDrawerOpen = ref(false);
const selectedJob = ref<SchedulerApi.JobItem>();
const editingJobKey = ref('');
const editingTriggerKey = ref('');
const jobRecords = ref<SchedulerApi.JobRecordItem[]>([]);
const jobTriggers = ref<SchedulerApi.TriggerItem[]>([]);
const clusterNodes = ref<SchedulerApi.ClusterNodeItem[]>([]);
const jobPagination = useLocalVxePagination(items, { defaultPageSize: 10 });
const triggerPagination = useLocalVxePagination(jobTriggers, { defaultPageSize: 10 });
const nodePagination = useLocalVxePagination(clusterNodes, { defaultPageSize: 10 });
const totalCount = ref(0);
const recordPageIndex = ref(1);
const recordPageSize = ref(10);
const recordTotalCount = ref(0);
const canExecuteScheduler = computed(() => hasAccessByCodes([SCHEDULER_EXECUTE_CODE]));
const canManageScheduler = computed(() => hasAccessByCodes([SCHEDULER_MANAGE_CODE]));

const jobForm = reactive<SchedulerApi.JobSaveInput>({
  cronExpression: '0 0/5 * * * ?',
  description: '',
  handlerKey: '',
  isEnabled: true,
  jobKey: '',
  title: '',
});

const triggerForm = reactive<SchedulerApi.TriggerSaveInput>({
  cronExpression: '0 0/5 * * * ?',
  description: '',
  isEnabled: true,
  title: '',
  triggerKey: '',
});

const metrics = computed(() => [
  { label: '任务总数', value: dashboard.value?.totalCount ?? items.value.length },
  { label: '启用任务', value: dashboard.value?.enabledCount ?? items.value.filter((item) => item.isEnabled).length },
  { label: '执行中', value: dashboard.value?.runningCount ?? items.value.filter((item) => item.isRunning).length },
  {
    label: '近 24h 失败',
    value: dashboard.value?.failedLast24Hours ?? 0,
  },
  {
    label: '近 24h 慢任务',
    value: dashboard.value?.slowLast24Hours ?? 0,
  },
]);

const handlerOptions = computed(() =>
  handlers.value.map((item) => ({
    label: item.handlerKey,
    value: item.handlerKey,
  })),
);

const columns = [
  { field: 'title', title: '任务名称', width: 180 },
  { field: 'jobKey', title: '任务键', width: 170 },
  { field: 'handlerKey', slots: { default: 'handlerKey' }, title: '执行器', width: 190 },
  { field: 'cronExpression', title: '默认 Cron', width: 160 },
  { field: 'triggerCount', title: '触发器', width: 90 },
  { field: 'nextRunTime', slots: { default: 'nextRunTime' }, title: '下次执行', width: 160 },
  { field: 'lastRunResult', slots: { default: 'lastRunResult' }, title: '最近结果', width: 240 },
  { field: 'isEnabled', slots: { default: 'isEnabled' }, title: '状态', width: 100 },
  { field: 'actions', fixed: 'right', slots: { default: 'actions' }, title: '操作', width: 430 },
];

const recordColumns = [
  { field: 'status', slots: { default: 'status' }, title: '状态', width: 90 },
  { field: 'triggerKey', slots: { default: 'triggerKey' }, title: '触发器', width: 120 },
  { field: 'triggerMode', slots: { default: 'triggerMode' }, title: '触发', width: 90 },
  { field: 'startTime', slots: { default: 'startTime' }, title: '开始时间', width: 170 },
  { field: 'endTime', slots: { default: 'endTime' }, title: '结束时间', width: 170 },
  { field: 'durationMilliseconds', slots: { default: 'durationMilliseconds' }, title: '耗时', width: 100 },
  { field: 'result', slots: { default: 'result' }, title: '结果', width: 360 },
];

const triggerColumns = [
  { field: 'title', title: '触发器', width: 160 },
  { field: 'triggerKey', title: '触发器键', width: 140 },
  { field: 'cronExpression', title: 'Cron', width: 160 },
  { field: 'nextRunTime', slots: { default: 'nextRunTime' }, title: '下次执行', width: 160 },
  { field: 'lastRunResult', title: '最近结果', width: 220 },
  { field: 'isEnabled', slots: { default: 'isEnabled' }, title: '状态', width: 100 },
  { field: 'actions', fixed: 'right', slots: { default: 'actions' }, title: '操作', width: 220 },
];

const nodeColumns = [
  { field: 'status', slots: { default: 'status' }, title: '状态', width: 90 },
  { field: 'hostName', title: '主机', width: 150 },
  { field: 'processId', title: '进程', width: 90 },
  { field: 'instanceId', title: '实例', width: 280 },
  { field: 'startedAt', slots: { default: 'startedAt' }, title: '启动时间', width: 170 },
  { field: 'lastHeartbeatTime', slots: { default: 'lastHeartbeatTime' }, title: '心跳时间', width: 170 },
  { field: 'description', title: '说明', width: 260 },
];

async function loadData() {
  loading.value = true;
  try {
    const [jobList, handlerList, dashboardData] = await Promise.all([
      getSchedulerListApi(),
      getSchedulerHandlersApi(),
      getSchedulerDashboardApi(),
    ]);
    items.value = jobList;
    handlers.value = handlerList;
    dashboard.value = dashboardData;
    selectedJobKeys.value = selectedJobKeys.value.filter((key) => jobList.some((item) => item.jobKey === key));
    totalCount.value = jobList.length;
  } finally {
    loading.value = false;
  }
}

const jobGridOptions = computed<VxeGridProps<SchedulerApi.JobItem>>(() => ({
  checkboxConfig: canExecuteScheduler.value || canManageScheduler.value
    ? {
        checkMethod: ({ row }: { row: SchedulerApi.JobItem }) => !row.isRunning,
      }
    : undefined,
  cellConfig: {
    height: 58,
  },
  cellClassName: ({ column }: { column: { field?: string } }) =>
    column.field === 'lastRunResult' ? 'scheduler__last-result-column' : '',
  columns: columns as VxeGridProps<SchedulerApi.JobItem>['columns'],
  data: jobPagination.pagedRows.value,
  emptyText: '暂无定时任务',
  loading: loading.value,
  pagerConfig: jobPagination.pagerConfig.value,
  rowConfig: { keyField: 'jobKey' },
  scrollX: { enabled: true },
}));

const jobGridEvents: NativeVxeGridEvents<SchedulerApi.JobItem> = {
  'checkbox-all': ({ records }: { records: SchedulerApi.JobItem[] }) => {
    selectedJobKeys.value = records.map((r) => r.jobKey);
  },
  'checkbox-change': ({ records }: { records: SchedulerApi.JobItem[] }) => {
    selectedJobKeys.value = records.map((r) => r.jobKey);
  },
  'page-change': jobPagination.handlePageChange,
};

const recordGridOptions = computed<VxeGridProps<SchedulerApi.JobRecordItem>>(() => ({
  columns: recordColumns as VxeGridProps<SchedulerApi.JobRecordItem>['columns'],
  data: jobRecords.value,
  emptyText: '暂无执行记录',
  loading: recordLoading.value,
  pagerConfig: {
    currentPage: recordPageIndex.value,
    pageSize: recordPageSize.value,
    total: recordTotalCount.value,
    pageSizes: [10, 20, 50],
  },
  rowConfig: { keyField: 'startTime' },
  scrollX: { enabled: true },
}));

const recordGridEvents: NativeVxeGridEvents<SchedulerApi.JobRecordItem> = {
  'page-change': ({ currentPage, pageSize: nextPageSize }) => {
    const normalizedPageSize = nextPageSize || recordPageSize.value;
    recordPageIndex.value = normalizedPageSize === recordPageSize.value ? currentPage || 1 : 1;
    recordPageSize.value = normalizedPageSize;
    if (selectedJob.value) {
      void loadJobRecords(selectedJob.value);
    }
  },
};

const triggerGridOptions = computed<VxeGridProps<SchedulerApi.TriggerItem>>(() => ({
  columns: triggerColumns as VxeGridProps<SchedulerApi.TriggerItem>['columns'],
  data: triggerPagination.pagedRows.value,
  emptyText: '暂无触发器',
  loading: triggerLoading.value,
  pagerConfig: triggerPagination.pagerConfig.value,
  rowConfig: { keyField: 'triggerKey' },
  scrollX: { enabled: true },
}));

const nodeGridOptions = computed<VxeGridProps<SchedulerApi.ClusterNodeItem>>(() => ({
  columns: nodeColumns as VxeGridProps<SchedulerApi.ClusterNodeItem>['columns'],
  data: nodePagination.pagedRows.value,
  emptyText: '暂无在线节点',
  loading: nodeLoading.value,
  pagerConfig: nodePagination.pagerConfig.value,
  rowConfig: { keyField: 'instanceId' },
  scrollX: { enabled: true },
}));

const [JobGrid] = useVbenVxeGrid<SchedulerApi.JobItem>({
  gridEvents: jobGridEvents,
  gridOptions: {},
});
const [RecordGrid] = useVbenVxeGrid<SchedulerApi.JobRecordItem>({
  gridEvents: recordGridEvents,
  gridOptions: {},
});
const triggerGridEvents: NativeVxeGridEvents<SchedulerApi.TriggerItem> = {
  'page-change': triggerPagination.handlePageChange,
};
const nodeGridEvents: NativeVxeGridEvents<SchedulerApi.ClusterNodeItem> = {
  'page-change': nodePagination.handlePageChange,
};
const [TriggerGrid] = useVbenVxeGrid<SchedulerApi.TriggerItem>({
  gridEvents: triggerGridEvents,
  gridOptions: {},
});
const [NodeGrid] = useVbenVxeGrid<SchedulerApi.ClusterNodeItem>({
  gridEvents: nodeGridEvents,
  gridOptions: {},
});

function formatDate(value?: null | string) {
  return value ? new Date(value).toLocaleString() : '-';
}

function formatDuration(value?: null | number) {
  if (value === null || value === undefined) {
    return '-';
  }
  if (value < 1000) {
    return `${value}ms`;
  }
  return `${(value / 1000).toFixed(1)}s`;
}

function getRecordStatusColor(status: string) {
  switch (status) {
    case 'Success': {
      return 'success';
    }
    case 'Failed': {
      return 'error';
    }
    case 'Cancelled': {
      return 'warning';
    }
    case 'Running': {
      return 'processing';
    }
    case 'Skipped': {
      return 'default';
    }
    default: {
      return 'default';
    }
  }
}

function patchJobForm(item?: SchedulerApi.JobItem) {
  editingJobKey.value = item?.jobKey ?? '';
  jobForm.cronExpression = item?.cronExpression ?? '0 0/5 * * * ?';
  jobForm.description = item?.description ?? '';
  jobForm.handlerKey = item?.handlerKey || handlers.value[0]?.handlerKey || '';
  jobForm.isEnabled = item?.isEnabled ?? true;
  jobForm.jobKey = item?.jobKey ?? '';
  jobForm.title = item?.title ?? '';
}

function patchTriggerForm(item?: SchedulerApi.TriggerItem) {
  editingTriggerKey.value = item?.triggerKey ?? '';
  triggerForm.cronExpression = item?.cronExpression ?? '0 0/5 * * * ?';
  triggerForm.description = item?.description ?? '';
  triggerForm.isEnabled = item?.isEnabled ?? true;
  triggerForm.title = item?.title ?? '';
  triggerForm.triggerKey = item?.triggerKey ?? '';
}

function validateJobForm() {
  if (!jobForm.jobKey.trim() || !jobForm.title.trim() || !jobForm.handlerKey.trim() || !jobForm.cronExpression.trim()) {
    message.warning('请补齐任务键、任务名称、执行器和 Cron');
    return false;
  }
  return true;
}

function validateTriggerForm() {
  if (!triggerForm.triggerKey.trim() || !triggerForm.title.trim() || !triggerForm.cronExpression.trim()) {
    message.warning('请补齐触发器键、触发器名称和 Cron');
    return false;
  }
  return true;
}

function openCreateJob() {
  patchJobForm();
  jobDrawerOpen.value = true;
}

function openEditJob(item: SchedulerApi.JobItem) {
  patchJobForm(item);
  jobDrawerOpen.value = true;
}

async function submitJob() {
  if (!validateJobForm()) {
    return;
  }

  actionLoadingKey.value = 'job:save';
  try {
    if (editingJobKey.value) {
      await updateSchedulerJobApi(editingJobKey.value, { ...jobForm });
      message.success('已更新任务');
    } else {
      await createSchedulerJobApi({ ...jobForm });
      message.success('已新增任务');
    }
    jobDrawerOpen.value = false;
    await loadData();
  } finally {
    actionLoadingKey.value = '';
  }
}

async function toggleJob(item: SchedulerApi.JobItem, isEnabled: boolean) {
  actionLoadingKey.value = `${item.jobKey}:toggle`;
  try {
    await toggleSchedulerJobApi(item.jobKey, isEnabled);
    message.success(isEnabled ? '已启用' : '已停用');
    await loadData();
  } finally {
    actionLoadingKey.value = '';
  }
}

async function cancelJob(item: SchedulerApi.JobItem) {
  actionLoadingKey.value = `${item.jobKey}:cancel`;
  try {
    const result = await cancelSchedulerJobApi(item.jobKey);
    message.info(result);
    await loadData();
    if (selectedJob.value?.jobKey === item.jobKey) {
      await loadJobRecords(item);
    }
  } finally {
    actionLoadingKey.value = '';
  }
}

async function deleteJob(item: SchedulerApi.JobItem) {
  actionLoadingKey.value = `${item.jobKey}:delete`;
  try {
    await deleteSchedulerJobApi(item.jobKey);
    message.success('已删除任务');
    await loadData();
  } finally {
    actionLoadingKey.value = '';
  }
}

async function runJob(item: SchedulerApi.JobItem) {
  actionLoadingKey.value = `${item.jobKey}:run`;
  try {
    const result = await runSchedulerJobApi(item.jobKey);
    if (result.includes('失败')) {
      message.warning(result);
    } else {
      message.success(result);
    }
    await loadData();
  } finally {
    actionLoadingKey.value = '';
  }
}

function ensureBatchSelection() {
  if (selectedJobKeys.value.length === 0) {
    message.warning('请先选择任务');
    return false;
  }

  return true;
}

async function batchToggle(isEnabled: boolean) {
  if (!ensureBatchSelection()) {
    return;
  }

  actionLoadingKey.value = isEnabled ? 'batch:enable' : 'batch:disable';
  try {
    await toggleSchedulerJobsBatchApi(selectedJobKeys.value, isEnabled);
    message.success(isEnabled ? '已批量启用任务' : '已批量停用任务');
    selectedJobKeys.value = [];
    await loadData();
  } finally {
    actionLoadingKey.value = '';
  }
}

async function batchRun() {
  if (!ensureBatchSelection()) {
    return;
  }

  actionLoadingKey.value = 'batch:run';
  try {
    const results = await runSchedulerJobsBatchApi(selectedJobKeys.value);
    const failed = results.filter((item) => !item.success);
    if (failed.length > 0) {
      message.warning(`批量执行完成，${failed.length} 个任务失败或跳过`);
    } else {
      message.success('批量执行完成');
    }
    await loadData();
  } finally {
    actionLoadingKey.value = '';
  }
}

async function batchClearRecords() {
  if (!ensureBatchSelection()) {
    return;
  }

  actionLoadingKey.value = 'batch:clear-records';
  try {
    await clearSchedulerJobRecordsBatchApi(selectedJobKeys.value, 100);
    message.success('已批量清理执行记录，保留最近 100 条');
  } finally {
    actionLoadingKey.value = '';
  }
}

async function loadJobRecords(item: SchedulerApi.JobItem) {
  selectedJob.value = item;
  recordLoading.value = true;
  try {
    const result = await getSchedulerJobRecordsApi(item.jobKey, {
      maxResultCount: recordPageSize.value,
      skipCount: (recordPageIndex.value - 1) * recordPageSize.value,
    });
    jobRecords.value = result.items;
    recordTotalCount.value = result.totalCount;
  } finally {
    recordLoading.value = false;
  }
}

async function openJobRecords(item: SchedulerApi.JobItem) {
  recordDrawerOpen.value = true;
  recordPageIndex.value = 1;
  await loadJobRecords(item);
}

async function clearJobRecords() {
  if (!selectedJob.value) {
    return;
  }

  actionLoadingKey.value = `${selectedJob.value.jobKey}:clear-records`;
  try {
    await clearSchedulerJobRecordsApi(selectedJob.value.jobKey, 100);
    message.success('已清理执行记录，保留最近 100 条');
    await loadJobRecords(selectedJob.value);
  } finally {
    actionLoadingKey.value = '';
  }
}

async function loadJobTriggers(item: SchedulerApi.JobItem) {
  selectedJob.value = item;
  triggerLoading.value = true;
  try {
    jobTriggers.value = await getSchedulerJobTriggersApi(item.jobKey);
  } finally {
    triggerLoading.value = false;
  }
}

async function openJobTriggers(item: SchedulerApi.JobItem) {
  triggerDrawerOpen.value = true;
  await loadJobTriggers(item);
}

function openCreateTrigger() {
  patchTriggerForm();
  triggerEditorDrawerOpen.value = true;
}

function openEditTrigger(item: SchedulerApi.TriggerItem) {
  patchTriggerForm(item);
  triggerEditorDrawerOpen.value = true;
}

async function submitTrigger() {
  if (!selectedJob.value || !validateTriggerForm()) {
    return;
  }

  actionLoadingKey.value = 'trigger:save';
  try {
    if (editingTriggerKey.value) {
      await updateSchedulerJobTriggerApi(selectedJob.value.jobKey, editingTriggerKey.value, { ...triggerForm });
      message.success('已更新触发器');
    } else {
      await createSchedulerJobTriggerApi(selectedJob.value.jobKey, { ...triggerForm });
      message.success('已新增触发器');
    }
    triggerEditorDrawerOpen.value = false;
    await loadJobTriggers(selectedJob.value);
    await loadData();
  } finally {
    actionLoadingKey.value = '';
  }
}

async function toggleTrigger(item: SchedulerApi.TriggerItem, isEnabled: boolean) {
  if (!selectedJob.value) {
    return;
  }

  actionLoadingKey.value = `${item.triggerKey}:toggle`;
  try {
    await toggleSchedulerJobTriggerApi(selectedJob.value.jobKey, item.triggerKey, isEnabled);
    message.success(isEnabled ? '已启用触发器' : '已停用触发器');
    await loadJobTriggers(selectedJob.value);
  } finally {
    actionLoadingKey.value = '';
  }
}

async function deleteTrigger(item: SchedulerApi.TriggerItem) {
  if (!selectedJob.value) {
    return;
  }

  actionLoadingKey.value = `${item.triggerKey}:delete`;
  try {
    await deleteSchedulerJobTriggerApi(selectedJob.value.jobKey, item.triggerKey);
    message.success('已删除触发器');
    await loadJobTriggers(selectedJob.value);
    await loadData();
  } finally {
    actionLoadingKey.value = '';
  }
}

async function openClusterNodes() {
  nodeDrawerOpen.value = true;
  nodeLoading.value = true;
  try {
    clusterNodes.value = await getSchedulerClusterNodesApi();
  } finally {
    nodeLoading.value = false;
  }
}

onMounted(loadData);
</script>

<template>
  <Page description="定时任务中心支持任务定义、触发器、执行记录、取消执行和集群节点心跳。" title="定时任务">
    <div class="scheduler__metrics">
      <Card v-for="metric in metrics" :key="metric.label" :bordered="false" class="rounded-2xl">
        <Statistic :title="metric.label" :value="metric.value" />
      </Card>
    </div>

    <Card :bordered="false" class="rounded-2xl" title="任务中心">
      <template #extra>
        <Space>
          <Tag v-if="selectedJobKeys.length" color="blue">已选 {{ selectedJobKeys.length }}</Tag>
          <Button
            v-if="canManageScheduler"
            :disabled="selectedJobKeys.length === 0"
            :loading="actionLoadingKey === 'batch:enable'"
            @click="batchToggle(true)"
          >
            批量启用
          </Button>
          <Button
            v-if="canManageScheduler"
            :disabled="selectedJobKeys.length === 0"
            :loading="actionLoadingKey === 'batch:disable'"
            @click="batchToggle(false)"
          >
            批量停用
          </Button>
          <Button
            v-if="canExecuteScheduler"
            :disabled="selectedJobKeys.length === 0"
            :loading="actionLoadingKey === 'batch:run'"
            @click="batchRun"
          >
            批量执行
          </Button>
          <Popconfirm title="确认批量清理所选任务的执行记录？将保留最近 100 条。" @confirm="batchClearRecords">
            <Button
              v-if="canManageScheduler"
              danger
              :disabled="selectedJobKeys.length === 0"
              :loading="actionLoadingKey === 'batch:clear-records'"
            >
              批量清理
            </Button>
          </Popconfirm>
          <Button @click="openClusterNodes">集群节点</Button>
          <Button v-if="canManageScheduler" type="primary" @click="openCreateJob">新增任务</Button>
          <Button :loading="loading" @click="loadData">刷新</Button>
        </Space>
      </template>

      <JobGrid
        class="app-vxe-card-grid"
        grid-class="scheduler__job-grid"
        :grid-events="jobGridEvents"
        :grid-options="jobGridOptions"
      >
        <template #handlerKey="{ row }">
          <Tag>{{ row.handlerKey }}</Tag>
        </template>
        <template #nextRunTime="{ row }">
          {{ formatDate(row.nextRunTime) }}
        </template>
        <template #lastRunResult="{ row }">
          <div class="scheduler__cell scheduler__cell--last-result">
            <Tag
              :color="
                row.lastRunResult.includes('失败')
                  ? 'error'
                  : row.lastRunResult
                    ? 'success'
                    : 'default'
              "
            >
              {{
                row.lastRunResult.includes('失败')
                  ? '失败'
                  : row.lastRunResult
                    ? '成功'
                    : '未执行'
              }}
            </Tag>
            <small>{{ row.lastRunResult || '-' }}</small>
          </div>
        </template>
        <template #isEnabled="{ row }">
          <Tag
            :color="
              row.isRunning
                ? 'processing'
                : row.isEnabled
                  ? 'success'
                  : 'default'
            "
          >
            {{
              row.isRunning
                ? '执行中'
                : row.isEnabled
                  ? '运行中'
                  : '已停用'
            }}
          </Tag>
        </template>
        <template #actions="{ row }">
          <Space>
            <Switch
              v-if="canManageScheduler"
              :checked="row.isEnabled"
              :loading="actionLoadingKey === `${row.jobKey}:toggle`"
              :disabled="row.isRunning"
              @change="(checked) => toggleJob(row, Boolean(checked))"
            />
            <Button
              v-if="canExecuteScheduler"
              size="small"
              :loading="actionLoadingKey === `${row.jobKey}:run`"
              :disabled="row.isRunning"
              @click="runJob(row)"
            >
              立即执行
            </Button>
            <Button size="small" @click="openJobTriggers(row)">触发器</Button>
            <Button size="small" @click="openJobRecords(row)">记录</Button>
            <Button
              v-if="canManageScheduler"
              size="small"
              :disabled="row.isRunning"
              @click="openEditJob(row)"
            >
              编辑
            </Button>
            <Button
              v-if="canExecuteScheduler && row.isRunning"
              danger
              size="small"
              :loading="actionLoadingKey === `${row.jobKey}:cancel`"
              @click="cancelJob(row)"
            >
              取消
            </Button>
            <Popconfirm title="删除任务会同时清理触发器和执行记录，确认删除？" @confirm="deleteJob(row)">
              <Button
                v-if="canManageScheduler"
                danger
                size="small"
                :disabled="row.isRunning"
                :loading="actionLoadingKey === `${row.jobKey}:delete`"
              >
                删除
              </Button>
            </Popconfirm>
          </Space>
        </template>
      </JobGrid>
    </Card>

    <Drawer
      v-model:open="jobDrawerOpen"
      :title="editingJobKey ? '编辑任务' : '新增任务'"
      destroy-on-close
      width="640px"
    >
      <Form layout="vertical">
        <Form.Item label="任务键" required>
          <Input v-model:value="jobForm.jobKey" :disabled="!!editingJobKey" placeholder="例如 data-sync" />
        </Form.Item>
        <Form.Item label="任务名称" required>
          <Input v-model:value="jobForm.title" placeholder="任务名称" />
        </Form.Item>
        <Form.Item label="执行器" required>
          <Select v-model:value="jobForm.handlerKey" :options="handlerOptions" placeholder="选择执行器" />
        </Form.Item>
        <Form.Item label="默认 Cron" required>
          <Input v-model:value="jobForm.cronExpression" placeholder="0 0/5 * * * ?" />
        </Form.Item>
        <Form.Item label="启用">
          <Switch v-model:checked="jobForm.isEnabled" />
        </Form.Item>
        <Form.Item label="说明">
          <Input.TextArea v-model:value="jobForm.description" :auto-size="{ minRows: 3, maxRows: 5 }" />
        </Form.Item>
      </Form>
      <template #footer>
        <div class="flex justify-end gap-2">
          <Button @click="jobDrawerOpen = false">取消</Button>
          <Button :loading="actionLoadingKey === 'job:save'" type="primary" @click="submitJob">保存</Button>
        </div>
      </template>
    </Drawer>

    <Drawer
      v-model:open="triggerDrawerOpen"
      :title="selectedJob ? `${selectedJob.title} - 触发器` : '触发器'"
      width="980"
    >
      <template #extra>
        <Space>
          <Button :loading="triggerLoading" @click="selectedJob && loadJobTriggers(selectedJob)">刷新</Button>
          <Button v-if="canManageScheduler" type="primary" @click="openCreateTrigger">新增触发器</Button>
        </Space>
      </template>

      <TriggerGrid class="app-vxe-card-grid" :grid-events="triggerGridEvents" :grid-options="triggerGridOptions">
        <template #nextRunTime="{ row }">
          {{ formatDate(row.nextRunTime) }}
        </template>
        <template #isEnabled="{ row }">
          <Tag :color="row.isEnabled ? 'success' : 'default'">
            {{ row.isEnabled ? '启用' : '停用' }}
          </Tag>
        </template>
        <template #actions="{ row }">
          <Space>
            <Switch
              v-if="canManageScheduler"
              :checked="row.isEnabled"
              :loading="actionLoadingKey === `${row.triggerKey}:toggle`"
              @change="(checked) => toggleTrigger(row, Boolean(checked))"
            />
            <Button v-if="canManageScheduler" size="small" @click="openEditTrigger(row)">编辑</Button>
            <Popconfirm title="确认删除该触发器？" @confirm="deleteTrigger(row)">
              <Button
                v-if="canManageScheduler"
                danger
                size="small"
                :loading="actionLoadingKey === `${row.triggerKey}:delete`"
              >
                删除
              </Button>
            </Popconfirm>
          </Space>
        </template>
      </TriggerGrid>
    </Drawer>

    <Drawer
      v-model:open="triggerEditorDrawerOpen"
      :title="editingTriggerKey ? '编辑触发器' : '新增触发器'"
      destroy-on-close
      width="600px"
    >
      <Form layout="vertical">
        <Form.Item label="触发器键" required>
          <Input v-model:value="triggerForm.triggerKey" :disabled="!!editingTriggerKey" placeholder="例如 default" />
        </Form.Item>
        <Form.Item label="触发器名称" required>
          <Input v-model:value="triggerForm.title" placeholder="触发器名称" />
        </Form.Item>
        <Form.Item label="Cron" required>
          <Input v-model:value="triggerForm.cronExpression" placeholder="0 0/5 * * * ?" />
        </Form.Item>
        <Form.Item label="启用">
          <Switch v-model:checked="triggerForm.isEnabled" />
        </Form.Item>
        <Form.Item label="说明">
          <Input.TextArea v-model:value="triggerForm.description" :auto-size="{ minRows: 3, maxRows: 5 }" />
        </Form.Item>
      </Form>
      <template #footer>
        <div class="flex justify-end gap-2">
          <Button @click="triggerEditorDrawerOpen = false">取消</Button>
          <Button :loading="actionLoadingKey === 'trigger:save'" type="primary" @click="submitTrigger">保存</Button>
        </div>
      </template>
    </Drawer>

    <Drawer
      v-model:open="recordDrawerOpen"
      :title="selectedJob ? `${selectedJob.title} - 执行记录` : '执行记录'"
      width="960"
    >
      <template #extra>
        <Space>
          <Button :loading="recordLoading" @click="selectedJob && loadJobRecords(selectedJob)">刷新</Button>
          <Popconfirm title="确认清理该任务的历史执行记录？将保留最近 100 条。" @confirm="clearJobRecords">
            <Button
              v-if="canManageScheduler"
              danger
              :disabled="!selectedJob || jobRecords.length === 0"
              :loading="selectedJob ? actionLoadingKey === `${selectedJob.jobKey}:clear-records` : false"
            >
              清理
            </Button>
          </Popconfirm>
        </Space>
      </template>

      <RecordGrid class="app-vxe-card-grid" :grid-events="recordGridEvents" :grid-options="recordGridOptions">
        <template #status="{ row }">
          <Tag :color="getRecordStatusColor(row.status)">
            {{ row.status }}
          </Tag>
        </template>
        <template #triggerKey="{ row }">
          {{ row.triggerKey || '-' }}
        </template>
        <template #triggerMode="{ row }">
          {{ row.triggerMode === 'manual' ? '手动' : '自动' }}
        </template>
        <template #startTime="{ row }">
          {{ formatDate(row.startTime) }}
        </template>
        <template #endTime="{ row }">
          {{ formatDate(row.endTime) }}
        </template>
        <template #durationMilliseconds="{ row }">
          {{ formatDuration(row.durationMilliseconds) }}
        </template>
        <template #result="{ row }">
          <span :title="row.errorMessage || row.result || ''">
            {{ row.result || '-' }}
          </span>
        </template>
      </RecordGrid>
    </Drawer>

    <Drawer v-model:open="nodeDrawerOpen" title="集群节点" width="980">
      <template #extra>
        <Button :loading="nodeLoading" @click="openClusterNodes">刷新</Button>
      </template>
      <NodeGrid class="app-vxe-card-grid" :grid-events="nodeGridEvents" :grid-options="nodeGridOptions">
        <template #status="{ row }">
          <Tag :color="row.status === 'Online' ? 'success' : 'default'">
            {{ row.status === 'Online' ? '在线' : '离线' }}
          </Tag>
        </template>
        <template #startedAt="{ row }">
          {{ formatDate(row.startedAt) }}
        </template>
        <template #lastHeartbeatTime="{ row }">
          {{ formatDate(row.lastHeartbeatTime) }}
        </template>
      </NodeGrid>
    </Drawer>
  </Page>
</template>

<style scoped>
@reference "@vben/tailwind-config/theme";
.scheduler__metrics {
  display: grid;
  grid-template-columns: repeat(5, minmax(0, 1fr));
  @apply gap-4;
  @apply mb-4;
}

.scheduler__empty {
  display: grid;
  place-items: center;
  min-height: 260px;
}

.scheduler__cell {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  width: 100%;
  min-width: 0;
  text-align: left;
}

.scheduler__cell--last-result {
  @apply gap-1;
  justify-content: center;
  min-height: 44px;
}

.scheduler__cell small {
  display: block;
  width: 100%;
  overflow: hidden;
  text-overflow: ellipsis;
  line-height: 1.35;
  color: var(--ant-color-text-secondary);
  text-align: left;
  white-space: nowrap;
}

:deep(.scheduler__job-grid .scheduler__last-result-column) {
  text-align: left;
}

:deep(.scheduler__job-grid .scheduler__last-result-column .vxe-cell) {
  display: flex;
  align-items: center;
  justify-content: flex-start;
}
</style>
