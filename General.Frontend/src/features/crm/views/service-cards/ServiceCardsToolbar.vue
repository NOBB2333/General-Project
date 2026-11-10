<script setup lang="ts">
import { Plus, Search } from 'lucide-vue-next';
import { computed } from 'vue';
import { UiButton, UiInput, UiSelect } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import type { ServiceCardFilter } from '@/features/crm/model/service-cards.types';

const props = defineProps<{
  query: string;
  customer: string;
  status: ServiceCardFilter;
  customers: readonly string[];
}>();

const emit = defineEmits<{
  'update:query': [value: string];
  'update:customer': [value: string];
  'update:status': [value: ServiceCardFilter];
  create: [];
}>();

const tabs: { value: ServiceCardFilter; label: string }[] = [
  { value: 'all', label: '全部' },
  { value: 'active', label: '生效中' },
  { value: 'expiring', label: '即将到期' },
  { value: 'expired', label: '已过期' },
];

const customerOptions = computed<UiSelectOption[]>(() => [
  { value: '', label: '按客户筛选' },
  ...props.customers.map((customer) => ({ value: customer, label: customer })),
]);
</script>

<template>
  <section :class="$style.toolbar">
    <UiButton
      variant="primary"
      size="sm"
      :class="$style.createButton"
      @click="emit('create')"
    >
      <template #leading><Plus :size="14" /></template>
      录入服务
    </UiButton>
    <div
      :class="$style.tabs"
      role="tablist"
      aria-label="服务卡状态"
    >
      <button
        v-for="tab in tabs"
        :key="tab.value"
        type="button"
        role="tab"
        :aria-selected="status === tab.value"
        :class="[$style.tab, status === tab.value && $style.tabActive]"
        @click="emit('update:status', tab.value)"
      >
        {{ tab.label }}
      </button>
    </div>
    <label :class="$style.search">
      <Search
        :size="14"
        :stroke-width="1.8"
      />
      <UiInput
        :model-value="query"
        :class="$style.searchInput"
        placeholder="搜索客户、服务名称、卡号..."
        aria-label="搜索服务权益卡"
        @update:model-value="emit('update:query', $event)"
      />
    </label>
    <UiSelect
      :model-value="customer"
      :options="customerOptions"
      label="按客户筛选"
      size="sm"
      :class="$style.customerFilter"
      @update:model-value="emit('update:customer', $event)"
    />
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.toolbar {
  @apply mt-4 flex min-h-9 items-center gap-3;
}

.createButton {
  @apply h-[34px] min-h-[34px] px-5 text-[13px];
}

.tabs {
  @apply flex h-[29px] items-stretch overflow-hidden rounded border;
  border-color: var(--ui-border-subtle);
  background: var(--ui-surface-hover);
}

.tab {
  @apply min-h-[29px] min-w-[54px] px-3 text-[12px] font-medium transition-colors;
  color: var(--ui-text-subtle);
}

.tab:hover {
  color: var(--ui-text);
}

.tabActive {
  color: var(--app-primary);
  background: white;
  box-shadow: 0 1px 3px rgb(9 30 66 / 12%);
}

.search {
  @apply ml-auto flex h-8 min-w-[250px] items-center gap-2 rounded border px-3;
  color: #97a0af;
  background: white;
  border-color: var(--ui-border);
}

.searchInput {
  @apply h-auto border-0 p-0;
  box-shadow: none;
}

.searchInput:focus {
  box-shadow: none;
}

.customerFilter {
  @apply min-w-[118px];
}

@media (max-width: 900px) {
  .toolbar {
    @apply flex-wrap;
  }

  .search {
    @apply ml-0 flex-1;
  }
}
</style>
