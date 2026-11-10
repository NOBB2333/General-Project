<script setup lang="ts">
import { Search } from 'lucide-vue-next';
import { UiInput } from '@/components/ui';
import type { SupplierScope } from '../../model/suppliers.types';

defineProps<{ modelValue: SupplierScope; query: string; followedCount: number }>();
const emit = defineEmits<{
  'update:modelValue': [value: SupplierScope];
  'update:query': [value: string];
}>();

const scopes: readonly { id: SupplierScope; label: string }[] = [
  { id: 'all', label: '所有' },
  { id: 'mine', label: '我负责的' },
  { id: 'subordinate', label: '下属负责的' },
  { id: 'starred', label: '我关注的' },
];
</script>

<template>
  <div :class="$style.bar">
    <button
      v-for="scope in scopes"
      :key="scope.id"
      type="button"
      :class="[$style.scope, modelValue === scope.id && $style.active]"
      :aria-pressed="modelValue === scope.id"
      @click="emit('update:modelValue', scope.id)"
    >
      {{ scope.label }}
      <span
        v-if="scope.id === 'starred' && followedCount"
        :class="$style.count"
      >
        {{ followedCount }}
      </span>
    </button>
    <label :class="$style.search">
      <Search
        :size="14"
        :stroke-width="1.8"
      />
      <UiInput
        :model-value="query"
        aria-label="搜索供应商名称"
        placeholder="搜索供应商名称..."
        @update:model-value="emit('update:query', $event)"
      />
    </label>
  </div>
</template>

<style module>
@reference "../../../../styles/index.css";

.bar {
  @apply flex min-h-[36px] items-end border-b;
  border-color: var(--ui-border);
}
.scope {
  @apply relative flex min-h-9 items-center gap-1 px-4 text-[13px] transition-colors;
  color: #5e6c84;
}
.scope:hover {
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
.count {
  @apply rounded-full bg-[#EBECF0] px-1.5 py-0.5 text-[10px] font-semibold;
  color: #5e6c84;
}
.search {
  @apply relative ml-auto block w-[260px];
}
.search > svg {
  @apply pointer-events-none absolute left-3 top-1/2 z-[1] -translate-y-1/2;
  color: var(--ui-text-subtle);
}
.search input {
  @apply h-8 border-0 bg-[#F4F5F7] pl-8;
  box-shadow: none;
}
@media (max-width: 720px) {
  .bar {
    @apply flex-wrap;
  }
  .search {
    @apply order-2 ml-0 w-full;
  }
}
</style>
