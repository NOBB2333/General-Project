<script lang="ts" setup>
import { computed, onMounted, ref } from 'vue';

import { Page } from '@vben/common-ui';

import { Card, Col, Empty, Row, Statistic, Table, Tag } from 'ant-design-vue';

import { getProjectListApi, type ProjectApi } from '#/api/core';

defineOptions({ name: 'ProjectPmoOverviewPage' });

const loading = ref(false);
const items = ref<ProjectApi.ProjectListItem[]>([]);

const columns = [
  { dataIndex: 'projectCode', key: 'projectCode', title: '项目编号', width: 140 },
  { dataIndex: 'name', key: 'name', title: '项目名称' },
  { dataIndex: 'managerName', key: 'managerName', title: '项目经理', width: 120 },
  { dataIndex: 'status', key: 'status', title: '状态', width: 120 },
  { dataIndex: 'overdueMilestoneCount', key: 'overdueMilestoneCount', title: '逾期里程碑', width: 120 },
  { dataIndex: 'highRiskCount', key: 'highRiskCount', title: '高风险', width: 100 },
];

const metrics = computed(() => [
  { label: '项目总数', value: items.value.length },
  { label: '重点项目', value: items.value.filter((item) => item.isKeyProject).length },
  {
    label: '异常项目',
    value: items.value.filter((item) => item.overdueTaskCount > 0 || item.highRiskCount > 0).length,
  },
  {
    label: '逾期里程碑项目',
    value: items.value.filter((item) => item.overdueMilestoneCount > 0).length,
  },
]);

async function loadOverview() {
  loading.value = true;
  try {
    items.value = await getProjectListApi();
  } finally {
    loading.value = false;
  }
}

onMounted(loadOverview);
</script>

<template>
  <Page description="PMO 执行总览按统一口径汇总重点项目、异常项目和里程碑压力。" title="PMO 执行总览">
    <section class="pmo-overview">
      <Row :gutter="[16, 16]">
        <Col v-for="metric in metrics" :key="metric.label" :lg="6" :md="12" :span="24">
          <Card :bordered="false"><Statistic :title="metric.label" :value="metric.value" /></Card>
        </Col>
      </Row>

      <Card id="pmo-overview-exceptions" :bordered="false" title="异常项目视图">
        <div v-if="!loading && items.length === 0" class="pmo-overview__empty">
          <Empty description="暂无项目数据" />
        </div>
        <Table
          v-else
          :columns="columns"
          :data-source="items"
          :loading="loading"
          :pagination="{ pageSize: 10 }"
          row-key="id"
        >
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'status'">
              <Tag :color="(record as ProjectApi.ProjectListItem).isKeyProject ? 'gold' : 'blue'">
                {{ (record as ProjectApi.ProjectListItem).status }}
              </Tag>
            </template>
          </template>
        </Table>
      </Card>
    </section>
  </Page>
</template>

<style scoped>
.pmo-overview {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.pmo-overview__empty {
  display: grid;
  min-height: 240px;
  place-items: center;
}
</style>
