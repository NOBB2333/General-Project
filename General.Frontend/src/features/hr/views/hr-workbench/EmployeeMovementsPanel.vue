<script setup lang="ts">
import { CalendarDays, UserMinus, UserPlus } from 'lucide-vue-next';
import type { EmployeeMovement } from '../../model/hr-workbench.types';
defineProps<{ movements: readonly EmployeeMovement[] }>();
const emit = defineEmits<{ open: [movement: EmployeeMovement]; viewAll: [] }>();
</script>

<template>
  <section :class="$style.panel">
    <header>
      <CalendarDays
        :size="16"
        :stroke-width="1.8"
      />
      <h2>入离职动态</h2>
      <button
        type="button"
        @click="emit('viewAll')"
      >
        查看全部 ›
      </button>
    </header>
    <button
      v-for="item in movements"
      :key="item.id"
      type="button"
      :class="$style.row"
      @click="emit('open', item)"
    >
      <span :class="[$style.avatar, item.type === '离职' && $style.leave]"
        ><UserPlus
          v-if="item.type === '入职'"
          :size="15"
          :stroke-width="1.8" /><UserMinus
          v-else
          :size="15"
          :stroke-width="1.8" /></span
      ><span :class="$style.copy"
        ><strong
          >{{ item.name }} <em :data-type="item.type">{{ item.type }}</em></strong
        ><small>{{ item.department }} · {{ item.position }}</small
        ><small>{{ item.date }}</small></span
      >
    </button>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";
.panel {
  @apply overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}
.panel > header {
  @apply flex h-[58px] items-center gap-2 border-b px-5;
  border-color: var(--ui-border);
  color: #00875a;
}
.panel h2 {
  @apply m-0 text-[14px];
  color: var(--ui-text);
}
.panel header button {
  @apply ml-auto text-[10px] font-semibold;
  color: var(--app-primary);
}
.row {
  @apply flex h-[88px] w-full items-center gap-3 border-b px-5 text-left transition-colors last:border-b-0;
  border-color: var(--ui-border-subtle);
}
.row:hover {
  background: var(--ui-surface-hover);
}
.avatar {
  @apply flex h-9 w-9 shrink-0 items-center justify-center rounded-full;
  color: #00875a;
  background: #e3fcef;
}
.leave {
  color: #de350b;
  background: #ffebe6;
}
.copy {
  @apply flex min-w-0 flex-col;
}
.copy strong {
  @apply text-[11px];
}
.copy em {
  @apply ml-1 rounded px-1.5 py-0.5 text-[8px] not-italic;
  color: #00875a;
  background: #e3fcef;
}
.copy em[data-type='离职'] {
  color: #de350b;
  background: #ffebe6;
}
.copy small {
  @apply mt-1 text-[9px];
  color: var(--ui-text-subtle);
}
</style>
