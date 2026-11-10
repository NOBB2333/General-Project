<script setup lang="ts">
import { Mail, MapPin, Phone } from 'lucide-vue-next';
import { UiButton, UiDialog } from '@/components/ui';
import type { CustomerRow } from '@/features/crm/model/customers.types';

defineProps<{ open: boolean; customer: CustomerRow | null }>();
const emit = defineEmits<{
  'update:open': [value: boolean];
  edit: [customer: CustomerRow];
  followUp: [customer: CustomerRow];
}>();

function money(value: number) {
  return `¥${value.toLocaleString('en-US')}`;
}
</script>

<template>
  <UiDialog
    :open="open"
    :title="customer?.name ?? '客户详情'"
    :description="customer ? `${customer.id} · ${customer.type} · ${customer.level}` : undefined"
    size="lg"
    @update:open="emit('update:open', $event)"
  >
    <div
      v-if="customer"
      :class="$style.content"
    >
      <section :class="$style.contactCard">
        <strong>{{ customer.contact }}</strong>
        <span><Phone :size="13" />{{ customer.phone }}</span>
        <span><Mail :size="13" />{{ customer.email }}</span>
        <span><MapPin :size="13" />{{ customer.address }}</span>
      </section>
      <dl :class="$style.metrics">
        <div>
          <dt>负责人</dt>
          <dd>{{ customer.owner }}</dd>
        </div>
        <div>
          <dt>信用额度</dt>
          <dd>{{ money(customer.creditLimit) }}</dd>
        </div>
        <div>
          <dt>账期</dt>
          <dd>{{ customer.paymentDays }}天</dd>
        </div>
        <div>
          <dt>累计销售</dt>
          <dd>{{ money(customer.totalSales) }}</dd>
        </div>
        <div>
          <dt>应收余额</dt>
          <dd>{{ money(customer.receivable) }}</dd>
        </div>
        <div>
          <dt>红绿灯说明</dt>
          <dd>{{ customer.trafficReason }}</dd>
        </div>
      </dl>
      <div :class="$style.tags">
        <span
          v-for="tag in customer.tags"
          :key="tag"
          >{{ tag }}</span
        >
      </div>
    </div>
    <template #footer>
      <UiButton @click="emit('update:open', false)">关闭</UiButton>
      <UiButton
        v-if="customer"
        @click="emit('edit', customer)"
      >
        编辑客户
      </UiButton>
      <UiButton
        v-if="customer"
        variant="primary"
        @click="emit('followUp', customer)"
      >
        新建跟进
      </UiButton>
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../../../styles/index.css";

.content {
  @apply grid gap-4 p-5;
}

.contactCard {
  @apply grid gap-2 rounded border p-4 text-[12px];
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}

.contactCard strong {
  @apply text-[15px];
  color: var(--ui-text);
}

.contactCard span {
  @apply flex items-center gap-2;
  color: var(--ui-text-muted);
}

.metrics {
  @apply grid grid-cols-3 gap-3;
}

.metrics div {
  @apply rounded border px-3 py-2;
  border-color: var(--ui-border-subtle);
}

.metrics dt {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}

.metrics dd {
  @apply mt-1 text-[12px] font-semibold;
  color: var(--ui-text);
}

.tags {
  @apply flex flex-wrap gap-2;
}

.tags span {
  @apply rounded px-2 py-1 text-[11px] font-medium;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
</style>
