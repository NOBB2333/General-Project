<script setup lang="ts">
import { Download, List, Plus, ShieldCheck } from 'lucide-vue-next';
import { UiButton } from '@/components/ui';
import type { SupplierView } from '../../model/suppliers.types';

defineProps<{ modelValue: SupplierView }>();
const emit = defineEmits<{
  'update:modelValue': [value: SupplierView];
  export: [];
  create: [];
}>();

const tabs: readonly { id: SupplierView; label: string; icon: typeof List }[] = [
  { id: 'list', label: '供应商列表', icon: List },
  { id: 'qualification', label: '资质管理', icon: ShieldCheck },
];
</script>

<template>
  <div :class="$style.tabsBar">
    <div
      :class="$style.tabs"
      role="tablist"
      aria-label="供应商业务视图"
    >
      <button
        v-for="tab in tabs"
        :key="tab.id"
        type="button"
        role="tab"
        :aria-selected="modelValue === tab.id"
        :class="[$style.tab, modelValue === tab.id && $style.active]"
        @click="emit('update:modelValue', tab.id)"
      >
        <component
          :is="tab.icon"
          :size="15"
          :stroke-width="1.8"
        />
        {{ tab.label }}
      </button>
    </div>
    <div
      v-if="modelValue === 'list'"
      :class="$style.actions"
    >
      <UiButton
        size="sm"
        variant="secondary"
        @click="emit('export')"
      >
        <template #leading>
          <Download
            :size="13"
            :stroke-width="1.8"
          />
        </template>
        导出
      </UiButton>
      <UiButton
        size="sm"
        variant="primary"
        @click="emit('create')"
      >
        <template #leading>
          <Plus
            :size="13"
            :stroke-width="1.8"
          />
        </template>
        新增供应商
      </UiButton>
    </div>
  </div>
</template>

<style module>
@reference "../../../../styles/index.css";

.tabsBar {
  @apply mb-5 flex min-h-[41px] items-end border-b;
  border-color: var(--ui-border);
}
.tabs {
  @apply flex self-stretch;
}
.tab {
  @apply relative flex min-h-[41px] items-center gap-1.5 px-5 text-[14px] transition-colors;
  color: #5e6c84;
}
.tab:hover {
  color: var(--ui-text-muted);
}
.active {
  @apply font-semibold;
  color: var(--app-primary);
}
.active::after {
  @apply absolute inset-x-0 bottom-0 h-0.5;
  content: '';
  background: var(--app-primary);
}
.actions {
  @apply ml-auto flex items-center gap-2 pb-2;
}
</style>
