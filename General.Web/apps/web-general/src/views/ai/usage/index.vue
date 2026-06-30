<script lang="ts" setup>
import type { AiApi } from '#/api/core';

import { computed, onMounted, reactive, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import { Page } from '@vben/common-ui';

import { Alert, Button, Card, DatePicker, Drawer, Input, Select, Space, Table, Tabs, Tag } from 'ant-design-vue';

import { getAiUsageDashboardApi, getAiUsageRecordsApi } from '#/api/core';

defineOptions({ name: 'AiUsagePage' });

const route = useRoute();
const router = useRouter();

const sourceOptions = [
  { label: '全部来源', value: '' },
  { label: '模型会话', value: 'ModelChat' },
  { label: '内置工具', value: 'BuiltinTool' },
  { label: 'MCP 工具', value: 'McpTool' },
];

const statusOptions = [
  { label: '全部状态', value: '' },
  { label: '成功', value: 'Success' },
  { label: '失败', value: 'Failed' },
  { label: '拦截', value: 'Blocked' },
  { label: '超时', value: 'Timeout' },
];

const columns = [
  { dataIndex: 'auditSummary', key: 'auditSummary', title: '审计摘要', width: 300 },
  { dataIndex: 'source', key: 'source', title: '来源', width: 110 },
  { dataIndex: 'status', key: 'status', title: '状态', width: 100 },
  { dataIndex: 'modelName', key: 'modelName', title: '模型 / 工具', width: 180 },
  { dataIndex: 'totalTokens', key: 'totalTokens', title: 'Token', width: 100 },
  { dataIndex: 'costAmount', key: 'costAmount', title: '成本', width: 120 },
  { dataIndex: 'promptInjectionRiskScore', key: 'promptInjectionRiskScore', title: '注入风险', width: 110 },
  { dataIndex: 'creationTime', key: 'creationTime', title: '时间', width: 170 },
  { key: 'action', title: '操作', width: 90 },
];

const bucketColumns = [
  { dataIndex: 'label', key: 'label', title: '维度' },
  { dataIndex: 'count', key: 'count', title: '次数', width: 90 },
  { dataIndex: 'totalTokens', key: 'totalTokens', title: 'Token', width: 110 },
  { dataIndex: 'costAmount', key: 'costAmount', title: '成本', width: 110 },
];

const dashboard = ref<AiApi.UsageDashboard>();
const records = ref<AiApi.UsageRecordItem[]>([]);
const loadingDashboard = ref(false);
const loadingRecords = ref(false);
const drawerVisible = ref(false);
const activeRecord = ref<AiApi.UsageRecordItem>();

const query = reactive<AiApi.UsageQuery>({
  keyword: '',
  maxResultCount: 200,
  source: '',
  status: '',
});

const dateRange = ref<[string, string]>();
const activeBucketKey = ref('model');
const primaryMetrics = computed(() => [
  { hint: '全部 AI、工具与 MCP 调用', label: '调用次数', value: dashboard.value?.callCount ?? 0 },
  { hint: '输入与输出 Token 合计', label: '总 Token', value: dashboard.value?.totalTokens ?? 0 },
  { hint: '按模型价格口径估算', label: '成本', value: `${dashboard.value?.costAmount ?? 0} ${dashboard.value?.currency || 'USD'}` },
]);
const governanceMetrics = computed(() => [
  { label: '工具调用', value: dashboard.value?.toolCallCount ?? 0 },
  { label: '失败', value: dashboard.value?.failedCount ?? 0 },
  { label: '高风险输入', value: dashboard.value?.highRiskPromptCount ?? 0 },
]);
const bucketTabs = computed(() => [
  { data: dashboard.value?.modelBuckets || [], key: 'model', title: '模型' },
  { data: dashboard.value?.userBuckets || [], key: 'user', title: '用户' },
  { data: dashboard.value?.projectBuckets || [], key: 'project', title: '项目' },
  { data: dashboard.value?.scenarioBuckets || [], key: 'scenario', title: '场景' },
  { data: dashboard.value?.templateBuckets || [], key: 'template', title: '模板' },
]);

function normalizeQuery() {
  return {
    ...query,
    createdFrom: dateRange.value?.[0],
    createdTo: dateRange.value?.[1],
    keyword: query.keyword?.trim() || undefined,
    source: query.source || undefined,
    status: query.status || undefined,
  };
}

function readQueryString(name: string) {
  const value = route.query[name];
  return typeof value === 'string' ? value : '';
}

function hydrateQueryFromRoute() {
  query.conversationId = readQueryString('conversationId') || undefined;
  query.keyword = readQueryString('keyword');
  query.projectId = readQueryString('projectId') || undefined;
  query.source = readQueryString('source');
  query.status = readQueryString('status');
  query.toolCode = readQueryString('toolCode') || undefined;
}

function statusColor(value: string) {
  if (value === 'Success' || value === 'Completed') {
    return 'green';
  }

  if (value === 'Blocked') {
    return 'orange';
  }

  if (value === 'Failed' || value === 'Timeout') {
    return 'red';
  }

  return 'default';
}

function riskColor(score: number) {
  if (score >= 80) {
    return 'red';
  }

  if (score >= 60) {
    return 'orange';
  }

  if (score > 0) {
    return 'gold';
  }

  return 'green';
}

function formatDate(value?: string) {
  return value ? new Date(value).toLocaleString() : '-';
}

function formatCost(record: AiApi.UsageRecordItem | AiApi.UsageBucket | Record<string, any>) {
  return Number(record.costAmount || 0).toFixed(6);
}

function modelOrTool(record: AiApi.UsageRecordItem | Record<string, any>) {
  return record.modelName || record.modelCode || record.toolCode || '-';
}

async function loadDashboard() {
  loadingDashboard.value = true;
  try {
    dashboard.value = await getAiUsageDashboardApi(normalizeQuery());
  } finally {
    loadingDashboard.value = false;
  }
}

async function loadRecords() {
  loadingRecords.value = true;
  try {
    records.value = await getAiUsageRecordsApi(normalizeQuery());
  } finally {
    loadingRecords.value = false;
  }
}

async function refresh() {
  await Promise.all([loadDashboard(), loadRecords()]);
}

function openDrawer(record: AiApi.UsageRecordItem | Record<string, any>) {
  activeRecord.value = record as AiApi.UsageRecordItem;
  drawerVisible.value = true;
}

function openConversation(record: AiApi.UsageRecordItem) {
  if (!record.conversationId) {
    return;
  }

  router.push({ path: '/ai/conversations', query: { id: record.conversationId } });
}

onMounted(async () => {
  hydrateQueryFromRoute();
  await refresh();
});
</script>

<template>
  <Page description="查看 AI 模型、内置工具和 MCP 工具的用量、成本、限额预警与安全审计。" title="用量与成本">
    <div class="ai-usage">
      <div class="ai-usage__filters">
        <Input v-model:value="query.keyword" allow-clear placeholder="搜索摘要 / 输入 / 输出 / 错误" @press-enter="refresh" />
        <Select v-model:value="query.source" :options="sourceOptions" @change="refresh" />
        <Select v-model:value="query.status" :options="statusOptions" @change="refresh" />
        <DatePicker.RangePicker v-model:value="dateRange" value-format="YYYY-MM-DD" @change="refresh" />
        <Button type="primary" @click="refresh">刷新</Button>
      </div>

      <div v-if="dashboard?.warnings?.length" class="ai-usage__warnings">
        <Alert
          v-for="warning in dashboard.warnings"
          :key="warning"
          show-icon
          type="warning"
          :message="warning"
        />
      </div>

      <div class="ai-usage__overview">
        <Card :bordered="false" class="ai-usage__primary-card rounded-2xl" :loading="loadingDashboard" title="核心用量">
          <div class="ai-usage__primary-metrics">
            <section v-for="item in primaryMetrics" :key="item.label" class="ai-usage__primary-metric">
              <span>{{ item.label }}</span>
              <strong>{{ item.value }}</strong>
              <em>{{ item.hint }}</em>
            </section>
          </div>
        </Card>
        <Card :bordered="false" class="ai-usage__governance-card rounded-2xl" :loading="loadingDashboard" title="治理信号">
          <div class="ai-usage__governance-metrics">
            <section v-for="item in governanceMetrics" :key="item.label" class="ai-usage__governance-metric">
              <span>{{ item.label }}</span>
              <strong>{{ item.value }}</strong>
            </section>
          </div>
        </Card>
      </div>

      <Card :bordered="false" class="ai-usage__bucket-card rounded-2xl" title="维度拆分">
        <Tabs v-model:active-key="activeBucketKey" class="pt-2.5">
          <Tabs.TabPane v-for="item in bucketTabs" :key="item.key" :tab="item.title">
            <Table
              :columns="bucketColumns"
              :data-source="item.data"
              :pagination="false"
              row-key="key"
              size="small"
            >
              <template #bodyCell="{ column, record }">
                <template v-if="column.key === 'costAmount'">{{ formatCost(record) }}</template>
              </template>
            </Table>
          </Tabs.TabPane>
        </Tabs>
      </Card>

      <Card :bordered="false" class="rounded-2xl" title="审计记录">
        <Table
          :columns="columns"
          :data-source="records"
          :loading="loadingRecords"
          :pagination="{ pageSize: 20, showSizeChanger: true }"
          row-key="id"
          size="middle"
          :scroll="{ x: 1280 }"
        >
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'auditSummary'">
              <div class="ai-usage__summary-cell">
                <strong>{{ record.auditSummary }}</strong>
                <span>{{ record.scenario || record.promptTemplateCode || '-' }}</span>
              </div>
            </template>
            <template v-else-if="column.key === 'source'">
              <Tag>{{ record.source }}</Tag>
            </template>
            <template v-else-if="column.key === 'status'">
              <Tag :color="statusColor(record.status)">{{ record.status }}</Tag>
            </template>
            <template v-else-if="column.key === 'modelName'">
              {{ modelOrTool(record) }}
            </template>
            <template v-else-if="column.key === 'costAmount'">
              {{ formatCost(record) }} {{ record.currency }}
            </template>
            <template v-else-if="column.key === 'promptInjectionRiskScore'">
              <Tag :color="riskColor(record.promptInjectionRiskScore)">{{ record.promptInjectionRiskScore }}</Tag>
            </template>
            <template v-else-if="column.key === 'creationTime'">
              {{ formatDate(record.creationTime) }}
            </template>
            <template v-else-if="column.key === 'action'">
              <Button size="small" type="link" @click="openDrawer(record)">查看</Button>
            </template>
          </template>
        </Table>
      </Card>
    </div>

    <Drawer v-model:open="drawerVisible" title="AI 用量审计详情" width="760px">
      <div v-if="activeRecord" class="ai-usage__drawer">
        <Space>
          <Tag>{{ activeRecord.source }}</Tag>
          <Tag :color="statusColor(activeRecord.status)">{{ activeRecord.status }}</Tag>
          <Tag :color="riskColor(activeRecord.promptInjectionRiskScore)">风险 {{ activeRecord.promptInjectionRiskScore }}</Tag>
        </Space>
        <h3>摘要</h3>
        <p>{{ activeRecord.auditSummary }}</p>
        <div class="ai-usage__drawer-actions">
          <Button v-if="activeRecord.conversationId" type="primary" @click="openConversation(activeRecord)">查看会话</Button>
        </div>
        <h3>输入预览</h3>
        <pre>{{ activeRecord.requestPreview || '-' }}</pre>
        <h3>输出预览</h3>
        <pre>{{ activeRecord.responsePreview || '-' }}</pre>
        <h3>错误</h3>
        <pre>{{ activeRecord.errorMessage || '-' }}</pre>
        <h3>Prompt Injection 信号</h3>
        <pre>{{ activeRecord.promptInjectionSignalsJson || '[]' }}</pre>
      </div>
    </Drawer>
  </Page>
</template>

<style scoped>
@reference "@vben/tailwind-config/theme";
.ai-usage {
  display: flex;
  flex-direction: column;
  @apply gap-4;
}

.ai-usage__filters {
  display: grid;
  grid-template-columns: minmax(240px, 1fr) 150px 140px 260px auto;
  @apply gap-3;
}

.ai-usage__warnings {
  display: flex;
  flex-direction: column;
  @apply gap-2;
}

.ai-usage__overview {
  display: grid;
  grid-template-columns: minmax(0, 1fr) minmax(320px, 420px);
  @apply gap-3;
}

.ai-usage__primary-metrics,
.ai-usage__governance-metrics {
  display: grid;
  @apply gap-3;
  @apply min-h-[132px];
}

.ai-usage__primary-metrics {
  grid-template-columns: repeat(3, minmax(0, 1fr));
}

.ai-usage__governance-metrics {
  grid-template-columns: repeat(3, minmax(0, 1fr));
}

.ai-usage__primary-metric,
.ai-usage__governance-metric {
  display: flex;
  flex-direction: column;
  @apply gap-1.5;
  min-width: 0;
  @apply p-3;
  background: var(--ant-color-fill-quaternary);
  border: 1px solid var(--ant-color-border-secondary);
  @apply rounded-lg;
}

.ai-usage__primary-metric span,
.ai-usage__governance-metric span,
.ai-usage__summary-cell span {
  color: var(--ant-color-text-secondary);
  font-size: 12px;
}

.ai-usage__primary-metric strong,
.ai-usage__governance-metric strong {
  font-size: 24px;
  font-weight: 650;
  line-height: 1.2;
}

.ai-usage__primary-metric em {
  overflow: hidden;
  color: var(--ant-color-text-tertiary);
  font-size: 12px;
  font-style: normal;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.ai-usage__bucket-card :deep(.ant-tabs-nav) {
  @apply mb-3;
}

.ai-usage__summary-cell {
  display: flex;
  flex-direction: column;
  @apply gap-1;
}

.ai-usage__summary-cell strong {
  color: var(--ant-color-text);
  font-weight: 600;
}

.ai-usage__drawer {
  display: flex;
  flex-direction: column;
  @apply gap-3;
}

.ai-usage__drawer h3 {
  font-size: 14px;
  font-weight: 600;
  @apply m-0;
}

.ai-usage__drawer p {
  @apply m-0;
}

.ai-usage__drawer-actions {
  display: flex;
  justify-content: flex-start;
}

.ai-usage__drawer pre {
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

@media (max-width: 1280px) {
  .ai-usage__overview {
    grid-template-columns: 1fr;
  }

  .ai-usage__governance-metrics {
    grid-template-columns: repeat(3, minmax(0, 1fr));
  }
}

@media (max-width: 900px) {
  .ai-usage__filters,
  .ai-usage__primary-metrics,
  .ai-usage__governance-metrics {
    grid-template-columns: 1fr;
  }
}
</style>
