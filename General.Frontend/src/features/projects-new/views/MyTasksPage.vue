<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue';
import { getProjectTaskPagedListApi, type ProjectApi } from '@/api/core/project';
import { ReferenceBoardPageSurface, type PageModel, type PageRow } from '@/components/page';
import { UiButton, UiSelect } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import ProjectTaskDetailDialog from '@/features/projects-new/views/ProjectTaskDetailDialog.vue';
import { myTasksModel } from '@/features/projects-new/model/my-tasks.model';

const pageSize = 20;
const pageNumber = ref(1);
const searchQuery = ref('');
const statusFilter = ref('');
const result = ref<ProjectApi.PagedTaskListResult | null>(null);
const loading = ref(false);
const error = ref('');
const selectedTask = ref<ProjectApi.TaskItem | null>(null);
const taskDialogOpen = ref(false);
const statusOptions: UiSelectOption[] = [
  { value: '', label: '全部状态' },
  ...['未开始', '进行中', '阻塞', '测试中', '验收中', '已完成', '已关闭'].map((value) => ({ value, label: value })),
];
const taskRows = computed<PageRow[]>(() =>
  (result.value?.items ?? []).map((task) => ({
    code: task.taskCode,
    customer: task.projectName,
    id: task.id,
    name: task.title,
    owner: task.ownerName,
    status: task.isBlocked ? '阻塞' : task.status,
    updatedAt: `计划结束 ${formatDate(task.plannedEndTime)}`,
  })),
);
const pageModel = computed<PageModel>(() => ({
  ...myTasksModel,
  description: `${result.value?.totalCount ?? 0} 项当前账号相关任务，进度与工时按项目权限提交。`,
  rows: taskRows.value,
  searchPlaceholder: '搜索任务编号、名称或项目',
  stats: [
    {
      key: 'total',
      label: '符合条件的任务',
      value: String(result.value?.totalCount ?? 0),
      tone: 'primary',
    },
  ],
  tools: ['刷新'],
}));
const remotePagination = computed(() => ({
  currentPage: pageNumber.value,
  pageSize,
  totalCount: result.value?.totalCount ?? 0,
}));

let activeRequest: AbortController | null = null;
let searchTimer: ReturnType<typeof setTimeout> | undefined;
let requestVersion = 0;

async function loadTasks() {
  activeRequest?.abort();
  const controller = new AbortController();
  activeRequest = controller;
  const currentRequest = ++requestVersion;
  loading.value = true;
  error.value = '';
  try {
    result.value = await getProjectTaskPagedListApi(
      {
        keyword: searchQuery.value.trim() || undefined,
        maxResultCount: pageSize,
        onlyMine: true,
        skipCount: (pageNumber.value - 1) * pageSize,
        status: statusFilter.value || undefined,
      },
      controller.signal,
    );
  } catch (cause) {
    if (controller.signal.aborted || currentRequest !== requestVersion) return;
    error.value = cause instanceof Error ? cause.message : '我的任务读取失败，请稍后重试。';
  } finally {
    if (currentRequest === requestVersion) loading.value = false;
  }
}

function handleSearchChange(value: string) {
  searchQuery.value = value;
  pageNumber.value = 1;
  if (searchTimer) clearTimeout(searchTimer);
  searchTimer = setTimeout(() => void loadTasks(), 250);
}

function handleStatusChange(value: string) {
  statusFilter.value = value;
  pageNumber.value = 1;
  void loadTasks();
}

function handlePageChange(value: number) {
  pageNumber.value = value;
  void loadTasks();
}

function handleTool(tool: string) {
  if (tool === '刷新') void loadTasks();
}

function openTask(row: PageRow) {
  const task = result.value?.items.find((item) => item.id === row.id);
  if (!task) return;
  selectedTask.value = task;
  taskDialogOpen.value = true;
}

function formatDate(value: null | string): string {
  return value ? value.slice(0, 10) : '未安排';
}

onMounted(() => void loadTasks());

onBeforeUnmount(() => {
  requestVersion += 1;
  activeRequest?.abort();
  if (searchTimer) clearTimeout(searchTimer);
});
</script>

<template>
  <ReferenceBoardPageSurface
    :model="pageModel"
    :loading="loading"
    :remote-mode="true"
    :remote-pagination="remotePagination"
    :remote-rows="taskRows"
    :remote-search="searchQuery"
    @page-change="handlePageChange"
    @row="openTask"
    @search-change="handleSearchChange"
    @tool="handleTool"
  >
    <template #notice>
      <div
        v-if="loading"
        role="status"
        aria-live="polite"
      >
        正在读取当前账号可见的任务…
      </div>
      <div
        v-else-if="error"
        role="alert"
        aria-live="assertive"
      >
        <span>{{ error }}</span>
        <UiButton
          variant="secondary"
          size="sm"
          @click="loadTasks"
        >
          重试
        </UiButton>
      </div>
    </template>
    <template #filters>
      <UiSelect
        :model-value="statusFilter"
        label="任务状态筛选"
        :options="statusOptions"
        :disabled="loading"
        @update:model-value="handleStatusChange"
      />
    </template>
  </ReferenceBoardPageSurface>

  <ProjectTaskDetailDialog
    v-model:open="taskDialogOpen"
    :project-id="selectedTask?.projectId ?? ''"
    :task="selectedTask"
    @changed="loadTasks"
  />
</template>
