<script setup lang="ts">
import { Bell, ChevronRight, Eye, FileText, Shield } from 'lucide-vue-next';
import type { TenderTask } from '@/features/crm/model/tenders.types';

defineProps<{ tasks: readonly TenderTask[] }>();
const emit = defineEmits<{ select: [task: TenderTask] }>();

const icons = { document: FileText, review: Eye, deposit: Shield } as const;
</script>

<template>
  <aside :class="$style.panel">
    <header>
      <Bell :size="15" /><strong>我的待办</strong><span>{{ tasks.length }}</span>
    </header>
    <button
      v-for="task in tasks"
      :key="task.id"
      type="button"
      :class="$style.task"
      @click="emit('select', task)"
    >
      <i :data-tone="task.tone"
        ><component
          :is="icons[task.tone]"
          :size="14"
      /></i>
      <span
        ><strong>{{ task.title }}</strong
        ><small>{{ task.project }}</small
        ><em>截止: {{ task.deadline }}</em></span
      >
      <ChevronRight :size="14" />
    </button>
  </aside>
</template>

<style module>
@reference "../../../../styles/index.css";

.panel {
  @apply overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}
.panel > header {
  @apply flex h-12 items-center gap-2 border-b px-4 text-[12px];
  border-color: var(--ui-border);
}
.panel > header > svg {
  color: var(--ui-warning);
}
.panel > header span {
  @apply flex h-5 min-w-5 items-center justify-center rounded-full px-1.5 text-[10px] text-white;
  background: var(--ui-danger);
}
.task {
  @apply flex min-h-[78px] w-full items-start gap-3 border-b px-4 py-3 text-left transition-colors;
  border-color: var(--ui-border-subtle);
}
.task:hover {
  background: var(--ui-surface-hover);
}
.task i {
  @apply flex h-7 w-7 shrink-0 items-center justify-center rounded not-italic;
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}
.task i[data-tone='review'] {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}
.task i[data-tone='deposit'] {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.task > span {
  @apply min-w-0 flex-1;
}
.task strong,
.task small,
.task em {
  @apply block truncate;
}
.task strong {
  @apply text-[12px];
  color: var(--ui-text);
}
.task small {
  @apply mt-1 text-[10px];
  color: var(--ui-text-subtle);
}
.task em {
  @apply mt-1 text-[10px] not-italic;
  color: var(--ui-text-muted);
}
.task > svg {
  @apply mt-1 shrink-0;
  color: #c1c7d0;
}
</style>
