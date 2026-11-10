<script setup lang="ts">
import { Search } from 'lucide-vue-next';
import { UiInput, UiSelect } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';

defineProps<{
  query: string;
  region: string;
  type: string;
  urgency: string;
  regionOptions: readonly UiSelectOption[];
  typeOptions: readonly UiSelectOption[];
  urgencyOptions: readonly UiSelectOption[];
  resultCount: number;
}>();

const emit = defineEmits<{
  'update:query': [value: string];
  'update:region': [value: string];
  'update:type': [value: string];
  'update:urgency': [value: string];
}>();
</script>

<template>
  <section
    :class="$style.toolbar"
    aria-label="待分派筛选"
  >
    <label :class="$style.searchField">
      <Search
        :size="14"
        :stroke-width="1.8"
        aria-hidden="true"
      />
      <UiInput
        :model-value="query"
        :class="$style.searchInput"
        placeholder="工单号 / 客户 / 标题"
        aria-label="搜索待分派工单"
        @update:model-value="emit('update:query', $event)"
      />
    </label>
    <UiSelect
      :model-value="region"
      :class="$style.regionSelect"
      :options="regionOptions"
      label="地区"
      size="sm"
      @update:model-value="emit('update:region', $event)"
    />
    <UiSelect
      :model-value="type"
      :class="$style.typeSelect"
      :options="typeOptions"
      label="服务类型"
      size="sm"
      @update:model-value="emit('update:type', $event)"
    />
    <UiSelect
      :model-value="urgency"
      :class="$style.urgencySelect"
      :options="urgencyOptions"
      label="紧急度"
      size="sm"
      @update:model-value="emit('update:urgency', $event)"
    />
    <span :class="$style.resultCount">共 {{ resultCount }} 单待分派</span>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.toolbar {
  @apply mb-3 flex flex-wrap items-center gap-2;
}

.searchField {
  @apply relative block w-[280px] shrink-0;
}

.searchField > svg {
  @apply pointer-events-none absolute left-3 top-1/2 z-[1] -translate-y-1/2;
  color: var(--ui-text-subtle);
}

.searchInput {
  @apply h-8 pl-9 text-[13px];
}

.regionSelect {
  @apply w-[120px];
}

.typeSelect,
.urgencySelect {
  @apply w-[120px];
}

.resultCount {
  @apply ml-auto text-[12px];
  color: var(--ui-text-subtle);
}
</style>
