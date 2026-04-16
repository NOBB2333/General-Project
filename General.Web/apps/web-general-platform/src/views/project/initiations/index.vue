<script lang="ts" setup>
import { onMounted, ref } from 'vue';

import { Page } from '@vben/common-ui';

import { Card, Empty, Table, Tag } from 'ant-design-vue';

import { getProjectListApi, type ProjectApi } from '#/api/core';

defineOptions({ name: 'ProjectInitiationPage' });

const loading = ref(false);
const items = ref<ProjectApi.ProjectListItem[]>([]);

const columns = [
  { dataIndex: 'projectCode', key: 'projectCode', title: '项目编号', width: 140 },
  { dataIndex: 'name', key: 'name', title: '项目名称' },
  { dataIndex: 'organizationUnitName', key: 'organizationUnitName', title: '主责部门', width: 140 },
  { dataIndex: 'managerName', key: 'managerName', title: '项目经理', width: 120 },
  { dataIndex: 'priority', key: 'priority', title: '优先级', width: 100 },
  { dataIndex: 'plannedEndDate', key: 'plannedEndDate', title: '目标完成', width: 140 },
];

async function loadInitiations() {
  loading.value = true;
  try {
    items.value = await getProjectListApi({ status: '待规划' });
  } finally {
    loading.value = false;
  }
}

function formatDate(value?: null | string) {
  return value ? new Date(value).toLocaleDateString() : '-';
}

onMounted(loadInitiations);
</script>

<template>
  <Page description="项目立项页当前聚焦待规划项目清单，后续继续补正式立项表单和审批链。" title="项目立项">
    <Card :bordered="false" title="待规划项目">
      <div v-if="!loading && items.length === 0" class="project-initiation__empty">
        <Empty description="暂无待规划项目" />
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
          <template v-if="column.key === 'priority'">
            <Tag :color="(record as ProjectApi.ProjectListItem).priority === '高' ? 'error' : 'processing'">
              {{ (record as ProjectApi.ProjectListItem).priority }}
            </Tag>
          </template>
          <template v-else-if="column.key === 'plannedEndDate'">
            {{ formatDate((record as ProjectApi.ProjectListItem).plannedEndDate) }}
          </template>
        </template>
      </Table>
    </Card>
  </Page>
</template>

<style scoped>
.project-initiation__empty {
  display: grid;
  min-height: 260px;
  place-items: center;
}
</style>
