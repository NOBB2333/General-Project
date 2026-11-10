<script setup lang="ts">
import { Download, Plus, Search } from 'lucide-vue-next';
import { computed } from 'vue';
import { UiButton, UiInput, UiSelect, UiTabs, UiTabsList, UiTabsTrigger } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import type { PaymentFlowRow, PaymentFlowScope } from '../../model/payment-flow.types';

const props = defineProps<{
  rows: readonly PaymentFlowRow[];
  activeScope: PaymentFlowScope;
  supplier: string;
  query: string;
  supplierOptions: readonly UiSelectOption[];
}>();

const emit = defineEmits<{
  'update:activeScope': [value: PaymentFlowScope];
  'update:supplier': [value: string];
  'update:query': [value: string];
  export: [];
  create: [];
}>();

const tabs = computed(() => [
  { value: 'all', label: '全部', count: props.rows.length },
  {
    value: 'incomplete',
    label: '未完成分配',
    count: props.rows.filter((row) => row.status !== 'allocated').length,
  },
  {
    value: 'allocated',
    label: '已分配',
    count: props.rows.filter((row) => row.status === 'allocated').length,
  },
]);
</script>

<template>
  <header :class="$style.toolbar">
    <UiTabs
      :model-value="activeScope"
      @update:model-value="emit('update:activeScope', $event as PaymentFlowScope)"
    >
      <UiTabsList
        label="付款流水分配状态"
        :class="$style.tabs"
      >
        <UiTabsTrigger
          v-for="tab in tabs"
          :key="tab.value"
          :value="tab.value"
          :class="$style.tab"
        >
          {{ tab.label }} <span>{{ tab.count }}</span>
        </UiTabsTrigger>
      </UiTabsList>
    </UiTabs>

    <div :class="$style.tools">
      <UiSelect
        :model-value="supplier"
        :options="supplierOptions"
        label="供应商筛选"
        size="sm"
        :class="$style.supplierSelect"
        @update:model-value="emit('update:supplier', $event)"
      />
      <label :class="$style.searchField">
        <Search
          :size="14"
          :stroke-width="1.8"
          aria-hidden="true"
        />
        <UiInput
          :model-value="query"
          placeholder="搜索流水号、供应商、订单、备注..."
          aria-label="搜索付款流水"
          @update:model-value="emit('update:query', $event)"
        />
      </label>
      <UiButton
        variant="secondary"
        size="sm"
        @click="emit('export')"
      >
        <template #leading><Download :size="14" /></template>
        导出
      </UiButton>
      <UiButton
        variant="primary"
        size="sm"
        @click="emit('create')"
      >
        <template #leading><Plus :size="14" /></template>
        申请付款
      </UiButton>
    </div>
  </header>
</template>

<style module>
@reference "../../../../styles/index.css";

.toolbar {
  @apply flex min-h-[62px] items-end justify-between gap-5 border-b px-4;
  border-color: var(--ui-border);
}

.tabs {
  @apply gap-0;
}

.tab {
  @apply min-h-[46px] rounded-none border-b-2 border-transparent px-4 text-[12px];
}

.tab span {
  @apply ml-0.5 text-[10px];
  color: #97a0af;
}

.tab[data-state='active'] {
  color: var(--app-primary);
  border-bottom-color: var(--app-primary);
  background: transparent;
}

.tools {
  @apply flex items-center gap-2 pb-2;
}

.supplierSelect {
  @apply w-[150px];
}

.searchField {
  @apply flex h-8 w-[250px] items-center gap-2 rounded px-2;
  color: var(--ui-text-subtle);
  background: var(--ui-surface-subtle);
}

.searchField input {
  @apply h-full border-0 bg-transparent p-0 shadow-none;
}
</style>
