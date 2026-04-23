<script lang="ts" setup>
import { computed, onMounted, ref } from 'vue';

import { Page } from '@vben/common-ui';

import { Alert, Button, Card, Col, Empty, Input, Row, Statistic, Table, Tabs, Tag } from 'ant-design-vue';

import { getAuditLogDashboardApi, type AuditLogApi } from '#/api/core';

defineOptions({ name: 'PlatformAuditLogsPage' });

const keyword = ref('');
const loading = ref(false);
const activeTab = ref('access');
const dashboard = ref<AuditLogApi.LogDashboard>({
  accessLogs: [],
  auditLogs: [],
  exceptionLogs: [],
  operationLogs: [],
  topApis: [],
  topMenus: [],
  topUsers: [],
});

const logColumns = [
  { dataIndex: 'executionTime', key: 'executionTime', title: '执行时间', width: 180 },
  { dataIndex: 'userName', key: 'userName', title: '用户', width: 120 },
  { dataIndex: 'tenantName', key: 'tenantName', title: '租户', width: 120 },
  { dataIndex: 'actionSummary', key: 'actionSummary', title: '动作摘要', width: 240 },
  { dataIndex: 'url', key: 'url', title: '请求地址', width: 280 },
  { dataIndex: 'httpStatusCode', key: 'httpStatusCode', title: '状态', width: 100 },
  { dataIndex: 'executionDuration', key: 'executionDuration', title: '耗时', width: 100 },
];

const statColumns = [
  { dataIndex: 'label', key: 'label', title: '对象' },
  { dataIndex: 'count', key: 'count', title: '次数', width: 100 },
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
  } finally {
    loading.value = false;
  }
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
  <Page description="日志管理聚合访问、操作、异常与审计留痕，并补充接口/用户访问统计。" title="日志管理">
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

        <Tabs v-model:activeKey="activeTab">
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
          :pagination="{ pageSize: 10 }"
          row-key="id"
          :scroll="{ x: 1180 }"
          size="middle"
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
          <Card :bordered="false" title="Top API">
            <Table
              :columns="statColumns"
              :data-source="dashboard.topApis"
              :pagination="false"
              row-key="key"
              size="small"
            />
          </Card>
        </Col>
        <Col :lg="8" :span="24">
          <Card :bordered="false" title="Top 菜单动作">
            <Table
              :columns="statColumns"
              :data-source="dashboard.topMenus"
              :pagination="false"
              row-key="key"
              size="small"
            />
          </Card>
        </Col>
        <Col :lg="8" :span="24">
          <Card :bordered="false" title="Top 用户">
            <Table
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

@media (max-width: 960px) {
  .platform-audit__toolbar {
    display: grid;
    grid-template-columns: 1fr;
  }
}
</style>
