<script setup lang="ts">
import { Star, UserRound, UsersRound } from 'lucide-vue-next';
import { UiSelect } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import type { PurchaseOrderScope } from '../../model/purchase-orders.types';

defineProps<{
  activeScope: PurchaseOrderScope;
  counts: Record<PurchaseOrderScope, number>;
  supplier: string;
  supplierOptions: readonly UiSelectOption[];
}>();

const emit = defineEmits<{
  'update:activeScope': [value: PurchaseOrderScope];
  'update:supplier': [value: string];
}>();

const scopes: readonly { value: PurchaseOrderScope; label: string; icon: typeof UserRound }[] = [
  { value: 'all', label: '所有', icon: UsersRound },
  { value: 'mine', label: '我负责的', icon: UserRound },
  { value: 'team', label: '下属负责的', icon: UsersRound },
  { value: 'followed', label: '我关注的', icon: Star },
];
</script>

<template>
  <section :class="$style.bar">
    <div :class="$style.scopes">
      <button
        v-for="scope in scopes"
        :key="scope.value"
        type="button"
        :class="[$style.scope, activeScope === scope.value && $style.scopeActive]"
        :aria-pressed="activeScope === scope.value"
        @click="emit('update:activeScope', scope.value)"
      >
        <component
          :is="scope.icon"
          :size="14"
          :stroke-width="1.8"
        />
        {{ scope.label }}
        <span>{{ counts[scope.value] }}</span>
      </button>
    </div>
    <UiSelect
      :model-value="supplier"
      :options="supplierOptions"
      label="供应商筛选"
      :class="$style.supplier"
      @update:model-value="emit('update:supplier', $event)"
    />
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.bar {
  @apply mt-4 flex h-[58px] items-center justify-between rounded border bg-white px-1;
  border-color: var(--ui-border);
}

.scopes {
  @apply flex h-full items-center gap-1;
}

.scope {
  @apply inline-flex h-10 items-center gap-2 rounded px-4 text-[12px] font-semibold transition-colors;
  color: var(--ui-text-muted);
}

.scope:hover {
  background: var(--ui-surface-hover);
}

.scope span {
  @apply flex h-5 min-w-5 items-center justify-center rounded-full px-1 text-[10px];
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
}

.scopeActive {
  color: white;
  background: var(--app-primary);
}

.scopeActive:hover {
  background: var(--app-primary);
}

.scopeActive span {
  color: white;
  background: rgb(255 255 255 / 20%);
}

.supplier {
  @apply mr-1 w-[155px];
}
</style>
