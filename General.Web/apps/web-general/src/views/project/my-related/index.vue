<script lang="ts" setup>
import { computed, onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';

import { Page } from '@vben/common-ui';

import { Button, Card, Empty, Table, Tag } from 'ant-design-vue';

import { getProjectMyRelatedApi, type ProjectApi } from '#/api/core';

defineOptions({ name: 'ProjectMyRelatedPage' });

const router = useRouter();
const loading = ref(false);
const payload = ref<null | ProjectApi.MyRelatedPayload>(null);

const projectColumns = [
  { dataIndex: 'projectCode', key: 'projectCode', title: '项目编号', width: 140 },
  { dataIndex: 'name', key: 'name', title: '项目名称' },
  { dataIndex: 'myRelation', key: 'myRelation', title: '我的关系', width: 160 },
  { dataIndex: 'dueSoonTaskCount', key: 'dueSoonTaskCount', title: '近期待办', width: 100 },
  { dataIndex: 'activeIssueCount', key: 'activeIssueCount', title: '待处理问题', width: 110 },
  { dataIndex: 'weekHours', key: 'weekHours', title: '本周工时', width: 100 },
  { key: 'actions', title: '操作', width: 120 },
];

const taskColumns = [
  { dataIndex: 'title', key: 'title', title: '任务' },
  { dataIndex: 'projectName', key: 'projectName', title: '项目', width: 180 },
  { dataIndex: 'plannedEndTime', key: 'plannedEndTime', title: '计划完成', width: 140 },
  { dataIndex: 'status', key: 'status', title: '状态', width: 110 },
];

const issueColumns = [
  { dataIndex: 'title', key: 'title', title: '问题项' },
  { dataIndex: 'type', key: 'type', title: '类型', width: 100 },
  { dataIndex: 'level', key: 'level', title: '等级', width: 100 },
  { dataIndex: 'dueDate', key: 'dueDate', title: '到期', width: 140 },
];

const worklogColumns = [
  { dataIndex: 'workDate', key: 'workDate', title: '日期', width: 140 },
  { dataIndex: 'taskTitle', key: 'taskTitle', title: '关联事项' },
  { dataIndex: 'summary', key: 'summary', title: '工作摘要' },
  { dataIndex: 'hours', key: 'hours', title: '工时', width: 100 },
];

const metrics = computed(() => {
  const data = payload.value;
  return [
    { label: '相关项目', value: data?.myProjects.length || 0 },
    { label: '我的待办', value: data?.myTasks.length || 0 },
    { label: '我的问题', value: data?.myIssues.length || 0 },
    {
      label: '本周工时',
      value: (data?.recentWorklogs || []).reduce((total, item) => total + item.hours, 0).toFixed(1),
    },
  ];
});

const quickLinks = [
  {
    title: '项目工作台',
    actions: [
      { label: '关键项目', path: '/project/workspace', hash: '#workspace-key-projects' },
      { label: '我的任务', path: '/project/workspace', hash: '#workspace-my-tasks' },
      { label: 'RAID 预警', path: '/project/workspace', hash: '#workspace-raid' },
    ],
  },
  {
    title: 'PM 工作台',
    actions: [
      { label: '执行任务', path: '/project/pm-dashboard', hash: '#pm-dashboard-tasks' },
      { label: '本周里程碑', path: '/project/pm-dashboard', hash: '#pm-dashboard-milestones' },
    ],
  },
  {
    title: 'PMO 总览',
    actions: [{ label: '异常项目', path: '/project/pmo-overview', hash: '#pmo-overview-exceptions' }],
  },
] as const;

async function loadData() {
  loading.value = true;
  try {
    payload.value = await getProjectMyRelatedApi();
  } finally {
    loading.value = false;
  }
}

function formatDate(value?: null | string) {
  return value ? new Date(value).toLocaleDateString() : '-';
}

function openDetail(projectId: string) {
  localStorage.setItem('general:last-project-id', projectId);
  router.push({
    path: '/project/detail',
    query: {
      projectId,
    },
  });
}

function openSection(path: string, hash?: string) {
  router.push({
    hash,
    path,
  });
}

onMounted(loadData);
</script>

<template>
  <Page description="与我相关聚合本人当前参与项目、待办事项、问题和工时回填，偏指引与跳转。" title="与我相关">
    <section class="my-related">
      <Row :gutter="[16, 16]">
        <Col v-for="metric in metrics" :key="metric.label" :lg="6" :md="12" :span="24">
          <Card :bordered="false">
            <div class="my-related__metric-label">{{ metric.label }}</div>
            <strong class="my-related__metric-value">{{ metric.value }}</strong>
          </Card>
        </Col>
      </Row>

      <Card :bordered="false" title="快速跳转">
        <div class="my-related__quick-links">
          <article v-for="group in quickLinks" :key="group.title" class="my-related__quick-group">
            <strong>{{ group.title }}</strong>
            <div class="my-related__quick-actions">
              <Button
                v-for="action in group.actions"
                :key="`${group.title}-${action.label}`"
                size="small"
                type="default"
                @click="openSection(action.path, action.hash)"
              >
                {{ action.label }}
              </Button>
            </div>
          </article>
        </div>
      </Card>

      <div class="my-related__grid">
        <Card :bordered="false" class="my-related__panel my-related__panel--wide" title="我参与的项目">
          <div v-if="!loading && (payload?.myProjects.length || 0) === 0" class="my-related__empty">
            <Empty description="暂无项目关系" />
          </div>
          <Table
            v-else
            :columns="projectColumns"
            :data-source="payload?.myProjects || []"
            :loading="loading"
            :pagination="false"
            row-key="id"
          >
            <template #bodyCell="{ column, record }">
              <template v-if="column.key === 'name'">
                <Button size="small" type="link" @click="openDetail((record as ProjectApi.MyRelatedItem).id)">
                  {{ (record as ProjectApi.MyRelatedItem).name }}
                </Button>
              </template>
              <template v-else-if="column.key === 'myRelation'">
                <Tag color="processing">{{ (record as ProjectApi.MyRelatedItem).myRelation }}</Tag>
              </template>
              <template v-else-if="column.key === 'actions'">
                <Button size="small" type="link" @click="openDetail((record as ProjectApi.MyRelatedItem).id)">
                  进入详情
                </Button>
              </template>
            </template>
          </Table>
        </Card>

        <Card :bordered="false" class="my-related__panel" title="我的任务">
          <Table
            :columns="taskColumns"
            :data-source="payload?.myTasks || []"
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
                <Tag :color="(record as ProjectApi.TaskItem).isBlocked ? 'error' : 'blue'">
                  {{ (record as ProjectApi.TaskItem).status }}
                </Tag>
              </template>
            </template>
          </Table>
        </Card>

        <Card :bordered="false" class="my-related__panel" title="我的问题">
          <Table
            :columns="issueColumns"
            :data-source="payload?.myIssues || []"
            :loading="loading"
            :pagination="false"
            row-key="id"
            size="small"
          >
            <template #bodyCell="{ column, record }">
              <template v-if="column.key === 'dueDate'">
                {{ formatDate((record as ProjectApi.IssueItem).dueDate) }}
              </template>
              <template v-else-if="column.key === 'level'">
                <Tag :color="(record as ProjectApi.IssueItem).level === '高' ? 'error' : 'gold'">
                  {{ (record as ProjectApi.IssueItem).level }}
                </Tag>
              </template>
            </template>
          </Table>
        </Card>

        <Card :bordered="false" class="my-related__panel" title="最近工时回填">
          <Table
            :columns="worklogColumns"
            :data-source="payload?.recentWorklogs || []"
            :loading="loading"
            :pagination="false"
            row-key="id"
            size="small"
          >
            <template #bodyCell="{ column, record }">
              <template v-if="column.key === 'workDate'">
                {{ formatDate((record as ProjectApi.WorklogItem).workDate) }}
              </template>
            </template>
          </Table>
        </Card>
      </div>
    </section>
  </Page>
</template>

<style scoped>
.my-related {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.my-related__grid {
  display: grid;
  gap: 16px;
  grid-template-columns: repeat(2, minmax(0, 1fr));
}

.my-related__panel,
.my-related__panel :deep(.ant-card-body) {
  min-width: 0;
}

.my-related__panel--wide {
  grid-column: span 2;
}

.my-related__metric-label {
  color: var(--ant-color-text-secondary);
  font-size: 13px;
}

.my-related__metric-value {
  display: block;
  margin-top: 8px;
  color: var(--ant-color-text);
  font-size: 28px;
}

.my-related__quick-links,
.my-related__quick-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
}

.my-related__quick-group {
  display: flex;
  gap: 12px;
  align-items: center;
  justify-content: space-between;
  min-width: 280px;
  padding: 12px 14px;
  background: var(--ant-color-fill-quaternary);
  border-radius: 14px;
}

.my-related__empty {
  display: grid;
  min-height: 220px;
  place-items: center;
}

@media (max-width: 1080px) {
  .my-related__grid {
    grid-template-columns: 1fr;
  }

  .my-related__panel--wide {
    grid-column: span 1;
  }
}
</style>
