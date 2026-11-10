<script setup lang="ts">
import { UiButton, UiDialog, UiFormField, UiInput } from '@/components/ui';
import type { DictTypeDraft } from './dict.model';

const open = defineModel<boolean>('open', { required: true });
const draft = defineModel<DictTypeDraft>('draft', { required: true });

defineProps<{
  error: string;
  mode: 'create' | 'edit';
  saving: boolean;
}>();

const emit = defineEmits<{ submit: [] }>();
</script>

<template>
  <UiDialog
    v-model:open="open"
    :title="mode === 'create' ? '新建字典类型' : '编辑字典类型'"
    :description="mode === 'create' ? '字典编码创建后不可修改。' : '编码由后端维护且不可修改。'"
    size="md"
  >
    <form
      :class="$style.form"
      @submit.prevent="emit('submit')"
    >
      <UiFormField
        label="字典编码"
        required
        :hint="mode === 'edit' ? '编码创建后不可修改。' : '保存时会转换为小写。'"
      >
        <UiInput
          v-model="draft.code"
          :disabled="mode === 'edit'"
          autofocus
          maxlength="64"
          placeholder="例如 project_status"
        />
      </UiFormField>
      <UiFormField
        label="字典名称"
        required
      >
        <UiInput
          v-model="draft.name"
          maxlength="128"
          placeholder="请输入字典名称"
        />
      </UiFormField>
      <UiFormField label="排序值">
        <UiInput
          v-model="draft.sort"
          type="number"
          min="0"
          max="2147483647"
          step="1"
        />
      </UiFormField>
      <UiFormField
        label="备注"
        wide
      >
        <UiInput
          v-model="draft.remark"
          maxlength="256"
          placeholder="可选"
        />
      </UiFormField>
      <p
        v-if="error"
        :class="$style.formError"
        role="alert"
      >
        {{ error }}
      </p>
      <div :class="$style.actions">
        <UiButton
          :disabled="saving"
          @click="open = false"
          >取消</UiButton
        >
        <UiButton
          type="submit"
          variant="primary"
          :loading="saving"
          >{{ saving ? '正在保存…' : mode === 'create' ? '创建字典' : '保存修改' }}</UiButton
        >
      </div>
    </form>
  </UiDialog>
</template>

<style module>
@reference "../../../styles/index.css";

.form {
  @apply grid grid-cols-2 gap-4 p-5;
}
.formError,
.actions {
  @apply col-span-2;
}
.formError {
  @apply m-0 text-[11px];
  color: var(--ui-danger);
}
.actions {
  @apply mt-1 flex justify-end gap-2 border-t pt-4;
  border-color: var(--ui-border-subtle);
}
</style>
