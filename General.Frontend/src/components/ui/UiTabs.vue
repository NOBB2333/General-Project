<script setup lang="ts">
import { TabsRoot } from 'reka-ui';

defineOptions({ inheritAttrs: false });

withDefaults(
  defineProps<{
    modelValue: string;
    orientation?: 'horizontal' | 'vertical';
    activationMode?: 'automatic' | 'manual';
  }>(),
  {
    orientation: 'horizontal',
    activationMode: 'automatic',
  },
);

const emit = defineEmits<{
  'update:modelValue': [value: string];
}>();

defineSlots<{
  default: (props: { value: string }) => unknown;
}>();
</script>

<template>
  <TabsRoot
    v-bind="$attrs"
    :model-value="modelValue"
    :orientation="orientation"
    :activation-mode="activationMode"
    :class="$style.root"
    @update:model-value="emit('update:modelValue', String($event))"
  >
    <slot :value="modelValue" />
  </TabsRoot>
</template>

<style module>
@reference "../../styles/index.css";

.root {
  @apply contents;
}
</style>
