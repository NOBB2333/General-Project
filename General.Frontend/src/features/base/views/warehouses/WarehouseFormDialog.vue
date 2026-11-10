<script setup lang="ts">
import { ref, watch } from 'vue';
import {
  UiButton,
  UiDialog,
  UiFormField,
  UiInput,
  UiSelect,
  UiTextarea,
  type UiSelectOption,
  toast,
} from '@/components/ui';
import type { WarehouseDraft, WarehouseRow } from '../../model/warehouses.types';

const props = defineProps<{ open: boolean; editing: WarehouseRow | null }>();
const emit = defineEmits<{
  'update:open': [value: boolean];
  save: [draft: WarehouseDraft];
}>();

const draft = ref<WarehouseDraft>(blankDraft());
const typeOptions: UiSelectOption[] = ['主仓', '项目仓', '备品仓', '退货仓', '中转仓', '委外仓'].map((value) => ({
  value,
  label: value,
}));

watch(
  [() => props.open, () => props.editing],
  ([open, editing]) => {
    if (!open) return;
    draft.value = editing
      ? {
          name: editing.name,
          type: editing.type,
          address: editing.address,
          manager: editing.manager,
          phone: editing.phone,
          remark: editing.remark ?? '',
        }
      : blankDraft();
  },
  { immediate: true },
);

function blankDraft(): WarehouseDraft {
  return { name: '', type: '主仓', address: '', manager: '', phone: '', remark: '' };
}

function save() {
  if (!draft.value.name.trim() || !draft.value.address.trim()) {
    toast.warning('请填写仓库名称和仓库地址');
    return;
  }
  emit('save', { ...draft.value });
}
</script>

<template>
  <UiDialog
    :open="open"
    :title="editing ? '编辑仓库' : '新增仓库'"
    :description="editing ? '编辑仓库信息' : '填写仓库信息'"
    size="md"
    @update:open="emit('update:open', $event)"
  >
    <form
      :class="$style.form"
      @submit.prevent="save"
    >
      <UiFormField
        label="仓库名称"
        required
        wide
      >
        <UiInput
          v-model="draft.name"
          placeholder="请输入仓库名称"
        />
      </UiFormField>
      <UiFormField
        label="仓库类型"
        required
      >
        <UiSelect
          v-model="draft.type"
          :options="typeOptions"
          label="仓库类型"
        />
      </UiFormField>
      <UiFormField label="负责人">
        <UiInput
          v-model="draft.manager"
          placeholder="仓库负责人"
        />
      </UiFormField>
      <UiFormField label="联系电话">
        <UiInput
          v-model="draft.phone"
          type="tel"
          placeholder="联系电话"
        />
      </UiFormField>
      <UiFormField
        label="仓库地址"
        required
        wide
      >
        <UiInput
          v-model="draft.address"
          placeholder="仓库详细地址"
        />
      </UiFormField>
      <UiFormField
        label="备注"
        wide
      >
        <UiTextarea
          v-model="draft.remark"
          rows="3"
          placeholder="备注信息"
        />
      </UiFormField>
    </form>
    <template #footer>
      <UiButton
        variant="secondary"
        @click="emit('update:open', false)"
        >取消</UiButton
      >
      <UiButton
        variant="primary"
        @click="save"
        >{{ editing ? '保存修改' : '确认创建' }}</UiButton
      >
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../../../styles/index.css";

.form {
  @apply grid grid-cols-2 gap-x-5 gap-y-4 p-6;
}
</style>
