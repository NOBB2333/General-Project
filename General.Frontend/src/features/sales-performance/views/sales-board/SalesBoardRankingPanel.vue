<script setup lang="ts">
import { Trophy } from 'lucide-vue-next';
import { salesBoardRankings } from '../../model/sales-board.data';

const emit = defineEmits<{ open: [id: string] }>();
</script>

<template>
  <article :class="$style.panel">
    <header :class="$style.header">
      <span :class="$style.icon">
        <Trophy
          :size="17"
          :stroke-width="1.8"
        />
      </span>
      <div>
        <h2>本期 Top 5 业绩榜</h2>
        <p>按确认业绩排序</p>
      </div>
    </header>
    <ol :class="$style.list">
      <li
        v-for="person in salesBoardRankings"
        :key="person.id"
      >
        <button
          type="button"
          :aria-label="`${person.rank} 名 ${person.name}，${person.team}，${person.amount}`"
          @click="emit('open', person.id)"
        >
          <span :class="$style.rank">{{ person.rank }}</span>
          <span :class="$style.avatar">{{ person.name.slice(0, 1) }}</span>
          <span :class="$style.person">
            <strong>{{ person.name }}</strong>
            <small>{{ person.team }}</small>
          </span>
          <span :class="$style.amount">
            <strong>{{ person.amount }}</strong>
            <small>目标 {{ person.rate }}</small>
          </span>
        </button>
      </li>
    </ol>
  </article>
</template>

<style module>
@reference "../../../../styles/index.css";

.panel {
  @apply min-w-0 overflow-hidden rounded border;
  background: var(--ui-surface);
  border-color: var(--ui-border);
}

.header {
  @apply flex min-h-[58px] items-center gap-3 px-5 py-3;
}

.icon {
  @apply flex h-7 w-7 shrink-0 items-center justify-center rounded;
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}

.header h2 {
  @apply text-[13px] font-semibold;
  color: var(--ui-text);
}

.header p {
  @apply mt-0.5 text-[11px];
  color: var(--ui-text-subtle);
}

.list {
  @apply border-t;
  border-color: var(--ui-border-subtle);
}

.list button {
  @apply flex w-full items-center gap-3 border-b px-4 py-2.5 text-left outline-none transition-colors;
  border-color: var(--ui-border-subtle);
}

.list li:last-child button {
  @apply border-b-0;
}

.list button:hover {
  background: var(--ui-surface-hover);
}

.list button:focus-visible {
  box-shadow: inset 0 0 0 2px var(--ui-focus);
}

.rank {
  @apply w-4 text-center text-[11px] font-bold;
  color: var(--ui-text-subtle);
}

.list li:nth-child(-n + 3) .rank {
  color: var(--ui-warning);
}

.avatar {
  @apply flex h-7 w-7 items-center justify-center rounded-full text-[11px] font-semibold;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.person,
.amount {
  @apply flex min-w-0 flex-col;
}

.person {
  @apply flex-1;
}

.amount {
  @apply items-end;
}

.person strong,
.amount strong {
  @apply truncate text-[11px] font-semibold;
  color: var(--ui-text);
}

.person small,
.amount small {
  @apply mt-0.5 truncate text-[9px];
  color: var(--ui-text-subtle);
}

@media (prefers-reduced-motion: reduce) {
  .list button {
    transition: none;
  }
}
</style>
