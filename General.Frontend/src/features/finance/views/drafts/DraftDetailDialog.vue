<script setup lang="ts">
import { FileText } from 'lucide-vue-next';
import { UiButton, UiDialog } from '@/components/ui';
import type { DraftRow } from '../../model/drafts.types';

defineProps<{
  open: boolean;
  row: DraftRow | null;
}>();

const emit = defineEmits<{
  'update:open': [value: boolean];
}>();

function currency(value: number) {
  return `¥${value.toLocaleString('zh-CN')}`;
}
</script>

<template>
  <UiDialog
    :open="open"
    :title="row ? `票据详情 · ${row.code}` : '票据详情'"
    :description="row ? `${row.direction} · ${row.status}` : undefined"
    size="lg"
    @update:open="emit('update:open', $event)"
  >
    <dl
      v-if="row"
      :class="$style.details"
    >
      <div :class="$style.wide">
        <dt>票号</dt>
        <dd>{{ row.noteNumber }}</dd>
      </div>
      <div>
        <dt>承兑类型</dt>
        <dd>{{ row.acceptanceKind }} · {{ row.medium }}</dd>
      </div>
      <div>
        <dt>票面金额</dt>
        <dd>{{ currency(row.amount) }}</dd>
      </div>
      <div>
        <dt>出票人</dt>
        <dd>{{ row.drawer }}</dd>
      </div>
      <div>
        <dt>承兑人</dt>
        <dd>{{ row.acceptor }}</dd>
      </div>
      <div>
        <dt>出票日</dt>
        <dd>{{ row.issueDate }}</dd>
      </div>
      <div>
        <dt>到期日</dt>
        <dd>{{ row.dueDate }} · 剩 {{ row.remainingDays }} 天</dd>
      </div>
    </dl>
    <template #icon>
      <span :class="$style.dialogIcon">
        <FileText
          :size="18"
          :stroke-width="1.8"
        />
      </span>
    </template>
    <template #footer>
      <UiButton
        variant="primary"
        size="sm"
        @click="emit('update:open', false)"
        >关闭</UiButton
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
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
</style>
