<script setup lang="ts">
import { ref, watch } from 'vue';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, type UiSelectOption, toast } from '@/components/ui';
import type { PurchaseRequestDraft, PurchaseRequestPriority } from '../../model/purchase-requests.types';

const props = defineProps<{ open: boolean }>();
const emit = defineEmits<{
  'update:open': [value: boolean];
  save: [draft: PurchaseRequestDraft];
}>();

const draft = ref<PurchaseRequestDraft>(blankDraft());
const priorityOptions: UiSelectOption[] = ['高', '中', '低'].map((value) => ({ value, label: value }));
const requestorOptions: UiSelectOption[] = ['张经理', '仓管员', '李工', '王工', '行政'].map((value) => ({
  value,
  label: value,
}));
const supplierOptions: UiSelectOption[] = [
  { value: '-', label: '待询价确认' },
  { value: '威纶通科技', label: '威纶通科技' },
  { value: '西门子中国区代理', label: '西门子中国区代理' },
  { value: '三菱电机自动化', label: '三菱电机自动化' },
  { value: '明纬电源直销', label: '明纬电源直销' },
];

watch(
  () => props.open,
  (open) => {
    if (open) draft.value = blankDraft();
  },
);

function blankDraft(): PurchaseRequestDraft {
  return { title: '', project: '', requestor: '张经理', supplier: '-', priority: '中', amount: '', items: '1' };
}

function save() {
  if (!draft.value.title.trim()) {
    toast.warning('请填写申请标题');
    return;
  }
  emit('save', { ...draft.value });
}
</script>

<template>
  <UiDialog
    :open="open"
    title="新建采购申请"
    description="填写申请信息，记录仅保存在当前本地会话"
    size="lg"
    @update:open="emit('update:open', $event)"
  >
    <form
      :class="$style.form"
      @submit.prevent="save"
    >
      <UiFormField
        label="申请标题"
        required
        wide
      >
        <UiInput
          v-model="draft.title"
          placeholder="请输入采购申请标题"
        />
      </UiFormField>
      <UiFormField label="关联项目">
        <UiInput
          v-model="draft.project"
          placeholder="留空表示库存采购"
        />
      </UiFormField>
      <UiFormField label="供应商">
        <UiSelect
          v-model="draft.supplier"
          :options="supplierOptions"
          label="供应商"
        />
      </UiFormField>
      <UiFormField
        label="申请人"
        required
      >
        <UiSelect
          v-model="draft.requestor"
          :options="requestorOptions"
          label="申请人"
        />
      </UiFormField>
      <UiFormField label="优先级">
        <UiSelect
          :model-value="draft.priority"
          :options="priorityOptions"
          label="优先级"
          @update:model-value="draft.priority = $event as PurchaseRequestPriority"
        />
      </UiFormField>
      <UiFormField label="商品数">
        <UiInput
          v-model="draft.items"
          type="number"
          min="1"
        />
      </UiFormField>
      <UiFormField label="预算金额">
        <UiInput
          v-model="draft.amount"
          type="number"
          min="0"
          placeholder="0"
        />
      </UiFormField>
    </form>
    <template #footer>
      <UiButton @click="emit('update:open', false)">取消</UiButton>
      <UiButton
        variant="primary"
        @click="save"
        >保存草稿</UiButton
      >
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../../../styles/index.css";

.form {
  @apply grid grid-cols-2 gap-4 p-5;
}
</style>
