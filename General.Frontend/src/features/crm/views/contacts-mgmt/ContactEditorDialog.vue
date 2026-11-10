<script setup lang="ts">
import { reactive, watch } from 'vue';
import { UiButton, UiCheckbox, UiDialog, UiFormField, UiInput, UiSelect, UiTextarea } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import type { ContactDecision, ContactDraft, ContactStatus } from '@/features/crm/model/contacts-mgmt.types';

const props = defineProps<{ open: boolean; owners: readonly string[] }>();
const emit = defineEmits<{ 'update:open': [value: boolean]; save: [draft: ContactDraft] }>();
const draft = reactive<ContactDraft>(emptyDraft());
const companyOptions = [
  '华能电力科技有限公司',
  '中石化仪表部',
  '国电南瑞集团',
  '宝钢工程技术',
  '上海宏控自动化',
  '深圳智控科技',
  '大唐发电设备部',
  '江苏恒源电力',
].map((value) => ({ value, label: value }));
const statusOptions: UiSelectOption[] = ['在职', '已离职', '已跳槽', '停用'].map((value) => ({ value, label: value }));
const decisionOptions: UiSelectOption[] = [
  '第一决策人',
  '关键决策人',
  '技术决策人',
  '财务决策人',
  '影响者',
  '无决策能力',
].map((value) => ({ value, label: value }));
const ownerOptions = () => props.owners.map((value) => ({ value, label: value }));
watch(
  () => props.open,
  (open) => {
    if (open) Object.assign(draft, emptyDraft());
  },
);
function emptyDraft(): ContactDraft {
  return {
    name: '',
    position: '',
    department: '',
    company: '',
    companyExtra: '',
    phone: '',
    landline: '',
    email: '',
    status: '在职',
    decision: '无决策能力',
    owner: props.owners[0] ?? '张伟',
    isPrimary: false,
    note: '',
  };
}
function updateStatus(value: string) {
  draft.status = value as ContactStatus;
}
function updateDecision(value: string) {
  draft.decision = value as ContactDecision;
}
</script>

<template>
  <UiDialog
    :open="open"
    title="新增联系人"
    description="记录客户联系人、任职状态和决策权重。"
    size="lg"
    @update:open="emit('update:open', $event)"
  >
    <form
      :class="$style.form"
      @submit.prevent="emit('save', { ...draft })"
    >
      <UiFormField
        label="姓名"
        required
        ><UiInput
          v-model="draft.name"
          placeholder="请输入联系人姓名"
      /></UiFormField>
      <UiFormField label="职位"
        ><UiInput
          v-model="draft.position"
          placeholder="例如 采购总监"
      /></UiFormField>
      <UiFormField label="部门"
        ><UiInput
          v-model="draft.department"
          placeholder="例如 采购部"
      /></UiFormField>
      <UiFormField label="所属公司"
        ><UiSelect
          v-model="draft.company"
          :options="companyOptions"
          label="所属公司"
          placeholder="选择系统客户"
      /></UiFormField>
      <UiFormField
        label="公司名称（自由填写）"
        wide
        ><UiInput
          v-model="draft.companyExtra"
          placeholder="公司不在系统内时填写"
      /></UiFormField>
      <UiFormField label="手机"
        ><UiInput
          v-model="draft.phone"
          placeholder="请输入手机号"
      /></UiFormField>
      <UiFormField label="座机"
        ><UiInput
          v-model="draft.landline"
          placeholder="请输入座机"
      /></UiFormField>
      <UiFormField label="邮箱"
        ><UiInput
          v-model="draft.email"
          type="email"
          placeholder="请输入邮箱"
      /></UiFormField>
      <UiFormField label="任职状态"
        ><UiSelect
          :model-value="draft.status"
          :options="statusOptions"
          label="任职状态"
          @update:model-value="updateStatus"
      /></UiFormField>
      <UiFormField label="决策权重"
        ><UiSelect
          :model-value="draft.decision"
          :options="decisionOptions"
          label="决策权重"
          @update:model-value="updateDecision"
      /></UiFormField>
      <UiFormField label="维护人"
        ><UiSelect
          v-model="draft.owner"
          :options="ownerOptions()"
          label="维护人"
      /></UiFormField>
      <label :class="$style.checkboxField"
        ><UiCheckbox
          v-model="draft.isPrimary"
          label="主要联系人"
        /><span>设为该公司主要联系人</span></label
      >
      <UiFormField
        label="备注"
        wide
        ><UiTextarea
          v-model="draft.note"
          :class="$style.note"
          placeholder="补充联系人备注"
      /></UiFormField>
    </form>
    <template #footer
      ><UiButton @click="emit('update:open', false)">取消</UiButton
      ><UiButton
        variant="primary"
        @click="emit('save', { ...draft })"
        >保存</UiButton
      ></template
    >
  </UiDialog>
</template>

<style module>
@reference "../../../../styles/index.css";
.form {
  @apply grid grid-cols-2 gap-4 p-5;
}
.checkboxField {
  @apply col-span-2 flex items-center gap-2 text-[12px];
  color: var(--ui-text-muted);
}
.note {
  min-height: 72px;
}
</style>
