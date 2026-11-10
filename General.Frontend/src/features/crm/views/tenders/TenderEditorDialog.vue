<script setup lang="ts">
import { reactive, watch } from 'vue';
import { Gavel } from 'lucide-vue-next';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import type { TenderDraft, TenderPriority, TenderRow, TenderStage } from '@/features/crm/model/tenders.types';

const props = defineProps<{
  open: boolean;
  owners: readonly string[];
  stages: readonly TenderStage[];
  priorities: readonly TenderPriority[];
  methods: readonly TenderRow['method'][];
}>();
const emit = defineEmits<{ 'update:open': [value: boolean]; save: [draft: TenderDraft] }>();

const draft = reactive<TenderDraft>({
  name: '',
  customer: '',
  owner: '张伟',
  stage: '报名中',
  deadline: '',
  openingDate: '',
  depositAmount: '',
  priority: '中',
  method: '公开招标',
});
const asOptions = (values: readonly string[]): UiSelectOption[] => values.map((value) => ({ value, label: value }));

watch(
  () => props.open,
  (open) => {
    if (!open) return;
    Object.assign(draft, {
      name: '',
      customer: '',
      owner: '张伟',
      stage: '报名中',
      deadline: '',
      openingDate: '',
      depositAmount: '',
      priority: '中',
      method: '公开招标',
    });
  },
);
</script>

<template>
  <UiDialog
    :open="open"
    title="新建投标项目"
    description="创建项目后可继续维护文件、任务、保证金和投标结果"
    size="lg"
    @update:open="emit('update:open', $event)"
  >
    <template #icon
      ><span :class="$style.dialogIcon"><Gavel :size="18" /></span
    ></template>
    <form
      id="tender-editor-form"
      :class="$style.form"
      @submit.prevent="emit('save', { ...draft })"
    >
      <UiFormField
        label="项目名称"
        required
        :class="$style.fullField"
      >
        <UiInput
          v-model="draft.name"
          placeholder="请输入投标项目名称"
        />
      </UiFormField>
      <UiFormField
        label="招标单位"
        required
        :class="$style.fullField"
      >
        <UiInput
          v-model="draft.customer"
          placeholder="请输入招标单位"
        />
      </UiFormField>
      <UiFormField label="负责人">
        <UiSelect
          v-model="draft.owner"
          :options="asOptions(owners)"
          label="负责人"
          :class="$style.fullControl"
        />
      </UiFormField>
      <UiFormField label="当前阶段">
        <UiSelect
          v-model="draft.stage"
          :options="asOptions(stages)"
          label="当前阶段"
          :class="$style.fullControl"
        />
      </UiFormField>
      <UiFormField
        label="投标截止"
        required
        ><UiInput
          v-model="draft.deadline"
          type="date"
      /></UiFormField>
      <UiFormField
        label="开标时间"
        required
        ><UiInput
          v-model="draft.openingDate"
          type="date"
      /></UiFormField>
      <UiFormField label="保证金金额"
        ><UiInput
          v-model="draft.depositAmount"
          type="number"
          placeholder="0"
      /></UiFormField>
      <UiFormField label="优先级">
        <UiSelect
          v-model="draft.priority"
          :options="asOptions(priorities)"
          label="优先级"
          :class="$style.fullControl"
        />
      </UiFormField>
      <UiFormField
        label="招标方式"
        :class="$style.fullField"
      >
        <UiSelect
          v-model="draft.method"
          :options="asOptions(methods)"
          label="招标方式"
          :class="$style.fullControl"
        />
      </UiFormField>
    </form>
    <template #footer="{ close }">
      <UiButton @click="close">取消</UiButton>
      <UiButton
        type="submit"
        form="tender-editor-form"
        variant="primary"
        >创建项目</UiButton
      >
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../../../styles/index.css";

.dialogIcon {
  @apply flex h-9 w-9 shrink-0 items-center justify-center rounded;
  color: #6554c0;
  background: #eae6ff;
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
</style>
