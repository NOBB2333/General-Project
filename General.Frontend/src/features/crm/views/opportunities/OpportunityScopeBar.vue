<script setup lang="ts">
import { Star, UserRound, UsersRound } from 'lucide-vue-next';
import type { OpportunityScope } from '@/features/crm/model/opportunities.types';

defineProps<{ scope: OpportunityScope; counts: Record<OpportunityScope, number> }>();
const emit = defineEmits<{ 'update:scope': [value: OpportunityScope] }>();

const scopes = [
  { value: 'all', label: '所有', icon: UsersRound },
  { value: 'mine', label: '我负责的', icon: UserRound },
  { value: 'subordinate', label: '下属负责的', icon: UsersRound },
  { value: 'following', label: '我关注的', icon: Star },
] as const;
</script>

<template>
  <section
    :class="$style.bar"
    aria-label="商机管辖范围"
  >
    <button
      v-for="item in scopes"
      :key="item.value"
      type="button"
      :class="[$style.scopeButton, scope === item.value && $style.active]"
      :aria-pressed="scope === item.value"
      @click="emit('update:scope', item.value)"
    >
      <component
        :is="item.icon"
        :size="14"
        :stroke-width="1.8"
      />
      {{ item.label }} <span>{{ counts[item.value] }}</span>
    </button>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.bar {
  @apply mt-4 flex min-h-10 items-center gap-1 rounded border bg-white p-1;
  border-color: var(--ui-border);
}
.scopeButton {
  @apply flex h-8 items-center gap-1.5 rounded px-3 text-[12px] font-medium transition-colors;
  color: var(--ui-text-subtle);
}
.scopeButton:hover {
  color: var(--ui-text);
  background: var(--ui-surface-hover);
}
.scopeButton span {
  @apply rounded-full px-1.5 py-0.5 text-[10px];
  background: var(--ui-surface-hover);
}
.active,
.active:hover {
  color: white;
  background: var(--app-primary);
}
.active span {
  color: white;
  background: rgb(255 255 255 / 20%);
}
@media (max-width: 680px) {
  .bar {
    @apply overflow-x-auto;
  }
  .scopeButton {
    @apply shrink-0;
  }
}
</style>
