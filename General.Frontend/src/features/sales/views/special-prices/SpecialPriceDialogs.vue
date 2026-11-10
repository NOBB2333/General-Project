<script setup lang="ts">
import { BadgePercent, ShoppingCart } from 'lucide-vue-next';
import { computed } from 'vue';
import { UiAlertDialog, UiButton, UiDialog, UiFormField, UiInput, UiSelect, UiTextarea } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import type { SpecialPriceDraft, SpecialPriceRecord } from '../../model/special-prices.types';

const props = defineProps<{
  formOpen: boolean;
  detailOpen: boolean;
  deleteOpen: boolean;
  editingRow: SpecialPriceRecord | null;
  activeRow: SpecialPriceRecord | null;
  draft: SpecialPriceDraft;
}>();
const emit = defineEmits<{
  'update:formOpen': [value: boolean];
  'update:detailOpen': [value: boolean];
  'update:deleteOpen': [value: boolean];
  'update:draft': [value: SpecialPriceDraft];
  save: [];
  order: [row: SpecialPriceRecord];
  remove: [];
}>();

const urgencyOptions: UiSelectOption[] = [
  { value: '普通', label: '普通' },
  { value: '加急', label: '加急' },
  { value: '特急', label: '特急' },
];
const formTitle = computed(() => (props.editingRow ? '编辑特价草稿' : '新建特价'));

function updateDraft<K extends keyof SpecialPriceDraft>(key: K, value: SpecialPriceDraft[K]) {
  emit('update:draft', { ...props.draft, [key]: value });
}

function currency(value: number) {
  return `¥${value.toLocaleString('zh-CN')}`;
}
</script>

<template>
  <UiDialog
    :open="formOpen"
    :title="formTitle"
    description="特价申请仅保存到当前本地会话。"
    size="lg"
    @update:open="emit('update:formOpen', $event)"
  >
    <template #icon
      ><span :class="$style.dialogIcon"
        ><BadgePercent
          :size="18"
          :stroke-width="1.8" /></span
    ></template>
    <form
      :class="$style.formGrid"
      @submit.prevent="emit('save')"
    >
      <UiFormField
        label="客户名称"
        required
        ><UiInput
          :model-value="draft.customer"
          placeholder="请输入客户名称"
          @update:model-value="updateDraft('customer', $event)"
      /></UiFormField>
      <UiFormField label="联系人"
        ><UiInput
          :model-value="draft.contact"
          placeholder="请输入联系人"
          @update:model-value="updateDraft('contact', $event)"
      /></UiFormField>
      <UiFormField label="来源报价单"
        ><UiInput
          :model-value="draft.sourceQuotationCode"
          placeholder="例如：QT-2026-0100"
          @update:model-value="updateDraft('sourceQuotationCode', $event)"
      /></UiFormField>
      <UiFormField label="紧急程度"
        ><UiSelect
          :model-value="draft.urgency"
          :options="urgencyOptions"
          label="选择紧急程度"
          @update:model-value="updateDraft('urgency', $event as SpecialPriceDraft['urgency'])"
      /></UiFormField>
      <UiFormField
        label="物料数"
        required
        ><UiInput
          :model-value="draft.itemCount"
          type="number"
          min="1"
          @update:model-value="updateDraft('itemCount', $event)"
      /></UiFormField>
      <UiFormField
        label="申请人"
        required
        ><UiInput
          :model-value="draft.createdBy"
          @update:model-value="updateDraft('createdBy', $event)"
      /></UiFormField>
      <UiFormField
        label="正常总价"
        required
        ><UiInput
          :model-value="draft.totalNormalAmount"
          type="number"
          min="1"
          @update:model-value="updateDraft('totalNormalAmount', $event)"
      /></UiFormField>
      <UiFormField
        label="申请总价"
        required
        ><UiInput
          :model-value="draft.totalRequestedAmount"
          type="number"
          min="1"
          @update:model-value="updateDraft('totalRequestedAmount', $event)"
      /></UiFormField>
      <UiFormField
        label="申请原因"
        wide
        required
        ><UiTextarea
          :model-value="draft.reason"
          rows="3"
          placeholder="说明项目背景与申请理由"
          @update:model-value="updateDraft('reason', $event)"
      /></UiFormField>
    </form>
    <template #footer
      ><UiButton @click="emit('update:formOpen', false)">取消</UiButton
      ><UiButton
        variant="primary"
        @click="emit('save')"
        >{{ editingRow ? '保存修改' : '保存草稿' }}</UiButton
      ></template
    >
  </UiDialog>

  <UiDialog
    :open="detailOpen"
    :title="activeRow?.code ?? '特价详情'"
    :description="activeRow ? `${activeRow.customer} · ${activeRow.status}` : undefined"
    size="lg"
    @update:open="emit('update:detailOpen', $event)"
  >
    <template #icon
      ><span :class="$style.dialogIcon"
        ><BadgePercent
          :size="18"
          :stroke-width="1.8" /></span
    ></template>
    <div
      v-if="activeRow"
      :class="$style.detailBody"
    >
      <dl :class="$style.detailGrid">
        <div>
          <dt>客户名称</dt>
          <dd>{{ activeRow.customer }}</dd>
        </div>
        <div>
          <dt>联系人</dt>
          <dd>{{ activeRow.contact }}</dd>
        </div>
        <div>
          <dt>来源报价单</dt>
          <dd>{{ activeRow.sourceQuotationCode || '—' }}</dd>
        </div>
        <div>
          <dt>紧急程度</dt>
          <dd>{{ activeRow.urgency }}</dd>
        </div>
        <div>
          <dt>正常总价</dt>
          <dd>{{ currency(activeRow.totalNormalAmount) }}</dd>
        </div>
        <div>
          <dt>申请总价</dt>
          <dd>{{ currency(activeRow.totalRequestedAmount) }}</dd>
        </div>
        <div>
          <dt>让利金额</dt>
          <dd :class="$style.dangerText">{{ currency(activeRow.totalLostAmount) }}</dd>
        </div>
        <div>
          <dt>下单状态</dt>
          <dd>{{ activeRow.orderStatus }}</dd>
        </div>
      </dl>
      <section :class="$style.reason">
        <h3>申请原因</h3>
        <p>{{ activeRow.reason }}</p>
      </section>
    </div>
    <template #footer
      ><UiButton @click="emit('update:detailOpen', false)">关闭</UiButton
      ><UiButton
        v-if="activeRow?.status === '已通过' && activeRow.orderStatus === '未下单'"
        variant="primary"
        @click="emit('order', activeRow)"
        ><template #leading
          ><ShoppingCart
            :size="14"
            :stroke-width="1.8" /></template
        >下单</UiButton
      ></template
    >
  </UiDialog>

  <UiAlertDialog
    :open="deleteOpen"
    title="删除特价草稿"
    :description="activeRow ? `确认删除 ${activeRow.code}？该操作仅影响当前本地会话。` : undefined"
    confirm-label="删除草稿"
    @update:open="emit('update:deleteOpen', $event)"
    @confirm="emit('remove')"
  />
</template>

<style module>
@reference "../../../../styles/index.css";

.dialogIcon {
  @apply flex h-9 w-9 items-center justify-center rounded-full;
  color: #de350b;
  background: #ffebe6;
}
.formGrid {
  @apply grid grid-cols-2 gap-4 p-5;
}
.detailBody {
  @apply p-5;
}
.detailGrid {
  @apply grid grid-cols-2 gap-x-8 gap-y-4;
}
.detailGrid div {
  @apply border-b pb-3;
  border-color: var(--ui-border-subtle);
}
.detailGrid dt {
  @apply mb-1 text-[11px] font-semibold;
  color: var(--ui-text-subtle);
}
.detailGrid dd {
  @apply text-[13px] font-medium;
  color: var(--ui-text);
}
.dangerText {
  color: var(--ui-danger);
}
.reason {
  @apply mt-5 rounded border p-4;
  border-color: var(--ui-border-subtle);
  background: var(--ui-surface-subtle);
}
.reason h3 {
  @apply mb-1 text-[11px] font-semibold;
  color: var(--ui-text-subtle);
}
.reason p {
  @apply text-[12px] leading-5;
  color: var(--ui-text);
}
</style>
