<script lang="ts" setup>
import type { VxeGridProps } from '#/adapter/vxe-table';

import { computed, onMounted, ref } from 'vue';

import { Page } from '@vben/common-ui';

import { Button, Card, Col, DatePicker, Empty, Row } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
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
  { align: 'left', field: 'label', title: '对象' },
  { field: 'count', title: '次数', width: 100 },
] satisfies VxeGridProps<AuditLogApi.LogStatItem>['columns'];

function createStatGridOptions(
  data: AuditLogApi.LogStatItem[],
): VxeGridProps<AuditLogApi.LogStatItem> {
  return {
    columns: statColumns,
    data,
    loading: loading.value,
    pagerConfig: { enabled: false },
    rowConfig: { keyField: 'key' },
    size: 'small',
  };
}

const apiStatGridOptions = computed(() => createStatGridOptions(dashboard.value.topApis));
const userStatGridOptions = computed(() => createStatGridOptions(dashboard.value.topUsers));
const menuStatGridOptions = computed(() => createStatGridOptions(dashboard.value.topMenus));

const [ApiStatGrid] = useVbenVxeGrid<AuditLogApi.LogStatItem>({ gridOptions: {} });
const [UserStatGrid] = useVbenVxeGrid<AuditLogApi.LogStatItem>({ gridOptions: {} });
const [MenuStatGrid] = useVbenVxeGrid<AuditLogApi.LogStatItem>({ gridOptions: {} });

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
      <Card class="rounded-2xl" :bordered="false" title="查询条件">
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
          <Card class="rounded-2xl" :bordered="false" title="接口统计">
            <Empty v-if="dashboard.topApis.length === 0" description="暂无统计数据" />
            <ApiStatGrid
              v-else
              class="app-vxe-card-grid"
              :grid-options="apiStatGridOptions"
            />
          </Card>
        </Col>
        <Col :lg="8" :span="24">
          <Card class="rounded-2xl" :bordered="false" title="用户统计">
            <Empty v-if="dashboard.topUsers.length === 0" description="暂无统计数据" />
            <UserStatGrid
              v-else
              class="app-vxe-card-grid"
              :grid-options="userStatGridOptions"
            />
          </Card>
        </Col>
        <Col :lg="8" :span="24">
          <Card class="rounded-2xl" :bordered="false" title="菜单动作统计">
            <Empty v-if="dashboard.topMenus.length === 0" description="暂无统计数据" />
            <MenuStatGrid
              v-else
              class="app-vxe-card-grid"
              :grid-options="menuStatGridOptions"
            />
          </Card>
        </Col>
      </Row>
    </section>
  </Page>
</template>

<style scoped>
@reference "@vben/tailwind-config/theme";
.platform-stats {
  display: flex;
  flex-direction: column;
  @apply gap-4;
}

.platform-stats__toolbar {
  display: flex;
  @apply gap-3;
}

@media (max-width: 960px) {
  .platform-stats__toolbar {
    display: grid;
    grid-template-columns: 1fr;
  }
}
</style>
