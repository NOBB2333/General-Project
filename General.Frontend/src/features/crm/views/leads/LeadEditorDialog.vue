<script setup lang="ts">
import { reactive, watch } from 'vue';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect } from '@/components/ui';
import type { LeadDraft, LeadSource } from '@/features/crm/model/leads.types';

const props = defineProps<{ open: boolean; sources: readonly LeadSource[] }>();
const emit = defineEmits<{ 'update:open': [value: boolean]; save: [draft: LeadDraft] }>();
const draft = reactive<LeadDraft>(emptyDraft());
const sourceOptions = () => props.sources.map((value) => ({ value, label: value }));

watch(
  () => props.open,
  (open) => {
    if (open) Object.assign(draft, emptyDraft());
  },
);

function emptyDraft(): LeadDraft {
  return {
    company: '',
    contact: '',
    role: '',
    source: '网站',
    amount: '',
    region: '',
    industry: '',
  };
}
</script>

<template>
  <UiDialog
    :open="open"
    title="新建线索"
    description="录入潜在客户和联系人，进入销售培育流程。"
    size="lg"
    @update:open="emit('update:open', $event)"
  >
    <form
      :class="$style.form"
      @submit.prevent="emit('save', { ...draft })"
    >
      <UiFormField
        label="公司名称"
        required
        :class="$style.full"
      >
        <UiInput
          v-model="draft.company"
          placeholder="请输入公司名称"
          autofocus
        />
      </UiFormField>
      <UiFormField
        label="联系人"
        required
      >
        <UiInput
          v-model="draft.contact"
          placeholder="请输入联系人"
        />
      </UiFormField>
      <UiFormField label="职位">
        <UiInput
          v-model="draft.role"
          placeholder="请输入职位"
        />
      </UiFormField>
      <UiFormField label="线索来源">
        <UiSelect
          :model-value="draft.source"
          :options="sourceOptions()"
          label="线索来源"
          @update:model-value="draft.source = $event as LeadSource"
        />
      </UiFormField>
      <UiFormField
        label="预估金额"
        required
      >
        <UiInput
          v-model="draft.amount"
          type="number"
          min="1"
          placeholder="请输入金额"
        />
      </UiFormField>
      <UiFormField label="地区">
        <UiInput
          v-model="draft.region"
          placeholder="例如：浙江杭州"
        />
      </UiFormField>
      <UiFormField label="行业">
        <UiInput
          v-model="draft.industry"
          placeholder="请输入所属行业"
        />
      </UiFormField>
    </form>
    <template #footer>
      <UiButton @click="emit('update:open', false)">取消</UiButton>
      <UiButton
        variant="primary"
        @click="emit('save', { ...draft })"
      >
        创建线索
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
