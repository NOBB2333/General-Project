<script lang="ts" setup>
import type { NativeVxeGridEvents, VxeGridProps } from '#/adapter/vxe-table';
import type { IntegrationApi } from '#/api/core';

import { computed, onMounted, reactive, ref } from 'vue';

import { useAccess } from '@vben/access';
import { Page } from '@vben/common-ui';

import {
  Alert,
  Button,
  Card,
  DatePicker,
  Drawer,
  Input,
  Modal,
  Popconfirm,
  Select,
  Space,
  Statistic,
  Tag,
  message,
} from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import {
  exportIntegrationDeliveryRecordsApi,
  getIntegrationConnectorsApi,
  getIntegrationDeliveryRecordsApi,
  getIntegrationDeliverySummaryApi,
  retryIntegrationDeliveryApi,
  skipIntegrationDeliveryApi,
} from '#/api/core';
import { useActionLoading } from '#/composables/platform/use-action-loading';

defineOptions({ name: 'IntegrationDeliveryRecordsPage' });

const DELIVERY_MANAGE_CODE = 'Integration.Delivery.Manage';
const DELIVERY_EXPORT_CODE = 'Integration.Delivery.Export';

const statusOptions = [
  { label: '全部状态', value: 'All' },
  { label: '待投递', value: 'Pending' },
  { label: '投递中', value: 'Sending' },
  { label: '已成功', value: 'Succeeded' },
  { label: '失败待重试', value: 'Failed' },
  { label: '死信', value: 'DeadLetter' },
  { label: '已跳过', value: 'Skipped' },
];

const messageTypeOptions = [
  { label: '全部类型', value: '' },
  { label: '文本', value: 'Text' },
  { label: '富文本', value: 'Markdown' },
  { label: '互动卡片', value: 'Card' },
];

const targetTypeOptions = [
  { label: '全部接收类型', value: '' },
  { label: 'Open ID', value: 'OpenId' },
  { label: 'User ID', value: 'User' },
  { label: '群 Chat ID', value: 'Chat' },
  { label: 'Union ID', value: 'UnionId' },
  { label: '邮箱', value: 'Email' },
];

const columns = [
  { field: 'creationTime', slots: { default: 'creationTime' }, title: '创建时间', width: 170 },
  { field: 'connectorCode', slots: { default: 'connector' }, title: '连接器', width: 150 },
  { field: 'status', slots: { default: 'status' }, title: '状态', width: 120 },
  { field: 'targetId', slots: { default: 'target' }, title: '接收对象', minWidth: 220 },
  { field: 'messageType', slots: { default: 'messageType' }, title: '消息类型', width: 110 },
  { align: 'left', field: 'payloadPreview', showOverflow: 'tooltip', title: '内容预览', minWidth: 280 },
  { field: 'attemptCount', slots: { default: 'attempt' }, title: '尝试', width: 100 },
  { field: 'durationMilliseconds', slots: { default: 'duration' }, title: '耗时', width: 100 },
  { field: 'nextRetryTime', slots: { default: 'nextRetryTime' }, title: '下次重试', width: 170 },
  { fixed: 'right', slots: { default: 'actions' }, title: '操作', width: 240 },
] satisfies VxeGridProps<IntegrationApi.DeliveryRecordItem>['columns'];

const { hasAccessByCodes } = useAccess();
const { actionLoadingKey, runAction } = useActionLoading();
const canManage = computed(() => hasAccessByCodes([DELIVERY_MANAGE_CODE]));
const canExport = computed(() => hasAccessByCodes([DELIVERY_EXPORT_CODE]));
const connectors = ref<IntegrationApi.ConnectorItem[]>([]);
const records = ref<IntegrationApi.DeliveryRecordItem[]>([]);
const totalCount = ref(0);
const loading = ref(false);
const exporting = ref(false);
const detailOpen = ref(false);
const skipModalOpen = ref(false);
const activeRecord = ref<IntegrationApi.DeliveryRecordItem>();
const skipReason = ref('');
const pageIndex = ref(1);
const pageSize = ref(20);
const dateRange = ref<[string, string]>();
const summary = ref<IntegrationApi.DeliverySummary>({
  averageDurationMilliseconds: 0,
  connectorRates: [],
  deadLetterCount: 0,
  failedCount: 0,
  pendingCount: 0,
  skippedCount: 0,
  succeededCount: 0,
  totalCount: 0,
});

const query = reactive<IntegrationApi.DeliveryRecordQuery>({
  connectorCode: '',
  keyword: '',
  messageType: '',
  status: 'All',
  targetType: '',
});

const connectorOptions = computed(() => [
  { label: '全部连接器', value: '' },
  ...connectors.value.map((item) => ({
    label: item.displayName,
    value: item.connectorCode,
  })),
]);

const metrics = computed(() => [
  { label: '投递总数', value: summary.value.totalCount },
  { label: '成功投递', value: summary.value.succeededCount },
  { label: '失败 / 死信', value: `${summary.value.failedCount} / ${summary.value.deadLetterCount}` },
  { label: '平均耗时', value: `${summary.value.averageDurationMilliseconds} ms` },
]);

const gridOptions = computed<VxeGridProps<IntegrationApi.DeliveryRecordItem>>(() => ({
  cellConfig: { height: 56 },
  columns,
  data: records.value,
  emptyText: '暂无投递记录',
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

const gridEvents: NativeVxeGridEvents<IntegrationApi.DeliveryRecordItem> = {
  'page-change': ({ currentPage, pageSize: nextPageSize }) => {
    const normalizedPageSize = nextPageSize || pageSize.value;
    pageIndex.value = normalizedPageSize === pageSize.value ? currentPage || 1 : 1;
    pageSize.value = normalizedPageSize;
    void loadRecords();
  },
};

const [DeliveryGrid] = useVbenVxeGrid<IntegrationApi.DeliveryRecordItem>({
  gridEvents,
  gridOptions: {},
});

function buildQuery() {
  return {
    ...query,
    connectorCode: query.connectorCode || undefined,
    createdFrom: dateRange.value?.[0],
    createdTo: dateRange.value?.[1],
    keyword: query.keyword?.trim() || undefined,
    maxResultCount: pageSize.value,
    messageType: query.messageType || undefined,
    skipCount: (pageIndex.value - 1) * pageSize.value,
    status: query.status || 'All',
    targetType: query.targetType || undefined,
  };
}

function buildExportQuery(): IntegrationApi.DeliveryRecordQuery {
  return {
    ...buildQuery(),
    maxResultCount: 5000,
    skipCount: 0,
  };
}

function connectorLabel(code: string) {
  return connectorOptions.value.find((item) => item.value === code)?.label || code || '-';
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
  if (value === 'Succeeded') {
    return 'green';
  }

  if (value === 'Failed') {
    return 'orange';
  }

  if (value === 'DeadLetter') {
    return 'red';
  }

  if (value === 'Pending' || value === 'Sending') {
    return 'blue';
  }

  return 'default';
}

function canRetry(record: IntegrationApi.DeliveryRecordItem) {
  return canManage.value && ['DeadLetter', 'Failed'].includes(record.status);
}

function canSkip(record: IntegrationApi.DeliveryRecordItem) {
  return canManage.value && !['Skipped', 'Succeeded'].includes(record.status);
}

async function loadConnectors() {
  connectors.value = await getIntegrationConnectorsApi({ includeDisabled: true });
}

async function loadRecords() {
  loading.value = true;
  try {
    const [recordResult, summaryResult] = await Promise.all([
      getIntegrationDeliveryRecordsApi(buildQuery()),
      getIntegrationDeliverySummaryApi(buildQuery()),
    ]);
    records.value = recordResult.items;
    totalCount.value = recordResult.totalCount;
    summary.value = summaryResult;
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

async function exportRecords() {
  if (!canExport.value) {
    message.warning('当前账号没有导出投递审计权限');
    return;
  }

  exporting.value = true;
  try {
    const blob = await exportIntegrationDeliveryRecordsApi(buildExportQuery());
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = `外部系统投递审计_${getExportTimestamp()}.csv`;
    document.body.append(anchor);
    anchor.click();
    anchor.remove();
    URL.revokeObjectURL(url);
    message.success('投递审计导出完成');
  } catch (error) {
    const description = error instanceof Error ? error.message : '投递审计导出失败';
    message.error(description || '投递审计导出失败');
  } finally {
    exporting.value = false;
  }
}

function getExportTimestamp() {
  return new Date().toISOString().slice(0, 19).replaceAll(':', '').replace('T', '');
}

function openDetail(record: IntegrationApi.DeliveryRecordItem) {
  activeRecord.value = record;
  detailOpen.value = true;
}

function openSkip(record: IntegrationApi.DeliveryRecordItem) {
  activeRecord.value = record;
  skipReason.value = '';
  skipModalOpen.value = true;
}

async function retryRecord(record: IntegrationApi.DeliveryRecordItem) {
  await runAction(`retry:${record.id}`, async () => {
    await retryIntegrationDeliveryApi(record.id);
    message.success('已重新发起投递。');
    await loadRecords();
  });
}

async function submitSkip() {
  if (!activeRecord.value) {
    return;
  }

  await runAction(`skip:${activeRecord.value.id}`, async () => {
    await skipIntegrationDeliveryApi(activeRecord.value!.id, { reason: skipReason.value.trim() });
    message.success('已标记跳过。');
    skipModalOpen.value = false;
    await loadRecords();
  });
}

onMounted(async () => {
  await loadConnectors();
  await loadRecords();
});
</script>

<template>
  <Page description="查看外部系统消息投递、失败重试、死信和连接器成功率，作为飞书、钉钉、企业微信通知的审计入口。" title="投递日志">
    <div class="integration-delivery flex flex-col gap-4">
      <Alert
        show-icon
        type="info"
        message="所有外部系统消息先记录投递日志，再调用连接器适配器。失败记录可人工重试或标记跳过，后续定时重试与死信处理也基于同一张投递记录表。"
      />

      <div class="integration-delivery__metrics grid grid-cols-4 gap-3 max-[760px]:grid-cols-1">
        <Card v-for="metric in metrics" :key="metric.label" :bordered="false" class="rounded-2xl" size="small">
          <Statistic :title="metric.label" :value="metric.value" />
        </Card>
      </div>

      <Card :bordered="false" class="rounded-2xl" title="投递记录">
        <div class="integration-delivery__filters mb-3 grid gap-3 max-[1280px]:grid-cols-2 max-[760px]:grid-cols-1">
          <Input v-model:value="query.keyword" allow-clear placeholder="搜索接收人、消息、外部消息 ID、错误" @press-enter="refresh(true)" />
          <Select v-model:value="query.connectorCode" :options="connectorOptions" @change="refresh(true)" />
          <Select v-model:value="query.status" :options="statusOptions" @change="refresh(true)" />
          <Select v-model:value="query.messageType" :options="messageTypeOptions" @change="refresh(true)" />
          <Select v-model:value="query.targetType" :options="targetTypeOptions" @change="refresh(true)" />
          <DatePicker.RangePicker v-model:value="dateRange" value-format="YYYY-MM-DD" @change="refresh(true)" />
          <Space>
            <Button type="primary" @click="refresh(true)">查询</Button>
            <Button v-if="canExport" :loading="exporting" @click="exportRecords">导出审计</Button>
          </Space>
        </div>

        <DeliveryGrid :grid-events="gridEvents" :grid-options="gridOptions" class="app-vxe-card-grid">
          <template #creationTime="{ row }">
            {{ formatDate(row.creationTime) }}
          </template>

          <template #connector="{ row }">
            <div class="integration-delivery__stack-cell">
              <strong>{{ connectorLabel(row.connectorCode) }}</strong>
              <span>{{ row.providerType }}</span>
            </div>
          </template>

          <template #status="{ row }">
            <Tag :color="statusColor(row.status)">{{ statusLabel(row.status) }}</Tag>
          </template>

          <template #target="{ row }">
            <div class="integration-delivery__stack-cell">
              <strong>{{ row.targetType }}</strong>
              <span>{{ row.targetId }}</span>
            </div>
          </template>

          <template #messageType="{ row }">
            <Tag>{{ row.messageType }}</Tag>
          </template>

          <template #attempt="{ row }">
            {{ row.attemptCount }} / {{ row.maxRetryCount }}
          </template>

          <template #duration="{ row }">
            {{ row.durationMilliseconds }} ms
          </template>

          <template #nextRetryTime="{ row }">
            {{ formatDate(row.nextRetryTime) }}
          </template>

          <template #actions="{ row }">
            <Space>
              <Button size="small" type="link" @click="openDetail(row)">详情</Button>
              <Popconfirm v-if="canRetry(row)" title="重新投递该消息？" @confirm="retryRecord(row)">
                <Button :loading="actionLoadingKey === `retry:${row.id}`" size="small" type="link">重试</Button>
              </Popconfirm>
              <Button v-if="canSkip(row)" size="small" type="link" danger @click="openSkip(row)">跳过</Button>
            </Space>
          </template>
        </DeliveryGrid>
      </Card>

      <Card :bordered="false" class="rounded-2xl" title="连接器成功率">
        <div class="integration-delivery__rate-list flex flex-col gap-2.5">
          <div v-for="item in summary.connectorRates" :key="item.connectorCode" class="integration-delivery__rate-item flex items-center justify-between gap-4 py-2.5 max-[760px]:flex-col max-[760px]:items-start">
            <div>
              <strong>{{ item.displayName }}</strong>
              <span>{{ item.connectorCode }}</span>
            </div>
            <div>
              <Tag color="green">成功 {{ item.succeededCount }}</Tag>
              <Tag color="red">失败 {{ item.failedCount }}</Tag>
              <Tag>{{ item.successRate }}%</Tag>
            </div>
          </div>
          <div v-if="summary.connectorRates.length === 0" class="integration-delivery__empty-rate">暂无连接器投递统计</div>
        </div>
      </Card>
    </div>

    <Drawer v-model:open="detailOpen" :title="activeRecord ? `投递详情 · ${activeRecord.connectorCode}` : '投递详情'" width="760">
      <template v-if="activeRecord">
        <div class="integration-delivery__detail flex flex-col gap-4">
          <div class="integration-delivery__detail-grid grid items-center gap-3">
            <span>状态</span>
            <Tag :color="statusColor(activeRecord.status)">{{ statusLabel(activeRecord.status) }}</Tag>
            <span>接收对象</span>
            <strong>{{ activeRecord.targetType }} · {{ activeRecord.targetId }}</strong>
            <span>外部消息 ID</span>
            <strong>{{ activeRecord.externalMessageId || '-' }}</strong>
            <span>失败原因</span>
            <strong>{{ activeRecord.errorMessage || activeRecord.deadLetterReason || '-' }}</strong>
            <span>创建时间</span>
            <strong>{{ formatDate(activeRecord.creationTime) }}</strong>
            <span>最近尝试</span>
            <strong>{{ formatDate(activeRecord.lastAttemptTime) }}</strong>
          </div>

          <section>
            <h3>请求 JSON</h3>
            <pre>{{ formatJson(activeRecord.requestJson) }}</pre>
          </section>
          <section>
            <h3>响应 JSON</h3>
            <pre>{{ formatJson(activeRecord.responseJson) }}</pre>
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
.integration-delivery {
  display: flex;
  flex-direction: column;
   @apply gap-4;
}

.integration-delivery__metrics {
  display: grid;
   @apply gap-3;
  grid-template-columns: repeat(4, minmax(0, 1fr));
}

.integration-delivery__filters {
  display: grid;
   @apply gap-3;
  grid-template-columns: minmax(220px, 1.2fr) repeat(4, minmax(140px, 0.7fr)) minmax(240px, 1fr) auto;
   @apply mb-3;
}

.integration-delivery__stack-cell {
  display: flex;
  flex-direction: column;
   @apply gap-0.5;
  min-width: 0;
}

.integration-delivery__stack-cell span {
  overflow: hidden;
  color: var(--ant-color-text-secondary);
  font-size: 12px;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.integration-delivery__rate-list {
  display: flex;
  flex-direction: column;
   @apply gap-2.5;
}

.integration-delivery__rate-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
   @apply gap-4;
   @apply py-2.5;
  border-bottom: 1px solid var(--ant-color-border-secondary);
}

.integration-delivery__rate-item:last-child {
  border-bottom: 0;
}

.integration-delivery__rate-item > div:first-child {
  display: flex;
  flex-direction: column;
   @apply gap-0.5;
}

.integration-delivery__rate-item span,
.integration-delivery__empty-rate {
  color: var(--ant-color-text-secondary);
  font-size: 12px;
}

.integration-delivery__detail {
  display: flex;
  flex-direction: column;
   @apply gap-4;
}

.integration-delivery__detail-grid {
  display: grid;
  align-items: center;
   @apply gap-x-4 gap-y-3;
  grid-template-columns: 110px minmax(0, 1fr);
}

.integration-delivery__detail-grid > span {
  color: var(--ant-color-text-secondary);
}

.integration-delivery__detail h3 {
   @apply mb-2;
  color: var(--ant-color-text);
  font-size: 14px;
  font-weight: 600;
}

.integration-delivery__detail pre {
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
  .integration-delivery__filters {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 760px) {
  .integration-delivery__metrics,
  .integration-delivery__filters {
    grid-template-columns: 1fr;
  }

  .integration-delivery__rate-item {
    align-items: flex-start;
    flex-direction: column;
  }
}
</style>
