<script setup lang="ts">
import { reactive, watch } from 'vue';
import { MessageSquarePlus } from 'lucide-vue-next';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, UiTextarea } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import type { FollowUpDraft, FollowUpType } from '@/features/crm/model/follow-ups.types';

const props = defineProps<{
  open: boolean;
  owners: readonly string[];
  types: readonly FollowUpType[];
}>();
const emit = defineEmits<{
  'update:open': [value: boolean];
  save: [draft: FollowUpDraft];
}>();

const draft = reactive<FollowUpDraft>({
  customer: '',
  contact: '',
  owner: '张明',
  type: '电话',
  content: '',
  result: '',
  nextDate: '',
  nextPlan: '',
});
const ownerOptions: UiSelectOption[] = props.owners.map((value) => ({ value, label: value }));
const typeOptions: UiSelectOption[] = props.types.map((value) => ({ value, label: value }));

watch(
  () => props.open,
  (open) => {
    if (!open) return;
    Object.assign(draft, {
      customer: '',
      contact: '',
      owner: '张明',
      type: '电话',
      content: '',
      result: '',
      nextDate: '',
      nextPlan: '',
    });
  },
);
</script>

<template>
  <UiDialog
    :open="open"
    title="新增跟进"
    description="记录客户沟通内容、结果与下一步安排"
    size="lg"
    @update:open="emit('update:open', $event)"
  >
    <template #icon>
      <span :class="$style.dialogIcon"><MessageSquarePlus :size="18" /></span>
    </template>
    <form
      id="follow-up-editor-form"
      :class="$style.form"
      @submit.prevent="emit('save', { ...draft })"
    >
      <UiFormField
        label="客户"
        required
      >
        <UiInput
          v-model="draft.customer"
          placeholder="请输入客户名称"
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
      <UiFormField label="跟进方式">
        <UiSelect
          v-model="draft.type"
          :options="typeOptions"
          label="跟进方式"
          :class="$style.fullControl"
        />
      </UiFormField>
      <UiFormField label="负责人">
        <UiSelect
          v-model="draft.owner"
          :options="ownerOptions"
          label="负责人"
          :class="$style.fullControl"
        />
      </UiFormField>
      <UiFormField
        label="跟进内容"
        required
        :class="$style.fullField"
      >
        <UiTextarea
          v-model="draft.content"
          placeholder="请输入本次沟通内容"
          :class="$style.textarea"
        />
      </UiFormField>
      <UiFormField
        label="跟进结果"
        :class="$style.fullField"
      >
        <UiTextarea
          v-model="draft.result"
          placeholder="请输入跟进结果"
          :class="$style.textarea"
        />
      </UiFormField>
      <UiFormField label="下次跟进日期">
        <UiInput
          v-model="draft.nextDate"
          type="date"
        />
      </UiFormField>
      <UiFormField label="下一步安排">
        <UiInput
          v-model="draft.nextPlan"
          placeholder="请输入下一步安排"
        />
      </UiFormField>
    </form>
    <template #footer="{ close }">
      <UiButton @click="close">取消</UiButton>
      <UiButton
        type="submit"
        form="follow-up-editor-form"
        variant="primary"
      >
        保存跟进
      </UiButton>
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../../../styles/index.css";

.dialogIcon {
  @apply flex h-9 w-9 shrink-0 items-center justify-center rounded;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.form {
  @apply grid grid-cols-2 gap-4 p-5;
}
.fullField {
  @apply col-span-2;
}
.fullControl {
  @apply w-full;
}
.textarea {
  @apply min-h-[76px];
}
</style>
