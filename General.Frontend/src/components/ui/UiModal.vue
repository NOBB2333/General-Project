<script setup lang="ts">
import { AlertTriangle, X } from 'lucide-vue-next';
import UiButton from './UiButton.vue';
import UiDialog from './UiDialog.vue';

const props = withDefaults(
  defineProps<{
    open: boolean;
    title: string;
    description?: string;
    confirmLabel?: string;
    tone?: 'info' | 'warning' | 'danger';
  }>(),
  { confirmLabel: '确认', tone: 'info' },
);

const emit = defineEmits<{ close: []; confirm: [] }>();
</script>

<template>
  <UiDialog
    :open="open"
    :title="title"
    :description="description"
    size="sm"
    bare
    @close="emit('close')"
  >
    <template #default="{ close }">
      <div :class="$style.content">
        <span :class="[$style.icon, $style[tone]]">
          <AlertTriangle :size="20" />
        </span>
        <div :class="$style.message">
          <h3 :class="$style.title">{{ title }}</h3>
          <p
            v-if="description"
            :class="$style.description"
          >
            {{ description }}
          </p>
          <slot />
        </div>
        <UiButton
          aria-label="关闭"
          variant="ghost"
          size="icon-sm"
          @click="close"
        >
          <X :size="16" />
        </UiButton>
      </div>
      <footer :class="$style.footer">
        <UiButton
          variant="secondary"
          @click="close"
          >取消</UiButton
        >
        <UiButton
          :variant="tone === 'danger' ? 'danger' : tone === 'warning' ? 'warning' : 'primary'"
          @click="emit('confirm')"
        >
          {{ confirmLabel }}
        </UiButton>
      </footer>
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../styles/index.css";
.content {
  @apply flex items-start gap-4 px-6 pb-4 pt-6;
}
.icon {
  @apply flex h-10 w-10 shrink-0 items-center justify-center rounded-full;
}
.info {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.warning {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}
.danger {
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}
.message {
  @apply min-w-0 flex-1;
}
.title {
  @apply mb-1 text-[16px] font-semibold;
  color: var(--ui-text);
}
.description {
  @apply text-[13px] leading-5;
  color: var(--ui-text-muted);
}
.footer {
  @apply flex items-center justify-end gap-2 border-t px-6 py-4;
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}
</style>
