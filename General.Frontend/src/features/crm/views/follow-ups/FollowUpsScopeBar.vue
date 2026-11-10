<script setup lang="ts">
import { Star, UserRound, UsersRound } from 'lucide-vue-next';
import type { FollowUpScope } from '@/features/crm/model/follow-ups.types';

defineProps<{ modelValue: FollowUpScope }>();
const emit = defineEmits<{ 'update:modelValue': [value: FollowUpScope] }>();

const scopes: { value: FollowUpScope; label: string; icon: typeof UserRound }[] = [
  { value: 'all', label: '所有', icon: UsersRound },
  { value: 'mine', label: '我负责的', icon: UserRound },
  { value: 'subordinate', label: '下属负责的', icon: UsersRound },
  { value: 'following', label: '我关注的', icon: Star },
];
</script>

<template>
  <nav
    :class="$style.scopeBar"
    aria-label="跟进记录管辖范围"
  >
    <button
      v-for="item in scopes"
      :key="item.value"
      type="button"
      :class="[$style.scopeButton, modelValue === item.value && $style.scopeActive]"
      :aria-pressed="modelValue === item.value"
      @click="emit('update:modelValue', item.value)"
    >
      <component
        :is="item.icon"
        :size="14"
      />
      {{ item.label }}
    </button>
  </nav>
</template>

<style module>
@reference "../../../../styles/index.css";

.scopeBar {
  @apply mt-4 flex min-h-10 items-center gap-1 rounded border bg-white p-1;
  border-color: var(--ui-border);
}
.scopeButton {
  @apply flex h-8 items-center gap-1.5 rounded px-4 text-[12px] font-medium transition-colors;
  color: var(--ui-text-subtle);
}
.scopeButton:hover {
  background: var(--ui-surface-hover);
}
.scopeActive {
  color: white;
  background: var(--app-primary);
}
.scopeActive:hover {
  background: var(--app-primary);
}
</style>
