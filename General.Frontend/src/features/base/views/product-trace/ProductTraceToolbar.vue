<script setup lang="ts">
import { Plus, Search } from 'lucide-vue-next';
import { UiButton, UiInput, UiSelect } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';

defineProps<{ query: string; status: string; source: string; warranty: string }>();
const emit = defineEmits<{
  'update:query': [value: string];
  'update:status': [value: string];
  'update:source': [value: string];
  'update:warranty': [value: string];
  create: [];
}>();
const statusOptions: UiSelectOption[] = [
  '全部状态',
  '在库',
  '已出库',
  '已报修',
  '维修中',
  '已换货',
  '已退回',
  '已报废',
].map((value) => ({ value, label: value }));
const sourceOptions: UiSelectOption[] = ['全部来源', '外购', '自制', '委外'].map((value) => ({ value, label: value }));
const warrantyOptions: UiSelectOption[] = ['全部质保', '未开始', '在保', '即将到期', '已过保'].map((value) => ({
  value,
  label: value,
}));
</script>

<template>
  <div :class="$style.toolbar">
    <label :class="$style.search"
      ><Search
        :size="14"
        :stroke-width="1.8" /><UiInput
        :model-value="query"
        aria-label="搜索追溯码外部SN商品名称或客户"
        placeholder="搜索追溯码、外部SN、商品名称、客户..."
        @update:model-value="emit('update:query', $event)"
    /></label>
    <UiSelect
      :model-value="status"
      :options="statusOptions"
      label="状态"
      size="sm"
      @update:model-value="emit('update:status', $event)"
    />
    <UiSelect
      :model-value="source"
      :options="sourceOptions"
      label="来源"
      size="sm"
      @update:model-value="emit('update:source', $event)"
    />
    <UiSelect
      :model-value="warranty"
      :options="warrantyOptions"
      label="质保"
      size="sm"
      @update:model-value="emit('update:warranty', $event)"
    />
    <UiButton
      :class="$style.create"
      variant="primary"
      size="sm"
      @click="emit('create')"
      ><template #leading
        ><Plus
          :size="14"
          :stroke-width="1.8" /></template
      >生成追溯码</UiButton
    >
  </div>
</template>

<style module>
@reference "../../../../styles/index.css";
.toolbar {
  @apply flex min-h-[54px] items-center gap-3;
}
.search {
  @apply relative block w-[410px];
}
.search > svg {
  @apply pointer-events-none absolute left-3 top-1/2 z-[1] -translate-y-1/2;
  color: var(--ui-text-subtle);
}
.search input {
  @apply pl-8;
}
.create {
  @apply ml-auto;
}
@media (max-width: 900px) {
  .toolbar {
    @apply flex-wrap;
  }
  .search {
    @apply w-full;
  }
  .create {
    @apply ml-0;
  }
}
</style>
