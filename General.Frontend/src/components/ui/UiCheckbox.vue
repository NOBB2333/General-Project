<script setup lang="ts">
import { Check, Minus } from 'lucide-vue-next';
import { CheckboxIndicator, CheckboxRoot } from 'reka-ui';

type CheckedState = boolean | 'indeterminate';
defineProps<{ modelValue: CheckedState; disabled?: boolean; label?: string }>();
const emit = defineEmits<{ 'update:modelValue': [value: CheckedState] }>();
</script>

<template>
  <CheckboxRoot
    :model-value="modelValue"
    :disabled="disabled"
    :aria-label="label"
    :class="$style.checkbox"
    @update:model-value="emit('update:modelValue', $event as CheckedState)"
  >
    <CheckboxIndicator :class="$style.indicator">
      <Minus
        v-if="modelValue === 'indeterminate'"
        :size="11"
        :stroke-width="2.5"
      />
      <Check
        v-else
        :size="11"
        :stroke-width="2.5"
      />
    </CheckboxIndicator>
  </CheckboxRoot>
</template>

<style module>
@reference "../../styles/index.css";
.checkbox {
  @apply flex h-4 w-4 shrink-0 items-center justify-center border bg-white text-white outline-none transition-colors disabled:cursor-not-allowed disabled:opacity-50;
  border-color: #c1c7d0;
  border-radius: var(--app-compact-control-radius);
}

.checkbox:focus-visible {
  box-shadow: 0 0 0 2px color-mix(in srgb, var(--ui-focus) 40%, transparent);
}
.checkbox[data-state='checked'],
.checkbox[data-state='indeterminate'] {
  border-color: var(--app-primary);
  background: var(--app-primary);
}
.indicator {
  @apply flex items-center justify-center;
}
</style>
