<script setup lang="ts">
import { reactive, watch } from 'vue';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, UiTextarea } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import type { CustomerFollowUpDraft, CustomerRow } from '@/features/crm/model/customers.types';

const props = defineProps<{ open: boolean; customer: CustomerRow | null }>();
const emit = defineEmits<{
  'update:open': [value: boolean];
  save: [draft: CustomerFollowUpDraft];
}>();

const methodOptions: UiSelectOption[] = ['电话', '拜访', '微信', '邮件', '会议', '其他'].map((method) => ({
  value: method,
  label: method,
}));
const draft = reactive<CustomerFollowUpDraft>(emptyDraft());

watch(
  () => props.open,
  (open) => {
    if (open) Object.assign(draft, emptyDraft());
  },
);

function emptyDraft(): CustomerFollowUpDraft {
  return { method: '电话', content: '', result: '', nextDate: '' };
}
</script>

<template>
  <UiDialog
    :open="open"
    title="新建跟进"
    :description="customer ? `${customer.name} · ${customer.contact}` : undefined"
    size="lg"
    @update:open="emit('update:open', $event)"
  >
    <form
      :class="$style.form"
      @submit.prevent="emit('save', { ...draft })"
    >
      <UiFormField label="跟进方式">
        <UiSelect
          v-model="draft.method"
          :options="methodOptions"
          label="跟进方式"
        />
      </UiFormField>
      <UiFormField label="下次跟进日期">
        <UiInput
          v-model="draft.nextDate"
          type="date"
        />
      </UiFormField>
      <UiFormField
        label="跟进内容"
        required
        wide
      >
        <UiTextarea
          v-model="draft.content"
          :class="$style.textarea"
          placeholder="记录本次沟通要点"
        />
      </UiFormField>
      <UiFormField
        label="跟进结果"
        wide
      >
        <UiInput
          v-model="draft.result"
          placeholder="简要总结客户态度与下一步"
        />
      </UiFormField>
    </form>
    <template #footer>
      <UiButton @click="emit('update:open', false)">取消</UiButton>
      <UiButton
        variant="primary"
        @click="emit('save', { ...draft })"
      >
        保存跟进
      </UiButton>
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../../../styles/index.css";

.form {
  @apply grid grid-cols-2 gap-4 p-5;
}

.textarea {
  min-height: 112px;
}
</style>
