<script lang="ts" setup>
import { computed, onMounted, reactive, ref } from 'vue';
import { useRouter } from 'vue-router';

import { Page } from '@vben/common-ui';

import { Button, Card, Col, Empty, Input, Row, Select, Space, Statistic, Table, Tag } from 'ant-design-vue';

import { getProjectListApi, type ProjectApi } from '#/api/core';

defineOptions({ name: 'ProjectListPage' });

const router = useRouter();
const loading = ref(false);
const items = ref<ProjectApi.ProjectListItem[]>([]);
const filters = reactive<ProjectApi.ProjectListInput>({
  keyword: '',
  onlyMyRelated: false,
  status: undefined,
});

const columns = [
  { dataIndex: 'projectCode', key: 'projectCode', title: '项目编号', width: 140 },
  { dataIndex: 'name', key: 'name', title: '项目名称', width: 220 },
  { dataIndex: 'organizationUnitName', key: 'organizationUnitName', title: '主责部门', width: 140 },
  { dataIndex: 'managerName', key: 'managerName', title: '项目经理', width: 120 },
  { dataIndex: 'memberCount', key: 'memberCount', title: '成员数', width: 96 },
  { dataIndex: 'myRelation', key: 'myRelation', title: '我的关系', width: 160 },
  { dataIndex: 'status', key: 'status', title: '状态', width: 110 },
  { dataIndex: 'priority', key: 'priority', title: '优先级', width: 96 },
  { dataIndex: 'highRiskCount', key: 'highRiskCount', title: '高风险', width: 90 },
  { dataIndex: 'plannedEndDate', key: 'plannedEndDate', title: '计划结束', width: 140 },
  { key: 'actions', title: '操作', width: 120 },
];

const metrics = computed(() => [
  { label: '项目总数', value: items.value.length },
  { label: '重点项目', value: items.value.filter((item) => item.isKeyProject).length },
  { label: '与我相关', value: items.value.filter((item) => item.myRelation !== '组织可见').length },
  {
    label: '异常项目',
    value: items.value.filter((item) => item.highRiskCount > 0 || item.overdueTaskCount > 0).length,
  },
]);

const statusOptions = ['待规划', '进行中', '已完成', '已关闭'].map((value) => ({
  label: value,
  value,
}));

async function loadProjects() {
  loading.value = true;
  try {
    items.value = await getProjectListApi({
      keyword: filters.keyword || undefined,
      onlyMyRelated: filters.onlyMyRelated,
      status: filters.status || undefined,
    });
  } finally {
    loading.value = false;
  }
}

function formatDate(value?: null | string) {
  return value ? new Date(value).toLocaleDateString() : '-';
}

function openDetail(record: ProjectApi.ProjectListItem) {
  localStorage.setItem('general:last-project-id', record.id);
  router.push({
    path: '/project/detail',
    query: {
      projectId: record.id,
    },
  });
}

onMounted(loadProjects);
</script>

<template>
  <Page description="项目列表按组织范围叠加项目成员覆盖返回，并支持直接进入项目详情横向页。" title="项目列表">
    <section class="project-list">
      <Row :gutter="[16, 16]">
        <Col v-for="metric in metrics" :key="metric.label" :lg="6" :md="12" :span="24">
          <Card :bordered="false" class="project-list__metric">
            <Statistic :title="metric.label" :value="metric.value" />
          </Card>
        </Col>
      </Row>

      <Card :bordered="false" title="项目清单">
        <template #extra>
          <div class="project-list__toolbar">
            <Input
              v-model:value="filters.keyword"
              allow-clear
              placeholder="按项目编号、名称、项目经理筛选"
              @pressEnter="loadProjects"
            />
            <Select
              v-model:value="filters.status"
              allow-clear
              class="project-list__select"
              :options="statusOptions"
              placeholder="状态"
            />
            <Button :type="filters.onlyMyRelated ? 'primary' : 'default'" @click="filters.onlyMyRelated = !filters.onlyMyRelated">
              与我相关
            </Button>
            <Button type="primary" @click="loadProjects">查询</Button>
          </div>
        </template>

        <div v-if="!loading && items.length === 0" class="project-list__empty">
          <Empty description="暂无项目数据" />
        </div>

        <Table
          v-else
          :columns="columns"
          :data-source="items"
          :loading="loading"
          :pagination="{ pageSize: 10 }"
          row-key="id"
          :scroll="{ x: 1360 }"
        >
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'name'">
              <button class="project-list__link" type="button" @click="openDetail(record as ProjectApi.ProjectListItem)">
                {{ (record as ProjectApi.ProjectListItem).name }}
              </button>
            </template>
            <template v-else-if="column.key === 'myRelation'">
              <Tag color="processing">{{ (record as ProjectApi.ProjectListItem).myRelation }}</Tag>
            </template>
            <template v-else-if="column.key === 'status'">
              <Tag :color="(record as ProjectApi.ProjectListItem).isKeyProject ? 'gold' : 'blue'">
                {{ (record as ProjectApi.ProjectListItem).status }}
              </Tag>
            </template>
            <template v-else-if="column.key === 'priority'">
              <Tag :color="(record as ProjectApi.ProjectListItem).priority === '高' ? 'error' : 'processing'">
                {{ (record as ProjectApi.ProjectListItem).priority }}
              </Tag>
            </template>
            <template v-else-if="column.key === 'plannedEndDate'">
              {{ formatDate((record as ProjectApi.ProjectListItem).plannedEndDate) }}
            </template>
            <template v-else-if="column.key === 'actions'">
              <Space>
                <Button size="small" type="link" @click="openDetail(record as ProjectApi.ProjectListItem)">
                  查看详情
                </Button>
              </Space>
            </template>
          </template>
        </Table>
      </Card>
    </section>
  </Page>
</template>

<style scoped>
.project-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.project-list__metric {
  min-height: 100%;
}

.project-list__toolbar {
  display: flex;
  gap: 12px;
}

.project-list__select {
  width: 160px;
}

.project-list__empty {
  display: grid;
  min-height: 280px;
  place-items: center;
}

.project-list__link {
  padding: 0;
  color: var(--ant-color-primary);
  background: transparent;
  border: 0;
  cursor: pointer;
  font-weight: 600;
}

@media (max-width: 960px) {
  .project-list__toolbar {
    display: grid;
    grid-template-columns: 1fr;
  }

  .project-list__select {
    width: 100%;
  }
}
</style>
