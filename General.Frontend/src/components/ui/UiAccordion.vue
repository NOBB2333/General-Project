<script setup lang="ts">
import { AccordionRoot } from 'reka-ui';

defineProps<{
  modelValue: string;
  disabled?: boolean;
}>();

const emit = defineEmits<{
  'update:modelValue': [value: string];
}>();
</script>

<template>
  <AccordionRoot
    :model-value="modelValue"
    :disabled="disabled"
    :class="$style.root"
    type="single"
    collapsible
    @update:model-value="emit('update:modelValue', String($event || ''))"
  >
    <slot />
  </AccordionRoot>
</template>

<style module>
@reference "../../styles/index.css";

.root {
  @apply min-w-0;
}
</style>
