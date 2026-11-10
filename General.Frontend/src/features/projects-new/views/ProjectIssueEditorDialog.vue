<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue';
import { createProjectIssueApi, updateProjectIssueApi, type ProjectApi } from '@/api/core/project';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, UiTextarea } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import {
  buildProjectIssueSaveInput,
  createProjectIssueForm,
  projectIssueFormFromItem,
  type ProjectIssueForm,
} from '../model/project-issues';

const props = defineProps<{
  detail: ProjectApi.Detail;
  issue: null | ProjectApi.IssueItem;
  open: boolean;
  projectId: string;
}>();

const emit = defineEmits<{
  'update:open': [value: boolean];
  saved: [];
}>();

const form = reactive<ProjectIssueForm>(createProjectIssueForm());
const saving = ref(false);
const error = ref('');

const userOptions = computed<UiSelectOption[]>(() => {
  const options = new Map<string, string>();
  for (const member of props.detail.members) {
    if (member.isActive) options.set(member.userId, displayName(member.userName));
  }
  const issue = props.issue;
  if (issue) {
    addCurrentUserOption(options, issue.ownerUserId, issue.ownerName);
    addCurrentUserOption(options, issue.productOwnerUserId, issue.productOwnerName);
    addCurrentUserOption(options, issue.developerOwnerUserId, issue.developerOwnerName);
    addCurrentUserOption(options, issue.testerOwnerUserId, issue.testerOwnerName);
  }
  return [...options].map(([value, label]) => ({ value, label }));
});

const ownerOptions = computed<UiSelectOption[]>(() => [
  { value: '', label: userOptions.value.length ? '请选择问题负责人' : '当前范围没有可选项目成员' },
  ...userOptions.value,
]);

const optionalOwnerOptions = computed<UiSelectOption[]>(() => [{ value: '', label: '未指定' }, ...userOptions.value]);

const priorityOptions: UiSelectOption[] = [
  { value: 'P0', label: 'P0' },
  { value: 'P1', label: 'P1' },
  { value: 'P2', label: 'P2' },
  { value: 'P3', label: 'P3' },
];

const acceptanceCriteriaOptions = computed<UiSelectOption[]>(() => [
  { value: '', label: '未关联' },
  ...props.detail.acceptanceCriteria.map((item) => ({
    value: item.id,
    label: item.title || item.name || item.id,
  })),
]);

const qualityChecklistOptions = computed<UiSelectOption[]>(() => [
  { value: '', label: '未关联' },
  ...props.detail.qualityChecklists.map((item) => ({
    value: item.id,
    label: item.title || item.id,
  })),
]);

watch(
  () => [props.open, props.issue?.id] as const,
  ([open]) => {
    if (!open) return;
    Object.assign(
      form,
      props.issue
        ? projectIssueFormFromItem(props.issue)
        : createProjectIssueForm(props.detail.members.find((member) => member.isActive)?.userId ?? ''),
    );
    error.value = '';
  },
);

function updateDialogOpen(value: boolean) {
  if (saving.value) return;
  emit('update:open', value);
}

async function saveIssue() {
  if (!form.title.trim() || !form.type.trim() || !form.level.trim() || !form.status.trim() || !form.ownerUserId) {
    error.value = '请填写标题、类型、等级、状态并选择负责人。';
    return;
  }

  saving.value = true;
  error.value = '';
  try {
    const input = buildProjectIssueSaveInput(form);
    if (props.issue) await updateProjectIssueApi(props.issue.id, input);
    else await createProjectIssueApi(props.projectId, input);
    emit('update:open', false);
    emit('saved');
  } catch (cause) {
    error.value = cause instanceof Error ? cause.message : '问题保存失败，请检查信息后重试。';
  } finally {
    saving.value = false;
  }
}

function addCurrentUserOption(options: Map<string, string>, userId: null | string, userName: null | string) {
  if (userId && !options.has(userId)) options.set(userId, displayName(userName));
}

function displayName(value: null | string): string {
  return value && value !== '-' && value !== '—' ? value : '未提供姓名';
}
</script>

<template>
  <UiDialog
    :open="open"
    :title="issue ? '编辑项目问题' : '新增项目问题'"
    description="负责人和质量关联由后端按当前项目校验。"
    @update:open="updateDialogOpen"
  >
    <form
      :class="$style.form"
      @submit.prevent="saveIssue"
    >
      <UiFormField
        label="问题标题"
        required
      >
        <UiInput
          v-model="form.title"
          :disabled="saving"
          maxlength="256"
        />
      </UiFormField>
      <UiFormField
        label="类型"
        hint="类型使用当前后端记录值；没有固定类型目录接口。"
        required
      >
        <UiInput
          v-model="form.type"
          :disabled="saving"
          maxlength="32"
        />
      </UiFormField>
      <UiFormField
        label="等级"
        required
      >
        <UiSelect
          v-model="form.level"
          label="问题等级"
          :options="priorityOptions"
          :disabled="saving"
        />
      </UiFormField>
      <UiFormField
        label="状态"
        hint="按后端现有状态文本填写，不在前端声明状态机。"
        required
      >
        <UiInput
          v-model="form.status"
          :disabled="saving"
          maxlength="32"
        />
      </UiFormField>
      <UiFormField
        label="负责人"
        required
      >
        <UiSelect
          v-model="form.ownerUserId"
          label="问题负责人"
          :options="ownerOptions"
          :disabled="saving || userOptions.length === 0"
        />
      </UiFormField>
      <UiFormField label="到期日期">
        <UiInput
          v-model="form.dueDate"
          type="date"
          :disabled="saving"
        />
      </UiFormField>
      <UiFormField label="产品负责人">
        <UiSelect
          v-model="form.productOwnerUserId"
          label="产品负责人"
          :options="optionalOwnerOptions"
          :disabled="saving"
        />
      </UiFormField>
      <UiFormField label="开发负责人">
        <UiSelect
          v-model="form.developerOwnerUserId"
          label="开发负责人"
          :options="optionalOwnerOptions"
          :disabled="saving"
        />
      </UiFormField>
      <UiFormField label="测试负责人">
        <UiSelect
          v-model="form.testerOwnerUserId"
          label="测试负责人"
          :options="optionalOwnerOptions"
          :disabled="saving"
        />
      </UiFormField>
      <UiFormField label="关联需求标题">
        <UiInput
          v-model="form.requirementTitle"
          :disabled="saving"
          maxlength="256"
        />
      </UiFormField>
      <UiFormField label="验收标准">
        <UiSelect
          v-model="form.acceptanceCriteriaId"
          label="关联验收标准"
          :options="acceptanceCriteriaOptions"
          :disabled="saving"
        />
      </UiFormField>
      <UiFormField label="质量检查单">
        <UiSelect
          v-model="form.qualityChecklistId"
          label="关联质量检查单"
          :options="qualityChecklistOptions"
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
          :disabled="userOptions.length === 0"
        >
          {{ issue ? '保存问题' : '创建问题' }}
        </UiButton>
        <UiButton
          type="button"
          variant="secondary"
          :disabled="saving"
          @click="updateDialogOpen(false)"
        >
          取消
        </UiButton>
      </div>
    </form>
  </UiDialog>
</template>

<style module>
@reference "../../../styles/index.css";

.form {
  @apply grid gap-3 sm:grid-cols-2;
}

.error {
  @apply rounded border px-3 py-2 text-[11px] sm:col-span-2;
  border-color: var(--ui-danger-border);
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}

.actions {
  @apply flex justify-end gap-2 pt-2 sm:col-span-2;
}
</style>
