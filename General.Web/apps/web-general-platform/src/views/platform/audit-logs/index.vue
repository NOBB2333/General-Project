<script lang="ts" setup>
import { computed, onMounted, ref } from 'vue';

import { Page } from '@vben/common-ui';

import { Button, Card, Col, Empty, Input, Row, Statistic, Table, Tag } from 'ant-design-vue';

import { getAuditLogListApi, type AuditLogApi } from '#/api/core';

defineOptions({ name: 'PlatformAuditLogsPage' });

const keyword = ref('');
const loading = ref(false);
const items = ref<AuditLogApi.AuditLogItem[]>([]);

const columns = [
  { dataIndex: 'executionTime', key: 'executionTime', title: '执行时间', width: 180 },
  { dataIndex: 'userName', key: 'userName', title: '用户', width: 120 },
  { dataIndex: 'tenantName', key: 'tenantName', title: '租户', width: 120 },
  { dataIndex: 'actionSummary', key: 'actionSummary', title: '动作摘要', width: 240 },
  { dataIndex: 'url', key: 'url', title: '请求地址' },
  { dataIndex: 'httpStatusCode', key: 'httpStatusCode', title: '状态', width: 110 },
  { dataIndex: 'executionDuration', key: 'executionDuration', title: '耗时', width: 110 },
];

const metrics = computed(() => [
  { label: '日志条数', value: items.value.length },
  {
    label: '异常请求',
    value: items.value.filter((item) => item.hasException).length,
  },
  {
    label: '成功请求',
    value: items.value.filter((item) => !item.hasException).length,
  },
  {
    label: '平均耗时(ms)',
    value:
      items.value.length === 0
        ? 0
        : Math.round(
            items.value.reduce((total, item) => total + item.executionDuration, 0) /
              items.value.length,
          ),
  },
]);

async function loadLogs() {
  loading.value = true;
  try {
    items.value = await getAuditLogListApi({
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
  <Page description="审计日志按最近 200 条记录展示平台请求留痕，支持按关键字筛选。" title="审计日志">
    <section class="platform-audit">
      <Row :gutter="[16, 16]">
        <Col v-for="metric in metrics" :key="metric.label" :lg="6" :md="12" :span="24">
          <Card :bordered="false">
            <Statistic :title="metric.label" :value="metric.value" />
          </Card>
        </Col>
      </Row>

      <Card :bordered="false" title="请求留痕">
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

        <div v-if="!loading && items.length === 0" class="platform-audit__empty">
          <Empty description="暂无审计日志" />
        </div>
        <Table
          v-else
          :columns="columns"
          :data-source="items"
          :loading="loading"
          :pagination="{ pageSize: 10 }"
          row-key="id"
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
