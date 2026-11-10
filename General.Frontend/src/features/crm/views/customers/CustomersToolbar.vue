<script setup lang="ts">
import { Archive, BarChart3, Download, ScanLine, Search, Upload } from 'lucide-vue-next';
import { computed } from 'vue';
import { UiButton, UiInput, UiSelect } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';

const props = defineProps<{
  query: string;
  owner: string;
  tag: string;
  receivableOnly: boolean;
  analyticsOpen: boolean;
  recycleCount: number;
  owners: readonly string[];
  tags: readonly string[];
}>();

const emit = defineEmits<{
  'update:query': [value: string];
  'update:owner': [value: string];
  'update:tag': [value: string];
  'update:receivableOnly': [value: boolean];
  action: [id: 'create' | 'import' | 'scan' | 'export' | 'recycle-bin' | 'analytics'];
}>();

const ownerOptions = computed<UiSelectOption[]>(() => [
  { value: '', label: '负责人' },
  ...props.owners.map((owner) => ({ value: owner, label: owner })),
]);
const tagOptions = computed<UiSelectOption[]>(() => [
  { value: '', label: '标签' },
  ...props.tags.map((tag) => ({ value: tag, label: tag })),
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
        新增客户
      </UiButton>
      <UiButton
        size="sm"
        @click="emit('action', 'import')"
      >
        <template #leading><Upload :size="14" /></template>
        批量导入
      </UiButton>
      <UiButton
        size="sm"
        @click="emit('action', 'scan')"
      >
        <template #leading><ScanLine :size="14" /></template>
        扫描名片
      </UiButton>
      <UiButton
        size="sm"
        @click="emit('action', 'export')"
      >
        <template #leading><Download :size="14" /></template>
        导出
      </UiButton>
      <UiButton
        size="sm"
        @click="emit('action', 'recycle-bin')"
      >
        <template #leading><Archive :size="14" /></template>
        回收站
        <span
          v-if="recycleCount"
          :class="$style.badge"
          >{{ recycleCount }}</span
        >
      </UiButton>
      <UiButton
        :variant="analyticsOpen ? 'subtle' : 'secondary'"
        size="sm"
        @click="emit('action', 'analytics')"
      >
        <template #leading><BarChart3 :size="14" /></template>
        统计分析
      </UiButton>
    </div>

    <label :class="$style.search">
      <Search
        :size="14"
        :stroke-width="1.8"
      />
      <UiInput
        :model-value="query"
        placeholder="搜索客户名称、联系人..."
        aria-label="搜索客户名称、联系人"
        :class="$style.searchInput"
        @update:model-value="emit('update:query', $event)"
      />
    </label>
    <UiSelect
      :model-value="owner"
      :options="ownerOptions"
      label="按负责人筛选"
      size="sm"
      @update:model-value="emit('update:owner', $event)"
    />
    <UiSelect
      :model-value="tag"
      :options="tagOptions"
      label="按标签筛选"
      size="sm"
      @update:model-value="emit('update:tag', $event)"
    />
    <UiButton
      :variant="receivableOnly ? 'subtle' : 'secondary'"
      size="sm"
      @click="emit('update:receivableOnly', !receivableOnly)"
    >
      应收款
    </UiButton>
  </div>
</template>

<style module>
@reference "../../../../styles/index.css";

.toolbar {
  @apply flex min-h-[60px] items-center gap-2 border-b px-4 py-3;
  border-color: var(--ui-border-subtle);
}

.actions {
  @apply flex min-w-0 items-center gap-2;
}

.badge {
  @apply ml-1 rounded-full px-1.5 py-0.5 text-[9px] font-semibold text-white;
  background: var(--ui-danger);
}

.search {
  @apply ml-auto flex h-8 min-w-[220px] items-center gap-2 rounded px-3;
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

@media (max-width: 1200px) {
  .toolbar {
    @apply flex-wrap;
  }

  .search {
    @apply ml-0;
  }
}
</style>
