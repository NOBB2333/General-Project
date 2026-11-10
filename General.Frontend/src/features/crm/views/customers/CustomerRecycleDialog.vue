<script setup lang="ts">
import { RotateCcw, Trash2 } from 'lucide-vue-next';
import { UiButton, UiDialog } from '@/components/ui';
import type { CustomerRow } from '@/features/crm/model/customers.types';

defineProps<{ open: boolean; customers: CustomerRow[] }>();
const emit = defineEmits<{
  'update:open': [value: boolean];
  restore: [customer: CustomerRow];
  remove: [customer: CustomerRow];
  clear: [];
}>();
</script>

<template>
  <UiDialog
    :open="open"
    title="客户回收站"
    description="删除的客户可在当前本地会话恢复。"
    size="lg"
    @update:open="emit('update:open', $event)"
  >
    <div :class="$style.list">
      <article
        v-for="customer in customers"
        :key="customer.id"
      >
        <div>
          <strong>{{ customer.name }}</strong>
          <span>{{ customer.id }} · {{ customer.owner }} · {{ customer.type }}</span>
        </div>
        <UiButton
          size="xs"
          @click="emit('restore', customer)"
        >
          <template #leading><RotateCcw :size="13" /></template>
          恢复
        </UiButton>
        <UiButton
          variant="ghost"
          size="icon-sm"
          aria-label="彻底删除"
          @click="emit('remove', customer)"
        >
          <Trash2 :size="14" />
        </UiButton>
      </article>
      <p v-if="!customers.length">回收站暂无客户</p>
    </div>
    <template #footer>
      <UiButton
        v-if="customers.length"
        variant="danger"
        @click="emit('clear')"
      >
        清空回收站
      </UiButton>
      <UiButton @click="emit('update:open', false)">关闭</UiButton>
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../../../styles/index.css";

.list {
  @apply grid gap-2 p-5;
}

.list article {
  @apply flex items-center gap-2 rounded border px-3 py-2;
  border-color: var(--ui-border-subtle);
}

.list article > div {
  @apply min-w-0 flex-1;
}

.list strong,
.list span {
  @apply block;
}

.list strong {
  @apply text-[13px];
  color: var(--ui-text);
}

.list span,
.list p {
  @apply mt-1 text-[11px];
  color: var(--ui-text-subtle);
}

.list p {
  @apply py-8 text-center;
}
</style>
