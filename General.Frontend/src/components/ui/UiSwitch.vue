<script setup lang="ts">
import { SwitchRoot, SwitchThumb } from 'reka-ui';

type SwitchSize = 'sm' | 'md';
const props = withDefaults(
  defineProps<{
    modelValue: boolean;
    size?: SwitchSize;
    tone?: 'primary' | 'success';
    disabled?: boolean;
    label?: string;
  }>(),
  { size: 'md', tone: 'primary' },
);

const emit = defineEmits<{ 'update:modelValue': [value: boolean] }>();
</script>

<template>
  <SwitchRoot
    :model-value="modelValue"
    :disabled="disabled"
    :aria-label="label"
    :class="[$style.switch, $style[size], tone === 'success' && $style.success]"
    @update:model-value="emit('update:modelValue', $event)"
  >
    <SwitchThumb :class="[$style.thumb, size === 'sm' ? $style.thumbSm : $style.thumbMd]" />
  </SwitchRoot>
</template>

<style module>
@reference "../../styles/index.css";
.switch {
  @apply relative inline-flex shrink-0 items-center rounded-full transition-colors disabled:cursor-not-allowed disabled:opacity-50;
  background: #c1c7d0;
}

.switch[data-state='checked'] {
  background: var(--app-primary);
}
.switch.success[data-state='checked'] {
  background: var(--ui-success);
}
.sm {
  @apply h-4 w-7;
}
.md {
  @apply h-5 w-10;
}
.thumb {
  @apply block rounded-full bg-white shadow transition-transform;
}
.thumbSm {
  @apply h-3 w-3;
}
.thumbSm[data-state='checked'] {
  transform: translateX(14px);
}
.thumbSm[data-state='unchecked'] {
  transform: translateX(2px);
}
.thumbMd {
  @apply h-4 w-4;
}
.thumbMd[data-state='checked'] {
  transform: translateX(20px);
}
.thumbMd[data-state='unchecked'] {
  transform: translateX(2px);
}
</style>
