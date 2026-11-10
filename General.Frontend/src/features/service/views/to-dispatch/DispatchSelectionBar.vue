<script setup lang="ts">
import { UserRoundPlus } from 'lucide-vue-next';
import { UiButton, UiCheckbox } from '@/components/ui';

type CheckedState = boolean | 'indeterminate';

defineProps<{
  selectionState: CheckedState;
  selectedCount: number;
}>();

const emit = defineEmits<{
  toggleAll: [checked: boolean];
  dispatch: [];
}>();
</script>

<template>
  <section :class="$style.bar">
    <label :class="$style.selectAll">
      <UiCheckbox
        :model-value="selectionState"
        label="全选当前待分派工单"
        @update:model-value="emit('toggleAll', $event === true)"
      />
      <strong>全选</strong>
    </label>
    <span>已选 {{ selectedCount }} 单</span>
    <UiButton
      :class="$style.batchButton"
      variant="primary"
      size="sm"
      :disabled="selectedCount === 0"
      @click="emit('dispatch')"
    >
      <template #leading>
        <UserRoundPlus
          :size="13"
          :stroke-width="1.8"
        />
      </template>
      批量派工
    </UiButton>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.bar {
  @apply mb-3 flex min-h-[49px] items-center gap-3 rounded-lg border bg-white px-4 py-2.5 text-[12px];
  color: var(--ui-text-subtle);
  border-color: var(--ui-border);
}

.selectAll {
  @apply flex cursor-pointer items-center gap-1.5;
  color: var(--ui-text-muted);
}

.selectAll strong {
  @apply text-[12px] font-semibold;
}

.batchButton {
  @apply ml-auto;
}
</style>
