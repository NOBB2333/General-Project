<script lang="ts" setup>
import { computed, onMounted, ref } from 'vue';

import { Page } from '@vben/common-ui';

import { Button, Card, Col, Empty, Input, Row, Statistic, Table, Tabs, Tag } from 'ant-design-vue';

import { getAuditLogDashboardApi, type AuditLogApi } from '#/api/core';

defineOptions({ name: 'PlatformAuditLogsPage' });

const activeKey = ref('accessLogs');
const keyword = ref('');
const loading = ref(false);
const dashboard = ref<AuditLogApi.LogDashboard | null>(null);

const columns = [
  { dataIndex: 'executionTime', key: 'executionTime', title: '执行时间', width: 180 },
  { dataIndex: 'userName', key: 'userName', title: '用户', width: 120 },
  { dataIndex: 'tenantName', key: 'tenantName', title: '租户', width: 120 },
  { dataIndex: 'actionSummary', key: 'actionSummary', title: '动作摘要', width: 240 },
  { dataIndex: 'url', key: 'url', title: '请求地址' },
  { dataIndex: 'httpStatusCode', key: 'httpStatusCode', title: '状态', width: 110 },
  { dataIndex: 'executionDuration', key: 'executionDuration', title: '耗时', width: 110 },
];

const currentItems = computed(() => dashboard.value?.[activeKey.value as keyof AuditLogApi.LogDashboard] || []);

const metrics = computed(() => [
  { label: '访问日志', value: dashboard.value?.accessLogs.length || 0 },
  { label: '操作日志', value: dashboard.value?.operationLogs.length || 0 },
  { label: '异常日志', value: dashboard.value?.exceptionLogs.length || 0 },
  { label: '审计日志', value: dashboard.value?.auditLogs.length || 0 },
]);

async function loadDashboard() {
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

onMounted(loadDashboard);
</script>

<template>
  <Page description="日志中心汇总访问、操作、异常与审计日志，并提供接口与菜单访问分析。" title="日志中心">
    <section class="platform-audit">
      <Row :gutter="[16, 16]">
        <Col v-for="metric in metrics" :key="metric.label" :lg="6" :md="12" :span="24">
          <Card :bordered="false">
            <Statistic :title="metric.label" :value="metric.value" />
          </Card>
        </Col>
      </Row>

      <Row :gutter="[16, 16]">
        <Col :lg="8" :span="24">
          <Card :bordered="false" title="Top 菜单访问">
            <div
              v-if="(dashboard?.topPages?.length || 0) === 0 && (dashboard?.topMenus?.length || 0) === 0"
              style="font-size: 12px; color: #999; padding: 8px 0"
            >
              暂无数据（菜单访问埋点将在用户浏览后自动收集）
            </div>
            <div
              v-for="item in (dashboard?.topPages?.length ? dashboard.topPages : dashboard?.topMenus) || []"
              :key="item.key"
              class="platform-audit__rank"
            >
              <span>{{ item.label }}</span>
              <strong>{{ item.count }}</strong>
            </div>
          </Card>
        </Col>
        <Col :lg="8" :span="24">
          <Card :bordered="false" title="Top API">
            <div v-for="item in dashboard?.topApis || []" :key="item.key" class="platform-audit__rank">
              <span>{{ item.label }}</span>
              <strong>{{ item.count }}</strong>
            </div>
          </Card>
        </Col>
        <Col :lg="8" :span="24">
          <Card :bordered="false" title="Top 用户">
            <div v-for="item in dashboard?.topUsers || []" :key="item.key" class="platform-audit__rank">
              <span>{{ item.label }}</span>
              <strong>{{ item.count }}</strong>
            </div>
          </Card>
        </Col>
      </Row>

      <Card :bordered="false" title="日志明细">
        <template #extra>
          <div class="platform-audit__toolbar">
            <Input
              v-model:value="keyword"
              allow-clear
              placeholder="按用户、租户、接口、动作筛选"
              @pressEnter="loadDashboard"
            />
            <Button type="primary" @click="loadDashboard">刷新</Button>
          </div>
        </template>

        <Tabs v-model:activeKey="activeKey">
          <Tabs.TabPane key="accessLogs" tab="访问日志" />
          <Tabs.TabPane key="operationLogs" tab="操作日志" />
          <Tabs.TabPane key="exceptionLogs" tab="异常日志" />
          <Tabs.TabPane key="auditLogs" tab="审计日志" />
        </Tabs>

        <div v-if="!loading && currentItems.length === 0" class="platform-audit__empty">
          <Empty description="暂无日志" />
        </div>
        <Table
          v-else
          :columns="columns"
          :data-source="currentItems"
          :loading="loading"
          :pagination="{ pageSize: 10 }"
          row-key="id"
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
              {{ (record as AuditLogApi.AuditLogItem).executionDuration }} ms
            </template>
          </template>
        </Table>
      </Card>
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

.platform-audit__rank {
  display: flex;
  justify-content: space-between;
  gap: 12px;
  padding: 10px 0;
  border-bottom: 1px solid var(--ant-color-border-secondary);
}

.platform-audit__rank span {
  flex: 1;
  min-width: 0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.platform-audit__rank strong {
  flex: none;
  white-space: nowrap;
}
</style>
