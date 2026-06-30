<script lang="ts" setup>
import type { NativeVxeGridEvents, VxeGridProps } from '#/adapter/vxe-table';
import type { IntegrationApi } from '#/api/core';

import { computed, onMounted, reactive, ref } from 'vue';

import { Page } from '@vben/common-ui';

import { Button, Card, Drawer, Form, Input, InputNumber, Select, Space, Statistic, Switch, Tag, message } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import {
  getIntegrationConnectorsApi,
  getIntegrationMessageTemplatesApi,
  getIntegrationNotificationRulesApi,
  saveIntegrationNotificationRuleApi,
  toggleIntegrationNotificationRuleApi,
} from '#/api/core';
import { useActionLoading } from '#/composables/platform/use-action-loading';

defineOptions({ name: 'IntegrationRulesPage' });

const statusOptions = [
  { label: '全部状态', value: 'All' },
  { label: '启用', value: 'Active' },
  { label: '停用', value: 'Disabled' },
  { label: '暂停', value: 'Paused' },
];

const editableStatusOptions = statusOptions.slice(1);

const eventOptions = [
  { label: '项目风险新增', value: 'Project.Risk.Created' },
  { label: '项目问题逾期', value: 'Project.Issue.Overdue' },
  { label: '排期变更', value: 'Project.Schedule.Changed' },
  { label: '会议纪要生成', value: 'Project.Meeting.MinutesCreated' },
  { label: '周报生成', value: 'Project.Report.WeeklyCreated' },
  { label: '回款逾期', value: 'Business.Receivable.Overdue' },
];

const eventQueryOptions = [{ label: '全部事件', value: '' }, ...eventOptions];

const channelOptions = [
  { label: '个人通知', value: 'Personal' },
  { label: '群通知', value: 'Group' },
  { label: '个人与群', value: 'Mixed' },
];

const targetTypeOptions = [
  { label: 'Open ID', value: 'OpenId' },
  { label: 'User ID', value: 'User' },
  { label: '群 Chat ID', value: 'Chat' },
  { label: 'Union ID', value: 'UnionId' },
  { label: '邮箱', value: 'Email' },
];

const columns = [
  { field: 'ruleCode', title: '规则编码', width: 180 },
  { field: 'name', slots: { default: 'name' }, title: '规则名称', minWidth: 220 },
  { field: 'eventCode', slots: { default: 'event' }, title: '触发事件', minWidth: 220 },
  { field: 'templateName', slots: { default: 'template' }, title: '消息模板', minWidth: 220 },
  { field: 'connectorCode', slots: { default: 'connector' }, title: '连接器', width: 150 },
  { field: 'channelType', slots: { default: 'channel' }, title: '通道', width: 120 },
  { field: 'status', slots: { default: 'status' }, title: '状态', width: 120 },
  { field: 'rateLimitWindowMinutes', slots: { default: 'rateLimit' }, title: '频控', width: 150 },
  { field: 'lastTriggeredTime', slots: { default: 'lastTriggeredTime' }, title: '最近触发', width: 170 },
  { fixed: 'right', slots: { default: 'actions' }, title: '操作', width: 210 },
] satisfies VxeGridProps<IntegrationApi.NotificationRuleItem>['columns'];

const connectors = ref<IntegrationApi.ConnectorItem[]>([]);
const templates = ref<IntegrationApi.MessageTemplateItem[]>([]);
const rules = ref<IntegrationApi.NotificationRuleItem[]>([]);
const totalCount = ref(0);
const loading = ref(false);
const drawerOpen = ref(false);
const saving = ref(false);
const editingId = ref<string>();
const pageIndex = ref(1);
const pageSize = ref(20);
const { actionLoadingKey, runAction } = useActionLoading();

const query = reactive<IntegrationApi.NotificationRuleQuery>({
  connectorId: undefined,
  eventCode: '',
  keyword: '',
  status: 'All',
});

const form = reactive<IntegrationApi.NotificationRuleSaveInput>({
  channelType: 'Personal',
  conditionJson: '{}',
  connectorId: undefined,
  eventCode: 'Project.Risk.Created',
  eventName: '项目风险新增',
  name: '',
  quietEndTime: '',
  quietStartTime: '',
  rateLimitMaxCount: 0,
  rateLimitWindowMinutes: 0,
  recipientRuleJson: '{\n  "mode": "ProjectRole",\n  "roles": ["PM", "Owner"]\n}',
  remark: '',
  ruleCode: '',
  status: 'Active',
  targetType: 'OpenId',
  templateId: '',
});

const connectorOptions = computed(() => [
  { label: '全部连接器', value: '' },
  ...connectors.value.map((item) => ({
    label: item.displayName,
    value: item.id,
  })),
]);

const editableConnectorOptions = computed(() =>
  connectors.value.map((item) => ({
    label: `${item.displayName} · ${item.providerType}`,
    value: item.id,
  })),
);

const templateOptions = computed(() =>
  templates.value.map((item) => ({
    label: `${item.name} · ${item.templateCode}`,
    value: item.id,
  })),
);

const metrics = computed(() => [
  { label: '规则总数', value: totalCount.value },
  { label: '启用规则', value: rules.value.filter((item) => item.status === 'Active').length },
  { label: '暂停规则', value: rules.value.filter((item) => item.status === 'Paused').length },
  { label: '群通知', value: rules.value.filter((item) => item.channelType === 'Group' || item.channelType === 'Mixed').length },
]);

const gridOptions = computed<VxeGridProps<IntegrationApi.NotificationRuleItem>>(() => ({
  cellConfig: { height: 58 },
  columns,
  data: rules.value,
  emptyText: '暂无通知规则',
  loading: loading.value,
  pagerConfig: {
    currentPage: pageIndex.value,
    pageSize: pageSize.value,
    pageSizes: [10, 20, 50, 100],
    total: totalCount.value,
  },
  rowConfig: { keyField: 'id' },
  scrollX: { enabled: true },
  size: 'medium',
}));

const gridEvents: NativeVxeGridEvents<IntegrationApi.NotificationRuleItem> = {
  'page-change': ({ currentPage, pageSize: nextPageSize }) => {
    const normalizedPageSize = nextPageSize || pageSize.value;
    pageIndex.value = normalizedPageSize === pageSize.value ? currentPage || 1 : 1;
    pageSize.value = normalizedPageSize;
    void loadRules();
  },
};

const [RuleGrid] = useVbenVxeGrid<IntegrationApi.NotificationRuleItem>({
  gridEvents,
  gridOptions: {},
});

function buildQuery() {
  return {
    connectorId: query.connectorId || undefined,
    eventCode: query.eventCode || undefined,
    keyword: query.keyword?.trim() || undefined,
    maxResultCount: pageSize.value,
    skipCount: (pageIndex.value - 1) * pageSize.value,
    status: query.status || 'All',
  };
}

function formatDate(value?: string) {
  return value ? new Date(value).toLocaleString() : '-';
}

function eventLabel(value: string) {
  return eventOptions.find((item) => item.value === value)?.label || value || '-';
}

function channelLabel(value: string) {
  return channelOptions.find((item) => item.value === value)?.label || value || '-';
}

function statusLabel(value: string) {
  return statusOptions.find((item) => item.value === value)?.label || value || '-';
}

function statusColor(value: string) {
  if (value === 'Active') {
    return 'green';
  }

  if (value === 'Paused') {
    return 'orange';
  }

  return 'default';
}

function connectorLabel(id?: string) {
  if (!id) {
    return '按规则动态选择';
  }

  return connectors.value.find((item) => item.id === id)?.displayName || id;
}

function assertJson(value: string, label: string) {
  try {
    JSON.parse(value || '{}');
    return true;
  } catch {
    message.warning(`${label} 必须是合法 JSON`);
    return false;
  }
}

function resetForm() {
  editingId.value = undefined;
  Object.assign(form, {
    channelType: 'Personal',
    conditionJson: '{}',
    connectorId: undefined,
    eventCode: 'Project.Risk.Created',
    eventName: '项目风险新增',
    id: undefined,
    name: '',
    quietEndTime: '',
    quietStartTime: '',
    rateLimitMaxCount: 0,
    rateLimitWindowMinutes: 0,
    recipientRuleJson: '{\n  "mode": "ProjectRole",\n  "roles": ["PM", "Owner"]\n}',
    remark: '',
    ruleCode: '',
    status: 'Active',
    targetType: 'OpenId',
    templateId: templates.value[0]?.id || '',
  });
}

function openCreate() {
  resetForm();
  drawerOpen.value = true;
}

function openEdit(item: IntegrationApi.NotificationRuleItem) {
  editingId.value = item.id;
  Object.assign(form, {
    channelType: item.channelType,
    conditionJson: item.conditionJson,
    connectorId: item.connectorId,
    eventCode: item.eventCode,
    eventName: item.eventName,
    id: item.id,
    name: item.name,
    quietEndTime: item.quietEndTime,
    quietStartTime: item.quietStartTime,
    rateLimitMaxCount: item.rateLimitMaxCount,
    rateLimitWindowMinutes: item.rateLimitWindowMinutes,
    recipientRuleJson: item.recipientRuleJson,
    remark: item.remark,
    ruleCode: item.ruleCode,
    status: item.status,
    targetType: item.targetType,
    templateId: item.templateId,
  });
  drawerOpen.value = true;
}

function handleEventChange(value: unknown) {
  form.eventName = eventLabel(typeof value === 'string' || typeof value === 'number' ? String(value) : '');
}

async function loadOptions() {
  const [connectorResult, templateResult] = await Promise.all([
    getIntegrationConnectorsApi({ includeDisabled: true }),
    getIntegrationMessageTemplatesApi({ maxResultCount: 100, status: 'All' }),
  ]);
  connectors.value = connectorResult;
  templates.value = templateResult.items;
}

async function loadRules() {
  loading.value = true;
  try {
    const result = await getIntegrationNotificationRulesApi(buildQuery());
    rules.value = result.items;
    totalCount.value = result.totalCount;
  } finally {
    loading.value = false;
  }
}

async function refresh(resetPage = false) {
  if (resetPage) {
    pageIndex.value = 1;
  }

  await loadRules();
}

async function handleSave() {
  if (!form.ruleCode.trim() || !form.name.trim()) {
    message.warning('请填写规则编码和规则名称');
    return;
  }

  if (!form.templateId) {
    message.warning('请选择消息模板');
    return;
  }

  if (!assertJson(form.recipientRuleJson, '接收人规则') || !assertJson(form.conditionJson, '触发条件')) {
    return;
  }

  saving.value = true;
  try {
    await saveIntegrationNotificationRuleApi({ ...form, id: editingId.value });
    message.success('通知规则已保存');
    drawerOpen.value = false;
    await loadRules();
  } finally {
    saving.value = false;
  }
}

async function handleToggle(item: IntegrationApi.NotificationRuleItem, checked: boolean | number | string) {
  await runAction(`toggle:${item.id}`, async () => {
    await toggleIntegrationNotificationRuleApi(item.id, { status: checked ? 'Active' : 'Disabled' });
    await loadRules();
  });
}

onMounted(async () => {
  await loadOptions();
  await loadRules();
});
</script>

<template>
  <Page description="配置外部系统通知触发事件、消息模板、接收人规则、通道、静默期和频控策略。" title="通知规则">
    <div class="integration-rules flex flex-col gap-4">
      <div class="integration-rules__metrics grid grid-cols-4 gap-3">
        <Card v-for="metric in metrics" :key="metric.label" :bordered="false" class="rounded-2xl" size="small">
          <Statistic :title="metric.label" :value="metric.value" />
        </Card>
      </div>

      <Card :bordered="false" class="rounded-2xl" title="规则列表">
        <template #extra>
          <Space>
            <Button @click="refresh(false)">刷新</Button>
            <Button type="primary" @click="openCreate">新增规则</Button>
          </Space>
        </template>

        <div class="integration-rules__filters mb-3 grid gap-3">
          <Input v-model:value="query.keyword" allow-clear placeholder="搜索编码、名称、事件、连接器" @press-enter="refresh(true)" />
          <Select v-model:value="query.eventCode" :options="eventQueryOptions" @change="refresh(true)" />
          <Select v-model:value="query.connectorId" :options="connectorOptions" @change="refresh(true)" />
          <Select v-model:value="query.status" :options="statusOptions" @change="refresh(true)" />
          <Button type="primary" @click="refresh(true)">查询</Button>
        </div>

        <RuleGrid :grid-events="gridEvents" :grid-options="gridOptions" class="app-vxe-card-grid">
          <template #name="{ row }">
            <div class="integration-rules__stack-cell">
              <strong>{{ row.name }}</strong>
              <span>{{ row.remark || '未填写说明' }}</span>
            </div>
          </template>

          <template #event="{ row }">
            <div class="integration-rules__stack-cell">
              <strong>{{ eventLabel(row.eventCode) }}</strong>
              <span>{{ row.eventCode }}</span>
            </div>
          </template>

          <template #template="{ row }">
            <div class="integration-rules__stack-cell">
              <strong>{{ row.templateName || '-' }}</strong>
              <span>{{ row.templateCode || '-' }}</span>
            </div>
          </template>

          <template #connector="{ row }">
            {{ row.connectorCode || connectorLabel(row.connectorId) }}
          </template>

          <template #channel="{ row }">
            <Tag color="blue">{{ channelLabel(row.channelType) }}</Tag>
          </template>

          <template #status="{ row }">
            <Tag :color="statusColor(row.status)">{{ statusLabel(row.status) }}</Tag>
          </template>

          <template #rateLimit="{ row }">
            <span v-if="row.rateLimitWindowMinutes > 0 && row.rateLimitMaxCount > 0">
              {{ row.rateLimitWindowMinutes }} 分钟 / {{ row.rateLimitMaxCount }} 次
            </span>
            <span v-else>不限</span>
          </template>

          <template #lastTriggeredTime="{ row }">
            {{ formatDate(row.lastTriggeredTime) }}
          </template>

          <template #actions="{ row }">
            <Space>
              <Switch
                :checked="row.status === 'Active'"
                :loading="actionLoadingKey === `toggle:${row.id}`"
                checked-children="启用"
                un-checked-children="停用"
                @change="(checked) => handleToggle(row, checked)"
              />
              <Button size="small" type="link" @click="openEdit(row)">编辑</Button>
            </Space>
          </template>
        </RuleGrid>
      </Card>
    </div>

    <Drawer v-model:open="drawerOpen" :destroy-on-close="true" :title="editingId ? '编辑通知规则' : '新增通知规则'" width="860">
      <template #extra>
        <Space>
          <Button @click="drawerOpen = false">取消</Button>
          <Button :loading="saving" type="primary" @click="handleSave">保存</Button>
        </Space>
      </template>

      <Form :label-col="{ span: 5 }" :model="form" :wrapper-col="{ span: 18 }" layout="horizontal">
        <Form.Item label="规则编码" required>
          <Input v-model:value="form.ruleCode" :maxlength="64" placeholder="project-risk-created-to-pm" />
        </Form.Item>
        <Form.Item label="规则名称" required>
          <Input v-model:value="form.name" :maxlength="128" placeholder="风险新增通知项目经理" />
        </Form.Item>
        <Form.Item label="触发事件" required>
          <Select v-model:value="form.eventCode" :options="eventOptions" @change="handleEventChange" />
        </Form.Item>
        <Form.Item label="事件名称">
          <Input v-model:value="form.eventName" :maxlength="128" />
        </Form.Item>
        <Form.Item label="消息模板" required>
          <Select v-model:value="form.templateId" :options="templateOptions" placeholder="请选择消息模板" />
        </Form.Item>
        <Form.Item label="连接器">
          <Select
            v-model:value="form.connectorId"
            allow-clear
            :options="editableConnectorOptions"
            placeholder="不选择时由接收人规则动态决定"
          />
        </Form.Item>
        <Form.Item label="通知通道">
          <Select v-model:value="form.channelType" :options="channelOptions" />
        </Form.Item>
        <Form.Item label="接收 ID 类型">
          <Select v-model:value="form.targetType" :options="targetTypeOptions" />
        </Form.Item>
        <Form.Item label="状态">
          <Select v-model:value="form.status" :options="editableStatusOptions" />
        </Form.Item>
        <Form.Item label="静默期">
          <div class="integration-rules__time-range flex items-center gap-2">
            <Input v-model:value="form.quietStartTime" :maxlength="16" placeholder="开始，如 22:00" />
            <span>至</span>
            <Input v-model:value="form.quietEndTime" :maxlength="16" placeholder="结束，如 08:00" />
          </div>
        </Form.Item>
        <Form.Item label="频控窗口">
          <div class="integration-rules__rate-fields flex items-center gap-2">
            <InputNumber v-model:value="form.rateLimitWindowMinutes" :min="0" :max="1440" addon-after="分钟" />
            <InputNumber v-model:value="form.rateLimitMaxCount" :min="0" :max="100000" addon-after="次" />
          </div>
        </Form.Item>
        <Form.Item label="接收人规则">
          <Input.TextArea v-model:value="form.recipientRuleJson" :auto-size="{ minRows: 7, maxRows: 14 }" :maxlength="16_000" />
        </Form.Item>
        <Form.Item label="触发条件">
          <Input.TextArea v-model:value="form.conditionJson" :auto-size="{ minRows: 5, maxRows: 12 }" :maxlength="16_000" />
        </Form.Item>
        <Form.Item label="备注">
          <Input.TextArea v-model:value="form.remark" :auto-size="{ minRows: 2, maxRows: 4 }" :maxlength="512" />
        </Form.Item>
      </Form>
    </Drawer>
  </Page>
</template>

<style scoped>
@reference "@vben/tailwind-config/theme";
.integration-rules {
  display: flex;
  flex-direction: column;
   @apply gap-4;
}

.integration-rules__metrics {
  display: grid;
   @apply gap-3;
  grid-template-columns: repeat(4, minmax(0, 1fr));
}

.integration-rules__filters {
  display: grid;
   @apply gap-3;
  grid-template-columns: minmax(260px, 1fr) minmax(190px, 220px) minmax(170px, 220px) minmax(140px, 170px) auto;
   @apply mb-3;
}

.integration-rules__stack-cell {
  display: flex;
  flex-direction: column;
   @apply gap-0.5;
  min-width: 0;
}

.integration-rules__stack-cell span {
  overflow: hidden;
  color: var(--ant-color-text-secondary);
  font-size: 12px;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.integration-rules__time-range,
.integration-rules__rate-fields {
  display: grid;
  align-items: center;
   @apply gap-2;
  grid-template-columns: 1fr auto 1fr;
}

.integration-rules__rate-fields {
  grid-template-columns: 1fr 1fr;
}

.integration-rules__time-range span {
  color: var(--ant-color-text-secondary);
}

@media (max-width: 960px) {
  .integration-rules__metrics {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .integration-rules__filters {
    grid-template-columns: 1fr;
  }
}
</style>
