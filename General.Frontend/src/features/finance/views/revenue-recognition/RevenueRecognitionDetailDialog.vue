<script setup lang="ts">
import { FileCheck2 } from 'lucide-vue-next';
import { UiButton, UiDialog } from '@/components/ui';
import type { RevenueRecognitionRow } from '../../model/revenue-recognition.types';

defineProps<{ open: boolean; row: RevenueRecognitionRow | null }>();
const emit = defineEmits<{
  'update:open': [value: boolean];
  submit: [row: RevenueRecognitionRow];
  approve: [row: RevenueRecognitionRow];
}>();
function money(value: number | null) {
  return value === null ? '-' : `¥${value.toLocaleString('zh-CN')}`;
}
</script>

<template>
  <UiDialog
    :open="open"
    :title="row ? `${row.code} · 收入确认详情` : '收入确认详情'"
    :description="row ? `${row.customer} · ${row.status}` : undefined"
    size="lg"
    @update:open="emit('update:open', $event)"
  >
    <dl
      v-if="row"
      :class="$style.details"
    >
      <div>
        <dt>来源合同</dt>
        <dd>{{ row.contract }}</dd>
      </div>
      <div>
        <dt>客户名称</dt>
        <dd>{{ row.customer }}</dd>
      </div>
      <div :class="$style.wide">
        <dt>关联项目</dt>
        <dd>{{ row.project }}</dd>
      </div>
      <div>
        <dt>确认方式</dt>
        <dd>{{ row.method }}</dd>
      </div>
      <div>
        <dt>本次确认金额</dt>
        <dd :class="$style.success">{{ money(row.amount) }}</dd>
      </div>
      <div>
        <dt>累计 / 合同</dt>
        <dd>{{ money(row.accumulated) }} / {{ money(row.contractAmount) }}</dd>
      </div>
      <div>
        <dt>剩余待确认</dt>
        <dd :class="$style.warning">{{ money(row.remaining) }}</dd>
      </div>
      <div>
        <dt>确认日期 / 期间</dt>
        <dd>{{ row.confirmedAt }} · {{ row.period }}</dd>
      </div>
      <div>
        <dt>开票状态</dt>
        <dd>{{ row.invoiceStatus }}</dd>
      </div>
      <div>
        <dt>创建人</dt>
        <dd>{{ row.creator }} · {{ row.createdAt }}</dd>
      </div>
      <div>
        <dt>审核人</dt>
        <dd>{{ row.reviewer }} · {{ row.reviewedAt }}</dd>
      </div>
    </dl>
    <template #icon
      ><span :class="$style.dialogIcon"
        ><FileCheck2
          :size="18"
          :stroke-width="1.8" /></span
    ></template>
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
        >提交审核</UiButton
      >
      <UiButton
        v-if="row?.status === '待审核'"
        variant="primary"
        size="sm"
        @click="emit('approve', row)"
        >审核通过</UiButton
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
.success {
  color: var(--ui-success) !important;
}
.warning {
  color: var(--ui-warning) !important;
}
.dialogIcon {
  @apply flex h-9 w-9 shrink-0 items-center justify-center rounded-full;
  color: var(--ui-success);
  background: #e3fcef;
}
</style>
