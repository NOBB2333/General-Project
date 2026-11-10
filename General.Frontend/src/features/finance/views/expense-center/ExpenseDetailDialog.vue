<script setup lang="ts">
import { ReceiptText } from 'lucide-vue-next';
import { UiButton, UiDialog } from '@/components/ui';
import type { ExpenseRow } from '../../model/expense-center.types';
defineProps<{ open: boolean; row: ExpenseRow | null }>();
const emit = defineEmits<{ 'update:open': [value: boolean]; advance: [row: ExpenseRow] }>();
function money(value: number) {
  return `¥${value.toLocaleString('zh-CN')}`;
}
</script>

<template>
  <UiDialog
    :open="open"
    :title="row ? `${row.code} · 费用详情` : '费用详情'"
    :description="row ? `${row.name} · ${row.status}` : undefined"
    size="lg"
    @update:open="emit('update:open', $event)"
  >
    <dl
      v-if="row"
      :class="$style.details"
    >
      <div :class="$style.wide">
        <dt>费用名称</dt>
        <dd>{{ row.name }}</dd>
      </div>
      <div>
        <dt>费用归属</dt>
        <dd>{{ row.ownership }}</dd>
      </div>
      <div>
        <dt>费用金额</dt>
        <dd :class="$style.amount">{{ money(row.amount) }}</dd>
      </div>
      <div>
        <dt>申请人 / 部门</dt>
        <dd>{{ row.applicant }} · {{ row.department }}</dd>
      </div>
      <div>
        <dt>费用日期</dt>
        <dd>{{ row.date }}</dd>
      </div>
      <div :class="$style.wide">
        <dt>关联项目</dt>
        <dd>{{ row.project }}</dd>
      </div>
    </dl>
    <template #icon
      ><span :class="$style.dialogIcon"
        ><ReceiptText
          :size="18"
          :stroke-width="1.8" /></span
    ></template>
    <template #footer
      ><UiButton
        variant="secondary"
        size="sm"
        @click="emit('update:open', false)"
        >关闭</UiButton
      ><UiButton
        v-if="row?.status === '待审核' || row?.status === '审核中' || row?.status === '已通过'"
        variant="primary"
        size="sm"
        @click="emit('advance', row)"
        >{{ row.status === '待审核' ? '开始审核' : row.status === '审核中' ? '审核通过' : '登记打款' }}</UiButton
      ></template
    >
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
.amount {
  color: var(--app-primary) !important;
}
.dialogIcon {
  @apply flex h-9 w-9 shrink-0 items-center justify-center rounded-full;
  color: #c2185b;
  background: #ffebf2;
}
</style>
