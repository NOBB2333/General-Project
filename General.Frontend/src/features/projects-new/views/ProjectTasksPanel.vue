<script setup lang="ts">
import { computed, onBeforeUnmount, ref, watch } from 'vue';
import { getProjectTaskPagedListApi, type ProjectApi } from '@/api/core/project';
import { UiButton, UiInput, UiSelect } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import ProjectTaskDependenciesPanel from './ProjectTaskDependenciesPanel.vue';
import ProjectTaskDetailDialog from './ProjectTaskDetailDialog.vue';
import ProjectTaskWorklogList from './ProjectTaskWorklogList.vue';

const props = defineProps<{ detail: ProjectApi.Detail; projectId: string }>();
const emit = defineEmits<{ changed: [] }>();

const pageSize = 20;
const pageNumber = ref(1);
const keyword = ref('');
const statusFilter = ref('');
const result = ref<ProjectApi.PagedTaskListResult | null>(null);
const loading = ref(false);
const error = ref('');
const notice = ref('');
const selectedTask = ref<ProjectApi.TaskItem | null>(null);
const taskDialogOpen = ref(false);
let activeRequest: AbortController | null = null;
let searchTimer: ReturnType<typeof setTimeout> | undefined;
let requestVersion = 0;

const taskStatuses = ['未开始', '进行中', '阻塞', '测试中', '验收中', '已完成', '已关闭'];
const statusOptions = computed<UiSelectOption[]>(() => [
  { value: '', label: '全部状态' },
  ...taskStatuses.map((value) => ({ value, label: value })),
]);
const dependencyTasks = computed(() => {
  const tasks = new Map<string, ProjectApi.TaskItem>();
  for (const task of [...props.detail.tasks, ...(result.value?.items ?? [])]) tasks.set(task.id, task);
  return [...tasks.values()];
});
const pageCount = computed(() => Math.max(1, Math.ceil((result.value?.totalCount ?? 0) / pageSize)));

watch(
  () => props.projectId,
  () => {
    pageNumber.value = 1;
    void loadTasks();
  },
  { immediate: true },
);

onBeforeUnmount(() => {
  requestVersion += 1;
  activeRequest?.abort();
  if (searchTimer) clearTimeout(searchTimer);
});

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
        keyword: keyword.value.trim() || undefined,
        maxResultCount: pageSize,
        projectId: props.projectId,
        skipCount: (pageNumber.value - 1) * pageSize,
        status: statusFilter.value || undefined,
      },
      controller.signal,
    );
  } catch (cause) {
    if (controller.signal.aborted || currentRequest !== requestVersion) return;
    error.value = cause instanceof Error ? cause.message : '任务列表读取失败，请稍后重试。';
  } finally {
    if (currentRequest === requestVersion) loading.value = false;
  }
}

function updateKeyword(value: string | number) {
  keyword.value = String(value);
  pageNumber.value = 1;
  if (searchTimer) clearTimeout(searchTimer);
  searchTimer = setTimeout(() => void loadTasks(), 250);
}

function updateStatus(value: string) {
  statusFilter.value = value;
  pageNumber.value = 1;
  void loadTasks();
}

function changePage(delta: number) {
  const nextPage = Math.max(1, Math.min(pageCount.value, pageNumber.value + delta));
  if (nextPage === pageNumber.value) return;
  pageNumber.value = nextPage;
  void loadTasks();
}

function openTask(task: ProjectApi.TaskItem) {
  selectedTask.value = task;
  taskDialogOpen.value = true;
}

async function reloadAfterProgress() {
  await loadTasks();
  emit('changed');
}

function formatDate(value: null | string): string {
  return value ? value.slice(0, 10) : '—';
}
</script>

<template>
  <section
    :class="$style.panel"
    aria-label="项目任务、依赖与工时"
  >
    <p
      v-if="notice"
      role="status"
      :class="$style.success"
    >
      {{ notice }}
    </p>
    <div
      v-if="error"
      role="alert"
      :class="$style.error"
    >
      <span>{{ error }}</span>
      <UiButton
        v-if="!result"
        variant="secondary"
        size="sm"
        @click="loadTasks"
      >
        重试
      </UiButton>
    </div>

    <section :class="$style.section">
      <header :class="$style.sectionHeader">
        <div>
          <h3 :class="$style.heading">项目任务</h3>
          <p :class="$style.description">任务由服务端按计划结束时间和编号排序，分页与筛选均由服务端执行。</p>
        </div>
      </header>

      <div :class="$style.toolbar">
        <UiInput
          :model-value="keyword"
          placeholder="搜索任务编号、名称或项目"
          :disabled="loading"
          @update:model-value="updateKeyword"
        />
        <UiSelect
          :model-value="statusFilter"
          label="任务状态筛选"
          :options="statusOptions"
          :disabled="loading"
          @update:model-value="updateStatus"
        />
        <span>{{ result?.totalCount ?? 0 }} 条任务</span>
        <UiButton
          variant="ghost"
          size="sm"
          :disabled="loading"
          @click="loadTasks"
        >
          刷新
        </UiButton>
      </div>

      <div
        v-if="loading && !result"
        role="status"
        :class="$style.empty"
      >
        正在读取任务…
      </div>
      <div
        v-else-if="result?.items.length"
        :class="$style.tableWrap"
      >
        <table :class="$style.table">
          <thead>
            <tr>
              <th>任务</th>
              <th>状态</th>
              <th>负责人</th>
              <th>计划结束</th>
              <th>估算工时</th>
              <th>实际工时</th>
              <th>操作</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="task in result.items"
              :key="task.id"
            >
              <td>
                <strong>{{ task.taskCode }} · {{ task.title }}</strong>
                <small>{{ task.wbsCode || task.requirementTitle || task.projectName }}</small>
              </td>
              <td>{{ task.isBlocked ? '阻塞' : task.status }}</td>
              <td>{{ task.ownerName }}</td>
              <td>{{ formatDate(task.plannedEndTime) }}</td>
              <td>{{ task.estimatedWorkHours ?? '—' }}</td>
              <td>{{ task.actualWorkHours ?? 0 }}</td>
              <td>
                <UiButton
                  variant="ghost"
                  size="sm"
                  @click="openTask(task)"
                >
                  查看 / 登记进展
                </UiButton>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      <div
        v-else-if="result"
        :class="$style.empty"
      >
        当前筛选条件下没有可见任务。
      </div>

      <footer
        v-if="result"
        :class="$style.pagination"
      >
        <span>第 {{ pageNumber }} / {{ pageCount }} 页</span>
        <div>
          <UiButton
            variant="secondary"
            size="sm"
            :disabled="loading || pageNumber <= 1"
            @click="changePage(-1)"
          >
            上一页
          </UiButton>
          <UiButton
            variant="secondary"
            size="sm"
            :disabled="loading || pageNumber >= pageCount"
            @click="changePage(1)"
          >
            下一页
          </UiButton>
        </div>
      </footer>
    </section>

    <ProjectTaskDependenciesPanel
      :dependencies="detail.taskDependencies"
      :project-id="projectId"
      :tasks="dependencyTasks"
      @changed="reloadAfterProgress"
    />
    <ProjectTaskWorklogList
      description="项目工时按当前用户和项目权限显示。"
      empty-hint="当前权限范围内没有工时记录。"
      :items="detail.worklogs"
      :limit="10"
      title="项目工时"
      :total-hours="detail.totalWorklogHours"
    />
  </section>

  <ProjectTaskDetailDialog
    v-model:open="taskDialogOpen"
    :project-id="projectId"
    :task="selectedTask"
    @changed="reloadAfterProgress"
  />
</template>

<style module>
@reference "../../../styles/index.css";
.panel {
  @apply space-y-4;
}
.section {
  @apply min-w-0 rounded border border-[#DFE1E6] bg-white p-4;
}
.sectionHeader {
  @apply flex items-start justify-between gap-3;
}
.heading {
  @apply text-[13px] font-semibold text-[#172B4D];
}
.description {
  @apply mt-1 text-[10px] leading-4 text-[#6B778C];
}
.toolbar {
  @apply mt-3 flex flex-wrap items-center gap-2 border-y border-[#F4F5F7] py-3 text-[11px] text-[#6B778C];
}
.toolbar > input {
  @apply max-w-[300px];
}
.toolbar > span {
  @apply ml-auto;
}
.tableWrap {
  @apply mt-2 overflow-x-auto;
}
.table {
  @apply w-full min-w-[860px] border-collapse text-left text-[11px];
}
.table th {
  @apply border-b border-[#DFE1E6] px-3 py-2 font-medium text-[#6B778C];
  background: #fafbfc;
}
.table td {
  @apply border-b border-[#F4F5F7] px-3 py-2 text-[#42526E];
}
.table td:first-child {
  @apply min-w-[220px];
}
.table td strong,
.table td small {
  @apply block;
}
.table td strong {
  @apply font-medium text-[#172B4D];
}
.table td small {
  @apply mt-1 text-[10px] text-[#97A0AF];
}
.pagination {
  @apply mt-3 flex items-center justify-between text-[10px] text-[#6B778C];
}
.pagination > div {
  @apply flex gap-2;
}
.empty {
  @apply flex min-h-[84px] items-center justify-center gap-3 text-[11px] text-[#6B778C];
}
.success,
.error {
  @apply rounded border px-3 py-2 text-[11px];
}
.success {
  @apply border-[#ABF5D1] bg-[#E3FCEF] text-[#006644];
}
.error {
  @apply flex items-center justify-between gap-3 border-[#FFBDAD] bg-[#FFEBE6] text-[#BF2600];
}
</style>
