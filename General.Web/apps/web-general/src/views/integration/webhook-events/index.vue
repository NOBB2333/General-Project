<script lang="ts" setup>
import type { NativeVxeGridEvents, VxeGridProps } from '#/adapter/vxe-table';
import type { IntegrationApi } from '#/api/core';

import { computed, onMounted, reactive, ref } from 'vue';

import { useAccess } from '@vben/access';
import { Page } from '@vben/common-ui';

import { Alert, Button, Card, Drawer, Form, Input, Modal, Popconfirm, Select, Space, Tag, message } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import {
  getIntegrationWebhookEventsApi,
  receiveIntegrationWebhookEventApi,
  retryIntegrationWebhookEventApi,
  skipIntegrationWebhookEventApi,
} from '#/api/core';
import { useActionLoading } from '#/composables/platform/use-action-loading';

defineOptions({ name: 'IntegrationWebhookEventsPage' });

const WEBHOOK_MANAGE_CODE = 'Integration.Webhook.Manage';

const statusOptions = [
  { label: '全部状态', value: 'All' },
  { label: '已接收', value: 'Received' },
  { label: '已解析', value: 'Parsed' },
  { label: '已处理', value: 'Processed' },
  { label: '解析失败', value: 'Failed' },
  { label: '重复忽略', value: 'Duplicate' },
  { label: '已跳过', value: 'Skipped' },
];

const providerOptions = [
  { label: '全部平台', value: '' },
  { label: '飞书', value: 'Feishu' },
  { label: '钉钉', value: 'DingTalk' },
  { label: '企业微信', value: 'WeCom' },
];

const signatureOptions = [
  { label: '未检查', value: 'Unchecked' },
  { label: '已通过', value: 'Passed' },
  { label: '未通过', value: 'Failed' },
];

const columns = [
  { field: 'creationTime', slots: { default: 'creationTime' }, title: '接收时间', width: 170 },
  { field: 'providerType', slots: { default: 'provider' }, title: '来源平台', width: 150 },
  { field: 'status', slots: { default: 'status' }, title: '状态', width: 120 },
  { field: 'eventType', slots: { default: 'eventType' }, title: '事件类型', minWidth: 220 },
  { field: 'actionCode', showOverflow: 'tooltip', title: '动作编码', minWidth: 160 },
  { field: 'businessKey', showOverflow: 'tooltip', title: '业务键', minWidth: 180 },
  { field: 'dedupeKey', showOverflow: 'tooltip', title: '幂等键', minWidth: 260 },
  { field: 'retryCount', title: '重试', width: 80 },
  { fixed: 'right', slots: { default: 'actions' }, title: '操作', width: 220 },
] satisfies VxeGridProps<IntegrationApi.WebhookEventItem>['columns'];

const { hasAccessByCodes } = useAccess();
const { actionLoadingKey, runAction } = useActionLoading();
const canManage = computed(() => hasAccessByCodes([WEBHOOK_MANAGE_CODE]));
const records = ref<IntegrationApi.WebhookEventItem[]>([]);
const loading = ref(false);
const totalCount = ref(0);
const pageIndex = ref(1);
const pageSize = ref(20);
const detailOpen = ref(false);
const receiveModalOpen = ref(false);
const skipModalOpen = ref(false);
const activeRecord = ref<IntegrationApi.WebhookEventItem>();
const skipReason = ref('');

const query = reactive<IntegrationApi.WebhookEventQuery>({
  eventType: '',
  keyword: '',
  providerType: '',
  status: 'All',
});

const receiveForm = reactive<IntegrationApi.WebhookReceiveInput>({
  actionCode: '',
  businessKey: '',
  connectorCode: '',
  dedupeKey: '',
  eventId: '',
  eventType: 'message',
  headersJson: '{}',
  providerType: 'Feishu',
  rawBody: '',
  rawPayloadJson: '{\n  "event": {\n    "action": "card_click",\n    "business_key": "PROJECT-001"\n  }\n}',
  signatureStatus: 'Unchecked',
  sourceIp: '',
});

const gridOptions = computed<VxeGridProps<IntegrationApi.WebhookEventItem>>(() => ({
  cellConfig: { height: 56 },
  columns,
  data: records.value,
  emptyText: '暂无 Webhook 事件',
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

const gridEvents: NativeVxeGridEvents<IntegrationApi.WebhookEventItem> = {
  'page-change': ({ currentPage, pageSize: nextPageSize }) => {
    const normalizedPageSize = nextPageSize || pageSize.value;
    pageIndex.value = normalizedPageSize === pageSize.value ? currentPage || 1 : 1;
    pageSize.value = normalizedPageSize;
    void loadRecords();
  },
};

const [WebhookGrid] = useVbenVxeGrid<IntegrationApi.WebhookEventItem>({
  gridEvents,
  gridOptions: {},
});

function buildQuery() {
  return {
    eventType: query.eventType?.trim() || undefined,
    keyword: query.keyword?.trim() || undefined,
    maxResultCount: pageSize.value,
    providerType: query.providerType || undefined,
    skipCount: (pageIndex.value - 1) * pageSize.value,
    status: query.status || 'All',
  };
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

function statusLabel(value: string) {
  return statusOptions.find((item) => item.value === value)?.label || value || '-';
}

function statusColor(value: string) {
  if (value === 'Parsed' || value === 'Processed') {
    return 'green';
  }

  if (value === 'Failed') {
    return 'red';
  }

  if (value === 'Duplicate' || value === 'Skipped') {
    return 'default';
  }

  return 'blue';
}

function providerLabel(value: string) {
  return providerOptions.find((item) => item.value === value)?.label || value || '-';
}

function validateJson(value: string, label: string) {
  try {
    JSON.parse(value.trim() || '{}');
  } catch {
    throw new Error(`${label} 必须是合法 JSON`);
  }
}

async function loadRecords() {
  loading.value = true;
  try {
    const result = await getIntegrationWebhookEventsApi(buildQuery());
    records.value = result.items;
    totalCount.value = result.totalCount;
  } finally {
    loading.value = false;
  }
}

async function refresh(resetPage = false) {
  if (resetPage) {
    pageIndex.value = 1;
  }

  await loadRecords();
}

function openDetail(record: IntegrationApi.WebhookEventItem) {
  activeRecord.value = record;
  detailOpen.value = true;
}

function openReceive() {
  receiveModalOpen.value = true;
}

function openSkip(record: IntegrationApi.WebhookEventItem) {
  activeRecord.value = record;
  skipReason.value = '';
  skipModalOpen.value = true;
}

async function submitReceive() {
  try {
    validateJson(receiveForm.headersJson, '请求头 JSON');
    validateJson(receiveForm.rawPayloadJson, '原始 Payload');
  } catch (error) {
    message.warning((error as Error).message);
    return;
  }

  await runAction('receive-webhook', async () => {
    await receiveIntegrationWebhookEventApi({
      ...receiveForm,
      actionCode: receiveForm.actionCode.trim(),
      businessKey: receiveForm.businessKey.trim(),
      connectorCode: receiveForm.connectorCode.trim(),
      dedupeKey: receiveForm.dedupeKey.trim(),
      eventId: receiveForm.eventId.trim(),
      eventType: receiveForm.eventType.trim() || 'Unknown',
      headersJson: receiveForm.headersJson.trim() || '{}',
      rawPayloadJson: receiveForm.rawPayloadJson.trim() || '{}',
    });
    message.success('Webhook 事件已接收。');
    receiveModalOpen.value = false;
    await refresh(true);
  });
}

async function retryRecord(record: IntegrationApi.WebhookEventItem) {
  await runAction(`retry:${record.id}`, async () => {
    await retryIntegrationWebhookEventApi(record.id);
    message.success('已重新解析 Webhook 事件。');
    await loadRecords();
  });
}

async function submitSkip() {
  if (!activeRecord.value) {
    return;
  }

  await runAction(`skip:${activeRecord.value.id}`, async () => {
    await skipIntegrationWebhookEventApi(activeRecord.value!.id, { reason: skipReason.value.trim() });
    message.success('已标记跳过。');
    skipModalOpen.value = false;
    await loadRecords();
  });
}

onMounted(loadRecords);
</script>

<template>
  <Page description="记录飞书、钉钉、企业微信等外部系统回调原始事件，支持幂等去重、解析状态、人工重试和跳过。" title="Webhook 事件">
    <div class="integration-webhook flex flex-col gap-4">
      <Alert
        show-icon
        type="info"
        message="当前已落地 Webhook 事件基座：原始事件记录、解析状态、幂等键去重、人工重试和跳过。真实平台验签和卡片回调业务处理会在连接器适配器中继续补齐。"
      />

      <Card :bordered="false" class="rounded-2xl" title="事件记录">
        <div class="integration-webhook__filters mb-3 grid gap-3">
          <Input v-model:value="query.keyword" allow-clear placeholder="搜索事件 ID、幂等键、业务键、错误" @press-enter="refresh(true)" />
          <Select v-model:value="query.providerType" :options="providerOptions" @change="refresh(true)" />
          <Select v-model:value="query.status" :options="statusOptions" @change="refresh(true)" />
          <Input v-model:value="query.eventType" allow-clear placeholder="事件类型" @press-enter="refresh(true)" />
          <Button type="primary" @click="refresh(true)">查询</Button>
          <Button v-if="canManage" @click="openReceive">模拟接收</Button>
        </div>

        <WebhookGrid :grid-events="gridEvents" :grid-options="gridOptions" class="app-vxe-card-grid">
          <template #creationTime="{ row }">
            {{ formatDate(row.creationTime) }}
          </template>

          <template #provider="{ row }">
            <div class="integration-webhook__stack-cell">
              <strong>{{ providerLabel(row.providerType) }}</strong>
              <span>{{ row.connectorCode || '-' }}</span>
            </div>
          </template>

          <template #status="{ row }">
            <Tag :color="statusColor(row.status)">{{ statusLabel(row.status) }}</Tag>
          </template>

          <template #eventType="{ row }">
            <div class="integration-webhook__stack-cell">
              <strong>{{ row.eventType }}</strong>
              <span>{{ row.eventId || '-' }}</span>
            </div>
          </template>

          <template #actions="{ row }">
            <Space>
              <Button size="small" type="link" @click="openDetail(row)">详情</Button>
              <Popconfirm v-if="canManage && row.status === 'Failed'" title="重新解析该事件？" @confirm="retryRecord(row)">
                <Button :loading="actionLoadingKey === `retry:${row.id}`" size="small" type="link">重试</Button>
              </Popconfirm>
              <Button v-if="canManage && !['Processed', 'Skipped', 'Duplicate'].includes(row.status)" size="small" type="link" danger @click="openSkip(row)">跳过</Button>
            </Space>
          </template>
        </WebhookGrid>
      </Card>
    </div>

    <Drawer v-model:open="detailOpen" :title="activeRecord ? `Webhook 详情 · ${activeRecord.providerType}` : 'Webhook 详情'" width="820">
      <template v-if="activeRecord">
        <div class="integration-webhook__detail flex flex-col gap-4">
          <div class="integration-webhook__detail-grid grid items-center gap-3">
            <span>状态</span>
            <Tag :color="statusColor(activeRecord.status)">{{ statusLabel(activeRecord.status) }}</Tag>
            <span>验签状态</span>
            <strong>{{ activeRecord.signatureStatus }}</strong>
            <span>动作编码</span>
            <strong>{{ activeRecord.actionCode || '-' }}</strong>
            <span>业务键</span>
            <strong>{{ activeRecord.businessKey || '-' }}</strong>
            <span>错误信息</span>
            <strong>{{ activeRecord.errorMessage || '-' }}</strong>
            <span>解析时间</span>
            <strong>{{ formatDate(activeRecord.resolvedTime) }}</strong>
          </div>

          <section>
            <h3>标准化 Payload</h3>
            <pre>{{ formatJson(activeRecord.normalizedPayloadJson) }}</pre>
          </section>
          <section>
            <h3>原始 Payload</h3>
            <pre>{{ formatJson(activeRecord.rawPayloadJson) }}</pre>
          </section>
          <section>
            <h3>请求头</h3>
            <pre>{{ formatJson(activeRecord.headersJson) }}</pre>
          </section>
        </div>
      </template>
    </Drawer>

    <Drawer
      v-model:open="receiveModalOpen"
      destroy-on-close
      title="模拟接收 Webhook"
      width="820px"
    >
      <Form :label-col="{ span: 5 }" :model="receiveForm" :wrapper-col="{ span: 18 }" layout="horizontal">
        <Form.Item label="平台 / 连接器">
          <div class="integration-webhook__compact flex flex-col gap-2">
            <Select v-model:value="receiveForm.providerType" :options="providerOptions.slice(1)" />
            <Input v-model:value="receiveForm.connectorCode" :maxlength="64" placeholder="可选，连接器编码" />
          </div>
        </Form.Item>
        <Form.Item label="事件类型 / ID">
          <div class="integration-webhook__compact flex flex-col gap-2">
            <Input v-model:value="receiveForm.eventType" :maxlength="128" />
            <Input v-model:value="receiveForm.eventId" :maxlength="256" />
          </div>
        </Form.Item>
        <Form.Item label="动作 / 业务键">
          <div class="integration-webhook__compact flex flex-col gap-2">
            <Input v-model:value="receiveForm.actionCode" :maxlength="128" />
            <Input v-model:value="receiveForm.businessKey" :maxlength="256" />
          </div>
        </Form.Item>
        <Form.Item label="幂等键">
          <Input v-model:value="receiveForm.dedupeKey" :maxlength="256" placeholder="为空时使用平台 + 事件 ID 或 Payload 哈希" />
        </Form.Item>
        <Form.Item label="验签状态">
          <Select v-model:value="receiveForm.signatureStatus" :options="signatureOptions" />
        </Form.Item>
        <Form.Item label="请求头 JSON">
          <Input.TextArea v-model:value="receiveForm.headersJson" :auto-size="{ minRows: 3, maxRows: 6 }" :maxlength="16_000" />
        </Form.Item>
        <Form.Item label="原始 Payload">
          <Input.TextArea v-model:value="receiveForm.rawPayloadJson" :auto-size="{ minRows: 8, maxRows: 14 }" :maxlength="64_000" />
        </Form.Item>
      </Form>
      <template #footer>
        <div class="flex justify-end gap-2">
          <Button @click="receiveModalOpen = false">取消</Button>
          <Button :loading="actionLoadingKey === 'receive-webhook'" type="primary" @click="submitReceive">提交</Button>
        </div>
      </template>
    </Drawer>

    <Modal
      v-model:open="skipModalOpen"
      :confirm-loading="actionLoadingKey === `skip:${activeRecord?.id}`"
      title="标记跳过"
      @ok="submitSkip"
    >
      <Input.TextArea v-model:value="skipReason" :auto-size="{ minRows: 4, maxRows: 8 }" :maxlength="2000" placeholder="请输入跳过原因" />
    </Modal>
  </Page>
</template>

<style scoped>
@reference "@vben/tailwind-config/theme";
.integration-webhook {
  display: flex;
  flex-direction: column;
   @apply gap-4;
}

.integration-webhook__filters {
  display: grid;
   @apply gap-3;
  grid-template-columns: minmax(240px, 1.2fr) 160px 160px 180px auto auto;
   @apply mb-3;
}

.integration-webhook__compact {
  display: grid;
   @apply gap-2;
  grid-template-columns: repeat(2, minmax(0, 1fr));
}

.integration-webhook__stack-cell {
  display: flex;
  flex-direction: column;
   @apply gap-0.5;
  min-width: 0;
}

.integration-webhook__stack-cell span {
  overflow: hidden;
  color: var(--ant-color-text-secondary);
  font-size: 12px;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.integration-webhook__detail {
  display: flex;
  flex-direction: column;
   @apply gap-4;
}

.integration-webhook__detail-grid {
  display: grid;
  align-items: center;
   @apply gap-x-4 gap-y-3;
  grid-template-columns: 110px minmax(0, 1fr);
}

.integration-webhook__detail-grid > span {
  color: var(--ant-color-text-secondary);
}

.integration-webhook__detail h3 {
   @apply mb-2;
  color: var(--ant-color-text);
  font-size: 14px;
  font-weight: 600;
}

.integration-webhook__detail pre {
  max-height: 260px;
   @apply p-3;
  overflow: auto;
  border: 1px solid var(--ant-color-border-secondary);
   @apply rounded-md;
  background: var(--ant-color-fill-quaternary);
  color: var(--ant-color-text);
  font-size: 12px;
  line-height: 1.6;
  white-space: pre-wrap;
}

@media (max-width: 1280px) {
  .integration-webhook__filters {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 760px) {
  .integration-webhook__compact,
  .integration-webhook__filters {
    grid-template-columns: 1fr;
  }
}
</style>
