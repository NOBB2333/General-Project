<script setup lang="ts">
import { UiButton, UiDialog, UiFormField, UiInput, UiSwitch } from '@/components/ui';
import type { DictDataDraft } from './dict.model';

const open = defineModel<boolean>('open', { required: true });
const draft = defineModel<DictDataDraft>('draft', { required: true });

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
    :title="mode === 'create' ? '新增字典项' : '编辑字典项'"
    description="字典值在同一类型内唯一；停用项仍保留在维护列表中，但不会出现在公共选项接口。"
    size="md"
  >
    <form
      :class="$style.form"
      @submit.prevent="emit('submit')"
    >
      <UiFormField
        label="显示名称"
        required
      >
        <UiInput
          v-model="draft.label"
          autofocus
          maxlength="128"
          placeholder="例如：进行中"
        />
      </UiFormField>
      <UiFormField
        label="字典值"
        required
      >
        <UiInput
          v-model="draft.value"
          maxlength="128"
          placeholder="例如：processing"
        />
      </UiFormField>
      <UiFormField label="标签颜色标识">
        <UiInput
          v-model="draft.tagColor"
          maxlength="32"
          placeholder="default / processing / success / warning / error"
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
      <UiFormField label="启用此选项">
        <UiSwitch
          v-model="draft.isEnabled"
          label="启用字典项"
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
          >{{ saving ? '正在保存…' : mode === 'create' ? '新增字典项' : '保存修改' }}</UiButton
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
