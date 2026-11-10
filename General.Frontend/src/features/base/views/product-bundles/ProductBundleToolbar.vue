<script setup lang="ts">
import { Plus, Search } from 'lucide-vue-next';
import { UiButton, UiInput } from '@/components/ui';
import type { ProductBundleStatusFilter } from '../../model/product-bundles.types';

defineProps<{ query: string; status: ProductBundleStatusFilter; total: number }>();
const emit = defineEmits<{
  'update:query': [value: string];
  'update:status': [value: ProductBundleStatusFilter];
  create: [];
}>();

const statuses: readonly { id: ProductBundleStatusFilter; label: string }[] = [
  { id: 'all', label: '全部' },
  { id: 'active', label: '启用' },
  { id: 'inactive', label: '停用' },
];
</script>

<template>
  <div :class="$style.toolbar">
    <div
      :class="$style.statuses"
      aria-label="状态筛选"
    >
      <button
        v-for="item in statuses"
        :key="item.id"
        type="button"
        :class="[$style.statusButton, status === item.id && $style.statusActive]"
        :aria-pressed="status === item.id"
        @click="emit('update:status', item.id)"
      >
        {{ item.label }}
      </button>
    </div>
    <label :class="$style.search">
      <Search
        :size="14"
        :stroke-width="1.8"
      />
      <UiInput
        :model-value="query"
        aria-label="搜索组合名称或编号"
        placeholder="搜索组合名称或编号..."
        @update:model-value="emit('update:query', $event)"
      />
    </label>
    <span :class="$style.total">共 {{ total }} 个组合</span>
    <UiButton
      size="sm"
      variant="primary"
      @click="emit('create')"
    >
      <template #leading
        ><Plus
          :size="14"
          :stroke-width="1.8"
      /></template>
      新建商品组合
    </UiButton>
  </div>
</template>

<style module>
@reference "../../../../styles/index.css";

.toolbar {
  @apply flex min-h-[44px] items-center gap-3;
}
.statuses {
  @apply flex items-center rounded bg-[#F4F5F7] p-0.5;
}
.statusButton {
  @apply min-h-7 rounded px-3 text-[12px] font-medium transition-colors;
  color: var(--ui-text-muted);
}
.statusButton:hover {
  @apply bg-white/70;
}
.statusActive {
  @apply bg-white font-semibold shadow-sm;
  color: var(--app-primary);
}
.search {
  @apply relative ml-1 block w-[280px];
}
.search > svg {
  @apply pointer-events-none absolute left-3 top-1/2 z-[1] -translate-y-1/2;
  color: var(--ui-text-subtle);
}
.search input {
  @apply h-8 bg-[#F4F5F7] pl-8;
  border-color: transparent;
}
.total {
  @apply ml-auto text-[12px];
  color: var(--ui-text-subtle);
}

@media (max-width: 820px) {
  .toolbar {
    @apply flex-wrap;
  }
  .search {
    @apply order-3 ml-0 w-full;
  }
  .total {
    @apply ml-auto;
  }
}
</style>
