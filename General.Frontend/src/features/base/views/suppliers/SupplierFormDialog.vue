<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import { CheckCircle2, Save, Send } from 'lucide-vue-next';
import {
  UiButton,
  UiDialog,
  UiFormField,
  UiInput,
  UiSelect,
  UiTabPanel,
  UiTabs,
  UiTabsList,
  UiTabsTrigger,
  UiTextarea,
  type UiSelectOption,
  toast,
} from '@/components/ui';
import type { SupplierDraft, SupplierLevel, SupplierRow } from '../../model/suppliers.types';

const props = defineProps<{ open: boolean; editing: SupplierRow | null }>();
const emit = defineEmits<{
  'update:open': [value: boolean];
  save: [draft: SupplierDraft, submitForApproval: boolean];
}>();

const activeTab = ref('basic');
const draft = ref<SupplierDraft>(blankDraft());

const categoryOptions: UiSelectOption[] = [
  { value: '', label: '请选择分类' },
  { value: '原材料供应商', label: '原材料供应商' },
  { value: '设备供应商', label: '设备供应商' },
  { value: '配件供应商', label: '配件供应商' },
  { value: '服务供应商', label: '服务供应商' },
  { value: 'OEM/代工供应商', label: 'OEM/代工供应商' },
  { value: '贸易商', label: '贸易商' },
];
const ownerOptions: UiSelectOption[] = [
  { value: '', label: '请选择负责人' },
  { value: '张经理', label: '张经理（采购部）' },
  { value: '李工', label: '李工（采购部）' },
  { value: '王工', label: '王工（采购部）' },
  { value: '陈主管', label: '陈主管（供应链部）' },
];
const paymentOptions: UiSelectOption[] = ['月结', '预付', '货到付款', '分期'].map((value) => ({
  value,
  label: value,
}));
const levels: readonly { value: SupplierLevel; label: string }[] = [
  { value: 'A', label: '战略供应商' },
  { value: 'B', label: '重要供应商' },
  { value: 'C', label: '一般供应商' },
  { value: 'D', label: '备选供应商' },
];

const selectedLevelLabel = computed(() => levels.find((item) => item.value === draft.value.level)?.label ?? '');

watch(
  [() => props.open, () => props.editing],
  ([open, editing]) => {
    if (!open) return;
    activeTab.value = 'basic';
    draft.value = editing
      ? {
          name: editing.name,
          category: '',
          owner: editing.owner,
          level: editing.level,
          payMethod: editing.payMethod,
          contact: editing.contact,
          phone: editing.phone,
          email: '',
          paymentDays: String(editing.paymentDays),
          address: '',
          remark: '',
        }
      : blankDraft();
  },
  { immediate: true },
);

function blankDraft(): SupplierDraft {
  return {
    name: '',
    category: '',
    owner: '',
    level: 'B',
    payMethod: '月结',
    contact: '',
    phone: '',
    email: '',
    paymentDays: '30',
    address: '',
    remark: '',
  };
}

function save(submitForApproval: boolean) {
  if (!draft.value.name.trim()) {
    toast.warning('请输入供应商名称');
    activeTab.value = 'basic';
    return;
  }
  if (submitForApproval && (!draft.value.contact.trim() || !draft.value.phone.trim() || !draft.value.owner)) {
    toast.warning('提交审核前请填写联系人、联系电话和负责人');
    activeTab.value = 'basic';
    return;
  }
  emit('save', { ...draft.value }, submitForApproval);
}
</script>

<template>
  <UiDialog
    :open="open"
    :title="editing ? '编辑供应商' : '新增供应商'"
    description="填写供应商信息，提交后进入审批流程"
    size="lg"
    @update:open="emit('update:open', $event)"
  >
    <UiTabs v-model="activeTab">
      <UiTabsList
        :class="$style.tabs"
        label="供应商表单"
      >
        <UiTabsTrigger value="basic">基本信息</UiTabsTrigger>
        <UiTabsTrigger value="approval">审批流程</UiTabsTrigger>
      </UiTabsList>

      <UiTabPanel
        value="basic"
        :class="$style.panel"
      >
        <form
          :class="$style.form"
          @submit.prevent="save(true)"
        >
          <UiFormField
            label="供应商名称"
            required
            wide
          >
            <UiInput
              v-model="draft.name"
              placeholder="请输入供应商公司全称"
            />
          </UiFormField>
          <UiFormField label="供应商分类">
            <UiSelect
              v-model="draft.category"
              :options="categoryOptions"
              label="供应商分类"
            />
          </UiFormField>
          <UiFormField
            label="负责人"
            required
          >
            <UiSelect
              v-model="draft.owner"
              :options="ownerOptions"
              label="负责人"
            />
          </UiFormField>
          <UiFormField
            label="供应商级别"
            wide
          >
            <div :class="$style.levels">
              <button
                v-for="level in levels"
                :key="level.value"
                type="button"
                :class="[
                  $style.level,
                  $style[`level${level.value}`],
                  draft.level === level.value && $style.levelActive,
                ]"
                :aria-pressed="draft.level === level.value"
                @click="draft.level = level.value"
              >
                {{ level.value }}
              </button>
            </div>
            <p :class="$style.levelHint">{{ draft.level }} - {{ selectedLevelLabel }}</p>
          </UiFormField>
          <UiFormField label="付款方式">
            <UiSelect
              v-model="draft.payMethod"
              :options="paymentOptions"
              label="付款方式"
            />
          </UiFormField>
          <UiFormField
            label="联系人"
            required
          >
            <UiInput
              v-model="draft.contact"
              placeholder="联系人姓名"
            />
          </UiFormField>
          <UiFormField
            label="联系电话"
            required
          >
            <UiInput
              v-model="draft.phone"
              placeholder="联系电话"
            />
          </UiFormField>
          <UiFormField label="邮箱">
            <UiInput
              v-model="draft.email"
              type="email"
              placeholder="邮箱地址"
            />
          </UiFormField>
          <UiFormField label="账期（天）">
            <UiInput
              v-model="draft.paymentDays"
              type="number"
              placeholder="30"
            />
          </UiFormField>
          <UiFormField
            label="地址"
            wide
          >
            <UiInput
              v-model="draft.address"
              placeholder="详细地址"
            />
          </UiFormField>
          <UiFormField
            label="备注"
            wide
          >
            <UiTextarea
              v-model="draft.remark"
              rows="2"
              placeholder="备注信息"
            />
          </UiFormField>
        </form>
      </UiTabPanel>

      <UiTabPanel
        value="approval"
        :class="$style.panel"
      >
        <div :class="$style.approvalHeading">
          <h4>审批流程预览</h4>
          <p>提交后将按以下流程进行审批</p>
        </div>
        <ol :class="$style.approvalSteps">
          <li>
            <span>1</span>
            <div>
              <strong>创建人</strong>
              <p>{{ draft.owner || '未选择' }} · 当前</p>
            </div>
          </li>
          <li>
            <span>2</span>
            <div>
              <strong>采购主管审批</strong>
              <p>陈主管 · 待审批</p>
            </div>
          </li>
          <li>
            <span>3</span>
            <div>
              <strong>财务确认</strong>
              <p>财务部 · 待审批</p>
            </div>
          </li>
        </ol>
        <div :class="$style.approvalNotes">
          <strong>审批说明</strong>
          <p><CheckCircle2 :size="12" />新增供应商需经采购主管审批，确认供应商资质</p>
          <p><CheckCircle2 :size="12" />财务部确认银行账户信息准确性</p>
          <p>审批通过前，供应商状态为“待审批”，不可用于采购订单</p>
        </div>
      </UiTabPanel>
    </UiTabs>

    <template #footer>
      <UiButton
        variant="secondary"
        @click="emit('update:open', false)"
        >取消</UiButton
      >
      <UiButton
        variant="secondary"
        @click="save(false)"
      >
        <template #leading
          ><Save
            :size="14"
            :stroke-width="1.8"
        /></template>
        保存草稿
      </UiButton>
      <UiButton
        variant="primary"
        @click="save(true)"
      >
        <template #leading
          ><Send
            :size="14"
            :stroke-width="1.8"
        /></template>
        提交审核
      </UiButton>
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../../../styles/index.css";

.tabs {
  @apply flex w-full border-b px-5 pt-1;
  border-color: var(--ui-border);
}
.panel {
  @apply max-h-[58vh] overflow-y-auto p-5;
}
.form {
  @apply grid grid-cols-2 gap-x-5 gap-y-4;
}
.levels {
  @apply flex gap-2;
}
.level {
  @apply min-h-9 flex-1 rounded border-2 text-[11px] transition-colors;
  border-color: var(--ui-border);
  color: var(--ui-text-subtle);
}
.levelA {
  color: #0052cc;
}
.levelB {
  color: #00875a;
}
.levelC {
  color: #ff8b00;
}
.levelD {
  color: #6b778c;
}
.levelActive {
  @apply font-bold;
  border-color: currentColor;
  background: var(--ui-surface-subtle);
}
.levelHint {
  @apply mt-1 text-[10px];
  color: var(--ui-text-subtle);
}
.approvalHeading h4 {
  @apply text-[14px] font-semibold;
  color: var(--ui-text);
}
.approvalHeading p {
  @apply mt-0.5 text-[11px];
  color: var(--ui-text-subtle);
}
.approvalSteps {
  @apply mt-4 rounded-lg bg-[#F4F5F7] p-5;
}
.approvalSteps li {
  @apply relative flex min-h-[58px] items-start gap-3;
}
.approvalSteps li > span {
  @apply relative z-[1] flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-[#DEEBFF] text-[12px] font-bold;
  color: var(--app-primary);
}
.approvalSteps li:not(:last-child)::before {
  @apply absolute left-[15px] top-8 h-[26px] w-px;
  content: '';
  background: var(--ui-border);
}
.approvalSteps strong {
  @apply block pt-1 text-[13px] font-medium;
  color: var(--ui-text);
}
.approvalSteps p {
  @apply text-[11px];
  color: var(--ui-text-subtle);
}
.approvalNotes {
  @apply mt-4 rounded-lg border bg-white p-4;
  border-color: var(--ui-border);
}
.approvalNotes > strong {
  @apply text-[12px] font-semibold;
  color: var(--ui-text);
}
.approvalNotes p {
  @apply mt-2 flex items-start gap-2 text-[11px];
  color: var(--ui-text-muted);
}
.approvalNotes svg {
  @apply mt-0.5 shrink-0;
  color: var(--ui-success);
}
@media (max-width: 680px) {
  .form {
    @apply grid-cols-1;
  }
}
</style>
