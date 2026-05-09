<script lang="ts" setup>
import { computed, onMounted, ref } from 'vue';

import { Page } from '@vben/common-ui';

import { Alert, Button, Card, Col, Empty, Input, Row, Statistic, Table, Tabs, Tag } from 'ant-design-vue';

import { getAuditLogDashboardApi, type AuditLogApi } from '#/api/core';

defineOptions({ name: 'PlatformAuditLogsPage' });

const keyword = ref('');
const loading = ref(false);
const activeTab = ref('access');
const logPagination = ref({
  current: 1,
  pageSize: 10,
});
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
  { dataIndex: 'executionTime', key: 'executionTime', title: '执行时间', width: 180 },
  { dataIndex: 'userName', key: 'userName', title: '用户', width: 120 },
  { dataIndex: 'tenantName', key: 'tenantName', title: '租户', width: 120 },
  { dataIndex: 'hostOperatorUserName', key: 'hostOperatorUserName', title: '运维来源', width: 140 },
  { dataIndex: 'actionSummary', key: 'actionSummary', title: '动作摘要', width: 240 },
  { dataIndex: 'url', key: 'url', title: '请求地址', width: 280 },
  { dataIndex: 'httpStatusCode', key: 'httpStatusCode', title: '状态', width: 100 },
  { dataIndex: 'executionDuration', key: 'executionDuration', title: '耗时', width: 100 },
];

const statColumns = [
  { dataIndex: 'label', key: 'label', title: '对象', ellipsis: true },
  { dataIndex: 'count', key: 'count', title: '次数', width: 88, align: 'right' as const },
];

const currentLogs = computed(() => {
  return {
    access: dashboard.value.accessLogs,
    audit: dashboard.value.auditLogs,
    exception: dashboard.value.exceptionLogs,
    operation: dashboard.value.operationLogs,
  }[activeTab.value] || [];
});

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
    dashboard.value = await getAuditLogDashboardApi({
      keyword: keyword.value || undefined,
      maxResultCount: 200,
    });
    logPagination.value.current = 1;
  } finally {
    loading.value = false;
  }
}

function handleTabChange() {
  logPagination.value.current = 1;
}

function handleLogTableChange(pagination: { current?: number; pageSize?: number }) {
  logPagination.value = {
    current: pagination.current || 1,
    pageSize: pagination.pageSize || logPagination.value.pageSize,
  };
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

      <Row :gutter="[16, 16]">
        <Col v-for="metric in metrics" :key="metric.label" :lg="4" :md="8" :span="24">
          <Card :bordered="false">
            <Statistic :title="metric.label" :value="metric.value" />
          </Card>
        </Col>
      </Row>

      <Card :bordered="false" title="日志查询">
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

        <div v-if="!loading && currentLogs.length === 0" class="platform-audit__empty">
          <Empty description="当前分类暂无日志" />
        </div>
        <Table
          v-else
          :columns="logColumns"
          :data-source="currentLogs"
          :loading="loading"
          :pagination="{
            current: logPagination.current,
            pageSize: logPagination.pageSize,
            showSizeChanger: true,
            showTotal: (total: number) => `共 ${total} 条`,
            total: currentLogs.length,
          }"
          row-key="id"
          :scroll="{ x: 1180 }"
          size="middle"
          @change="handleLogTableChange"
        >
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'executionTime'">
              {{ new Date((record as AuditLogApi.AuditLogItem).executionTime).toLocaleString() }}
            </template>
            <template v-else-if="column.key === 'httpStatusCode'">
              <Tag :color="resolveStatusColor(record as AuditLogApi.AuditLogItem)">
                {{ (record as AuditLogApi.AuditLogItem).httpStatusCode || ((record as AuditLogApi.AuditLogItem).hasException ? 500 : 200) }}
              </Tag>
            </template>
            <template v-else-if="column.key === 'hostOperatorUserName'">
              <Tag v-if="(record as AuditLogApi.AuditLogItem).isHostTenantOperation" color="blue">
                {{ (record as AuditLogApi.AuditLogItem).hostOperatorUserName || 'Host 运维' }}
              </Tag>
              <span v-else>-</span>
            </template>
            <template v-else-if="column.key === 'executionDuration'">
              {{ formatDuration((record as AuditLogApi.AuditLogItem).executionDuration) }}
            </template>
            <template v-else-if="column.key === 'actionSummary'">
              <div class="platform-audit__action">
                <strong>{{ (record as AuditLogApi.AuditLogItem).actionSummary || '-' }}</strong>
                <span v-if="(record as AuditLogApi.AuditLogItem).exceptionMessage">{{ (record as AuditLogApi.AuditLogItem).exceptionMessage }}</span>
              </div>
            </template>
          </template>
        </Table>
      </Card>

      <Row :gutter="[16, 16]">
        <Col :lg="8" :span="24">
          <Card :bordered="false" title="Top 菜单访问">
            <Table
              class="platform-audit__stat-table"
              :columns="statColumns"
              :data-source="dashboard.topPages.length ? dashboard.topPages : dashboard.topMenus"
              :pagination="false"
              row-key="key"
              size="small"
            >
              <template #emptyText>
                <span style="font-size: 12px; color: #999">暂无数据（菜单访问埋点将在用户浏览后自动收集）</span>
              </template>
            </Table>
          </Card>
        </Col>
        <Col :lg="8" :span="24">
          <Card :bordered="false" title="Top API">
            <Table
              class="platform-audit__stat-table"
              :columns="statColumns"
              :data-source="dashboard.topApis"
              :pagination="false"
              row-key="key"
              size="small"
            />
          </Card>
        </Col>
        <Col :lg="8" :span="24">
          <Card :bordered="false" title="Top 用户">
            <Table
              class="platform-audit__stat-table"
              :columns="statColumns"
              :data-source="dashboard.topUsers"
              :pagination="false"
              row-key="key"
              size="small"
            />
          </Card>
        </Col>
      </Row>
    </section>
  </Page>
</template>

<style scoped>
.platform-audit {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.platform-audit__toolbar {
  display: flex;
  gap: 12px;
}

.platform-audit__empty {
  display: grid;
  min-height: 240px;
  place-items: center;
}

.platform-audit__action {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.platform-audit__action span {
  color: var(--ant-color-text-secondary);
  font-size: 12px;
}

.platform-audit__stat-table :deep(table) {
  table-layout: fixed;
  width: 100%;
}

.platform-audit__stat-table :deep(.ant-table-cell-ellipsis) {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.platform-audit__stat-table :deep(.ant-table-tbody > tr > td:last-child) {
  white-space: nowrap;
}

@media (max-width: 960px) {
  .platform-audit__toolbar {
    display: grid;
    grid-template-columns: 1fr;
  }
}
</style>
