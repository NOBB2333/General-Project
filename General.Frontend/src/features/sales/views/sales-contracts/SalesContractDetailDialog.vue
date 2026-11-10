<script setup lang="ts">
import { FileSignature, ReceiptText } from 'lucide-vue-next';
import { UiButton, UiDialog } from '@/components/ui';
import type { SalesContractRow } from '@/features/sales/model/sales-contracts.types';

defineProps<{
  open: boolean;
  row: SalesContractRow | null;
}>();

const emit = defineEmits<{
  'update:open': [value: boolean];
  invoice: [row: SalesContractRow];
}>();
</script>

<template>
  <UiDialog
    :open="open"
    :title="row?.code ?? '销售合同详情'"
    :description="row ? `${row.name} · ${row.status}` : undefined"
    size="xl"
    @update:open="emit('update:open', $event)"
  >
    <template #icon>
      <span :class="$style.dialogIcon">
        <FileSignature
          :size="18"
          :stroke-width="1.8"
        />
      </span>
    </template>

    <div
      v-if="row"
      :class="$style.content"
    >
      <section :class="$style.summary">
        <div>
          <span>合同金额</span>
          <strong>{{ row.amount }}</strong>
        </div>
        <div>
          <span>已开票</span>
          <strong>{{ row.invoicePct }}</strong>
        </div>
        <div>
          <span>已发货</span>
          <strong>{{ row.shipPct }}</strong>
        </div>
        <div>
          <span>已收款</span>
          <strong>{{ row.collectPct }}</strong>
        </div>
      </section>

      <section :class="$style.section">
        <h3>合同基本信息</h3>
        <dl :class="$style.detailGrid">
          <div>
            <dt>合同编号</dt>
            <dd>{{ row.code }}</dd>
          </div>
          <div>
            <dt>合同分类 / 项目</dt>
            <dd>{{ row.subtitle }}</dd>
          </div>
          <div>
            <dt>客户</dt>
            <dd>{{ row.customer }}</dd>
          </div>
          <div>
            <dt>来源</dt>
            <dd>{{ row.source }}</dd>
          </div>
          <div>
            <dt>签订日期</dt>
            <dd>{{ row.signDate }}</dd>
          </div>
          <div>
            <dt>经办人</dt>
            <dd>{{ row.owner }}</dd>
          </div>
          <div>
            <dt>合同状态</dt>
            <dd>{{ row.status }}</dd>
          </div>
          <div>
            <dt>订单状态</dt>
            <dd>{{ row.orderStatus }}</dd>
          </div>
        </dl>
      </section>
    </div>

    <template #footer>
      <UiButton
        variant="secondary"
        @click="emit('update:open', false)"
        >关闭</UiButton
      >
      <UiButton
        v-if="row?.status === '执行中'"
        variant="primary"
        @click="row && emit('invoice', row)"
      >
        <template #leading>
          <ReceiptText
            :size="14"
            :stroke-width="1.8"
          />
        </template>
        申请开票
      </UiButton>
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../../../styles/index.css";

.dialogIcon {
  @apply flex h-9 w-9 shrink-0 items-center justify-center rounded;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.content {
  @apply p-5;
}

.summary {
  @apply grid grid-cols-4 gap-3;
}

.summary div {
  @apply rounded border p-3;
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}

.summary span,
.detailGrid dt {
  @apply text-[11px];
  color: var(--ui-text-subtle);
}

.summary strong {
  @apply mt-1 block text-[18px] font-semibold;
  color: var(--ui-text);
}

.section {
  @apply mt-5 border-t pt-4;
  border-color: var(--ui-border-subtle);
}

.section h3 {
  @apply mb-4 text-[14px] font-semibold;
  color: var(--ui-text);
}

.detailGrid {
  @apply grid grid-cols-2 gap-x-8 gap-y-4;
}

.detailGrid dd {
  @apply mt-1 text-[12px] font-medium;
  color: var(--ui-text);
}
</style>
