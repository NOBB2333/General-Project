<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import type { OrganizationApi } from '@/api/core/organization';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, type UiSelectOption } from '@/components/ui';

type DialogMode = 'create' | 'edit' | 'move';

const props = defineProps<{
  errorMessage: string;
  mode: DialogMode;
  open: boolean;
  parentOptions: readonly UiSelectOption[];
  saving: boolean;
  unit: null | OrganizationApi.TreeItem;
}>();

const emit = defineEmits<{
  'update:open': [value: boolean];
  move: [input: OrganizationApi.MoveInput];
  save: [input: OrganizationApi.SaveInput];
}>();

const displayName = ref('');
const parentId = ref('');
const localError = ref('');

const title = computed(() => {
  if (props.mode === 'create') return '新增组织';
  if (props.mode === 'move') return '移动组织';
  return '编辑组织名称';
});

const description = computed(() => {
  if (props.mode === 'move') return '选择新的上级组织，保存后服务端会校验组织层级规则。';
  if (props.mode === 'create') return '新组织会保存到当前选择的上级组织下。';
  return '组织名称最多 64 个字符。';
});

watch(
  () => [props.open, props.mode, props.unit] as const,
  ([open, mode, unit]) => {
    if (!open) return;
    displayName.value = mode === 'edit' ? (unit?.displayName ?? '') : '';
    parentId.value = mode === 'create' ? (unit?.id ?? '') : (unit?.parentId ?? '');
    localError.value = '';
  },
  { immediate: true },
);

function submit() {
  localError.value = '';
  if (props.mode === 'move') {
    emit('move', { parentId: parentId.value || null });
    return;
  }

  const normalizedName = displayName.value.trim();
  if (!normalizedName) {
    localError.value = '请输入组织名称。';
    return;
  }
  if (normalizedName.length > 64) {
    localError.value = '组织名称不能超过 64 个字符。';
    return;
  }

  emit('save', {
    displayName: normalizedName,
    parentId: props.mode === 'create' ? parentId.value || null : (props.unit?.parentId ?? null),
  });
}
</script>

<template>
  <UiDialog
    :open="open"
    :title="title"
    :description="description"
    size="md"
    @update:open="emit('update:open', $event)"
  >
    <form
      id="organization-node-form"
      :class="$style.form"
      @submit.prevent="submit"
    >
      <UiFormField
        v-if="mode !== 'move'"
        label="组织名称"
        for="organization-display-name"
        required
        :error="localError"
      >
        <UiInput
          id="organization-display-name"
          v-model="displayName"
          aria-label="组织名称"
          autocomplete="organization-title"
          maxlength="64"
          placeholder="请输入组织名称"
          required
          :invalid="Boolean(localError)"
        />
      </UiFormField>
      <UiFormField
        v-if="mode !== 'edit'"
        label="上级组织"
        for="organization-parent"
      >
        <UiSelect
          id="organization-parent"
          v-model="parentId"
          label="上级组织"
          :options="parentOptions"
          :disabled="saving"
          placeholder="选择顶级组织"
        />
      </UiFormField>
      <p
        v-if="errorMessage"
        :class="$style.error"
        role="alert"
      >
        {{ errorMessage }}
      </p>
    </form>
    <template #footer>
      <UiButton
        variant="secondary"
        :disabled="saving"
        @click="emit('update:open', false)"
        >取消</UiButton
      >
      <UiButton
        form="organization-node-form"
        type="submit"
        variant="primary"
        :loading="saving"
        >保存</UiButton
      >
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../../styles/index.css";

.form {
  @apply grid gap-4 p-5;
}
.error {
  @apply m-0 border px-3 py-2 text-[12px];
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
  border-color: color-mix(in srgb, var(--ui-danger) 25%, transparent);
  border-radius: var(--app-control-radius);
}
</style>
