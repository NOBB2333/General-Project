<script setup lang="ts">
import { computed, ref } from 'vue';
import { createProjectTaskDependencyApi, type ProjectApi } from '@/api/core/project';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import { hasAccessCode } from '@/session/access-store';

const props = defineProps<{
  dependencies: ProjectApi.TaskDependency[];
  projectId: string;
  tasks: ProjectApi.TaskItem[];
}>();

const emit = defineEmits<{ changed: [] }>();

const canManageTasks = computed(() => hasAccessCode('Project.Task.Manage'));
const dependencyOpen = ref(false);
const saving = ref(false);
const error = ref('');
const notice = ref('');
const predecessorTaskId = ref('');
const successorTaskId = ref('');
const taskOptions = computed<UiSelectOption[]>(() => {
  const tasks = new Map<string, ProjectApi.TaskItem>();
  for (const task of props.tasks) tasks.set(task.id, task);
  return [
    { value: '', label: '请选择任务' },
    ...[...tasks.values()].map((task) => ({ value: task.id, label: `${task.taskCode} · ${task.title}` })),
  ];
});

function openForm() {
  predecessorTaskId.value = '';
  successorTaskId.value = '';
  error.value = '';
  notice.value = '';
  dependencyOpen.value = true;
}

function closeForm() {
  if (saving.value) return;
  dependencyOpen.value = false;
}

function updateDialogOpen(value: boolean) {
  if (!value) closeForm();
  else dependencyOpen.value = true;
}

async function createDependency() {
  if (!predecessorTaskId.value || !successorTaskId.value) {
    error.value = '请选择前置任务和后续任务。';
    return;
  }
  if (predecessorTaskId.value === successorTaskId.value) {
    error.value = '前置任务和后续任务不能相同。';
    return;
  }

  saving.value = true;
  error.value = '';
  notice.value = '';
  try {
    await createProjectTaskDependencyApi(props.projectId, {
      dependencyType: 'FinishToStart',
      predecessorTaskId: predecessorTaskId.value,
      successorTaskId: successorTaskId.value,
    });
    dependencyOpen.value = false;
    notice.value = '任务依赖已创建。';
    emit('changed');
  } catch (cause) {
    error.value = cause instanceof Error ? cause.message : '任务依赖创建失败，请重试。';
  } finally {
    saving.value = false;
  }
}

function formatDependencyType(value: string): string {
  return value === 'FinishToStart' ? '完成后开始（FinishToStart）' : value;
}
</script>

<template>
  <section :class="$style.section">
    <header :class="$style.sectionHeader">
      <div>
        <h3 :class="$style.heading">任务依赖</h3>
        <p :class="$style.description">关系由后端校验是否重复、跨项目或形成循环；当前接口没有删除操作。</p>
      </div>
      <UiButton
        v-if="canManageTasks"
        variant="primary"
        size="sm"
        @click="openForm"
      >
        新增任务依赖
      </UiButton>
    </header>
    <p
      v-if="notice"
      role="status"
      :class="$style.success"
    >
      {{ notice }}
    </p>
    <p
      v-if="dependencies.length"
      :class="$style.count"
    >
      {{ dependencies.length }} 条当前权限范围内的依赖
    </p>
    <div
      v-if="dependencies.length"
      :class="$style.dependencyList"
    >
      <article
        v-for="item in dependencies"
        :key="item.id"
        :class="$style.dependency"
      >
        <span>{{ item.predecessorTaskTitle }}</span>
        <strong>{{ formatDependencyType(item.dependencyType) }}</strong>
        <span>{{ item.successorTaskTitle }}</span>
      </article>
    </div>
    <p
      v-else
      :class="$style.empty"
    >
      当前权限范围内没有可见的任务依赖。
    </p>
  </section>

  <UiDialog
    :open="dependencyOpen"
    title="新增任务依赖"
    description="依赖关系需要在同一项目内；后端会再次校验循环和重复。"
    @update:open="updateDialogOpen"
  >
    <form
      :class="$style.form"
      @submit.prevent="createDependency"
    >
      <UiFormField
        label="前置任务"
        required
      >
        <UiSelect
          v-model="predecessorTaskId"
          label="前置任务"
          :options="taskOptions"
          :disabled="saving"
        />
      </UiFormField>
      <UiFormField
        label="依赖类型"
        hint="当前后端进度影响计算支持完成-开始关系。"
      >
        <UiInput
          model-value="完成-开始（FinishToStart）"
          disabled
        />
      </UiFormField>
      <UiFormField
        label="后续任务"
        required
      >
        <UiSelect
          v-model="successorTaskId"
          label="后续任务"
          :options="taskOptions"
          :disabled="saving"
        />
      </UiFormField>
      <p
        v-if="error"
        role="alert"
        :class="$style.error"
      >
        {{ error }}
      </p>
      <div :class="$style.actions">
        <UiButton
          type="submit"
          variant="primary"
          :loading="saving"
        >
          创建依赖
        </UiButton>
        <UiButton
          variant="secondary"
          :disabled="saving"
          @click="closeForm"
        >
          取消
        </UiButton>
      </div>
    </form>
  </UiDialog>
</template>

<style module>
@reference "../../../styles/index.css";
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
.count {
  @apply mt-2 text-[10px] text-[#6B778C];
}
.dependencyList {
  @apply mt-3 space-y-2;
}
.dependency {
  @apply grid grid-cols-[1fr_auto_1fr] items-center gap-3 rounded border border-[#DFE1E6] bg-[#FAFBFC] px-3 py-2 text-[11px];
}
.dependency strong {
  @apply text-[#0052CC];
}
.dependency span:last-child {
  @apply text-right;
}
.empty {
  @apply mt-3 border-t border-[#F4F5F7] pt-3 text-[11px] text-[#6B778C];
}
.form {
  @apply space-y-3;
}
.actions {
  @apply flex justify-end gap-2 pt-2;
}
.success,
.error {
  @apply mt-2 rounded border px-3 py-2 text-[11px];
}
.success {
  @apply border-[#ABF5D1] bg-[#E3FCEF] text-[#006644];
}
.error {
  @apply border-[#FFBDAD] bg-[#FFEBE6] text-[#BF2600];
}
@media (max-width: 720px) {
  .dependency {
    @apply grid-cols-1 gap-1;
  }
  .dependency span:last-child {
    @apply text-left;
  }
}
</style>
