<script setup lang="ts">
import { ClipboardPaste, Download, FileSpreadsheet, Plus, Search } from 'lucide-vue-next';
import { computed } from 'vue';
import { UiButton, UiInput, UiSelect } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import type { CustomerMaterialToolbarAction } from '@/features/crm/model/customer-materials.types';

const props = defineProps<{
  query: string;
  customer: string;
  customers: readonly string[];
}>();

const emit = defineEmits<{
  'update:query': [value: string];
  'update:customer': [value: string];
  action: [id: CustomerMaterialToolbarAction];
}>();

const customerOptions = computed<UiSelectOption[]>(() => [
  { value: '', label: '按客户筛选' },
  ...props.customers.map((customer) => ({ value: customer, label: customer })),
]);
</script>

<template>
  <div :class="$style.toolbar">
    <div :class="$style.actions">
      <UiButton
        variant="primary"
        size="sm"
        @click="emit('action', 'create')"
      >
        <template #leading><Plus :size="14" /></template>
        新增对照
      </UiButton>
      <UiButton
        size="sm"
        @click="emit('action', 'import-file')"
      >
        <template #leading><FileSpreadsheet :size="14" /></template>
        导入清单
      </UiButton>
      <UiButton
        size="sm"
        @click="emit('action', 'import-paste')"
      >
        <template #leading><ClipboardPaste :size="14" /></template>
        粘贴导入
      </UiButton>
      <UiButton
        size="sm"
        @click="emit('action', 'export')"
      >
        <template #leading><Download :size="14" /></template>
        导出
      </UiButton>
    </div>

    <label :class="$style.search">
      <Search
        :size="14"
        :stroke-width="1.8"
      />
      <UiInput
        :model-value="query"
        :class="$style.searchInput"
        placeholder="搜索我方/客户的编码或名称..."
        aria-label="搜索物料对照"
        @update:model-value="emit('update:query', $event)"
      />
    </label>
    <div :class="$style.customerFilter">
      <UiSelect
        :model-value="customer"
        :options="customerOptions"
        label="按客户筛选"
        size="sm"
        @update:model-value="emit('update:customer', $event)"
      />
    </div>
  </div>
</template>

<style module>
@reference "../../../../styles/index.css";

.toolbar {
  @apply flex min-h-[60px] items-center gap-3 border-b px-4 py-3;
  border-color: var(--ui-border-subtle);
}

.actions {
  @apply flex items-center gap-2;
}

.search {
  @apply ml-auto flex h-8 min-w-[300px] items-center gap-2 rounded px-3;
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
}

.search:focus-within {
  box-shadow: 0 0 0 1px var(--ui-focus);
  background: var(--ui-surface);
}

.searchInput {
  @apply h-8 border-0 bg-transparent px-0 shadow-none;
}

.searchInput:focus {
  border-color: transparent;
  box-shadow: none;
}

.customerFilter {
  @apply w-[150px];
}

@media (max-width: 1040px) {
  .toolbar {
    @apply flex-wrap;
  }

  .search {
    @apply ml-0;
  }
}
</style>
