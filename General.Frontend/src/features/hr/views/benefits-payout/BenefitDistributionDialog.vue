<script setup lang="ts">
import { reactive, watch } from 'vue';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, toast, type UiSelectOption } from '@/components/ui';
import type { BenefitDraft } from '../../model/benefits-payout.types';
const props = defineProps<{ open: boolean }>();
const emit = defineEmits<{ 'update:open': [value: boolean]; save: [draft: BenefitDraft] }>();
const kindOptions: readonly UiSelectOption[] = [
  { value: '兑换码', label: '兑换码' },
  { value: '实物', label: '实物' },
];
const draft = reactive<BenefitDraft>(empty());
watch(
  () => props.open,
  (open) => {
    if (open) Object.assign(draft, empty());
  },
);
function empty(): BenefitDraft {
  return {
    title: '',
    kind: '兑换码',
    audience: '全体在职员工',
    source: '',
    people: '1',
    quantityPerPerson: '1',
    totalValue: '',
    note: '',
  };
}
function submit() {
  if (!draft.title.trim() || !draft.source.trim() || Number(draft.totalValue) <= 0) {
    toast.warning('请填写名称、来源和发放总值');
    return;
  }
  emit('save', { ...draft });
}
</script>
<template>
  <UiDialog
    :open="open"
    title="新建发放"
    description="福利发放仅保存到当前本地会话"
    size="lg"
    @update:open="emit('update:open', $event)"
    ><form
      :class="$style.form"
      @submit.prevent="submit"
    >
      <UiFormField
        label="发放名称"
        required
        wide
        ><UiInput
          v-model="draft.title"
          aria-label="发放名称" /></UiFormField
      ><UiFormField
        label="福利类型"
        required
        ><UiSelect
          v-model="draft.kind"
          :options="kindOptions"
          label="福利类型" /></UiFormField
      ><UiFormField
        label="福利来源"
        required
        ><UiInput
          v-model="draft.source"
          aria-label="福利来源" /></UiFormField
      ><UiFormField
        label="发放对象"
        required
        wide
        ><UiInput
          v-model="draft.audience"
          aria-label="发放对象" /></UiFormField
      ><UiFormField
        label="人数"
        required
        ><UiInput
          v-model="draft.people"
          type="number"
          min="1"
          aria-label="发放人数" /></UiFormField
      ><UiFormField
        label="每人数量"
        required
        ><UiInput
          v-model="draft.quantityPerPerson"
          type="number"
          min="1"
          aria-label="每人数量" /></UiFormField
      ><UiFormField
        label="发放总值"
        required
        ><UiInput
          v-model="draft.totalValue"
          type="number"
          min="1"
          aria-label="发放总值" /></UiFormField
      ><UiFormField label="备注"
        ><UiInput
          v-model="draft.note"
          aria-label="发放备注"
      /></UiFormField>
    </form>
    <template #footer
      ><UiButton
        size="sm"
        @click="emit('update:open', false)"
        >取消</UiButton
      ><UiButton
        variant="primary"
        size="sm"
        @click="submit"
        >保存发放</UiButton
      ></template
    ></UiDialog
  >
</template>
<style module>
@reference "../../../../styles/index.css";
.form {
  @apply grid grid-cols-2 gap-4 p-5;
}
</style>
