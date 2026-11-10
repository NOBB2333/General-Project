<script setup lang="ts">
defineOptions({ inheritAttrs: false });

withDefaults(
  defineProps<{
    modelValue: string | number;
    type?: 'text' | 'email' | 'tel' | 'url' | 'number' | 'password' | 'date' | 'datetime-local';
    invalid?: boolean;
  }>(),
  { type: 'text' },
);

const emit = defineEmits<{ 'update:modelValue': [value: string] }>();
</script>

<template>
  <input
    v-bind="$attrs"
    :value="modelValue"
    :type="type"
    :aria-invalid="invalid || undefined"
    :class="[$style.input, invalid && $style.invalid]"
    @input="emit('update:modelValue', ($event.target as HTMLInputElement).value)"
  />
</template>

<style module>
@reference "../../styles/index.css";
.input {
  @apply h-9 w-full border px-3 text-[12px] outline-none transition-colors;
  color: var(--ui-text);
  background: var(--ui-surface);
  border-color: var(--ui-border);
  border-radius: var(--app-control-radius);
}

.input::placeholder {
  color: #97a0af;
}
.input:focus {
  border-color: var(--ui-focus);
  box-shadow: 0 0 0 1px var(--ui-focus);
}
.input:disabled {
  @apply cursor-not-allowed opacity-60;
  background: var(--ui-surface-hover);
}
.invalid {
  border-color: var(--ui-danger);
}
</style>
