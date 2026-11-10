<script setup lang="ts">
import { ReceiptText } from 'lucide-vue-next';
import { UiButton, UiDialog } from '@/components/ui';
import type { RefundRow } from '../../model/refunds.types';

defineProps<{
  open: boolean;
  row: RefundRow | null;
}>();

const emit = defineEmits<{
  'update:open': [value: boolean];
  submit: [row: RefundRow];
}>();

function currency(row: RefundRow) {
  const symbols = { CNY: '¥', USD: '$', EUR: '€' } as const;
  return `${symbols[row.currency]}${row.amount.toLocaleString('zh-CN')}`;
}
</script>

<template>
  <UiDialog
    :open="open"
    :title="row?.code ?? '退款申请详情'"
    :description="row ? `${row.party} · ${row.status}` : undefined"
    size="lg"
    @update:open="emit('update:open', $event)"
  >
    <dl
      v-if="row"
      :class="$style.details"
    >
      <div>
        <dt>{{ row.kind === 'customer' ? '关联合同' : '关联采购单' }}</dt>
        <dd>{{ row.contract }}</dd>
      </div>
      <div>
        <dt>申请日期</dt>
        <dd>{{ row.requestedAt }}</dd>
      </div>
      <div :class="$style.wide">
        <dt>业务说明</dt>
        <dd>{{ row.description }}</dd>
      </div>
      <div>
        <dt>实退金额</dt>
        <dd>{{ currency(row) }} · {{ row.currency }}</dd>
      </div>
      <div>
        <dt>退款方式</dt>
        <dd>{{ row.method }}</dd>
      </div>
      <div>
        <dt>财务审批</dt>
        <dd>{{ row.approvalStatus }}</dd>
      </div>
      <div>
        <dt>付款 / 核销</dt>
        <dd>{{ row.paymentStatus }} / {{ row.writeoffStatus }}</dd>
      </div>
    </dl>

    <template #icon>
      <span :class="$style.dialogIcon">
        <ReceiptText
          :size="18"
          :stroke-width="1.8"
        />
      </span>
    </template>
    <template #footer>
      <UiButton
        variant="secondary"
        size="sm"
        @click="emit('update:open', false)"
        >关闭</UiButton
      >
      <UiButton
        v-if="row?.status === '草稿'"
        variant="primary"
        size="sm"
        @click="emit('submit', row)"
        >提交审批</UiButton
      >
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../../../styles/index.css";

.details {
  @apply m-0 grid grid-cols-2 gap-4 p-5;
}

.details > div {
  @apply rounded border p-3;
  border-color: var(--ui-border-subtle);
  background: var(--ui-surface-subtle);
}

.details dt,
.details dd {
  @apply m-0;
}

.details dt {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}

.details dd {
  @apply mt-1 text-[12px] font-semibold;
  color: var(--ui-text);
}

.wide {
  @apply col-span-2;
}

.dialogIcon {
  @apply flex h-9 w-9 shrink-0 items-center justify-center rounded-full;
  color: #6554c0;
  background: #eae6ff;
}
</style>
