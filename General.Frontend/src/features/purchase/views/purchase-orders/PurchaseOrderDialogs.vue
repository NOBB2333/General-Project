<script setup lang="ts">
import { CreditCard, ShoppingCart, Truck } from 'lucide-vue-next';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import type { PurchaseOrderDraft, PurchaseOrderRow } from '../../model/purchase-orders.types';

defineProps<{
  createOpen: boolean;
  detailOpen: boolean;
  paymentOpen: boolean;
  activeRow: PurchaseOrderRow | null;
  draft: PurchaseOrderDraft;
  supplierOptions: readonly UiSelectOption[];
}>();

const emit = defineEmits<{
  'update:createOpen': [value: boolean];
  'update:detailOpen': [value: boolean];
  'update:paymentOpen': [value: boolean];
  'update:draft': [value: PurchaseOrderDraft];
  save: [];
  submitPayment: [];
}>();

const buyerOptions: readonly UiSelectOption[] = [
  { value: '张经理', label: '张经理' },
  { value: '王工', label: '王工' },
  { value: '李工', label: '李工' },
  { value: '周采购', label: '周采购' },
];

function currency(value: number) {
  return `¥${value.toLocaleString('en-US')}`;
}
</script>

<template>
  <UiDialog
    :open="createOpen"
    title="新建采购单"
    description="创建采购订单草稿，仅保存到当前本地会话"
    size="lg"
    @update:open="emit('update:createOpen', $event)"
  >
    <template #icon>
      <span :class="$style.dialogIcon"><ShoppingCart :size="18" /></span>
    </template>
    <form
      :class="$style.form"
      @submit.prevent="emit('save')"
    >
      <UiFormField
        label="供应商"
        required
        wide
      >
        <UiSelect
          :model-value="draft.supplier"
          :options="supplierOptions.filter((option) => option.value !== '全部供应商')"
          label="采购供应商"
          placeholder="请选择供应商"
          @update:model-value="emit('update:draft', { ...draft, supplier: $event })"
        />
      </UiFormField>
      <UiFormField label="关联项目">
        <UiInput
          :model-value="draft.project"
          placeholder="留空则为库存采购"
          @update:model-value="emit('update:draft', { ...draft, project: $event })"
        />
      </UiFormField>
      <UiFormField label="采购员">
        <UiSelect
          :model-value="draft.buyer"
          :options="buyerOptions"
          label="采购员"
          @update:model-value="emit('update:draft', { ...draft, buyer: $event })"
        />
      </UiFormField>
      <UiFormField
        label="采购金额"
        required
      >
        <UiInput
          :model-value="draft.amount"
          type="number"
          placeholder="0.00"
          @update:model-value="emit('update:draft', { ...draft, amount: $event })"
        />
      </UiFormField>
      <UiFormField label="商品种类">
        <UiInput
          :model-value="draft.itemCount"
          type="number"
          placeholder="1"
          @update:model-value="emit('update:draft', { ...draft, itemCount: $event })"
        />
      </UiFormField>
    </form>
    <template #footer>
      <UiButton
        variant="secondary"
        size="sm"
        @click="emit('update:createOpen', false)"
        >取消</UiButton
      >
      <UiButton
        variant="primary"
        size="sm"
        @click="emit('save')"
        >保存采购单</UiButton
      >
    </template>
  </UiDialog>

  <UiDialog
    :open="detailOpen"
    :title="activeRow?.code ?? '采购订单详情'"
    :description="activeRow ? `${activeRow.supplier} · ${activeRow.status}` : undefined"
    size="lg"
    @update:open="emit('update:detailOpen', $event)"
  >
    <template #icon>
      <span :class="$style.dialogIcon"><Truck :size="18" /></span>
    </template>
    <dl
      v-if="activeRow"
      :class="$style.details"
    >
      <div>
        <dt>关联项目</dt>
        <dd>{{ activeRow.project }}</dd>
      </div>
      <div>
        <dt>采购员</dt>
        <dd>{{ activeRow.buyer }}</dd>
      </div>
      <div>
        <dt>采购金额</dt>
        <dd>{{ currency(activeRow.amount) }}</dd>
      </div>
      <div>
        <dt>商品种类</dt>
        <dd>{{ activeRow.itemCount }} 种</dd>
      </div>
      <div>
        <dt>开票进度</dt>
        <dd>{{ activeRow.invoiceProgress }}%</dd>
      </div>
      <div>
        <dt>入库进度</dt>
        <dd>{{ activeRow.stockProgress }}%</dd>
      </div>
      <div>
        <dt>付款进度</dt>
        <dd>{{ activeRow.paymentProgress }}%</dd>
      </div>
      <div>
        <dt>订单日期</dt>
        <dd>{{ activeRow.date }}</dd>
      </div>
    </dl>
    <template #footer>
      <UiButton
        variant="secondary"
        size="sm"
        @click="emit('update:detailOpen', false)"
        >关闭</UiButton
      >
    </template>
  </UiDialog>

  <UiDialog
    :open="paymentOpen"
    title="付款申请"
    :description="activeRow ? `${activeRow.code} · ${activeRow.supplier}` : undefined"
    size="sm"
    @update:open="emit('update:paymentOpen', $event)"
  >
    <template #icon>
      <span :class="[$style.dialogIcon, $style.paymentIcon]"><CreditCard :size="18" /></span>
    </template>
    <div
      v-if="activeRow"
      :class="$style.paymentBody"
    >
      <p>
        订单金额 <strong>{{ currency(activeRow.amount) }}</strong>
      </p>
      <p>
        当前付款进度 <strong>{{ activeRow.paymentProgress }}%</strong>
      </p>
      <small>确认后将在当前本地会话中增加 20% 付款进度。</small>
    </div>
    <template #footer>
      <UiButton
        variant="secondary"
        size="sm"
        @click="emit('update:paymentOpen', false)"
        >取消</UiButton
      >
      <UiButton
        variant="primary"
        size="sm"
        @click="emit('submitPayment')"
        >确认申请</UiButton
      >
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../../../styles/index.css";

.dialogIcon {
  @apply flex h-9 w-9 shrink-0 items-center justify-center rounded-full;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.paymentIcon {
  color: var(--ui-success);
  background: var(--ui-success-soft);
}

.form {
  @apply grid grid-cols-2 gap-4 p-5;
}

.details {
  @apply grid grid-cols-2 gap-3 p-5;
}

.details div {
  @apply rounded border p-3;
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}

.details dt {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}

.details dd {
  @apply m-0 mt-1 text-[12px] font-semibold;
  color: var(--ui-text);
}

.paymentBody {
  @apply space-y-3 p-5 text-[12px];
  color: var(--ui-text-muted);
}

.paymentBody p {
  @apply flex items-center justify-between rounded border p-3;
  border-color: var(--ui-border);
}

.paymentBody strong {
  color: var(--ui-text);
}

.paymentBody small {
  color: var(--ui-text-subtle);
}
</style>
