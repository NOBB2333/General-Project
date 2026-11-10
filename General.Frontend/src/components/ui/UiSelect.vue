<script setup lang="ts">
import { Check, ChevronDown, ChevronUp } from 'lucide-vue-next';
import { computed } from 'vue';
import {
  SelectContent,
  SelectIcon,
  SelectItem,
  SelectItemIndicator,
  SelectItemText,
  SelectPortal,
  SelectRoot,
  SelectScrollDownButton,
  SelectScrollUpButton,
  SelectTrigger,
  SelectValue,
  SelectViewport,
  type AcceptableValue,
} from 'reka-ui';
import type { UiSelectOption } from './UiSelect.types';

const EMPTY_OPTION_VALUE = '__ui-select-empty-option__';

defineOptions({ inheritAttrs: false });

const props = withDefaults(
  defineProps<{
    modelValue: string;
    options: readonly UiSelectOption[];
    label: string;
    placeholder?: string;
    disabled?: boolean;
    name?: string;
    required?: boolean;
    size?: 'sm' | 'md';
  }>(),
  {
    placeholder: '请选择',
    size: 'md',
  },
);

const emit = defineEmits<{
  'update:modelValue': [value: string];
  'update:open': [value: boolean];
}>();

const hasEmptyOption = computed(() => props.options.some((option) => option.value === ''));
const normalizedModelValue = computed(() =>
  props.modelValue === '' && hasEmptyOption.value ? EMPTY_OPTION_VALUE : props.modelValue,
);

function normalizeOptionValue(value: string) {
  return value === '' ? EMPTY_OPTION_VALUE : value;
}

function updateValue(value: AcceptableValue) {
  const normalizedValue = String(value);
  emit('update:modelValue', normalizedValue === EMPTY_OPTION_VALUE ? '' : normalizedValue);
}
</script>

<template>
  <SelectRoot
    :model-value="normalizedModelValue"
    :disabled="disabled"
    :name="name"
    :required="required"
    @update:model-value="updateValue"
    @update:open="emit('update:open', $event)"
  >
    <SelectTrigger
      v-bind="$attrs"
      data-ui-select
      :class="[$style.trigger, $style[size]]"
      :aria-label="label"
    >
      <SelectValue :placeholder="placeholder" />
      <SelectIcon :class="$style.triggerIcon">
        <ChevronDown
          :size="14"
          :stroke-width="1.8"
        />
      </SelectIcon>
    </SelectTrigger>

    <SelectPortal to="#ui-portals">
      <SelectContent
        position="popper"
        :side-offset="4"
        :class="$style.content"
      >
        <SelectScrollUpButton :class="$style.scrollButton">
          <ChevronUp
            :size="14"
            :stroke-width="1.8"
          />
        </SelectScrollUpButton>

        <SelectViewport :class="$style.viewport">
          <SelectItem
            v-for="option in options"
            :key="option.value"
            :value="normalizeOptionValue(option.value)"
            :disabled="option.disabled"
            :text-value="option.label"
            :class="$style.item"
          >
            <SelectItemText>{{ option.label }}</SelectItemText>
            <SelectItemIndicator :class="$style.itemIndicator">
              <Check
                :size="14"
                :stroke-width="1.8"
              />
            </SelectItemIndicator>
          </SelectItem>
        </SelectViewport>

        <SelectScrollDownButton :class="$style.scrollButton">
          <ChevronDown
            :size="14"
            :stroke-width="1.8"
          />
        </SelectScrollDownButton>
      </SelectContent>
    </SelectPortal>
  </SelectRoot>
</template>

<style module>
@reference "../../styles/index.css";

.trigger {
  @apply inline-flex min-w-32 items-center justify-between gap-2 whitespace-nowrap border px-3 text-left text-[12px] outline-none transition-colors disabled:cursor-not-allowed disabled:opacity-60;
  color: var(--ui-text-muted);
  background: var(--ui-surface);
  border-color: var(--ui-border);
  border-radius: var(--app-control-radius);
}

.trigger > span:first-child {
  @apply min-w-0 truncate;
}

.trigger:hover:not(:disabled) {
  background: var(--ui-surface-hover);
}

.trigger:focus-visible,
.trigger[data-state='open'] {
  border-color: var(--ui-focus);
  box-shadow: 0 0 0 1px var(--ui-focus);
}

.sm {
  @apply h-8;
}

.md {
  @apply h-9;
}

.triggerIcon {
  @apply shrink-0 transition-transform;
  color: var(--ui-text-subtle);
}

.trigger[data-state='open'] .triggerIcon {
  transform: rotate(180deg);
}

.content {
  @apply overflow-hidden border shadow-lg outline-none;
  z-index: var(--ui-z-floating);
  min-width: var(--reka-select-trigger-width);
  max-height: min(320px, var(--reka-select-content-available-height));
  color: var(--ui-text);
  background: var(--ui-surface);
  border-color: var(--ui-border);
  border-radius: var(--app-control-radius);
  transform-origin: var(--reka-select-content-transform-origin, top);
}

.content[data-state='open'] {
  animation: selectIn 150ms var(--ease-out-ui);
}

.content[data-state='closed'] {
  animation: selectOut 100ms ease-in;
}

.viewport {
  @apply p-1;
}

.item {
  @apply relative flex min-h-8 cursor-default select-none items-center rounded py-1.5 pl-3 pr-8 text-[12px] outline-none transition-colors;
}

.item[data-highlighted] {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.item[data-state='checked'] {
  @apply font-medium;
}

.item[data-disabled] {
  @apply pointer-events-none opacity-45;
}

.itemIndicator {
  @apply absolute right-2 inline-flex items-center justify-center;
  color: var(--app-primary);
}

.scrollButton {
  @apply flex h-7 cursor-default items-center justify-center;
  color: var(--ui-text-subtle);
  background: var(--ui-surface);
}

@keyframes selectIn {
  from {
    opacity: 0;
    transform: scale(0.97);
  }
  to {
    opacity: 1;
    transform: scale(1);
  }
}

@keyframes selectOut {
  from {
    opacity: 1;
    transform: scale(1);
  }
  to {
    opacity: 0;
    transform: scale(0.98);
  }
}

@media (prefers-reduced-motion: reduce) {
  .content[data-state='open'],
  .content[data-state='closed'] {
    animation: none;
  }

  .triggerIcon {
    transition: none;
  }
}
</style>
