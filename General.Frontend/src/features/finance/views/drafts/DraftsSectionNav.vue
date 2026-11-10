<script setup lang="ts">
import { Archive, BadgeDollarSign, Download, FileScan, FileText, Landmark, Plus, TriangleAlert } from 'lucide-vue-next';
import { UiButton, UiTabsList, UiTabsTrigger } from '@/components/ui';

defineProps<{
  receivableCount: number;
  warningCount: number;
  payableCount: number;
}>();

const emit = defineEmits<{
  export: [];
  recognize: [];
  createPayable: [];
  createReceivable: [];
}>();

const sections = [
  { value: 'receivable', label: '应收票据池', countKey: 'receivableCount', icon: FileText },
  { value: 'warnings', label: '到期预警', countKey: 'warningCount', icon: TriangleAlert },
  { value: 'payable', label: '应付票据', countKey: 'payableCount', icon: BadgeDollarSign },
  { value: 'history', label: '历史台账', countKey: null, icon: Archive },
] as const;
</script>

<template>
  <header :class="$style.header">
    <UiTabsList
      label="汇票业务视图"
      :class="$style.tabs"
    >
      <UiTabsTrigger
        v-for="section in sections"
        :key="section.value"
        :value="section.value"
        :class="$style.tab"
      >
        <component
          :is="section.icon"
          :size="14"
          :stroke-width="1.8"
        />
        {{ section.label }}
        <span v-if="section.countKey !== null">{{ $props[section.countKey] }}</span>
      </UiTabsTrigger>
    </UiTabsList>

    <div :class="$style.actions">
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
      <UiButton
        variant="secondary"
        size="sm"
        @click="emit('recognize')"
      >
        <template #leading>
          <FileScan
            :size="14"
            :stroke-width="1.8"
          />
        </template>
        批量识别
      </UiButton>
      <UiButton
        :class="$style.outlineDanger"
        variant="secondary"
        size="sm"
        @click="emit('createPayable')"
      >
        <template #leading>
          <Landmark
            :size="14"
            :stroke-width="1.8"
          />
        </template>
        开新银承付款
      </UiButton>
      <UiButton
        variant="primary"
        size="sm"
        @click="emit('createReceivable')"
      >
        <template #leading>
          <Plus
            :size="14"
            :stroke-width="1.8"
          />
        </template>
        登记应收票据
      </UiButton>
    </div>
  </header>
</template>

<style module>
@reference "../../../../styles/index.css";

.header {
  @apply flex min-h-[53px] items-center border-b px-4;
  border-color: var(--ui-border);
}

.tabs {
  @apply h-full gap-2;
}

.tab {
  @apply min-h-[51px] rounded-none border-b-2 border-transparent px-3 text-[11px];
  color: var(--ui-text-subtle);
}

.tab:hover,
.tab[data-state='active'] {
  color: var(--app-primary);
  background: transparent;
}

.tab[data-state='active'] {
  border-bottom-color: var(--app-primary);
}

.tab span {
  @apply inline-flex h-[18px] min-w-[18px] items-center justify-center rounded-full px-1 text-[9px];
  color: var(--ui-text-subtle);
  background: var(--ui-border-subtle);
}

.actions {
  @apply ml-auto flex items-center gap-2;
}

.outlineDanger {
  color: var(--ui-danger);
  border-color: var(--ui-danger);
}
</style>
