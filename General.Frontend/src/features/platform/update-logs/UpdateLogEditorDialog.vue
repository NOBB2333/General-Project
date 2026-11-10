<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue';
import type { PlatformUpdateLogApi } from '@/api/core/platform-update-log';
import { UiButton, UiDialog, UiFormField, UiInput, UiTextarea } from '@/components/ui';
import {
  createUpdateLogDraft,
  toUpdateLogDraft,
  toUpdateLogSaveInput,
  validateUpdateLogDraft,
  type UpdateLogDraft,
} from './update-log.model';

const props = defineProps<{
  error: string;
  item: null | PlatformUpdateLogApi.Item;
  open: boolean;
  saving: boolean;
}>();

const emit = defineEmits<{
  'update:open': [open: boolean];
  submit: [input: PlatformUpdateLogApi.SaveInput];
}>();

const draft = reactive<UpdateLogDraft>(createUpdateLogDraft());
const localError = ref('');
const isCreating = computed(() => props.item === null);
const shownError = computed(() => localError.value || props.error);

watch(
  () => [props.open, props.item] as const,
  ([open]) => {
    if (!open) return;
    localError.value = '';
    Object.assign(draft, props.item ? toUpdateLogDraft(props.item) : createUpdateLogDraft());
  },
  { immediate: true },
);

function submit() {
  const validationMessage = validateUpdateLogDraft(draft);
  if (validationMessage) {
    localError.value = validationMessage;
    return;
  }
  try {
    localError.value = '';
    emit('submit', toUpdateLogSaveInput(draft));
  } catch (error) {
    localError.value = error instanceof Error ? error.message : '更新内容无效，请检查后重试。';
  }
}
</script>

<template>
  <UiDialog
    :open="open"
    :title="isCreating ? '发布更新公告' : '编辑更新公告'"
    :description="isCreating ? '创建后会立即向全体用户发送更新通知。' : '编辑公告内容不会再次发送通知。'"
    size="lg"
    @update:open="emit('update:open', $event)"
  >
    <form @submit.prevent="submit">
      <UiFormField
        label="版本号"
        required
      >
        <UiInput
          v-model="draft.version"
          maxlength="64"
          autocomplete="off"
          required
        />
      </UiFormField>
      <UiFormField
        label="更新标题"
        required
      >
        <UiInput
          v-model="draft.title"
          maxlength="128"
          autocomplete="off"
          required
        />
      </UiFormField>
      <UiFormField
        label="发布时间"
        required
      >
        <UiInput
          v-model="draft.publishedAt"
          type="datetime-local"
          required
        />
      </UiFormField>
      <UiFormField label="影响范围">
        <UiInput
          v-model="draft.impactScope"
          maxlength="256"
          autocomplete="off"
          placeholder="例如：平台管理员、全部用户"
        />
      </UiFormField>
      <UiFormField
        label="更新内容"
        required
      >
        <UiTextarea
          v-model="draft.summary"
          maxlength="2048"
          rows="8"
          required
        />
      </UiFormField>
      <p
        v-if="shownError"
        role="alert"
      >
        {{ shownError }}
      </p>
    </form>
    <template #footer>
      <UiButton
        :disabled="saving"
        @click="emit('update:open', false)"
        >取消</UiButton
      >
      <UiButton
        variant="primary"
        :loading="saving"
        :disabled="saving"
        @click="submit"
        >{{ isCreating ? '继续发布' : '保存修改' }}</UiButton
      >
    </template>
  </UiDialog>
</template>
