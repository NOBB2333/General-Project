<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue';
import { getProjectDetailSectionApi, recordProjectTaskProgressApi, type ProjectApi } from '@/api/core/project';
import { UiButton, UiDialog } from '@/components/ui';
import ProjectTaskProgressForm from './ProjectTaskProgressForm.vue';
import ProjectTaskWorklogList from './ProjectTaskWorklogList.vue';

const props = defineProps<{
  open: boolean;
  projectId: string;
  task: ProjectApi.TaskItem | null;
}>();

const emit = defineEmits<{
  'update:open': [value: boolean];
  changed: [];
}>();

const detail = ref<ProjectApi.Detail | null>(null);
const loading = ref(false);
const saving = ref(false);
const error = ref('');
const notice = ref('');
const baselineExecution = ref('');
const writeNeedsRefresh = ref(false);
let activeRequest: AbortController | null = null;
let requestVersion = 0;

const progressForm = reactive({
  blockReason: '',
  hours: '1',
  status: '进行中',
  summary: '',
  workDate: localDateInput(new Date()),
});

const visibleTask = computed(() =>
  detail.value ? (detail.value.tasks.find((item) => item.id === props.task?.id) ?? null) : props.task,
);
const taskWorklogs = computed(() => detail.value?.worklogs.filter((item) => item.taskId === props.task?.id) ?? []);
const isBlocked = computed(() => progressForm.status === '阻塞');

watch(
  () => [props.open, props.projectId, props.task?.id] as const,
  ([open, projectId, taskId]) => {
    activeRequest?.abort();
    requestVersion += 1;
    detail.value = null;
    error.value = '';
    notice.value = '';
    writeNeedsRefresh.value = false;
    if (!open || !projectId || !taskId || !props.task) {
      loading.value = false;
      return;
    }
    resetProgressForm(props.task);
    void loadTaskDetail(true);
  },
  { immediate: true },
);

function localDateInput(value: Date): string {
  const year = value.getFullYear();
  const month = String(value.getMonth() + 1).padStart(2, '0');
  const day = String(value.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

function resetProgressForm(task: ProjectApi.TaskItem) {
  Object.assign(progressForm, {
    blockReason: task.blockReason ?? '',
    hours: '1',
    status: task.status,
    summary: '',
    workDate: localDateInput(new Date()),
  });
  baselineExecution.value = executionSignature(task);
}

function executionSignature(task: ProjectApi.TaskItem): string {
  return JSON.stringify([task.status, task.isBlocked, task.blockReason, task.actualWorkHours]);
}

async function loadTaskDetail(resetForm: boolean) {
  if (!props.task || !props.projectId) return;
  activeRequest?.abort();
  const controller = new AbortController();
  activeRequest = controller;
  const currentRequest = ++requestVersion;
  loading.value = true;
  error.value = '';

  try {
    const loaded = await getProjectDetailSectionApi('tasks', props.projectId, controller.signal);
    if (currentRequest !== requestVersion) return;
    detail.value = loaded;
    const task = loaded.tasks.find((item) => item.id === props.task?.id);
    if (!task) {
      error.value = '当前账号已无法读取这条任务，请刷新任务列表后重试。';
      return;
    }
    if (resetForm) {
      resetProgressForm(task);
      writeNeedsRefresh.value = false;
    }
  } catch (cause) {
    if (controller.signal.aborted || currentRequest !== requestVersion) return;
    error.value = cause instanceof Error ? cause.message : '任务详情读取失败，请稍后重试。';
  } finally {
    if (currentRequest === requestVersion) loading.value = false;
  }
}

async function recordProgress() {
  if (!props.task || !progressForm.summary.trim() || !progressForm.workDate) {
    error.value = '请填写进展说明和工作日期。';
    return;
  }

  const hours = Number(progressForm.hours);
  if (!Number.isFinite(hours) || hours <= 0 || hours > 24) {
    error.value = '本次工时需大于 0 且不超过 24 小时。';
    return;
  }
  if (isBlocked.value && !progressForm.blockReason.trim()) {
    error.value = '任务标记为阻塞时必须填写阻塞原因。';
    return;
  }

  saving.value = true;
  error.value = '';
  notice.value = '';
  try {
    const latest = await getProjectDetailSectionApi('tasks', props.projectId);
    const latestTask = latest.tasks.find((item) => item.id === props.task?.id);
    if (!latestTask) {
      detail.value = latest;
      error.value = '当前账号已无法读取这条任务，进展未提交。';
      return;
    }
    detail.value = latest;
    if (executionSignature(latestTask) !== baselineExecution.value) {
      baselineExecution.value = executionSignature(latestTask);
      progressForm.status = latestTask.status;
      progressForm.blockReason = latestTask.blockReason ?? '';
      error.value = '任务执行状态或累计工时已被更新。已读取最新数据，请确认后重新提交；本次工时未写入。';
      return;
    }

    await recordProjectTaskProgressApi(props.task.id, {
      blockReason: progressForm.blockReason.trim(),
      hours,
      isBlocked: isBlocked.value,
      status: progressForm.status,
      summary: progressForm.summary.trim(),
      workDate: `${progressForm.workDate}T00:00:00`,
    });
    writeNeedsRefresh.value = true;
    progressForm.summary = '';
    notice.value = '提交已成功，正在刷新任务与工时记录…';
    emit('changed');
    await loadTaskDetail(true);
    if (error.value) {
      notice.value = '进展已成功登记，但刷新读取失败。请刷新记录后再继续登记，避免重复提交。';
      return;
    }
    notice.value = '进展和当前用户工时已记录。';
  } catch (cause) {
    error.value = cause instanceof Error ? cause.message : '进展提交失败，请检查后重试。';
  } finally {
    saving.value = false;
  }
}

function updateDialogOpen(value: boolean) {
  if (!value && saving.value) return;
  emit('update:open', value);
}

function formatDate(value: null | string): string {
  return value ? value.slice(0, 10) : '—';
}
</script>

<template>
  <UiDialog
    :open="open"
    :title="task ? `${task.taskCode} · ${task.title}` : '任务详情'"
    description="工时按当前登录用户记录；任务状态和累计工时由服务端汇总。"
    size="xl"
    @update:open="updateDialogOpen"
  >
    <div
      v-if="loading && !detail"
      role="status"
      aria-live="polite"
      :class="$style.message"
    >
      正在读取任务与工时记录…
    </div>
    <div
      v-else-if="!detail && error"
      role="alert"
      :class="$style.message"
    >
      <span>{{ error }}</span>
      <UiButton
        variant="secondary"
        size="sm"
        @click="loadTaskDetail(true)"
      >
        重试读取
      </UiButton>
    </div>
    <div
      v-else-if="detail && !visibleTask"
      role="alert"
      :class="$style.message"
    >
      <span>{{ error || '当前账号已无法读取这条任务，请刷新后重试。' }}</span>
      <UiButton
        variant="secondary"
        size="sm"
        @click="loadTaskDetail(true)"
      >
        重试读取
      </UiButton>
    </div>
    <div
      v-else-if="visibleTask"
      :class="$style.content"
    >
      <div
        v-if="error"
        role="alert"
        :class="$style.error"
      >
        {{ error }}
      </div>
      <div
        v-if="notice"
        role="status"
        :class="$style.success"
      >
        {{ notice }}
      </div>

      <section :class="$style.summary">
        <div>
          <span>状态</span><strong>{{ visibleTask.status }}</strong>
        </div>
        <div>
          <span>负责人</span><strong>{{ visibleTask.ownerName }}</strong>
        </div>
        <div>
          <span>计划结束</span><strong>{{ formatDate(visibleTask.plannedEndTime) }}</strong>
        </div>
        <div>
          <span>估算工时</span><strong>{{ visibleTask.estimatedWorkHours ?? '—' }} 小时</strong>
        </div>
        <div>
          <span>累计工时</span><strong>{{ visibleTask.actualWorkHours ?? 0 }} 小时</strong>
        </div>
        <div>
          <span>项目</span><strong>{{ visibleTask.projectName }}</strong>
        </div>
      </section>

      <ProjectTaskWorklogList
        :description="writeNeedsRefresh ? '上一条进展已保存，请刷新后再继续登记。' : '记录显示范围由项目数据权限控制。'"
        empty-hint="当前权限范围内还没有工时记录。"
        :items="taskWorklogs"
        refreshable
        title="工时记录"
        @refresh="loadTaskDetail(writeNeedsRefresh)"
      />

      <ProjectTaskProgressForm
        v-model="progressForm"
        :disabled="loading || writeNeedsRefresh"
        :saving="saving"
        @submit="recordProgress"
      />
    </div>
    <template #footer>
      <UiButton
        variant="secondary"
        :disabled="saving"
        @click="emit('update:open', false)"
      >
        关闭
      </UiButton>
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../../styles/index.css";
.content {
  @apply space-y-4;
}
.message,
.error,
.success {
  @apply flex items-center justify-between gap-3 rounded border px-3 py-2 text-[12px];
}
.message {
  @apply border-dashed text-[#6B778C];
}
.error {
  @apply border-[#FFBDAD] bg-[#FFEBE6] text-[#BF2600];
}
.success {
  @apply border-[#ABF5D1] bg-[#E3FCEF] text-[#006644];
}
.summary {
  @apply grid grid-cols-3 gap-2;
}
.summary > div {
  @apply flex min-h-[62px] flex-col justify-between rounded border border-[#DFE1E6] bg-[#FAFBFC] p-3;
}
.summary span,
.sectionHeader p {
  @apply text-[10px] text-[#6B778C];
}
.summary strong {
  @apply mt-2 text-[12px] font-semibold text-[#172B4D];
}
@media (max-width: 720px) {
  .summary {
    @apply grid-cols-2;
  }
}
</style>
