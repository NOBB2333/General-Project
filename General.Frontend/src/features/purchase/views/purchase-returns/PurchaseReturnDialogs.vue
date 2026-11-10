<script setup lang="ts">
import { ClipboardList, RotateCcw } from 'lucide-vue-next';
import { computed } from 'vue';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, UiTextarea } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import type { PurchaseReturnDraft, PurchaseReturnRecord } from '../../model/purchase-returns.types';

const props = defineProps<{
  createOpen: boolean;
  detailOpen: boolean;
  activeRow: PurchaseReturnRecord | null;
  draft: PurchaseReturnDraft;
  supplierOptions: readonly UiSelectOption[];
  reasonOptions: readonly UiSelectOption[];
}>();

const emit = defineEmits<{
  'update:createOpen': [value: boolean];
  'update:detailOpen': [value: boolean];
  'update:draft': [value: PurchaseReturnDraft];
  save: [];
  advance: [];
}>();

const actionLabel = computed(() => {
  if (!props.activeRow) return '';
  return {
    草稿: '提交审核',
    待审核: '通过审核',
    已审核: '开始退货',
    退货中: '确认退货',
    已退货: '登记退款',
    已退款: '',
  }[props.activeRow.status];
});

function updateDraft<K extends keyof PurchaseReturnDraft>(key: K, value: PurchaseReturnDraft[K]) {
  emit('update:draft', { ...props.draft, [key]: value });
}
</script>

<template>
  <UiDialog
    :open="createOpen"
    title="新建采购退货"
    description="创建本地采购退货记录，保存后进入草稿状态。"
    size="lg"
    @update:open="emit('update:createOpen', $event)"
  >
    <template #icon
      ><span :class="$style.dialogIcon"
        ><RotateCcw
          :size="18"
          :stroke-width="1.8" /></span
    ></template>
    <form
      :class="$style.formGrid"
      @submit.prevent="emit('save')"
    >
      <UiFormField
        label="供应商"
        required
      >
        <UiSelect
          :model-value="draft.supplier"
          :options="supplierOptions.filter((option) => option.value)"
          label="选择供应商"
          @update:model-value="updateDraft('supplier', $event)"
        />
      </UiFormField>
      <UiFormField label="关联采购单">
        <UiInput
          :model-value="draft.poId"
          placeholder="例如：PO-2026-0301"
          @update:model-value="updateDraft('poId', $event)"
        />
      </UiFormField>
      <UiFormField
        label="退货原因"
        required
      >
        <UiSelect
          :model-value="draft.reason"
          :options="reasonOptions"
          label="选择退货原因"
          @update:model-value="updateDraft('reason', $event)"
        />
      </UiFormField>
      <UiFormField label="退货日期">
        <UiInput
          :model-value="draft.date"
          type="date"
          @update:model-value="updateDraft('date', $event)"
        />
      </UiFormField>
      <UiFormField
        label="退货数量"
        required
      >
        <UiInput
          :model-value="draft.items"
          type="number"
          min="1"
          @update:model-value="updateDraft('items', $event)"
        />
      </UiFormField>
      <UiFormField
        label="退货金额"
        required
      >
        <UiInput
          :model-value="draft.amount"
          type="number"
          min="0"
          @update:model-value="updateDraft('amount', $event)"
        />
      </UiFormField>
      <UiFormField
        label="退货说明"
        wide
      >
        <UiTextarea
          :model-value="draft.note"
          rows="3"
          placeholder="补充质量、规格或发货异常说明"
          @update:model-value="updateDraft('note', $event)"
        />
      </UiFormField>
    </form>
    <template #footer>
      <UiButton @click="emit('update:createOpen', false)">取消</UiButton>
      <UiButton
        variant="primary"
        @click="emit('save')"
        >保存退货草稿</UiButton
      >
    </template>
  </UiDialog>

  <UiDialog
    :open="detailOpen"
    :title="activeRow?.id ?? '采购退货详情'"
    :description="activeRow ? `${activeRow.supplier} · ${activeRow.status}` : undefined"
    size="lg"
    @update:open="emit('update:detailOpen', $event)"
  >
    <template #icon
      ><span :class="$style.dialogIcon"
        ><ClipboardList
          :size="18"
          :stroke-width="1.8" /></span
    ></template>
    <dl
      v-if="activeRow"
      :class="$style.detailGrid"
    >
      <div>
        <dt>关联采购单</dt>
        <dd>{{ activeRow.poId === '-' ? '普通退货' : activeRow.poId }}</dd>
      </div>
      <div>
        <dt>供应商</dt>
        <dd>{{ activeRow.supplier }}</dd>
      </div>
      <div>
        <dt>退货原因</dt>
        <dd>{{ activeRow.reason }}</dd>
      </div>
      <div>
        <dt>经办人</dt>
        <dd>{{ activeRow.operator }}</dd>
      </div>
      <div>
        <dt>退货数量</dt>
        <dd>{{ activeRow.items }}</dd>
      </div>
      <div>
        <dt>退货金额</dt>
        <dd>-¥{{ activeRow.amount.toLocaleString('zh-CN') }}</dd>
      </div>
      <div>
        <dt>退货日期</dt>
        <dd>{{ activeRow.date }}</dd>
      </div>
      <div>
        <dt>附件</dt>
        <dd>{{ activeRow.attachments ? `${activeRow.attachments} 个` : '无' }}</dd>
      </div>
    </dl>
    <template #footer>
      <UiButton @click="emit('update:detailOpen', false)">关闭</UiButton>
      <UiButton
        v-if="actionLabel"
        variant="primary"
        @click="emit('advance')"
        >{{ actionLabel }}</UiButton
      >
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../../../styles/index.css";

.dialogIcon {
  @apply flex h-9 w-9 shrink-0 items-center justify-center rounded-full;
  color: #de350b;
  background: #ffebe6;
}
.formGrid {
  @apply grid grid-cols-2 gap-4 p-5;
}
.detailGrid {
  @apply grid grid-cols-2 gap-x-8 gap-y-4 p-5;
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
</style>
