<script setup lang="ts">
import { AlertTriangle, FileCheck2, History, Layers3, ListChecks, ShieldCheck, Tag } from 'lucide-vue-next';
import { UiTabs, UiTabsList, UiTabsTrigger } from '@/components/ui';
import type { PricingTabId } from '../../model/pricing-center.types';

defineProps<{ modelValue: PricingTabId }>();
const emit = defineEmits<{ 'update:modelValue': [tab: PricingTabId] }>();

const tabs = [
  { id: 'pricing', label: '商品定价', icon: Tag },
  { id: 'pricegroup', label: '价格组管理', icon: Layers3 },
  { id: 'agreement', label: '框架协议价', icon: FileCheck2 },
  { id: 'special', label: '特价审批', icon: ShieldCheck },
  { id: 'alert', label: '价格预警', icon: AlertTriangle },
  { id: 'adjustment', label: '价格调整记录', icon: ListChecks },
  { id: 'history', label: '价格历史', icon: History },
] as const;
</script>

<template>
  <UiTabs
    :model-value="modelValue"
    @update:model-value="emit('update:modelValue', $event as PricingTabId)"
  >
    <UiTabsList
      label="价格策略视图"
      :class="$style.tabs"
    >
      <UiTabsTrigger
        v-for="tab in tabs"
        :key="tab.id"
        :value="tab.id"
      >
        <component
          :is="tab.icon"
          :size="15"
          :stroke-width="1.8"
        />{{ tab.label }}
      </UiTabsTrigger>
    </UiTabsList>
  </UiTabs>
</template>

<style module>
@reference "../../../../styles/index.css";

.tabs {
  @apply mt-3 flex min-h-[48px] w-full gap-0 border-b;
  border-color: var(--ui-border);
}
.tabs button[data-state] {
  @apply min-h-[47px] rounded-none px-4 text-[12px];
  background: transparent;
}
.tabs button[data-state='active'] {
  @apply font-semibold;
  box-shadow: inset 0 -2px 0 var(--app-primary);
}
</style>
