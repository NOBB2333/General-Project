<script setup lang="ts">
import { reactive, ref, watch } from 'vue';
import { createProjectQualityChecklistApi, type ProjectApi } from '@/api/core/project';
import { UiButton, UiDialog, UiFormField, UiInput, UiTextarea } from '@/components/ui';
import {
  buildProjectQualityChecklistSaveInput,
  createProjectQualityChecklistForm,
  type ProjectQualityChecklistForm,
} from '../model/project-quality';

const props = defineProps<{
  open: boolean;
  projectId: string;
}>();

const emit = defineEmits<{
  'update:open': [value: boolean];
  saved: [];
}>();

const form = reactive<ProjectQualityChecklistForm>(createProjectQualityChecklistForm());
const saving = ref(false);
const error = ref('');

watch(
  () => props.open,
  (open) => {
    if (!open) return;
    Object.assign(form, createProjectQualityChecklistForm());
    error.value = '';
  },
);

function updateDialogOpen(value: boolean) {
  if (saving.value) return;
  emit('update:open', value);
}

async function saveChecklist() {
  const input = buildProjectQualityChecklistSaveInput(form);
  if (!input.title || !input.category || !input.description) {
    error.value = '请填写检查单名称、类别和说明。';
    return;
  }

  saving.value = true;
  error.value = '';
  try {
    await createProjectQualityChecklistApi(props.projectId, input);
    emit('update:open', false);
    emit('saved');
  } catch (cause) {
    error.value = cause instanceof Error ? cause.message : '质量检查单创建失败，请重试。';
  } finally {
    saving.value = false;
  }
}
</script>

<template>
  <UiDialog
    :open="open"
    title="新建质量检查单"
    description="检查单名称在当前项目内需唯一；检查单保存后可用于登记实际检查记录。"
    @update:open="updateDialogOpen"
  >
    <form
      :class="$style.form"
      @submit.prevent="saveChecklist"
    >
      <UiFormField label="检查单名称">
        <UiInput
          v-model="form.title"
          :disabled="saving"
          maxlength="128"
          required
        />
      </UiFormField>
      <UiFormField label="类别">
        <UiInput
          v-model="form.category"
          :disabled="saving"
          maxlength="64"
          required
        />
      </UiFormField>
      <UiFormField
        label="说明"
        wide
      >
        <UiTextarea
          v-model="form.description"
          :disabled="saving"
          maxlength="1024"
          rows="4"
          required
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
          保存检查单
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
  border-color: var(--ui-border);
  color: var(--ui-danger);
}

.actions {
  @apply flex justify-end gap-2 pt-2 sm:col-span-2;
}
</style>
