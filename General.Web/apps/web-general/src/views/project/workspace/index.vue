<script lang="ts" setup>
import { onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';

import { Page } from '@vben/common-ui';

import { Button, Card, Col, Empty, Row, Space, Statistic, Table, Tag } from 'ant-design-vue';

import { getProjectWorkspaceApi, type ProjectApi } from '#/api/core';

defineOptions({ name: 'ProjectWorkspacePage' });

const router = useRouter();
const loading = ref(false);
const workspace = ref<null | ProjectApi.WorkspaceItem>(null);

const projectColumns = [
  { dataIndex: 'projectCode', key: 'projectCode', title: '项目编号', width: 140 },
  { dataIndex: 'name', key: 'name', title: '项目名称' },
  { dataIndex: 'managerName', key: 'managerName', title: '项目经理', width: 120 },
  { dataIndex: 'status', key: 'status', title: '状态', width: 120 },
  { dataIndex: 'overdueTaskCount', key: 'overdueTaskCount', title: '逾期任务', width: 100 },
];

const taskColumns = [
  { dataIndex: 'taskCode', key: 'taskCode', title: '任务编号', width: 120 },
  { dataIndex: 'title', key: 'title', title: '任务标题' },
  { dataIndex: 'projectName', key: 'projectName', title: '所属项目', width: 180 },
  { dataIndex: 'status', key: 'status', title: '状态', width: 120 },
  { dataIndex: 'plannedEndTime', key: 'plannedEndTime', title: '计划完成', width: 180 },
];

const milestoneColumns = [
  { dataIndex: 'name', key: 'name', title: '里程碑' },
  { dataIndex: 'projectName', key: 'projectName', title: '所属项目', width: 180 },
  { dataIndex: 'ownerName', key: 'ownerName', title: '责任人', width: 120 },
  { dataIndex: 'plannedCompletionDate', key: 'plannedCompletionDate', title: '计划日期', width: 160 },
];

const raidColumns = [
  { dataIndex: 'type', key: 'type', title: '类型', width: 120 },
  { dataIndex: 'title', key: 'title', title: '事项' },
  { dataIndex: 'projectName', key: 'projectName', title: '所属项目', width: 180 },
  { dataIndex: 'level', key: 'level', title: '等级', width: 100 },
  { dataIndex: 'plannedResolveDate', key: 'plannedResolveDate', title: '计划处理', width: 160 },
];

async function loadWorkspace() {
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

onMounted(loadWorkspace);
</script>

<template>
  <Page description="项目工作台按一期口径聚合我的任务、关键项目、周里程碑和 RAID 预警。" title="工作台">
    <section class="project-workspace">
      <Card :bordered="false" class="project-workspace__hero">
        <div class="project-workspace__copy">
          <p class="project-workspace__eyebrow">Phase 1 Delivery</p>
          <h2>项目执行闭环已经接入真实数据：项目、任务、里程碑、RAID 同屏查看。</h2>
          <p>
            当前工作台优先服务 PM / PMO 的高频路径，后续继续补项目立项、PM 工作台和 PMO 总览的深度交互。
          </p>
          <Space wrap>
            <Button type="primary" @click="router.push('/project/projects')">进入项目列表</Button>
            <Button @click="router.push('/project/tasks')">进入任务页</Button>
            <Button @click="router.push('/project/raid')">查看 RAID</Button>
          </Space>
        </div>
      </Card>

      <Row :gutter="[16, 16]">
        <Col :lg="6" :md="12" :span="24">
          <Card :bordered="false"><Statistic title="进行中项目" :value="workspace?.ongoingProjectCount || 0" /></Card>
        </Col>
        <Col :lg="6" :md="12" :span="24">
          <Card :bordered="false"><Statistic title="我的待办任务" :value="workspace?.myTodoCount || 0" /></Card>
        </Col>
        <Col :lg="6" :md="12" :span="24">
          <Card :bordered="false"><Statistic title="阻塞任务" :value="workspace?.blockedTaskCount || 0" /></Card>
        </Col>
        <Col :lg="6" :md="12" :span="24">
          <Card :bordered="false"><Statistic title="高风险事项" :value="workspace?.highRiskCount || 0" /></Card>
        </Col>
      </Row>

      <Row :gutter="[16, 16]">
        <Col :lg="14" :span="24">
          <Card id="workspace-key-projects" :bordered="false" title="关键项目">
            <div v-if="!loading && (workspace?.keyProjects.length || 0) === 0" class="project-workspace__empty">
              <Empty description="暂无项目数据" />
            </div>
            <Table
              v-else
              :columns="projectColumns"
              :data-source="workspace?.keyProjects || []"
              :loading="loading"
              :pagination="false"
              row-key="id"
              size="small"
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
        </Col>

        <Col :lg="10" :span="24">
          <Card id="workspace-my-tasks" :bordered="false" title="我的任务">
            <div v-if="!loading && (workspace?.myTasks.length || 0) === 0" class="project-workspace__empty">
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
                <template v-else-if="column.key === 'status'">
                  <Tag :color="(record as ProjectApi.TaskItem).isBlocked ? 'error' : 'processing'">
                    {{ (record as ProjectApi.TaskItem).status }}
                  </Tag>
                </template>
              </template>
            </Table>
          </Card>
        </Col>
      </Row>

      <Row :gutter="[16, 16]">
        <Col :lg="12" :span="24">
          <Card id="workspace-milestones" :bordered="false" title="本周关键里程碑">
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

        <Col :lg="12" :span="24">
          <Card id="workspace-raid" :bordered="false" title="RAID 预警">
            <Table
              :columns="raidColumns"
              :data-source="workspace?.raidAlerts || []"
              :loading="loading"
              :pagination="false"
              row-key="id"
              size="small"
            >
              <template #bodyCell="{ column, record }">
                <template v-if="column.key === 'level'">
                  <Tag :color="(record as ProjectApi.RaidItem).level === '高' ? 'error' : 'warning'">
                    {{ (record as ProjectApi.RaidItem).level }}
                  </Tag>
                </template>
                <template v-else-if="column.key === 'plannedResolveDate'">
                  {{ formatDate((record as ProjectApi.RaidItem).plannedResolveDate) }}
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
.project-workspace {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.project-workspace__hero {
  background:
    radial-gradient(circle at top left, rgb(37 99 235 / 0.18), transparent 34%),
    linear-gradient(135deg, var(--ant-color-bg-container), var(--ant-color-fill-quaternary));
}

.project-workspace__copy h2 {
  margin: 0 0 12px;
  color: var(--ant-color-text);
  font-size: 28px;
  line-height: 1.25;
}

.project-workspace__copy p {
  color: var(--ant-color-text-secondary);
  line-height: 1.8;
}

.project-workspace__eyebrow {
  margin: 0 0 10px;
  color: var(--ant-color-primary);
  font-size: 12px;
  font-weight: 700;
  letter-spacing: 0.14em;
  text-transform: uppercase;
}

.project-workspace__empty {
  display: grid;
  min-height: 220px;
  place-items: center;
}
</style>
