<script setup lang="ts">
import { Ellipsis, Plus, Search } from 'lucide-vue-next';
import { UiButton, UiDropdownMenu, UiInput } from '@/components/ui';

defineProps<{ query: string }>();
const emit = defineEmits<{ 'update:query': [value: string]; create: []; export: [] }>();

const menuItems = [
  { id: 'export', label: '导出当前项目' },
  { id: 'refresh', label: '刷新列表' },
] as const;
</script>

<template>
  <section :class="$style.titleBar">
    <div>
      <h2>招投标管理</h2>
      <p>管理所有投标项目的进度、风险、任务和保证金状态</p>
    </div>
    <label :class="$style.search">
      <Search :size="14" />
      <UiInput
        :model-value="query"
        :class="$style.searchInput"
        placeholder="搜索项目名称、编号、招标单位..."
        aria-label="搜索投标项目"
        @update:model-value="emit('update:query', $event)"
      />
    </label>
    <UiDropdownMenu
      :items="menuItems"
      label="更多投标操作"
      @select="$event === 'export' && emit('export')"
    >
      <template #trigger>
        <UiButton
          size="icon"
          aria-label="更多投标操作"
        >
          <Ellipsis :size="16" />
        </UiButton>
      </template>
    </UiDropdownMenu>
    <UiButton
      variant="primary"
      @click="emit('create')"
    >
      <template #leading><Plus :size="15" /></template>
      新建投标项目
    </UiButton>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.titleBar {
  @apply mt-4 flex min-h-[82px] items-center border-b bg-white px-0;
  border-color: var(--ui-border);
}
.titleBar > div:first-child {
  @apply mr-auto;
}
.titleBar h2 {
  @apply text-[21px] font-semibold;
  color: var(--ui-text);
}
.titleBar p {
  @apply mt-1 text-[12px];
  color: var(--ui-text-subtle);
}
.search {
  @apply mr-2 flex h-9 w-[300px] items-center gap-2 rounded border px-3;
  color: #97a0af;
  border-color: var(--ui-border);
}
.searchInput {
  @apply h-auto border-0 bg-transparent p-0;
  box-shadow: none;
}
.searchInput:focus {
  box-shadow: none;
}
</style>
