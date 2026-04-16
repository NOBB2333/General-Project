<script lang="ts" setup>
import { onMounted, ref } from 'vue';

import { Page } from '@vben/common-ui';

import { Card, Col, Empty, Row, Statistic, Table, Tag } from 'ant-design-vue';

import { getProjectWorkspaceApi, type ProjectApi } from '#/api/core';

defineOptions({ name: 'ProjectPmDashboardPage' });

const loading = ref(false);
const workspace = ref<null | ProjectApi.WorkspaceItem>(null);

const taskColumns = [
  { dataIndex: 'taskCode', key: 'taskCode', title: '任务编号', width: 120 },
  { dataIndex: 'title', key: 'title', title: '任务标题' },
  { dataIndex: 'projectName', key: 'projectName', title: '所属项目', width: 180 },
  { dataIndex: 'plannedEndTime', key: 'plannedEndTime', title: '计划完成', width: 160 },
];

const milestoneColumns = [
  { dataIndex: 'name', key: 'name', title: '里程碑' },
  { dataIndex: 'projectName', key: 'projectName', title: '所属项目', width: 180 },
  { dataIndex: 'plannedCompletionDate', key: 'plannedCompletionDate', title: '计划日期', width: 160 },
];

async function loadDashboard() {
  loading.value = true;
  try {
    workspace.value = await getProjectWorkspaceApi();
  } finally {
    loading.value = false;
  }
}

function formatDate(value?: null | string) {
  return value ? new Date(value).toLocaleDateString() : '-';
}

onMounted(loadDashboard);
</script>

<template>
  <Page description="PM 工作台聚焦本人待办、关键里程碑和执行阻塞。" title="PM 工作台">
    <section class="pm-dashboard">
      <Row :gutter="[16, 16]">
        <Col :lg="6" :md="12" :span="24">
          <Card :bordered="false"><Statistic title="我的待办" :value="workspace?.myTodoCount || 0" /></Card>
        </Col>
        <Col :lg="6" :md="12" :span="24">
          <Card :bordered="false"><Statistic title="阻塞任务" :value="workspace?.blockedTaskCount || 0" /></Card>
        </Col>
        <Col :lg="6" :md="12" :span="24">
          <Card :bordered="false"><Statistic title="逾期任务" :value="workspace?.overdueTaskCount || 0" /></Card>
        </Col>
        <Col :lg="6" :md="12" :span="24">
          <Card :bordered="false"><Statistic title="高风险事项" :value="workspace?.highRiskCount || 0" /></Card>
        </Col>
      </Row>

      <Row :gutter="[16, 16]">
        <Col :lg="14" :span="24">
          <Card id="pm-dashboard-tasks" :bordered="false" title="我的执行任务">
            <div v-if="!loading && (workspace?.myTasks.length || 0) === 0" class="pm-dashboard__empty">
              <Empty description="暂无任务" />
            </div>
            <Table
              v-else
              :columns="taskColumns"
              :data-source="workspace?.myTasks || []"
              :loading="loading"
              :pagination="false"
              row-key="id"
              size="small"
            >
              <template #bodyCell="{ column, record }">
                <template v-if="column.key === 'plannedEndTime'">
                  {{ formatDate((record as ProjectApi.TaskItem).plannedEndTime) }}
                </template>
                <template v-else-if="column.key === 'title'">
                  <div class="pm-dashboard__task">
                    <strong>{{ (record as ProjectApi.TaskItem).title }}</strong>
                    <Tag v-if="(record as ProjectApi.TaskItem).isBlocked" color="error">阻塞</Tag>
                  </div>
                </template>
              </template>
            </Table>
          </Card>
        </Col>

        <Col :lg="10" :span="24">
          <Card id="pm-dashboard-milestones" :bordered="false" title="本周里程碑">
            <Table
              :columns="milestoneColumns"
              :data-source="workspace?.weeklyMilestones || []"
              :loading="loading"
              :pagination="false"
              row-key="id"
              size="small"
            >
              <template #bodyCell="{ column, record }">
                <template v-if="column.key === 'plannedCompletionDate'">
                  {{ formatDate((record as ProjectApi.WorkspaceMilestoneItem).plannedCompletionDate) }}
                </template>
              </template>
            </Table>
          </Card>
        </Col>
      </Row>
    </section>
  </Page>
</template>

<style scoped>
.pm-dashboard {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.pm-dashboard__empty {
  display: grid;
  min-height: 220px;
  place-items: center;
}

.pm-dashboard__task {
  display: flex;
  gap: 8px;
  align-items: center;
}
</style>
