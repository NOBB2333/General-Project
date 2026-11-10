<script setup lang="ts">
import { Banknote, CircleDollarSign, Download, Link2Off, ShieldCheck, Sparkles } from 'lucide-vue-next';
import { UiButton, UiTabsList, UiTabsTrigger } from '@/components/ui';

const emit = defineEmits<{
  export: [];
}>();

const tabs = [
  { value: 'tasks', label: '付款任务', count: 9, icon: Banknote },
  { value: 'writeoffs', label: '付款核销', icon: ShieldCheck },
  { value: 'advances', label: '预付款管理', icon: CircleDollarSign },
  { value: 'auto-match', label: '自动流水匹配', count: 20, icon: Sparkles },
  { value: 'unmatched', label: '未匹配付款流水', count: 11, icon: Link2Off },
] as const;
</script>

<template>
  <header :class="$style.header">
    <UiTabsList
      label="付款业务视图"
      :class="$style.tabs"
    >
      <UiTabsTrigger
        v-for="tab in tabs"
        :key="tab.value"
        :value="tab.value"
        :class="$style.tab"
      >
        <component
          :is="tab.icon"
          :size="15"
          :stroke-width="1.8"
        />
        {{ tab.label }}
        <span
          v-if="'count' in tab"
          :class="$style.count"
          >{{ tab.count }}</span
        >
      </UiTabsTrigger>
    </UiTabsList>
    <UiButton
      variant="secondary"
      size="sm"
      @click="emit('export')"
    >
      <template #leading>
        <Download
          :size="14"
          :stroke-width="1.8"
        />
      </template>
      导出
    </UiButton>
  </header>
</template>

<style module>
@reference "../../../../styles/index.css";

.header {
  @apply flex min-h-[43px] items-center justify-between gap-4 border-b px-4;
  border-color: var(--ui-border);
}

.tabs {
  @apply min-w-0 gap-1 overflow-x-auto;
  scrollbar-width: thin;
}

.tab {
  @apply min-h-[37px] shrink-0 rounded-none border-b-2 border-transparent px-3 text-[11px];
  color: var(--ui-text-muted);
}

.tab:hover,
.tab[data-state='active'] {
  color: var(--app-primary);
  background: transparent;
}

.tab[data-state='active'] {
  border-bottom-color: var(--app-primary);
}

.count {
  @apply ml-0.5 inline-flex h-5 min-w-5 items-center justify-center rounded-full px-1 text-[9px] font-semibold;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

@media (prefers-reduced-motion: reduce) {
  .tab {
    transition: none;
  }
}
</style>
