<script setup lang="ts">
import { Download, LayoutDashboard, Link2Off, ReceiptText, ShieldCheck, Sparkles, WalletCards } from 'lucide-vue-next';
import { UiButton, UiTabsList, UiTabsTrigger } from '@/components/ui';

const emit = defineEmits<{
  export: [];
}>();

const tabs = [
  { value: 'receipts', label: '收款单', icon: ReceiptText },
  { value: 'writeoffs', label: '收款核销', icon: ShieldCheck },
  { value: 'advances', label: '预收款管理', icon: WalletCards },
  { value: 'auto-match', label: '自动流水匹配', count: 11, icon: Sparkles },
  { value: 'unmatched', label: '未匹配收款流水', count: 5, icon: Link2Off },
  { value: 'dashboard', label: '回款看板', count: 12, icon: LayoutDashboard },
] as const;
</script>

<template>
  <header :class="$style.sectionHeader">
    <UiTabsList
      label="收款业务视图"
      :class="$style.sectionTabs"
    >
      <UiTabsTrigger
        v-for="tab in tabs"
        :key="tab.value"
        :value="tab.value"
        :class="$style.sectionTab"
      >
        <component
          :is="tab.icon"
          :size="15"
          :stroke-width="1.8"
        />
        {{ tab.label }}
        <span
          v-if="'count' in tab"
          :class="$style.tabCount"
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

.sectionHeader {
  @apply flex min-h-[54px] items-center justify-between gap-4 border-b px-4;
  border-color: var(--ui-border);
}

.sectionTabs {
  @apply min-w-0 gap-1 overflow-x-auto;
  scrollbar-width: thin;
}

.sectionTab {
  @apply min-h-[46px] shrink-0 rounded-none border-b-2 border-transparent px-3 text-[11px];
  color: var(--ui-text-muted);
}

.sectionTab:hover {
  color: var(--app-primary);
  background: transparent;
}

.sectionTab[data-state='active'] {
  color: var(--app-primary);
  border-bottom-color: var(--app-primary);
  background: transparent;
}

.tabCount {
  @apply ml-0.5 inline-flex h-5 min-w-5 items-center justify-center rounded-full px-1 text-[9px] font-semibold;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

@media (prefers-reduced-motion: reduce) {
  .sectionTab {
    transition: none;
  }
}
</style>
