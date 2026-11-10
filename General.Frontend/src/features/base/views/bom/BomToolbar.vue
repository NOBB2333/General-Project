<script setup lang="ts">
import { Search } from 'lucide-vue-next';
import { UiInput } from '@/components/ui';
import type { BomStatusFilter, BomTypeFilter } from '../../model/bom.types';

defineProps<{ status: BomStatusFilter; type: BomTypeFilter; query: string; total: number }>();
const emit = defineEmits<{
  'update:status': [value: BomStatusFilter];
  'update:type': [value: BomTypeFilter];
  'update:query': [value: string];
}>();
const statuses: readonly { id: BomStatusFilter; label: string; count: number }[] = [
  { id: 'all', label: '全部', count: 4 },
  { id: 'draft', label: '草稿', count: 1 },
  { id: 'review', label: '待评审', count: 1 },
  { id: 'active', label: '已生效', count: 2 },
  { id: 'invalid', label: '已失效', count: 0 },
  { id: 'archived', label: '已归档', count: 0 },
];
const types: readonly { id: BomTypeFilter; label: string }[] = [
  { id: 'all', label: '全部类型' },
  { id: 'standard', label: '标准' },
  { id: 'project', label: '项目' },
];
</script>

<template>
  <div :class="$style.toolbar">
    <div
      :class="$style.statuses"
      aria-label="BOM 状态"
    >
      <button
        v-for="item in statuses"
        :key="item.id"
        type="button"
        :class="[$style.textButton, status === item.id && $style.active]"
        :aria-pressed="status === item.id"
        @click="emit('update:status', item.id)"
      >
        {{ item.label }} <small>{{ item.count }}</small>
      </button>
    </div>
    <i :class="$style.divider" />
    <div
      :class="$style.types"
      aria-label="BOM 类型"
    >
      <button
        v-for="item in types"
        :key="item.id"
        type="button"
        :class="[$style.typeButton, type === item.id && $style.typeActive]"
        :aria-pressed="type === item.id"
        @click="emit('update:type', item.id)"
      >
        {{ item.label }}
      </button>
    </div>
    <span :class="$style.total">共 {{ total }} 项</span>
    <label :class="$style.search"
      ><Search
        :size="14"
        :stroke-width="1.8" /><UiInput
        :model-value="query"
        aria-label="搜索BOM编号名称或产品"
        placeholder="搜索编号/名称/产品..."
        @update:model-value="emit('update:query', $event)"
    /></label>
  </div>
</template>

<style module>
@reference "../../../../styles/index.css";
.toolbar {
  @apply flex min-h-[52px] items-center gap-2 border-b px-3;
  border-color: var(--ui-border);
}
.statuses,
.types {
  @apply flex items-center gap-0.5;
}
.textButton {
  @apply min-h-8 rounded px-2 text-[11.5px] font-medium transition-colors;
  color: var(--ui-text-muted);
}
.textButton:hover,
.typeButton:hover {
  background: var(--ui-surface-hover);
}
.textButton small {
  @apply ml-0.5 text-[10px];
  color: var(--ui-text-subtle);
}
.active {
  @apply bg-[#DEEBFF] font-semibold;
  color: var(--app-primary);
}
.divider {
  @apply mx-1 h-5 w-px;
  background: var(--ui-border);
}
.typeButton {
  @apply min-h-8 rounded px-2.5 text-[11.5px] font-medium;
  color: var(--ui-text-muted);
}
.typeActive {
  @apply bg-[#DEEBFF] font-semibold;
  color: var(--app-primary);
}
.total {
  @apply ml-auto shrink-0 text-[11.5px];
  color: var(--ui-text-subtle);
}
.search {
  @apply relative block w-[250px];
}
.search > svg {
  @apply pointer-events-none absolute left-3 top-1/2 z-[1] -translate-y-1/2;
  color: var(--ui-text-subtle);
}
.search input {
  @apply h-8 pl-8;
}
@media (max-width: 1100px) {
  .types,
  .divider {
    @apply hidden;
  }
}
</style>
