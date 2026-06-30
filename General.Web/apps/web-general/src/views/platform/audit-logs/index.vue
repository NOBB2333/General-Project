<script lang="ts" setup>
import type { NativeVxeGridEvents, VxeGridProps } from '#/adapter/vxe-table';

import { computed, onMounted, ref } from 'vue';

import { Page } from '@vben/common-ui';

import { Alert, Button, Card, Col, Empty, Input, Row, Statistic, Tabs, Tag } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { getAuditLogDashboardApi, getAuditLogPagedApi, type AuditLogApi } from '#/api/core';

defineOptions({ name: 'PlatformAuditLogsPage' });

const keyword = ref('');
const loading = ref(false);
const activeTab = ref('access');
const logPagination = ref({
  current: 1,
  pageSize: 10,
});
const logTotalCount = ref(0);
const logItems = ref<AuditLogApi.AuditLogItem[]>([]);
const dashboard = ref<AuditLogApi.LogDashboard>({
  accessLogs: [],
  auditLogs: [],
  exceptionLogs: [],
  operationLogs: [],
  topApis: [],
  topMenus: [],
  topPages: [],
  topUsers: [],
});

const logColumns = [
  { field: 'executionTime', slots: { default: 'executionTime' }, title: '执行时间', width: 180 },
  { field: 'userName', title: '用户', width: 120 },
  { field: 'tenantName', title: '租户', width: 120 },
  { field: 'hostOperatorUserName', slots: { default: 'hostOperatorUserName' }, title: '运维来源', width: 140 },
  { align: 'left', field: 'actionSummary', slots: { default: 'actionSummary' }, title: '动作摘要', width: 240 },
  { align: 'left', field: 'url', title: '请求地址', width: 280 },
  { field: 'httpStatusCode', slots: { default: 'httpStatusCode' }, title: '状态', width: 100 },
  { field: 'executionDuration', slots: { default: 'executionDuration' }, title: '耗时', width: 100 },
] satisfies VxeGridProps<AuditLogApi.AuditLogItem>['columns'];

const statColumns = [
  { align: 'left', field: 'label', showOverflow: 'ellipsis', title: '对象' },
  { align: 'right', field: 'count', title: '次数', width: 88 },
] satisfies VxeGridProps<AuditLogApi.LogStatItem>['columns'];

const logGridOptions = computed<VxeGridProps<AuditLogApi.AuditLogItem>>(() => ({
  columns: logColumns,
  data: logItems.value,
  loading: loading.value,
  pagerConfig: {
    currentPage: logPagination.value.current,
    pageSize: logPagination.value.pageSize,
    pageSizes: [10, 20, 50, 100],
    total: logTotalCount.value,
  },
  cellConfig: { height: 48 },
  rowConfig: { keyField: 'id' },
  scrollX: { enabled: true },
  size: 'medium',
}));

const topMenuRows = computed(() =>
  dashboard.value.topPages.length ? dashboard.value.topPages : dashboard.value.topMenus,
);

function createStatGridOptions(
  data: AuditLogApi.LogStatItem[],
): VxeGridProps<AuditLogApi.LogStatItem> {
  return {
    columns: statColumns,
    data,
    emptyText: '暂无数据（菜单访问埋点将在用户浏览后自动收集）',
    pagerConfig: { enabled: false },
    cellConfig: { height: 40 },
    rowConfig: { keyField: 'key' },
    size: 'small',
  };
}

const topMenuGridOptions = computed(() => createStatGridOptions(topMenuRows.value));
const topApiGridOptions = computed(() => createStatGridOptions(dashboard.value.topApis));
const topUserGridOptions = computed(() => createStatGridOptions(dashboard.value.topUsers));

const logGridEvents: NativeVxeGridEvents<AuditLogApi.AuditLogItem> = {
  'page-change': ({ currentPage, pageSize }) => {
    const normalizedPageSize = pageSize || logPagination.value.pageSize;
    logPagination.value = {
      current: normalizedPageSize === logPagination.value.pageSize ? currentPage || 1 : 1,
      pageSize: normalizedPageSize,
    };
    void loadLogs();
  },
};

const [LogGrid] = useVbenVxeGrid<AuditLogApi.AuditLogItem>({
  gridEvents: logGridEvents,
  gridOptions: {},
});
const [TopMenuGrid] = useVbenVxeGrid<AuditLogApi.LogStatItem>({ gridOptions: {} });
const [TopApiGrid] = useVbenVxeGrid<AuditLogApi.LogStatItem>({ gridOptions: {} });
const [TopUserGrid] = useVbenVxeGrid<AuditLogApi.LogStatItem>({ gridOptions: {} });

const metrics = computed(() => {
  const allLogs = [
    ...dashboard.value.accessLogs,
    ...dashboard.value.auditLogs,
    ...dashboard.value.exceptionLogs,
    ...dashboard.value.operationLogs,
  ];

  return [
    { label: '访问日志', value: dashboard.value.accessLogs.length },
    { label: '操作日志', value: dashboard.value.operationLogs.length },
    { label: '异常日志', value: dashboard.value.exceptionLogs.length },
    { label: '审计日志', value: dashboard.value.auditLogs.length },
    {
      label: '平均耗时(ms)',
      value:
        allLogs.length === 0
          ? 0
          : Math.round(
              allLogs.reduce((total, item) => total + item.executionDuration, 0) / allLogs.length,
            ),
    },
  ];
});

async function loadLogs() {
  loading.value = true;
  try {
    const [dashboardData, pagedData] = await Promise.all([
      getAuditLogDashboardApi({
        keyword: keyword.value || undefined,
        maxResultCount: 200,
      }),
      getAuditLogPagedApi({
        category: activeTab.value,
        keyword: keyword.value || undefined,
        maxResultCount: logPagination.value.pageSize,
        skipCount: (logPagination.value.current - 1) * logPagination.value.pageSize,
      }),
    ]);
    dashboard.value = dashboardData;
    logItems.value = pagedData.items;
    logTotalCount.value = pagedData.totalCount;
  } finally {
    loading.value = false;
  }
}

function handleTabChange() {
  logPagination.value.current = 1;
  void loadLogs();
}

function resolveStatusColor(record: AuditLogApi.AuditLogItem) {
  if (record.hasException || (record.httpStatusCode ?? 500) >= 500) {
    return 'error';
  }
  if ((record.httpStatusCode ?? 200) >= 400) {
    return 'warning';
  }
  return 'success';
}

function formatDuration(value: number) {
  return `${value} ms`;
}

onMounted(loadLogs);
</script>

<template>
  <Page description="日志中心聚合访问、操作、异常与审计留痕，并补充接口与菜单访问分析。" title="日志中心">
    <section class="platform-audit">
      <Alert
        banner
        message="当前环境优先使用轻量请求留痕，覆盖 GET/POST/PUT/DELETE 的平台接口访问；若无轻量日志，再回退读取历史数据库审计数据。"
        type="info"
      />

      <div class="platform-audit__metrics">
        <Card v-for="metric in metrics" :key="metric.label" :bordered="false" class="rounded-2xl" size="small">
          <Statistic :title="metric.label" :value="metric.value" />
        </Card>
      </div>

      <Card :bordered="false" class="platform-audit__query-card rounded-2xl" title="日志查询">
        <template #extra>
          <div class="platform-audit__toolbar">
            <Input
              v-model:value="keyword"
              allow-clear
              placeholder="按用户、租户、接口、动作筛选"
              @pressEnter="loadLogs"
            />
            <Button type="primary" @click="loadLogs">刷新</Button>
          </div>
        </template>

        <Tabs v-model:activeKey="activeTab" @change="handleTabChange">
          <Tabs.TabPane key="access" tab="访问日志" />
          <Tabs.TabPane key="operation" tab="操作日志" />
          <Tabs.TabPane key="exception" tab="异常日志" />
          <Tabs.TabPane key="audit" tab="审计日志" />
        </Tabs>

        <div v-if="!loading && logItems.length === 0" class="platform-audit__empty">
          <Empty description="当前分类暂无日志" />
        </div>
        <LogGrid
          v-else
          class="app-vxe-card-grid"
          :grid-events="logGridEvents"
          :grid-options="logGridOptions"
        >
          <template #executionTime="{ row }">
            {{ new Date(row.executionTime).toLocaleString() }}
          </template>
          <template #httpStatusCode="{ row }">
            <Tag :color="resolveStatusColor(row)">
              {{ row.httpStatusCode || (row.hasException ? 500 : 200) }}
            </Tag>
          </template>
          <template #hostOperatorUserName="{ row }">
            <Tag v-if="row.isHostTenantOperation" color="blue">
              {{ row.hostOperatorUserName || 'Host 运维' }}
            </Tag>
            <span v-else>-</span>
          </template>
          <template #executionDuration="{ row }">
            {{ formatDuration(row.executionDuration) }}
          </template>
          <template #actionSummary="{ row }">
            <div class="platform-audit__action">
              <strong>{{ row.actionSummary || '-' }}</strong>
              <span v-if="row.exceptionMessage">{{ row.exceptionMessage }}</span>
            </div>
          </template>
        </LogGrid>
      </Card>

      <Row :gutter="[16, 16]">
        <Col :lg="8" :span="24">
          <Card :bordered="false" class="rounded-2xl" title="Top 菜单访问">
            <TopMenuGrid
              class="app-vxe-card-grid platform-audit__stat-table"
              :grid-options="topMenuGridOptions"
            />
          </Card>
        </Col>
        <Col :lg="8" :span="24">
          <Card :bordered="false" class="rounded-2xl" title="Top API">
            <TopApiGrid
              class="app-vxe-card-grid platform-audit__stat-table"
              :grid-options="topApiGridOptions"
            />
          </Card>
        </Col>
        <Col :lg="8" :span="24">
          <Card :bordered="false" class="rounded-2xl" title="Top 用户">
            <TopUserGrid
              class="app-vxe-card-grid platform-audit__stat-table"
              :grid-options="topUserGridOptions"
            />
          </Card>
        </Col>
      </Row>
    </section>
  </Page>
</template>

<style scoped>
@reference "@vben/tailwind-config/theme";
.platform-audit {
  display: flex;
  flex-direction: column;
  @apply gap-4;
}

.platform-audit__metrics {
  display: grid;
  @apply gap-4;
  grid-template-columns: repeat(5, minmax(0, 1fr));
}

.platform-audit__metrics :deep(.ant-statistic-title) {
  @apply mb-1;
}

.platform-audit__metrics :deep(.ant-statistic-content) {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.platform-audit__toolbar {
  display: flex;
  @apply gap-3;
}

.platform-audit__empty {
  display: grid;
  min-height: 240px;
  place-items: center;
}

.platform-audit__action {
  display: flex;
  flex-direction: column;
  @apply gap-1;
}

.platform-audit__action span {
  color: var(--ant-color-text-secondary);
  font-size: 12px;
}

@media (max-width: 960px) {
  .platform-audit__toolbar {
    display: grid;
    grid-template-columns: 1fr;
  }

  .platform-audit__metrics {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}
</style>
