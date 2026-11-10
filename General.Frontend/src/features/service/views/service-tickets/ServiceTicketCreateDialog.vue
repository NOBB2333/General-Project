<script setup lang="ts">
import { reactive, watch } from 'vue';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, UiTextarea } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import type { ServiceTicketDraft } from '../../model/service-tickets.types';

const props = defineProps<{
  open: boolean;
}>();

const emit = defineEmits<{
  'update:open': [value: boolean];
  save: [draft: ServiceTicketDraft];
}>();

const typeOptions: UiSelectOption[] = [
  '售后报修',
  '安装调试',
  '巡检保养',
  '备件更换',
  '有偿服务',
  '远程支持',
  '客户投诉',
  '其他服务',
].map((value) => ({ value, label: value }));
const urgencyOptions: UiSelectOption[] = ['紧急', '高', '中', '低'].map((value) => ({ value, label: value }));

const draft = reactive<ServiceTicketDraft>(emptyDraft());

watch(
  () => props.open,
  (open) => {
    if (open) Object.assign(draft, emptyDraft());
  },
);

function emptyDraft(): ServiceTicketDraft {
  return {
    title: '',
    type: '售后报修',
    customer: '',
    contact: '',
    product: '',
    serialNumber: '',
    urgency: '中',
    description: '',
  };
}

function save() {
  emit('save', { ...draft });
}
</script>

<template>
  <UiDialog
    :open="open"
    title="新建工单"
    description="新工单仅保存在当前本地复刻会话，不会请求外部参考站。"
    size="lg"
    @update:open="emit('update:open', $event)"
  >
    <form
      :class="$style.formGrid"
      @submit.prevent="save"
    >
      <UiFormField
        label="工单标题"
        required
        wide
      >
        <UiInput
          v-model="draft.title"
          placeholder="请输入服务问题或任务标题"
        />
      </UiFormField>
      <UiFormField label="服务类型">
        <UiSelect
          v-model="draft.type"
          :options="typeOptions"
          label="服务类型"
        />
      </UiFormField>
      <UiFormField label="紧急度">
        <UiSelect
          v-model="draft.urgency"
          :options="urgencyOptions"
          label="紧急度"
        />
      </UiFormField>
      <UiFormField
        label="客户"
        required
      >
        <UiInput
          v-model="draft.customer"
          placeholder="客户名称"
        />
      </UiFormField>
      <UiFormField label="联系人">
        <UiInput
          v-model="draft.contact"
          placeholder="联系人"
        />
      </UiFormField>
      <UiFormField label="产品">
        <UiInput
          v-model="draft.product"
          placeholder="产品或型号"
        />
      </UiFormField>
      <UiFormField label="产品 SN">
        <UiInput
          v-model="draft.serialNumber"
          placeholder="序列号"
        />
      </UiFormField>
      <UiFormField
        label="问题描述"
        wide
      >
        <UiTextarea
          v-model="draft.description"
          :class="$style.descriptionInput"
          rows="4"
          placeholder="描述故障现象、影响范围或服务要求"
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
        >保存工单</UiButton
      >
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../../../styles/index.css";

.formGrid {
  @apply grid grid-cols-2 gap-4 p-5;
}

.descriptionInput {
  min-height: 86px;
}

@media (max-width: 620px) {
  .formGrid {
    @apply grid-cols-1;
  }
}
</style>
