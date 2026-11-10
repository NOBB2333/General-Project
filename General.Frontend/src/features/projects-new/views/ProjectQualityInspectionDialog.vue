<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue';
import { createProjectQualityInspectionApi, type ProjectApi } from '@/api/core/project';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, UiTextarea } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import {
  buildProjectQualityInspectionSaveInput,
  createProjectQualityInspectionForm,
  type ProjectQualityInspectionForm,
} from '../model/project-quality';

const props = defineProps<{
  detail: ProjectApi.Detail;
  open: boolean;
  projectId: string;
}>();

const emit = defineEmits<{
  'update:open': [value: boolean];
  saved: [];
}>();

const form = reactive<ProjectQualityInspectionForm>(createProjectQualityInspectionForm());
const saving = ref(false);
const error = ref('');

const checklistOptions = computed<UiSelectOption[]>(() => [
  { value: '', label: '请选择质量检查单' },
  ...props.detail.qualityChecklists.map((item) => ({
    value: item.id,
    label: `${item.category} · ${item.title}`,
  })),
]);

const inspectorOptions = computed<UiSelectOption[]>(() => [
  { value: '', label: '请选择检查人' },
  ...props.detail.members
    .filter((member) => member.isActive)
    .map((member) => ({
      value: member.userId,
      label: member.userName && member.userName !== '-' ? member.userName : '项目成员（姓名未提供）',
    })),
]);

const canSubmit = computed(() => props.detail.qualityChecklists.length > 0 && inspectorOptions.value.length > 1);

watch(
  () => props.open,
  (open) => {
    if (!open) return;
    Object.assign(form, createProjectQualityInspectionForm());
    error.value = '';
  },
);

function updateDialogOpen(value: boolean) {
  if (saving.value) return;
  emit('update:open', value);
}

async function saveInspection() {
  const input = buildProjectQualityInspectionSaveInput(form);
  if (!input.qualityChecklistId || !input.inspectorUserId || !input.inspectionResult) {
    error.value = '请选择检查单和检查人，并填写检查结果。';
    return;
  }
  if (!canSubmit.value) {
    error.value = '当前详情没有可选的在岗项目成员或检查单，请刷新后重试。';
    return;
  }

  saving.value = true;
  error.value = '';
  try {
    await createProjectQualityInspectionApi(props.projectId, input);
    emit('update:open', false);
    emit('saved');
  } catch (cause) {
    error.value = cause instanceof Error ? cause.message : '质量检查记录创建失败，请重试。';
  } finally {
    saving.value = false;
  }
}
</script>

<template>
  <UiDialog
    :open="open"
    title="登记质量检查"
    description="每次检查都会追加一条记录；整改后可再次登记复检，原检查历史保留。"
    @update:open="updateDialogOpen"
  >
    <form
      :class="$style.form"
      @submit.prevent="saveInspection"
    >
      <UiFormField
        label="质量检查单"
        wide
      >
        <UiSelect
          v-model="form.qualityChecklistId"
          label="质量检查单"
          :options="checklistOptions"
          :disabled="saving || detail.qualityChecklists.length === 0"
        />
      </UiFormField>
      <UiFormField label="检查人">
        <UiSelect
          v-model="form.inspectorUserId"
          label="检查人"
          :options="inspectorOptions"
          :disabled="saving || inspectorOptions.length <= 1"
        />
      </UiFormField>
      <UiFormField label="检查时间">
        <UiInput
          v-model="form.inspectionTime"
          type="datetime-local"
          :disabled="saving"
        />
        <template #description>留空时由后端记录当前时间。</template>
      </UiFormField>
      <UiFormField label="检查结果">
        <UiInput
          v-model="form.inspectionResult"
          :disabled="saving"
          maxlength="32"
          required
        />
      </UiFormField>
      <UiFormField
        label="发现摘要"
        wide
      >
        <UiTextarea
          v-model="form.findingSummary"
          :disabled="saving"
          rows="3"
        />
      </UiFormField>
      <UiFormField
        label="整改要求"
        wide
      >
        <UiTextarea
          v-model="form.rectificationRequirement"
          :disabled="saving"
          rows="3"
        />
      </UiFormField>
      <p
        v-if="!canSubmit"
        :class="$style.note"
      >
        {{ detail.qualityChecklists.length === 0 ? '请先创建质量检查单。' : '详情中没有可选的在岗项目成员。' }}
      </p>
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
          :disabled="!canSubmit"
        >
          保存检查记录
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

.note,
.error {
  @apply rounded border px-3 py-2 text-[11px] sm:col-span-2;
  border-color: var(--ui-border);
}

.error {
  color: var(--ui-danger);
}

.actions {
  @apply flex justify-end gap-2 pt-2 sm:col-span-2;
}
</style>
