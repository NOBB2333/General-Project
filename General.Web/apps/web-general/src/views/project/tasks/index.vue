<script lang="ts" setup>
import type { NativeVxeGridEvents, VxeGridProps } from '#/adapter/vxe-table';

import { computed, onMounted, reactive, ref } from 'vue';

import { Page } from '@vben/common-ui';

import {
  Button,
  Card,
  Empty,
  Input,
  Select,
  Switch,
  Tag,
} from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { getProjectTaskListApi, type ProjectApi } from '#/api/core';
import {
  createDictOptionsFallback,
  useDictOptions,
} from '#/composables/platform/use-dict-options';
import { useLocalVxePagination } from '#/composables/platform/use-local-vxe-pagination';

defineOptions({ name: 'ProjectTaskPage' });

const loading = ref(false);
const items = ref<ProjectApi.TaskItem[]>([]);
const pagination = useLocalVxePagination(items, { defaultPageSize: 10 });
const filters = reactive<ProjectApi.TaskListInput>({
  keyword: '',
  onlyMine: false,
  status: undefined,
});
const taskDicts = useDictOptions(
  ['task_status', 'task_priority'],
  createDictOptionsFallback({
    task_priority: ['P0', 'P1', 'P2', 'P3'],
    task_status: ['未开始', '进行中', '阻塞', '已完成', '已关闭'],
  }),
);

const columns = [
  { field: 'taskCode', title: '任务编号', width: 120 },
  { align: 'left', field: 'title', slots: { default: 'title' }, title: '任务标题', width: 220 },
  { field: 'projectName', title: '所属项目', width: 180 },
  { field: 'ownerName', title: '负责人', width: 120 },
  { field: 'organizationUnitName', title: '部门', width: 120 },
  { field: 'status', slots: { default: 'status' }, title: '状态', width: 120 },
  { field: 'priority', slots: { default: 'priority' }, title: '优先级', width: 100 },
  { field: 'plannedEndTime', slots: { default: 'plannedEndTime' }, title: '计划结束', width: 160 },
] satisfies VxeGridProps<ProjectApi.TaskItem>['columns'];

const statusOptions = taskDicts.getOptions('task_status');

const gridOptions = computed<VxeGridProps<ProjectApi.TaskItem>>(() => ({
  columns,
  data: pagination.pagedRows.value,
  loading: loading.value,
  pagerConfig: pagination.pagerConfig.value,
  rowConfig: { keyField: 'id' },
}));

const gridEvents: NativeVxeGridEvents<ProjectApi.TaskItem> = {
  'page-change': pagination.handlePageChange,
};

const [Grid] = useVbenVxeGrid<ProjectApi.TaskItem>({
  gridEvents,
  gridOptions: {},
});

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

onMounted(async () => {
  await Promise.all([taskDicts.load(), loadTasks()]);
});
</script>

<template>
  <Page
    description="任务页按项目范围展示负责人、阻塞原因和计划完成时间，支持只看我的任务。"
    title="任务"
  >
    <Card class="rounded-2xl" :bordered="false" title="执行任务">
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
      <Grid
        v-else
        :grid-events="gridEvents"
        :grid-options="gridOptions"
      >
        <template #status="{ row }">
          <Tag
            :color="
              taskDicts.getColor(
                'task_status',
                row.status,
                row.isBlocked ? 'error' : 'processing',
              )
            "
          >
            {{ row.status }}
          </Tag>
        </template>
        <template #priority="{ row }">
          <Tag
            :color="
              taskDicts.getColor(
                'task_priority',
                row.priority,
                'blue',
              )
            "
          >
            {{ row.priority }}
          </Tag>
        </template>
        <template #plannedEndTime="{ row }">
          {{ formatDate(row.plannedEndTime) }}
        </template>
        <template #title="{ row }">
          <div class="project-task__title">
            <strong>{{ row.title }}</strong>
            <span v-if="row.blockReason">
              {{ row.blockReason }}
            </span>
          </div>
        </template>
      </Grid>
    </Card>
  </Page>
</template>

<style scoped>
@reference "@vben/tailwind-config/theme";
.project-task__toolbar {
  display: flex;
  @apply gap-3;
  align-items: center;
}

.project-task__select {
  width: 160px;
}

.project-task__switch {
  display: inline-flex;
  @apply gap-2;
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
  @apply gap-1;
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
