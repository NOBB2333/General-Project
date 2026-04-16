<script lang="ts" setup>
import { onMounted, reactive, ref } from 'vue';

import { Page } from '@vben/common-ui';

import { Button, Card, Empty, Input, Select, Switch, Table, Tag } from 'ant-design-vue';

import { getProjectTaskListApi, type ProjectApi } from '#/api/core';

defineOptions({ name: 'ProjectTaskPage' });

const loading = ref(false);
const items = ref<ProjectApi.TaskItem[]>([]);
const filters = reactive<ProjectApi.TaskListInput>({
  keyword: '',
  onlyMine: false,
  status: undefined,
});

const columns = [
  { dataIndex: 'taskCode', key: 'taskCode', title: '任务编号', width: 120 },
  { dataIndex: 'title', key: 'title', title: '任务标题', width: 220 },
  { dataIndex: 'projectName', key: 'projectName', title: '所属项目', width: 180 },
  { dataIndex: 'ownerName', key: 'ownerName', title: '负责人', width: 120 },
  { dataIndex: 'organizationUnitName', key: 'organizationUnitName', title: '部门', width: 120 },
  { dataIndex: 'status', key: 'status', title: '状态', width: 120 },
  { dataIndex: 'priority', key: 'priority', title: '优先级', width: 100 },
  { dataIndex: 'plannedEndTime', key: 'plannedEndTime', title: '计划结束', width: 160 },
];

const statusOptions = ['未开始', '进行中', '阻塞', '已完成', '已关闭'].map((value) => ({
  label: value,
  value,
}));

async function loadTasks() {
  loading.value = true;
  try {
    items.value = await getProjectTaskListApi({
      keyword: filters.keyword || undefined,
      onlyMine: filters.onlyMine,
      status: filters.status || undefined,
    });
  } finally {
    loading.value = false;
  }
}

function formatDate(value?: null | string) {
  return value ? new Date(value).toLocaleString() : '-';
}

onMounted(loadTasks);
</script>

<template>
  <Page description="任务页按项目范围展示负责人、阻塞原因和计划完成时间，支持只看我的任务。" title="任务">
    <Card :bordered="false" title="执行任务">
      <template #extra>
        <div class="project-task__toolbar">
          <Input
            v-model:value="filters.keyword"
            allow-clear
            placeholder="按任务编号、标题、项目筛选"
            @pressEnter="loadTasks"
          />
          <Select
            v-model:value="filters.status"
            allow-clear
            class="project-task__select"
            :options="statusOptions"
            placeholder="状态"
          />
          <label class="project-task__switch">
            <span>只看我的</span>
            <Switch v-model:checked="filters.onlyMine" />
          </label>
          <Button type="primary" @click="loadTasks">查询</Button>
        </div>
      </template>

      <div v-if="!loading && items.length === 0" class="project-task__empty">
        <Empty description="暂无任务数据" />
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
            <Tag :color="(record as ProjectApi.TaskItem).isBlocked ? 'error' : 'processing'">
              {{ (record as ProjectApi.TaskItem).status }}
            </Tag>
          </template>
          <template v-else-if="column.key === 'priority'">
            <Tag :color="(record as ProjectApi.TaskItem).priority === '高' ? 'error' : 'blue'">
              {{ (record as ProjectApi.TaskItem).priority }}
            </Tag>
          </template>
          <template v-else-if="column.key === 'plannedEndTime'">
            {{ formatDate((record as ProjectApi.TaskItem).plannedEndTime) }}
          </template>
          <template v-else-if="column.key === 'title'">
            <div class="project-task__title">
              <strong>{{ (record as ProjectApi.TaskItem).title }}</strong>
              <span v-if="(record as ProjectApi.TaskItem).blockReason">
                {{ (record as ProjectApi.TaskItem).blockReason }}
              </span>
            </div>
          </template>
        </template>
      </Table>
    </Card>
  </Page>
</template>

<style scoped>
.project-task__toolbar {
  display: flex;
  gap: 12px;
  align-items: center;
}

.project-task__select {
  width: 160px;
}

.project-task__switch {
  display: inline-flex;
  gap: 8px;
  align-items: center;
  color: var(--ant-color-text-secondary);
}

.project-task__empty {
  display: grid;
  min-height: 260px;
  place-items: center;
}

.project-task__title {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.project-task__title span {
  color: var(--ant-color-text-secondary);
  font-size: 12px;
}

@media (max-width: 960px) {
  .project-task__toolbar {
    display: grid;
    grid-template-columns: 1fr;
    align-items: stretch;
  }

  .project-task__select {
    width: 100%;
  }
}
</style>
