<script setup lang="ts">
import { reactive, watch } from 'vue';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import type { PublicSeaDraft, PublicSeaLevel, PublicSeaSource } from '@/features/crm/model/public-sea.types';

const props = defineProps<{ open: boolean; sources: readonly PublicSeaSource[] }>();
const emit = defineEmits<{ 'update:open': [value: boolean]; save: [draft: PublicSeaDraft] }>();
const draft = reactive<PublicSeaDraft>(emptyDraft());
const levelOptions: UiSelectOption[] = ['A', 'B', 'C'].map((value) => ({ value, label: value }));
const sourceOptions = () => props.sources.map((value) => ({ value, label: value }));

watch(
  () => props.open,
  (open) => {
    if (open) Object.assign(draft, emptyDraft());
  },
);

function emptyDraft(): PublicSeaDraft {
  return {
    customer: '',
    industry: '',
    level: 'B',
    contact: '',
    city: '',
    source: '新建',
    estimatedValue: '',
  };
}
</script>

<template>
  <UiDialog
    :open="open"
    title="新建公海客户"
    description="将待分配的客户资源录入公海池。"
    size="lg"
    @update:open="emit('update:open', $event)"
  >
    <form
      :class="$style.form"
      @submit.prevent="emit('save', { ...draft })"
    >
      <UiFormField
        label="客户名称"
        required
        :class="$style.full"
      >
        <UiInput
          v-model="draft.customer"
          placeholder="请输入客户名称"
          autofocus
        />
      </UiFormField>
      <UiFormField label="行业">
        <UiInput
          v-model="draft.industry"
          placeholder="请输入所属行业"
        />
      </UiFormField>
      <UiFormField label="客户级别">
        <UiSelect
          :model-value="draft.level"
          :options="levelOptions"
          label="客户级别"
          @update:model-value="draft.level = $event as PublicSeaLevel"
        />
      </UiFormField>
      <UiFormField label="联系人">
        <UiInput
          v-model="draft.contact"
          placeholder="请输入联系人"
        />
      </UiFormField>
      <UiFormField label="所在城市">
        <UiInput
          v-model="draft.city"
          placeholder="请输入城市"
        />
      </UiFormField>
      <UiFormField label="来源">
        <UiSelect
          :model-value="draft.source"
          :options="sourceOptions()"
          label="来源"
          @update:model-value="draft.source = $event as PublicSeaSource"
        />
      </UiFormField>
      <UiFormField
        label="预估价值"
        required
      >
        <UiInput
          v-model="draft.estimatedValue"
          type="number"
          min="1"
          placeholder="请输入金额"
        />
      </UiFormField>
    </form>
    <template #footer>
      <UiButton @click="emit('update:open', false)">取消</UiButton>
      <UiButton
        variant="primary"
        @click="emit('save', { ...draft })"
      >
        创建客户
      </UiButton>
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../../../styles/index.css";

.form {
  @apply grid grid-cols-2 gap-4 p-5;
}
.full {
  @apply col-span-2;
}
</style>
