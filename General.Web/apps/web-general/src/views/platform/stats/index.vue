<script lang="ts" setup>
import { onMounted, ref } from 'vue';

import { Page } from '@vben/common-ui';

import { Button, Card, Col, DatePicker, Empty, Row, Table } from 'ant-design-vue';

import { getAuditLogDashboardApi, type AuditLogApi } from '#/api/core';

defineOptions({ name: 'PlatformStatsPage' });

const loading = ref(false);
const rangeValue = ref<[string, string] | null>(null);
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

const statColumns = [
  { dataIndex: 'label', key: 'label', title: '对象' },
  { dataIndex: 'count', key: 'count', title: '次数', width: 100 },
];

async function loadStats() {
  loading.value = true;
  try {
    dashboard.value = await getAuditLogDashboardApi({
      endTime: rangeValue.value?.[1],
      maxResultCount: 300,
      startTime: rangeValue.value?.[0],
    });
  } finally {
    loading.value = false;
  }
}

onMounted(loadStats);
</script>

<template>
  <Page description="访问统计按接口、用户、菜单聚合当前平台访问情况。" title="访问统计">
    <section class="platform-stats">
      <Card :bordered="false" title="查询条件">
        <div class="platform-stats__toolbar">
          <DatePicker.RangePicker
            show-time
            @change="(_, dateStrings) => { rangeValue = dateStrings as [string, string]; }"
          />
          <Button type="primary" @click="loadStats">刷新统计</Button>
        </div>
      </Card>

      <Row :gutter="[16, 16]">
        <Col :lg="8" :span="24">
          <Card :bordered="false" title="接口统计">
            <Empty v-if="dashboard.topApis.length === 0" description="暂无统计数据" />
            <Table
              v-else
              :columns="statColumns"
              :data-source="dashboard.topApis"
              :loading="loading"
              :pagination="false"
              row-key="key"
              size="small"
            />
          </Card>
        </Col>
        <Col :lg="8" :span="24">
          <Card :bordered="false" title="用户统计">
            <Empty v-if="dashboard.topUsers.length === 0" description="暂无统计数据" />
            <Table
              v-else
              :columns="statColumns"
              :data-source="dashboard.topUsers"
              :loading="loading"
              :pagination="false"
              row-key="key"
              size="small"
            />
          </Card>
        </Col>
        <Col :lg="8" :span="24">
          <Card :bordered="false" title="菜单动作统计">
            <Empty v-if="dashboard.topMenus.length === 0" description="暂无统计数据" />
            <Table
              v-else
              :columns="statColumns"
              :data-source="dashboard.topMenus"
              :loading="loading"
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
.platform-stats {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.platform-stats__toolbar {
  display: flex;
  gap: 12px;
}

@media (max-width: 960px) {
  .platform-stats__toolbar {
    display: grid;
    grid-template-columns: 1fr;
  }
}
</style>
