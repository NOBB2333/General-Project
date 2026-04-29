<script lang="ts" setup>
import { computed, onMounted, reactive, ref } from 'vue';

import { Page } from '@vben/common-ui';

import {
  Button,
  Card,
  Col,
  Drawer,
  Empty,
  Form,
  Input,
  Modal,
  Popconfirm,
  Row,
  Select,
  Space,
  Statistic,
  Switch,
  Table,
  Tag,
  message,
} from 'ant-design-vue';

import {
  cancelSchedulerJobApi,
  clearSchedulerJobRecordsApi,
  createSchedulerJobApi,
  createSchedulerJobTriggerApi,
  deleteSchedulerJobApi,
  deleteSchedulerJobTriggerApi,
  getSchedulerClusterNodesApi,
  getSchedulerHandlersApi,
  getSchedulerJobRecordsApi,
  getSchedulerJobTriggersApi,
  getSchedulerListApi,
  runSchedulerJobApi,
  toggleSchedulerJobApi,
  toggleSchedulerJobTriggerApi,
  updateSchedulerJobApi,
  updateSchedulerJobTriggerApi,
  type SchedulerApi,
} from '#/api/core';

defineOptions({ name: 'PlatformSchedulerPage' });

const loading = ref(false);
const actionLoadingKey = ref('');
const items = ref<SchedulerApi.JobItem[]>([]);
const handlers = ref<SchedulerApi.HandlerItem[]>([]);
const recordDrawerOpen = ref(false);
const recordLoading = ref(false);
const triggerDrawerOpen = ref(false);
const triggerLoading = ref(false);
const nodeDrawerOpen = ref(false);
const nodeLoading = ref(false);
const jobModalOpen = ref(false);
const triggerModalOpen = ref(false);
const selectedJob = ref<SchedulerApi.JobItem>();
const editingJobKey = ref('');
const editingTriggerKey = ref('');
const jobRecords = ref<SchedulerApi.JobRecordItem[]>([]);
const jobTriggers = ref<SchedulerApi.TriggerItem[]>([]);
const clusterNodes = ref<SchedulerApi.ClusterNodeItem[]>([]);

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
  { label: '任务总数', value: items.value.length },
  { label: '启用任务', value: items.value.filter((item) => item.isEnabled).length },
  { label: '执行中', value: items.value.filter((item) => item.isRunning).length },
  {
    label: '触发器',
    value: items.value.reduce((total, item) => total + item.triggerCount, 0),
  },
]);

const handlerOptions = computed(() =>
  handlers.value.map((item) => ({
    label: item.handlerKey,
    value: item.handlerKey,
  })),
);

const columns = [
  { dataIndex: 'title', key: 'title', title: '任务名称', width: 180 },
  { dataIndex: 'jobKey', key: 'jobKey', title: '任务键', width: 170 },
  { dataIndex: 'handlerKey', key: 'handlerKey', title: '执行器', width: 190 },
  { dataIndex: 'cronExpression', key: 'cronExpression', title: '默认 Cron', width: 160 },
  { dataIndex: 'triggerCount', key: 'triggerCount', title: '触发器', width: 90 },
  { dataIndex: 'nextRunTime', key: 'nextRunTime', title: '下次执行', width: 160 },
  { dataIndex: 'lastRunResult', key: 'lastRunResult', title: '最近结果', width: 240 },
  { dataIndex: 'isEnabled', key: 'isEnabled', title: '状态', width: 100 },
  { key: 'actions', title: '操作', width: 430 },
];

const recordColumns = [
  { dataIndex: 'status', key: 'status', title: '状态', width: 90 },
  { dataIndex: 'triggerKey', key: 'triggerKey', title: '触发器', width: 120 },
  { dataIndex: 'triggerMode', key: 'triggerMode', title: '触发', width: 90 },
  { dataIndex: 'startTime', key: 'startTime', title: '开始时间', width: 170 },
  { dataIndex: 'endTime', key: 'endTime', title: '结束时间', width: 170 },
  { dataIndex: 'durationMilliseconds', key: 'durationMilliseconds', title: '耗时', width: 100 },
  { dataIndex: 'result', key: 'result', title: '结果', ellipsis: true, width: 360 },
];

const triggerColumns = [
  { dataIndex: 'title', key: 'title', title: '触发器', width: 160 },
  { dataIndex: 'triggerKey', key: 'triggerKey', title: '触发器键', width: 140 },
  { dataIndex: 'cronExpression', key: 'cronExpression', title: 'Cron', width: 160 },
  { dataIndex: 'nextRunTime', key: 'nextRunTime', title: '下次执行', width: 160 },
  { dataIndex: 'lastRunResult', key: 'lastRunResult', title: '最近结果', width: 220 },
  { dataIndex: 'isEnabled', key: 'isEnabled', title: '状态', width: 100 },
  { key: 'actions', title: '操作', width: 220 },
];

const nodeColumns = [
  { dataIndex: 'status', key: 'status', title: '状态', width: 90 },
  { dataIndex: 'hostName', key: 'hostName', title: '主机', width: 150 },
  { dataIndex: 'processId', key: 'processId', title: '进程', width: 90 },
  { dataIndex: 'instanceId', key: 'instanceId', title: '实例', width: 280 },
  { dataIndex: 'startedAt', key: 'startedAt', title: '启动时间', width: 170 },
  { dataIndex: 'lastHeartbeatTime', key: 'lastHeartbeatTime', title: '心跳时间', width: 170 },
  { dataIndex: 'description', key: 'description', title: '说明', ellipsis: true, width: 260 },
];

async function loadData() {
  loading.value = true;
  try {
    const [jobList, handlerList] = await Promise.all([getSchedulerListApi(), getSchedulerHandlersApi()]);
    items.value = jobList;
    handlers.value = handlerList;
  } finally {
    loading.value = false;
  }
}

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
  jobModalOpen.value = true;
}

function openEditJob(item: SchedulerApi.JobItem) {
  patchJobForm(item);
  jobModalOpen.value = true;
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
    jobModalOpen.value = false;
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

async function loadJobRecords(item: SchedulerApi.JobItem) {
  selectedJob.value = item;
  recordLoading.value = true;
  try {
    jobRecords.value = await getSchedulerJobRecordsApi(item.jobKey, {
      maxResultCount: 100,
    });
  } finally {
    recordLoading.value = false;
  }
}

async function openJobRecords(item: SchedulerApi.JobItem) {
  recordDrawerOpen.value = true;
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
  triggerModalOpen.value = true;
}

function openEditTrigger(item: SchedulerApi.TriggerItem) {
  patchTriggerForm(item);
  triggerModalOpen.value = true;
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
    triggerModalOpen.value = false;
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
    <Row :gutter="[16, 16]" class="mb-1">
      <Col v-for="metric in metrics" :key="metric.label" :lg="6" :md="12" :span="24">
        <Card :bordered="false">
          <Statistic :title="metric.label" :value="metric.value" />
        </Card>
      </Col>
    </Row>

    <Card :bordered="false" title="任务中心">
      <template #extra>
        <Space>
          <Button @click="openClusterNodes">集群节点</Button>
          <Button type="primary" @click="openCreateJob">新增任务</Button>
          <Button :loading="loading" @click="loadData">刷新</Button>
        </Space>
      </template>

      <div v-if="!loading && items.length === 0" class="scheduler__empty">
        <Empty description="暂无定时任务" />
      </div>
      <Table
        v-else
        :columns="columns"
        :data-source="items"
        :loading="loading"
        :pagination="false"
        row-key="jobKey"
        :scroll="{ x: 1660 }"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'handlerKey'">
            <Tag>{{ (record as SchedulerApi.JobItem).handlerKey }}</Tag>
          </template>
          <template v-else-if="column.key === 'triggerCount'">
            {{ (record as SchedulerApi.JobItem).triggerCount }}
          </template>
          <template v-else-if="column.key === 'nextRunTime'">
            {{ formatDate((record as SchedulerApi.JobItem).nextRunTime) }}
          </template>
          <template v-else-if="column.key === 'lastRunResult'">
            <div class="scheduler__cell">
              <Tag
                :color="
                  (record as SchedulerApi.JobItem).lastRunResult.includes('失败')
                    ? 'error'
                    : (record as SchedulerApi.JobItem).lastRunResult
                      ? 'success'
                      : 'default'
                "
              >
                {{
                  (record as SchedulerApi.JobItem).lastRunResult.includes('失败')
                    ? '失败'
                    : (record as SchedulerApi.JobItem).lastRunResult
                      ? '成功'
                      : '未执行'
                }}
              </Tag>
              <small>{{ (record as SchedulerApi.JobItem).lastRunResult || '-' }}</small>
            </div>
          </template>
          <template v-else-if="column.key === 'isEnabled'">
            <Tag
              :color="
                (record as SchedulerApi.JobItem).isRunning
                  ? 'processing'
                  : (record as SchedulerApi.JobItem).isEnabled
                    ? 'success'
                    : 'default'
              "
            >
              {{
                (record as SchedulerApi.JobItem).isRunning
                  ? '执行中'
                  : (record as SchedulerApi.JobItem).isEnabled
                    ? '运行中'
                    : '已停用'
              }}
            </Tag>
          </template>
          <template v-else-if="column.key === 'actions'">
            <Space>
              <Switch
                :checked="(record as SchedulerApi.JobItem).isEnabled"
                :loading="actionLoadingKey === `${(record as SchedulerApi.JobItem).jobKey}:toggle`"
                :disabled="(record as SchedulerApi.JobItem).isRunning"
                @change="(checked) => toggleJob(record as SchedulerApi.JobItem, Boolean(checked))"
              />
              <Button
                size="small"
                :loading="actionLoadingKey === `${(record as SchedulerApi.JobItem).jobKey}:run`"
                :disabled="(record as SchedulerApi.JobItem).isRunning"
                @click="runJob(record as SchedulerApi.JobItem)"
              >
                立即执行
              </Button>
              <Button size="small" @click="openJobTriggers(record as SchedulerApi.JobItem)">触发器</Button>
              <Button size="small" @click="openJobRecords(record as SchedulerApi.JobItem)">记录</Button>
              <Button
                size="small"
                :disabled="(record as SchedulerApi.JobItem).isRunning"
                @click="openEditJob(record as SchedulerApi.JobItem)"
              >
                编辑
              </Button>
              <Button
                v-if="(record as SchedulerApi.JobItem).isRunning"
                danger
                size="small"
                :loading="actionLoadingKey === `${(record as SchedulerApi.JobItem).jobKey}:cancel`"
                @click="cancelJob(record as SchedulerApi.JobItem)"
              >
                取消
              </Button>
              <Popconfirm title="删除任务会同时清理触发器和执行记录，确认删除？" @confirm="deleteJob(record as SchedulerApi.JobItem)">
                <Button
                  danger
                  size="small"
                  :disabled="(record as SchedulerApi.JobItem).isRunning"
                  :loading="actionLoadingKey === `${(record as SchedulerApi.JobItem).jobKey}:delete`"
                >
                  删除
                </Button>
              </Popconfirm>
            </Space>
          </template>
        </template>
      </Table>
    </Card>

    <Modal
      v-model:open="jobModalOpen"
      :confirm-loading="actionLoadingKey === 'job:save'"
      :title="editingJobKey ? '编辑任务' : '新增任务'"
      width="640px"
      @ok="submitJob"
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
    </Modal>

    <Drawer
      v-model:open="triggerDrawerOpen"
      :title="selectedJob ? `${selectedJob.title} - 触发器` : '触发器'"
      width="980"
    >
      <template #extra>
        <Space>
          <Button :loading="triggerLoading" @click="selectedJob && loadJobTriggers(selectedJob)">刷新</Button>
          <Button type="primary" @click="openCreateTrigger">新增触发器</Button>
        </Space>
      </template>

      <Table
        :columns="triggerColumns"
        :data-source="jobTriggers"
        :loading="triggerLoading"
        :pagination="{ pageSize: 10, showSizeChanger: false }"
        row-key="triggerKey"
        :scroll="{ x: 1210 }"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'nextRunTime'">
            {{ formatDate((record as SchedulerApi.TriggerItem).nextRunTime) }}
          </template>
          <template v-else-if="column.key === 'lastRunResult'">
            {{ (record as SchedulerApi.TriggerItem).lastRunResult || '-' }}
          </template>
          <template v-else-if="column.key === 'isEnabled'">
            <Tag :color="(record as SchedulerApi.TriggerItem).isEnabled ? 'success' : 'default'">
              {{ (record as SchedulerApi.TriggerItem).isEnabled ? '启用' : '停用' }}
            </Tag>
          </template>
          <template v-else-if="column.key === 'actions'">
            <Space>
              <Switch
                :checked="(record as SchedulerApi.TriggerItem).isEnabled"
                :loading="actionLoadingKey === `${(record as SchedulerApi.TriggerItem).triggerKey}:toggle`"
                @change="(checked) => toggleTrigger(record as SchedulerApi.TriggerItem, Boolean(checked))"
              />
              <Button size="small" @click="openEditTrigger(record as SchedulerApi.TriggerItem)">编辑</Button>
              <Popconfirm title="确认删除该触发器？" @confirm="deleteTrigger(record as SchedulerApi.TriggerItem)">
                <Button
                  danger
                  size="small"
                  :loading="actionLoadingKey === `${(record as SchedulerApi.TriggerItem).triggerKey}:delete`"
                >
                  删除
                </Button>
              </Popconfirm>
            </Space>
          </template>
        </template>
      </Table>
    </Drawer>

    <Modal
      v-model:open="triggerModalOpen"
      :confirm-loading="actionLoadingKey === 'trigger:save'"
      :title="editingTriggerKey ? '编辑触发器' : '新增触发器'"
      width="600px"
      @ok="submitTrigger"
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
    </Modal>

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
              danger
              :disabled="!selectedJob || jobRecords.length === 0"
              :loading="selectedJob ? actionLoadingKey === `${selectedJob.jobKey}:clear-records` : false"
            >
              清理
            </Button>
          </Popconfirm>
        </Space>
      </template>

      <Table
        :columns="recordColumns"
        :data-source="jobRecords"
        :loading="recordLoading"
        :pagination="{ pageSize: 10, showSizeChanger: false }"
        row-key="startTime"
        :scroll="{ x: 1100 }"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'status'">
            <Tag :color="getRecordStatusColor((record as SchedulerApi.JobRecordItem).status)">
              {{ (record as SchedulerApi.JobRecordItem).status }}
            </Tag>
          </template>
          <template v-else-if="column.key === 'triggerKey'">
            {{ (record as SchedulerApi.JobRecordItem).triggerKey || '-' }}
          </template>
          <template v-else-if="column.key === 'triggerMode'">
            {{ (record as SchedulerApi.JobRecordItem).triggerMode === 'manual' ? '手动' : '自动' }}
          </template>
          <template v-else-if="column.key === 'startTime'">
            {{ formatDate((record as SchedulerApi.JobRecordItem).startTime) }}
          </template>
          <template v-else-if="column.key === 'endTime'">
            {{ formatDate((record as SchedulerApi.JobRecordItem).endTime) }}
          </template>
          <template v-else-if="column.key === 'durationMilliseconds'">
            {{ formatDuration((record as SchedulerApi.JobRecordItem).durationMilliseconds) }}
          </template>
          <template v-else-if="column.key === 'result'">
            <span :title="(record as SchedulerApi.JobRecordItem).errorMessage || (record as SchedulerApi.JobRecordItem).result || ''">
              {{ (record as SchedulerApi.JobRecordItem).result || '-' }}
            </span>
          </template>
        </template>
      </Table>
    </Drawer>

    <Drawer v-model:open="nodeDrawerOpen" title="集群节点" width="980">
      <template #extra>
        <Button :loading="nodeLoading" @click="openClusterNodes">刷新</Button>
      </template>
      <Table
        :columns="nodeColumns"
        :data-source="clusterNodes"
        :loading="nodeLoading"
        :pagination="{ pageSize: 10, showSizeChanger: false }"
        row-key="instanceId"
        :scroll="{ x: 1210 }"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'status'">
            <Tag :color="(record as SchedulerApi.ClusterNodeItem).status === 'Online' ? 'success' : 'default'">
              {{ (record as SchedulerApi.ClusterNodeItem).status === 'Online' ? '在线' : '离线' }}
            </Tag>
          </template>
          <template v-else-if="column.key === 'startedAt'">
            {{ formatDate((record as SchedulerApi.ClusterNodeItem).startedAt) }}
          </template>
          <template v-else-if="column.key === 'lastHeartbeatTime'">
            {{ formatDate((record as SchedulerApi.ClusterNodeItem).lastHeartbeatTime) }}
          </template>
        </template>
      </Table>
    </Drawer>
  </Page>
</template>

<style scoped>
.scheduler__empty {
  display: grid;
  min-height: 260px;
  place-items: center;
}

.scheduler__cell {
  display: flex;
  flex-direction: column;
}

.scheduler__cell small {
  color: var(--ant-color-text-secondary);
}
</style>
