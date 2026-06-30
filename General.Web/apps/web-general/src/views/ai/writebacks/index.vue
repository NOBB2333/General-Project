<script lang="ts" setup>
import type { AiApi } from '#/api/core';

import { computed, onMounted, reactive, ref } from 'vue';

import { useAccess } from '@vben/access';
import { Page } from '@vben/common-ui';

import { Alert, Button, Card, DatePicker, Drawer, Input, Popconfirm, Select, Space, Table, Tag, message } from 'ant-design-vue';

import {
  approveAiWritebackApi,
  cancelAiWritebackApi,
  getAiWritebacksApi,
  rejectAiWritebackApi,
} from '#/api/core';
import { useActionLoading } from '#/composables/platform/use-action-loading';

defineOptions({ name: 'AiWritebacksPage' });

const WRITEBACK_APPROVE_CODE = 'Ai.Writeback.Approve';

const statusOptions = [
  { label: '全部状态', value: 'All' },
  { label: '待确认', value: 'Pending' },
  { label: '已确认', value: 'Approved' },
  { label: '已驳回', value: 'Rejected' },
  { label: '已取消', value: 'Cancelled' },
  { label: '执行失败', value: 'Failed' },
];

const moduleOptions = [
  { label: '全部模块', value: '' },
  { label: '项目执行', value: 'Project' },
  { label: '经营管理', value: 'Business' },
  { label: '平台治理', value: 'Platform' },
];

const columns = [
  { dataIndex: 'summary', key: 'summary', title: '写回摘要', width: 330 },
  { dataIndex: 'businessModule', key: 'businessModule', title: '模块 / 动作', width: 190 },
  { dataIndex: 'targetEntityType', key: 'targetEntityType', title: '目标对象', width: 220 },
  { dataIndex: 'status', key: 'status', title: '状态', width: 110 },
  { dataIndex: 'sourceType', key: 'sourceType', title: '来源', width: 110 },
  { dataIndex: 'creationTime', key: 'creationTime', title: '创建时间', width: 170 },
  { dataIndex: 'approvedTime', key: 'approvedTime', title: '处理时间', width: 170 },
  { key: 'action', title: '操作', width: 230 },
];

const { hasAccessByCodes } = useAccess();
const { actionLoadingKey, runAction } = useActionLoading();
const canApprove = computed(() => hasAccessByCodes([WRITEBACK_APPROVE_CODE]));
const loading = ref(false);
const drawerVisible = ref(false);
const rejectDrawerVisible = ref(false);
const activeRequest = ref<AiApi.WritebackRequestItem>();
const rejectReason = ref('');
const items = ref<AiApi.WritebackRequestItem[]>([]);
const totalCount = ref(0);
const pageIndex = ref(1);
const pageSize = ref(20);
const dateRange = ref<[string, string]>();

const query = reactive<AiApi.WritebackRequestQuery>({
  businessModule: '',
  keyword: '',
  status: 'Pending',
});

const metrics = computed(() => [
  { label: '待确认', value: items.value.filter((item) => item.status === 'Pending').length },
  { label: '已确认', value: items.value.filter((item) => item.status === 'Approved').length },
  { label: '已驳回', value: items.value.filter((item) => item.status === 'Rejected').length },
  { label: '当前页合计', value: items.value.length },
]);

function normalizeQuery() {
  return {
    ...query,
    businessModule: query.businessModule || undefined,
    createdFrom: dateRange.value?.[0],
    createdTo: dateRange.value?.[1],
    keyword: query.keyword?.trim() || undefined,
    maxResultCount: pageSize.value,
    skipCount: (pageIndex.value - 1) * pageSize.value,
    status: query.status || 'All',
  };
}

function moduleLabel(value: string) {
  return moduleOptions.find((item) => item.value === value)?.label || value || '-';
}

function statusLabel(value: string) {
  return statusOptions.find((item) => item.value === value)?.label || value || '-';
}

function statusColor(value: string) {
  if (value === 'Approved') {
    return 'green';
  }

  if (value === 'Pending') {
    return 'blue';
  }

  if (value === 'Rejected' || value === 'Failed') {
    return 'red';
  }

  if (value === 'Cancelled') {
    return 'orange';
  }

  return 'default';
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

async function loadRequests() {
  loading.value = true;
  try {
    const result = await getAiWritebacksApi(normalizeQuery());
    items.value = result.items;
    totalCount.value = result.totalCount;
  } finally {
    loading.value = false;
  }
}

async function refresh(resetPage = false) {
  if (resetPage) {
    pageIndex.value = 1;
  }

  await loadRequests();
}

function handleTableChange(pagination: { current?: number; pageSize?: number }) {
  const nextPageSize = pagination.pageSize || pageSize.value;
  pageIndex.value = nextPageSize === pageSize.value ? pagination.current || 1 : 1;
  pageSize.value = nextPageSize;
  void loadRequests();
}

function asWritebackRequest(record: AiApi.WritebackRequestItem | Record<string, unknown>) {
  return record as AiApi.WritebackRequestItem;
}

function openDrawer(record: AiApi.WritebackRequestItem | Record<string, unknown>) {
  activeRequest.value = asWritebackRequest(record);
  drawerVisible.value = true;
}

function openReject(record: AiApi.WritebackRequestItem | Record<string, unknown>) {
  activeRequest.value = asWritebackRequest(record);
  rejectReason.value = '';
  rejectDrawerVisible.value = true;
}

async function approveRequest(record: AiApi.WritebackRequestItem | Record<string, unknown>) {
  const request = asWritebackRequest(record);
  await runAction(`approve:${request.id}`, async () => {
    await approveAiWritebackApi(request.id);
    message.success('已确认 AI 写回申请。');
    await loadRequests();
  });
}

async function cancelRequest(record: AiApi.WritebackRequestItem | Record<string, unknown>) {
  const request = asWritebackRequest(record);
  await runAction(`cancel:${request.id}`, async () => {
    await cancelAiWritebackApi(request.id);
    message.success('已取消 AI 写回申请。');
    await loadRequests();
  });
}

async function submitReject() {
  if (!activeRequest.value) {
    return;
  }

  await runAction(`reject:${activeRequest.value.id}`, async () => {
    await rejectAiWritebackApi(activeRequest.value!.id, { reason: rejectReason.value.trim() });
    message.success('已驳回 AI 写回申请。');
    rejectDrawerVisible.value = false;
    await loadRequests();
  });
}

onMounted(loadRequests);
</script>

<template>
  <Page description="集中审核 AI 生成的业务写回申请，确认前不直接修改项目、经营或平台数据。" title="写回确认">
    <div class="ai-writebacks">
      <Alert
        show-icon
        type="info"
        message="AI 只能生成写回申请；项目状态、风险、变更、金额等业务对象必须在这里人工确认后，再由对应业务模块执行。"
      />

      <div class="ai-writebacks__metrics">
        <Card v-for="item in metrics" :key="item.label" :bordered="false" class="ai-writebacks__metric rounded-2xl" size="small">
          <div class="ai-writebacks__metric-body">
            <span>{{ item.label }}</span>
            <strong>{{ item.value }}</strong>
          </div>
        </Card>
      </div>

      <Card :bordered="false" class="rounded-2xl" title="确认队列">
        <div class="ai-writebacks__filters">
          <Input v-model:value="query.keyword" allow-clear placeholder="搜索摘要 / 目标 / JSON 内容" @press-enter="refresh(true)" />
          <Select v-model:value="query.status" :options="statusOptions" @change="refresh(true)" />
          <Select v-model:value="query.businessModule" :options="moduleOptions" @change="refresh(true)" />
          <DatePicker.RangePicker v-model:value="dateRange" value-format="YYYY-MM-DD" @change="refresh(true)" />
          <Button type="primary" @click="refresh(true)">查询</Button>
        </div>

        <Table
          :columns="columns"
          :data-source="items"
          :loading="loading"
          :pagination="{
            current: pageIndex,
            pageSize,
            pageSizeOptions: ['10', '20', '50', '100'],
            showSizeChanger: true,
            showTotal: (total: number) => `共 ${total} 条`,
            total: totalCount,
          }"
          row-key="id"
          :scroll="{ x: 1420 }"
          size="middle"
          @change="handleTableChange"
        >
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'summary'">
              <div class="ai-writebacks__summary-cell">
                <strong>{{ record.summary }}</strong>
                <span>{{ record.actionCode }}</span>
              </div>
            </template>
            <template v-else-if="column.key === 'businessModule'">
              <div class="ai-writebacks__stack-cell">
                <Tag>{{ moduleLabel(record.businessModule) }}</Tag>
                <span>{{ record.actionCode }}</span>
              </div>
            </template>
            <template v-else-if="column.key === 'targetEntityType'">
              <div class="ai-writebacks__stack-cell">
                <strong>{{ record.targetEntityType }}</strong>
                <span>{{ record.targetEntityId || '-' }}</span>
              </div>
            </template>
            <template v-else-if="column.key === 'status'">
              <Tag :color="statusColor(record.status)">{{ statusLabel(record.status) }}</Tag>
            </template>
            <template v-else-if="column.key === 'creationTime'">
              {{ formatDate(record.creationTime) }}
            </template>
            <template v-else-if="column.key === 'approvedTime'">
              {{ formatDate(record.approvedTime) }}
            </template>
            <template v-else-if="column.key === 'action'">
              <Space>
                <Button size="small" type="link" @click="openDrawer(record)">查看</Button>
                <Popconfirm
                  v-if="canApprove && record.status === 'Pending'"
                  title="确认该 AI 写回申请？"
                  @confirm="approveRequest(record)"
                >
                  <Button :loading="actionLoadingKey === `approve:${record.id}`" size="small" type="link">确认</Button>
                </Popconfirm>
                <Button
                  v-if="canApprove && record.status === 'Pending'"
                  danger
                  :loading="actionLoadingKey === `reject:${record.id}`"
                  size="small"
                  type="link"
                  @click="openReject(record)"
                >
                  驳回
                </Button>
                <Popconfirm
                  v-if="canApprove && record.status === 'Pending'"
                  title="取消该 AI 写回申请？"
                  @confirm="cancelRequest(record)"
                >
                  <Button :loading="actionLoadingKey === `cancel:${record.id}`" size="small" type="link">取消</Button>
                </Popconfirm>
              </Space>
            </template>
          </template>
        </Table>
      </Card>
    </div>

    <Drawer v-model:open="drawerVisible" title="写回申请详情" width="820px">
      <div v-if="activeRequest" class="ai-writebacks__drawer">
        <Space>
          <Tag>{{ moduleLabel(activeRequest.businessModule) }}</Tag>
          <Tag :color="statusColor(activeRequest.status)">{{ statusLabel(activeRequest.status) }}</Tag>
          <Tag>{{ activeRequest.sourceType || 'Ai' }}</Tag>
        </Space>

        <section>
          <h3>摘要</h3>
          <p>{{ activeRequest.summary }}</p>
        </section>

        <section class="ai-writebacks__detail-grid">
          <div>
            <span>动作编码</span>
            <strong>{{ activeRequest.actionCode }}</strong>
          </div>
          <div>
            <span>目标类型</span>
            <strong>{{ activeRequest.targetEntityType }}</strong>
          </div>
          <div>
            <span>目标 ID</span>
            <strong>{{ activeRequest.targetEntityId || '-' }}</strong>
          </div>
          <div>
            <span>项目 ID</span>
            <strong>{{ activeRequest.projectId || '-' }}</strong>
          </div>
          <div>
            <span>会话 ID</span>
            <strong>{{ activeRequest.conversationId || '-' }}</strong>
          </div>
          <div>
            <span>处理时间</span>
            <strong>{{ formatDate(activeRequest.approvedTime) }}</strong>
          </div>
        </section>

        <section v-if="activeRequest.rejectReason">
          <h3>驳回原因</h3>
          <pre>{{ activeRequest.rejectReason }}</pre>
        </section>

        <section v-if="activeRequest.errorMessage">
          <h3>错误信息</h3>
          <pre>{{ activeRequest.errorMessage }}</pre>
        </section>

        <section>
          <h3>差异预览</h3>
          <pre>{{ formatJson(activeRequest.diffPreviewJson) }}</pre>
        </section>

        <section>
          <h3>写回载荷</h3>
          <pre>{{ formatJson(activeRequest.payloadJson) }}</pre>
        </section>
      </div>
    </Drawer>

    <Drawer
      v-model:open="rejectDrawerVisible"
      title="驳回写回申请"
      width="520px"
    >
      <Input.TextArea v-model:value="rejectReason" :auto-size="{ minRows: 4, maxRows: 8 }" placeholder="填写驳回原因" />
      <template #footer>
        <Space>
          <Button @click="rejectDrawerVisible = false">取消</Button>
          <Button
            :loading="activeRequest ? actionLoadingKey === `reject:${activeRequest.id}` : false"
            danger
            type="primary"
            @click="submitReject"
          >
            驳回
          </Button>
        </Space>
      </template>
    </Drawer>
  </Page>
</template>

<style scoped>
@reference "@vben/tailwind-config/theme";
.ai-writebacks {
  display: flex;
  flex-direction: column;
  @apply gap-4;
}

.ai-writebacks__metrics {
  display: grid;
  @apply gap-3;
  grid-template-columns: repeat(4, minmax(0, 1fr));
}

.ai-writebacks__metric-body {
  display: flex;
  flex-direction: column;
  @apply gap-1.5;
}

.ai-writebacks__metric span,
.ai-writebacks__summary-cell span,
.ai-writebacks__stack-cell span,
.ai-writebacks__detail-grid span {
  color: var(--ant-color-text-secondary);
  font-size: 12px;
}

.ai-writebacks__metric strong {
  font-size: 24px;
  font-weight: 650;
  line-height: 1.2;
}

.ai-writebacks__filters {
  display: grid;
  @apply gap-3;
  grid-template-columns: minmax(260px, 1fr) 140px 150px 260px auto;
  @apply mb-4;
}

.ai-writebacks__summary-cell,
.ai-writebacks__stack-cell {
  display: flex;
  flex-direction: column;
  @apply gap-1;
}

.ai-writebacks__summary-cell strong,
.ai-writebacks__stack-cell strong {
  color: var(--ant-color-text);
  font-weight: 600;
}

.ai-writebacks__drawer {
  display: flex;
  flex-direction: column;
  @apply gap-4;
}

.ai-writebacks__drawer section {
  display: flex;
  flex-direction: column;
  @apply gap-2;
}

.ai-writebacks__drawer h3 {
  font-size: 14px;
  font-weight: 600;
  @apply m-0;
}

.ai-writebacks__drawer p {
  @apply m-0;
}

.ai-writebacks__drawer pre {
  background: var(--ant-color-fill-quaternary);
  border: 1px solid var(--ant-color-border-secondary);
  @apply rounded-md;
  font-size: 12px;
  line-height: 1.6;
  @apply m-0;
  max-height: 360px;
  overflow: auto;
  @apply p-3;
  white-space: pre-wrap;
  word-break: break-word;
}

.ai-writebacks__detail-grid {
  display: grid;
  @apply gap-2.5;
  grid-template-columns: repeat(2, minmax(0, 1fr));
}

.ai-writebacks__detail-grid div {
  background: var(--ant-color-fill-quaternary);
  border: 1px solid var(--ant-color-border-secondary);
  @apply rounded-md;
  display: flex;
  flex-direction: column;
  @apply gap-1;
  min-width: 0;
  @apply px-3 py-2.5;
}

.ai-writebacks__detail-grid strong {
  font-size: 13px;
  font-weight: 500;
  overflow-wrap: anywhere;
}

@media (max-width: 1100px) {
  .ai-writebacks__metrics,
  .ai-writebacks__filters,
  .ai-writebacks__detail-grid {
    grid-template-columns: 1fr;
  }
}
</style>
