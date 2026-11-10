<script setup lang="ts">
import { BarChart3, List, Star, UserRound, UsersRound } from 'lucide-vue-next';
import type { QuoteScope, QuoteView } from '@/features/crm/model/sales-quotes.types';

defineProps<{ scope: QuoteScope; view: QuoteView; counts: Record<QuoteScope, number> }>();
const emit = defineEmits<{ 'update:scope': [value: QuoteScope]; 'update:view': [value: QuoteView] }>();

const scopes = [
  { value: 'all', label: '所有', icon: UsersRound },
  { value: 'mine', label: '我负责的', icon: UserRound },
  { value: 'subordinate', label: '下属负责的', icon: UsersRound },
  { value: 'following', label: '我关注的', icon: Star },
] as const;
</script>

<template>
  <section :class="$style.bar">
    <div
      :class="$style.scopeGroup"
      aria-label="报价管辖范围"
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
        />
        {{ item.label }} <span>{{ counts[item.value] }}</span>
      </button>
    </div>
    <div
      :class="$style.viewGroup"
      aria-label="报价视图"
    >
      <button
        type="button"
        :class="[$style.viewButton, view === 'list' && $style.viewActive]"
        :aria-pressed="view === 'list'"
        @click="emit('update:view', 'list')"
      >
        <List :size="14" />报价列表
      </button>
      <button
        type="button"
        :class="[$style.viewButton, view === 'analysis' && $style.viewActive]"
        :aria-pressed="view === 'analysis'"
        @click="emit('update:view', 'analysis')"
      >
        <BarChart3 :size="14" />报价分析
      </button>
    </div>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.bar {
  @apply mt-4 flex items-center justify-between;
}
.scopeGroup,
.viewGroup {
  @apply inline-flex overflow-hidden rounded border bg-white p-1;
  border-color: var(--ui-border);
}
.scopeButton,
.viewButton {
  @apply flex h-8 items-center gap-1.5 rounded px-3 text-[12px] font-medium transition-colors;
  color: var(--ui-text-subtle);
}
.scopeButton:hover,
.viewButton:hover {
  color: var(--ui-text);
  background: var(--ui-surface-hover);
}
.scopeButton span {
  @apply rounded-full px-1.5 py-0.5 text-[10px];
  background: var(--ui-surface-hover);
}
.active {
  color: white;
  background: var(--app-primary);
}
.active:hover {
  color: white;
  background: var(--app-primary-hover);
}
.active span {
  color: white;
  background: rgb(255 255 255 / 20%);
}
.viewGroup {
  @apply border-0;
  background: var(--ui-surface-hover);
}
.viewActive {
  color: var(--ui-text);
  background: white;
  box-shadow: 0 1px 2px rgb(9 30 66 / 12%);
}
@media (max-width: 760px) {
  .bar {
    @apply items-start gap-2 overflow-x-auto;
  }
  .scopeGroup,
  .viewGroup {
    @apply shrink-0;
  }
}
</style>
